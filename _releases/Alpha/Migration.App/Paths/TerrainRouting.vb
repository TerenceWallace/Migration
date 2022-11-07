Imports System.IO
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters.Binary
Imports Migration.Common
Imports Migration.Core

Namespace Migration
	''' <summary>
	''' In constrast to user controlables, which are currently not supported, AI controlables need 
	''' to be planned under strong constraints, since the user has almost no chance to correct
	''' misplanned paths or even cycle loops. The following class will make strong path planning
	''' more efficient by providing precomputed routes through the map. So instead of having to
	''' calculate the entire path, a movable only needs to compute the path to the next global 
	''' path node and then follow the precomputed route to the target global path node. Now again
	''' another short path is being computed to the precise target. Later, this it will be possible
	''' to implement some sort of easy and fast cooperative path planning along these routes while
	''' using the same technique as used for user controlables (which is not implemented yet) to
	''' schedule the cooperative paths from and to the global path route. Both techniques together
	''' will allow to handle even 60.000 units in a cooperative and solid way maybe even on embedded
	''' devices. Later, the routing should also be updated when terrain changes through priest spells
	''' for example. For now only buildings and foilage are taken into account as dynamic parameter.
	''' </summary>
	<Serializable> _
	Partial Friend Class TerrainRouting
		Private m_Tiling(,) As Byte
		Private m_SizeInCells As Integer
		Private m_SizeInNodes As Integer
		Private Const GranularityShift As Int32 = 4
		Private Const Granularity As Int32 = (1 << GranularityShift)
		Private m_RoutingTable()() As RoutingNode
		Private ReadOnly m_TmpBufferSrc(3) As RoutingNode
		Private ReadOnly m_TmpBufferDst(3) As RoutingNode

		<NonSerialized()> _
		Private m_CoopAStar As CooperativeAStar

		Private privateCurrentCycle As Int64
		Friend Property CurrentCycle() As Int64
			Get
				Return privateCurrentCycle
			End Get
			Private Set(ByVal value As Int64)
				privateCurrentCycle = value
			End Set
		End Property

		Private privateCycleResolution As Int64
		Friend Property CycleResolution() As Int64
			Get
				Return privateCycleResolution
			End Get
			Private Set(ByVal value As Int64)
				privateCycleResolution = value
			End Set
		End Property

		Friend ReadOnly Property Size() As Integer
			Get
				Return m_SizeInCells
			End Get
		End Property

		Private privateTerrain As TerrainDefinition
		Public Property Terrain() As TerrainDefinition
			Get
				Return privateTerrain
			End Get
			Private Set(ByVal value As TerrainDefinition)
				privateTerrain = value
			End Set
		End Property

		<OnDeserialized> _
		Private Sub OnDeserialized(ByVal ctx As StreamingContext)
			m_CoopAStar = New CooperativeAStar(Me)
		End Sub

		Friend Sub New(ByVal inTerrain As TerrainDefinition, ByVal inCurrentCycle As Int64, ByVal inCycleResolution As Int64)
			Terrain = inTerrain
			CycleResolution = inCycleResolution
			CurrentCycle = inCurrentCycle
			m_SizeInCells = inTerrain.Size
			m_SizeInNodes = inTerrain.Size >> GranularityShift
			m_CoopAStar = New CooperativeAStar(Me)
		End Sub

		Friend Sub AddOneCycle()
			CurrentCycle += 1
		End Sub

		Public Sub Analyze()
			Dim routingNodeAStar As New RoutingNodeAStar(Me)
			Dim backwardAStar As New AStarSolver(Terrain)

			If (m_SizeInCells Mod Granularity) <> 0 Then
				Throw New ApplicationException()
			End If

			m_Tiling = New Byte(m_SizeInCells - 1, m_SizeInCells - 1){}

			' put all nodes into the open list
			Dim openList As New Stack(Of Point)()

			For x As Integer = 0 To m_SizeInCells - 1
				For y As Integer = 0 To m_SizeInCells - 1
					openList.Push(New Point(x, y))
				Next y
			Next x

			' compute a map of unconnected map tiles, providing a constant time mapping between a 
			' map position and the tile it belongs to.
			Dim tileIndex As Integer = 1
			Dim tileCache As New List(Of Point)()

			Do While openList.Count > 0
				Dim node As Point = openList.Pop()

				If m_Tiling(node.X, node.Y) <> 0 Then
					Continue Do ' node is already assigned to a map tile
				End If

				If Terrain.GetWallAt(node.X, node.Y) = WallValue.WaterBorder Then
					Continue Do ' water borders are not threaded as walkable for anything, so neither threaded as tile
				End If

				' tile can NOT belong to any existing tile, otherwise A-Star would have found the connection in first place.
				If tileIndex > 255 Then
					Throw New NotSupportedException("Map contains more than 255 unconnected tiles.")
				End If

				' enumerate all nodes reachable from this map node
				tileCache.Clear()
				backwardAStar.ComputeConnectedTileAround(node, Convert.ToInt32(CInt(WallValue.Building)), tileCache)

				For Each entry As Point In tileCache
					m_Tiling(entry.X, entry.Y) = Convert.ToByte(tileIndex)
				Next entry

				tileIndex += 1
			Loop

			tileCache.Clear()

			'             Create the routine node table by computing neighbors and their a-star distances for/to each
			'             * routing node.
			'             

			' since all computation attempts are guaranteed to succeed, A-Star operates resonably fast here!
			Dim sizeInNodes As Integer = m_SizeInCells \ Granularity
			Dim directions() As RoutingDirection = { RoutingDirection._315, RoutingDirection._270, RoutingDirection._225, RoutingDirection._000, RoutingDirection._180, RoutingDirection._045, RoutingDirection._090, RoutingDirection._135 }

			m_RoutingTable = New RoutingNode(sizeInNodes - 1)(){}

			For i As Integer = 0 To sizeInNodes - 1
				m_RoutingTable(i) = New RoutingNode(sizeInNodes - 1){}
			Next i

			For xSrc As Integer = 0 To m_SizeInCells - 1 Step Granularity
				For ySrc As Integer = 0 To m_SizeInCells - 1 Step Granularity
					If Terrain.IsWater(New Point(xSrc, ySrc)) Then
						Continue For
					End If

					Dim srcNode As RoutingNode = m_RoutingTable(xSrc \ Granularity)(ySrc \ Granularity)

					If srcNode Is Nothing Then
						srcNode = New RoutingNode(sizeInNodes, xSrc, ySrc, GranularityShift)
						m_RoutingTable(xSrc \ Granularity)(ySrc \ Granularity) = srcNode
					End If

					' compute distance to neighbors
					' ATTENTION: if you change these loops, you may have to adjust "directions" properly!
					Dim xDst As Integer = xSrc - Granularity
					Dim offset As Integer = 0
					Do While xDst < xSrc + 2 * Granularity
						For yDst As Integer = ySrc - Granularity To ySrc + 2 * Granularity - 1 Step Granularity
							If (xDst < 0) OrElse (yDst < 0) OrElse (xDst >= m_SizeInCells) OrElse (yDst >= m_SizeInCells) Then
								Continue For
							End If

							Dim src As New Point(xSrc, ySrc)
							Dim dst As New Point(xDst, yDst)

							If (src = dst) OrElse (Not(AreConnected(src, dst))) OrElse Terrain.IsWater(dst) Then
								Continue For
							End If

							'                             Computing a path to a node neighbour may seem akward, but in fact such paths
							'                             * can get very long on ugly shaped maps. And later, the entire heuristic will be
							'                             * computed based on these distances between nodes we are computing now and thus
							'                             * they have to be as accurate as possible.
							'                             
							Dim dstNode As RoutingNode = m_RoutingTable(xDst \ Granularity)(yDst \ Granularity)
							Dim pathCosts As Double = 0

							If dstNode Is Nothing Then
								dstNode = New RoutingNode(sizeInNodes, xDst, yDst, GranularityShift)
								m_RoutingTable(xDst \ Granularity)(yDst \ Granularity) = dstNode
							End If

							backwardAStar.BeginSearch(src, dst, Convert.ToInt32(CInt(WallValue.Building)))
							pathCosts = backwardAStar.GetClosedGValue(dst)

							If pathCosts < 0 Then
								Throw New ApplicationException("Nodes are connected but path costs are negative!")
							End If

							srcNode.Neighbors(Convert.ToInt32(CInt(directions(offset)))) = New RoutingNodeLink(dstNode, pathCosts)
							offset += 1
						Next yDst
						xDst += Granularity
					Loop
				Next ySrc
			Next xSrc

			'             A node surrounds a cell if it is within a "Granularity - 1" radius (manhattan metric) 
			'             * around the cell AND is reachable from that cell. This limits the maximum count of
			'             * surroundings to four.
			'             * 
			'             * If a cell has no surroundings, the map is INVALID and will raise an exception!
			'             * For example a small island within a block of four nodes would produce this error.
			'             
			Dim surroundings(3) As RoutingNode

			For x As Integer = 0 To m_SizeInCells - 1
				For y As Integer = 0 To m_SizeInCells - 1
					Dim cell As New Point(x, y)

					If (Not(AreConnected(cell, cell))) OrElse Terrain.IsWater(cell) Then
						Continue For ' this won't work for water (border) cells
					End If

					GetSurroundings(cell, surroundings)

					If surroundings.Count(Function(e) e IsNot Nothing) = 0 Then
						Throw New ArgumentException("Invalid map! Each cell shall have at least one surrounding.")
					End If
				Next y
			Next x

			'            
			'             * Compute granulated heuristics between all non-water nodes. This implies again a wide 
			'             * range of maps being tagged invalid, since all connected land node must be heuristicly
			'             * connected, which is not the case for all ugly shaped maps. Further even if this succeeds,
			'             * the heuristic distance might be far off an optimal path when the map is not well shaped...
			'             
			For xSrc As Integer = 0 To m_SizeInNodes - 1
				For ySrc As Integer = 0 To m_SizeInNodes - 1
					Dim srcNode As RoutingNode = m_RoutingTable(xSrc)(ySrc)

					If srcNode Is Nothing Then
						Continue For
					End If

					routingNodeAStar.Initialize(srcNode)

					For xDst As Integer = 0 To m_SizeInNodes - 1
						For yDst As Integer = 0 To m_SizeInNodes - 1
							Dim dstNode As RoutingNode = m_RoutingTable(xDst)(yDst)

							If dstNode Is Nothing Then
								srcNode.Heuristics(xDst)(yDst) = Single.NaN

								Continue For
							End If

							If Not(AreConnected(srcNode.CellXY, dstNode.CellXY)) Then
								Continue For
							End If

							' distance between distinct nodes can never be zero, so it's just not computed yet

							Dim heuristic As Double

							If Not(routingNodeAStar.GetHeuristic(dstNode, heuristic)) Then
								Throw New ArgumentException("Map is INVALID! No heuristic connection between [" & srcNode.CellXY.ToString() & "] and [" & dstNode.CellXY.ToString() & "]")
							End If

							srcNode.Heuristics(dstNode.NodeX)(dstNode.NodeY) = Convert.ToSingle(heuristic)
						Next yDst
					Next xDst
				Next ySrc
			Next xSrc

			' all other data is computed on the fly and cached...

			'             Before we can start using this concept, we need to proof that connections on
			'             * cell level are equal to connections on a heuristic level, otherwise we might
			'             * run into ugly bugs later on...
			'             * 
			'             * "=>": If two cells are connected, then so they are heuristicly connected.
			'             *      # By precondition, each cells must have at least one surrounding
			'             *      # So let's assume the source and target node are NOT connected but
			'             *        the cells are
			'             *      # Since both cells are connected to their surroundings by definition,
			'             *        and connected to each other, it follows that their surrounding must
			'             *        be connected as well (since we can now construct an explicit path).
			'             *      
			'             * "<=": If two cells are heuristicly connected, then so they are connected.
			'             *      # Follows constructively by using the paths from the cells to their
			'             *        surroundings and by precondition, the paths between these surroundings.
			'             *        
			'             * So whats the sense about proving this more or less obvious assumption? Well
			'             * its not that obvious actually and also we've seen that the far-fetched 
			'             * restrictions to reject maps with unsurrounded cells, is a requirement to
			'             * proof this assumption. But of course we didn't proof that we couldn't proof it
			'             * without this restrictions, but I guess things start to get messy when we try 
			'             * to, also heavily reducing the speed of heuristic lookups. So even if the
			'             * restriction might not be a mathematical requirement, it seems to be a 
			'             * requirement in terms of speed.
			'             
		End Sub

		Public Sub SaveToStream(ByVal inStream As Stream)
			Dim format As New BinaryFormatter()

			format.Serialize(inStream, Me)
		End Sub

		Public Sub LoadFromStream(ByVal inStream As Stream)
			Dim format As New BinaryFormatter()
			Dim source As TerrainRouting = CType(format.Deserialize(inStream), TerrainRouting)

			If source.Size <> Size Then
				Throw New ArgumentException("The routing data stored in the stream is not compatible with this instance.")
			End If

			Me.m_RoutingTable = source.m_RoutingTable
			Me.m_Tiling = source.m_Tiling
		End Sub

		''' <summary>
		''' Checks wether a path could be found between two nodes, if attempted. This method
		''' executes in constant time and should always be checked before a path search is
		''' invoked. Failed path searches are computational extremely expensive.
		''' </summary>
		Public Function AreConnected(ByVal inSource As Point, ByVal inTarget As Point) As Boolean
			Dim mIsEqual As Boolean = Convert.ToBoolean(m_Tiling(inSource.X, inSource.Y) = m_Tiling(inTarget.X, inTarget.Y))
			Dim mNotZero As Boolean = Convert.ToBoolean(m_Tiling(inSource.X, inSource.Y) <> 0)
			Dim IsConnected As Boolean = Convert.ToBoolean(mIsEqual AndAlso mNotZero)

			Return IsConnected
		End Function

		Private Sub GetSurroundings(ByVal inCell As Point, ByVal outSurroundings() As RoutingNode)
			Dim x As Integer = inCell.X >> GranularityShift
			Dim y As Integer = inCell.Y >> GranularityShift

			outSurroundings(0) = m_RoutingTable(x)(y)

			If (inCell.X = (x << GranularityShift)) AndAlso (inCell.Y = (y << GranularityShift)) Then
				outSurroundings(1) = Nothing
				outSurroundings(2) = Nothing
				outSurroundings(3) = Nothing
			Else
				outSurroundings(1) = If(x + 1 < m_SizeInNodes, m_RoutingTable(x + 1)(y), Nothing)
				outSurroundings(2) = If((x + 1 < m_SizeInNodes) AndAlso (y + 1 < m_SizeInNodes), m_RoutingTable(x + 1)(y + 1), Nothing)
				outSurroundings(3) = If(y + 1 < m_SizeInNodes, m_RoutingTable(x)(y + 1), Nothing)
			End If
		End Sub

		Friend Function GetHeuristic(ByVal inSrc As Point, ByVal inDst As Point) As Double
			Dim minSrcNode As RoutingNode = Nothing
			Dim minDstNode As RoutingNode = Nothing
			Dim minPathCosts As Double = Double.PositiveInfinity

			GetSurroundings(inSrc, m_TmpBufferSrc)
			GetSurroundings(inDst, m_TmpBufferDst)

			For iSrc As Integer = 0 To 3
				Dim srcSurrounding As RoutingNode = m_TmpBufferSrc(iSrc)

				If srcSurrounding Is Nothing Then
					Continue For
				End If

				Dim pathCosts_Src As Double = inSrc.DistanceTo(srcSurrounding.CellXY)

				For iDst As Integer = 0 To 3
					Dim dstSurrounding As RoutingNode = m_TmpBufferDst(iDst)

					If (dstSurrounding Is Nothing) OrElse (Not(AreConnected(srcSurrounding.CellXY, dstSurrounding.CellXY))) Then
						Continue For
					End If

					Dim pathCosts As Double = pathCosts_Src + inDst.DistanceTo(dstSurrounding.CellXY) + srcSurrounding.Heuristics(dstSurrounding.NodeX)(dstSurrounding.NodeY)

					If pathCosts < minPathCosts Then
						minPathCosts = pathCosts
						minDstNode = dstSurrounding
						minSrcNode = srcSurrounding
					End If
				Next iDst
			Next iSrc

			If minDstNode IsNot Nothing Then
				If minDstNode.CellXY = minSrcNode.CellXY Then
					Return inSrc.DistanceTo(inDst)
				End If
			Else
				Return Double.NaN
			End If

			Return minPathCosts
		End Function

		Public Sub ClearPath(ByVal inMovable As Movable)
			m_CoopAStar.UnregisterPath(inMovable.Path)
			inMovable.Path.Clear()
		End Sub

		Public Sub SetDynamicPath(ByVal inMovable As Movable, ByVal inEnd As Point, ByVal inMovableType As MovableType)
			If (inMovable.CycleSpeed < 1) OrElse (inMovable.CycleSpeed > 1000) Then
				Throw New ArgumentOutOfRangeException()
			End If

			Debug.Assert((CurrentCycle Mod CycleResolution) = 0)

			' search for current path position and remove outdated path nodes
			Dim pathNodes As LinkedListNode(Of MovablePathNode) = inMovable.Path.Last
			Dim currentPathNode As LinkedListNode(Of MovablePathNode) = Nothing

			Do While pathNodes IsNot Nothing
				Dim prev As LinkedListNode(Of MovablePathNode) = pathNodes.Previous

				If pathNodes.Value.Time <> CurrentCycle Then
					m_CoopAStar.UnregisterPathNode(pathNodes.Value)
					inMovable.Path.Remove(pathNodes)
				Else
					currentPathNode = pathNodes
				End If

				pathNodes = prev
			Loop

			Debug.Assert(currentPathNode IsNot Nothing, "Movable path does not contain a path node with current cycle time.")

			Dim startNode As MovablePathNode = currentPathNode.Value

			ClearPath(inMovable)

			Dim IsConnected As Boolean = AreConnected(startNode.Position, inEnd)
			If Not IsConnected Then
				XMLTerrainGenerator.Save(Terrain.m_WallMap)

				Throw New ArgumentException()
			End If

			m_CoopAStar.Search(inMovable, New PathNodeKey(startNode.Position.X, startNode.Position.Y, 0), New PathNodeKey(inEnd.X, inEnd.Y, 0), WallValue.Reserved)

			inMovable.CurrentNode = inMovable.Path.First
			inMovable.IsInvalidated = False

		End Sub
	End Class
End Namespace
