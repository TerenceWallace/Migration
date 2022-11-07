Imports Migration.Core

Namespace Migration

	Friend Class AStarSolver
		Private m_ClosedSet As SolverMap
		Private m_OpenSet As SolverMap
		Private m_OrderedOpenSet As PriorityQueue(Of PathFinderNode)
		Private m_CameFrom() As PathFinderNode
		Private m_SearchSpace() As PathFinderNode
		Private m_StartNode As PathFinderNode
		Private m_EndNode As PathFinderNode
		Private m_UserGrid() As Byte
		Private m_UserContext As Integer
		Private Size As Integer
		Private SizeShift As Integer
		Private privateTerrain As TerrainDefinition
		Friend Property Terrain() As TerrainDefinition
			Get
				Return privateTerrain
			End Get
			Private Set(ByVal value As TerrainDefinition)
				privateTerrain = value
			End Set
		End Property

		Friend Sub New(ByVal inTerrain As TerrainDefinition)
			Dim log2Factor As Double = 1.0 / Math.Log(2.0)
			Dim x As Integer = 0
			Dim y As Integer = 0
			Dim node As PathFinderNode = Nothing

			Terrain = inTerrain
			Size = inTerrain.Size
			SizeShift = Convert.ToInt32(CInt(Fix(Math.Floor((Math.Log(Convert.ToDouble(Size)) * log2Factor) + 0.5))))

			If Convert.ToInt32(CInt(Fix(Math.Pow(2, Convert.ToDouble(SizeShift))))) <> Size Then
				Throw New ArgumentException()
			End If

			m_ClosedSet = New SolverMap(Size)
			m_OpenSet = New SolverMap(Size)
			m_OrderedOpenSet = New PriorityQueue(Of PathFinderNode)()
			m_CameFrom = New PathFinderNode(Size * Size - 1){}
			m_SearchSpace = New PathFinderNode(Size * Size - 1){}

			For x = 0 To Size - 1
				For y = 0 To Size - 1
					m_SearchSpace(x + (y << SizeShift)) = New PathFinderNode()
					node = m_SearchSpace(x + (y << SizeShift))

					node.X = x
					node.Y = y
				Next y
			Next x
		End Sub


		Private Const SQRT_2 As Double = 1.41421356

		Friend Function Heuristic(ByVal inStart As Point, ByVal inEnd As Point) As Double
			Return Math.Sqrt(Convert.ToDouble((inStart.X - inEnd.X) * (inStart.X - inEnd.X) + (inStart.Y - inEnd.Y) * (inStart.Y - inEnd.Y)))
		End Function

		Private Function Heuristic(ByVal inStart As PathFinderNode, ByVal inEnd As PathFinderNode) As Double
			Return Math.Sqrt(Convert.ToDouble((inStart.X - inEnd.X) * (inStart.X - inEnd.X) + (inStart.Y - inEnd.Y) * (inStart.Y - inEnd.Y)))
		End Function

		Private Function NeighborDistance(ByVal inStart As PathFinderNode, ByVal inEnd As PathFinderNode) As Double
			Dim diffX As Integer = Math.Abs(inStart.X - inEnd.X)
			Dim diffY As Integer = Math.Abs(inStart.Y - inEnd.Y)

			Select Case diffX + diffY
				Case 1
					Return 1
				Case 2
					Return SQRT_2
				Case Else
					Return 0
			End Select
		End Function

		Private Function ClipPoint(ByVal inPoint As Point) As Point
			If inPoint.X < 0 Then
				inPoint.X = 0
			End If

			If inPoint.Y < 0 Then
				inPoint.Y = 0
			End If

			If inPoint.X > Size - 1 Then
				inPoint.X = Size - 1
			End If

			If inPoint.Y > Size - 1 Then
				inPoint.Y = Size - 1
			End If

			Return inPoint
		End Function

		Friend Sub BeginSearch(ByVal inStartNode As Point, ByVal inEndNode As Point, ByVal inUserContext As Integer)
			Dim startNode As Point = ClipPoint(inStartNode)
			Dim endNode As Point = ClipPoint(inEndNode)

			m_StartNode = m_SearchSpace(startNode.X + (startNode.Y << SizeShift))
			m_EndNode = m_SearchSpace(endNode.X + (endNode.Y << SizeShift))
			m_UserContext = inUserContext
			m_UserGrid = Terrain.GetWallMap()

			m_ClosedSet.Clear()
			m_OpenSet.Clear()
			m_OrderedOpenSet.Clear()

			Array.Clear(m_CameFrom, 0, m_CameFrom.Length)

			m_StartNode.G = 0
			m_StartNode.H = Heuristic(m_StartNode, m_EndNode)
			m_StartNode.F = m_StartNode.H

			If m_UserGrid(m_StartNode.X + (m_StartNode.Y << SizeShift)) > m_UserContext Then
				Return ' if we can't walk on the start node we can't walk at all...
			End If

			m_OpenSet.Add(m_StartNode)
			m_OrderedOpenSet.Push(m_StartNode)
		End Sub

		Friend Function GetClosedGValue(ByVal inForNode As Point) As Double
			inForNode = ClipPoint(inForNode)

			SearchLoop(inForNode)

			If Not(m_ClosedSet.IsSet(inForNode)) Then
				Return -1
			End If

			Return m_ClosedSet(inForNode).G
		End Function

		Friend Sub SearchLoop(ByVal inRequiredClosedNode As Point)
			SearchLoop(inRequiredClosedNode, Int32.MaxValue \ 2, Nothing)
		End Sub

		Friend Sub ComputeConnectedTileAround(ByVal inAround As Point, ByVal inUserContext As Integer, ByVal outConnectedTileAround As List(Of Point))
			BeginSearch(inAround, inAround, inUserContext)

			outConnectedTileAround.Add(inAround)

			SearchLoop(Nothing, Int32.MaxValue \ 2, outConnectedTileAround)
		End Sub

		Friend Sub ComputePath(ByVal inStart As Point, ByVal inEnd As Point, ByVal inUserContext As Integer, ByVal outPathNodes As List(Of Point))
			BeginSearch(inStart, inEnd, inUserContext)

			If GetClosedGValue(inEnd) < 0 Then
				Throw New ArgumentException("No path could be found from [" & inStart.ToString() & "] to [" & inEnd.ToString() & "]!")
			End If

			Dim node As PathFinderNode = m_CameFrom(inEnd.X + (inEnd.Y << SizeShift))

			outPathNodes.Add(inEnd)

			Do While (node.X <> inStart.X) OrElse (node.Y <> inStart.Y)
				outPathNodes.Insert(0, New Point(node.X, node.Y))

				node = m_CameFrom(node.X + (node.Y << SizeShift))
			Loop

			outPathNodes.Add(inStart)
		End Sub

		Private Sub SearchLoop(ByVal inRequiredClosedNode? As Point, ByVal inMaxRadius As Int32, ByVal outConnectedTileAround As List(Of Point))
			Dim tentative_g_score As Double = 0
			Dim wasAdded As Boolean = False
			Dim neighborNodes(7) As PathFinderNode
			Dim y As PathFinderNode = Nothing
			Dim x As PathFinderNode = Nothing
			Dim tentative_is_better As Boolean = False
			Dim i As Integer = 0
			Dim reqCloseNode As New Point(0, 0)
			Dim hasReqCloseNode As Boolean = (inRequiredClosedNode.HasValue) AndAlso inRequiredClosedNode.HasValue

			If hasReqCloseNode Then
				reqCloseNode = ClipPoint(inRequiredClosedNode.Value)
			End If

			If hasReqCloseNode AndAlso m_ClosedSet.IsSet(reqCloseNode) Then
				Return
			End If

			Do While Not m_OpenSet.IsEmpty
				If hasReqCloseNode AndAlso m_ClosedSet.IsSet(reqCloseNode) Then
					Return
				End If

				x = m_OrderedOpenSet.Pop()

				m_OpenSet.Remove(x)
				m_ClosedSet.Add(x)

				If (Math.Abs(x.X - m_StartNode.X) > inMaxRadius) OrElse (Math.Abs(x.Y - m_StartNode.Y) > inMaxRadius) Then
					Exit Do
				End If

				StoreNeighborNodes(x, neighborNodes)

				For i = 0 To 7
					y = neighborNodes(i)

					If y Is Nothing Then
						Continue For
					End If

					If m_UserGrid(y.X + (y.Y << SizeShift)) > m_UserContext Then
						m_ClosedSet.Add(y)
					End If

					If m_ClosedSet.IsSet(y) Then
						Continue For
					End If

					tentative_g_score = x.G + NeighborDistance(x, y)
					wasAdded = False

					If Not(m_OpenSet.IsSet(y)) Then
						m_OpenSet.Add(y)
						tentative_is_better = True
						wasAdded = True

						If outConnectedTileAround IsNot Nothing Then
							outConnectedTileAround.Add(New Point(y.X, y.Y))
						End If
					ElseIf tentative_g_score < y.G Then
						tentative_is_better = True
					Else
						tentative_is_better = False
					End If

					If tentative_is_better Then
						m_CameFrom(y.X + (y.Y << SizeShift)) = x

						y.G = tentative_g_score
						y.H = Heuristic(y, m_EndNode)
						y.F = y.G + y.H

						If wasAdded Then
							m_OrderedOpenSet.Push(y)
						Else
							m_OrderedOpenSet.Update(y)
						End If
					End If
				Next i
			Loop
		End Sub


		Private Sub StoreNeighborNodes(ByVal inAround As PathFinderNode, ByVal inNeighbors() As PathFinderNode)
			Dim x As Integer = inAround.X
			Dim y As Integer = inAround.Y

			For i As Integer = 0 To inNeighbors.Length - 1
				inNeighbors(i) = Nothing
			Next i

			If Not((x > 0) AndAlso (y > 0)) Then
			Else
				inNeighbors(0) = m_SearchSpace(x - 1 + ((y - 1) << SizeShift))
			End If

			'if (!(y > 0)) { }
			'else
			'    inNeighbors[1] = m_SearchSpace[x + ((y - 1) << SizeShift)];

			If Not((x < Size - 1) AndAlso (y > 0)) Then
			Else
				inNeighbors(2) = m_SearchSpace(x + 1 + ((y - 1) << SizeShift))
			End If

			If Not(x > 0) Then
			Else
				inNeighbors(3) = m_SearchSpace(x - 1 + (y << SizeShift))
			End If

			If Not(x < Size - 1) Then
			Else
				inNeighbors(4) = m_SearchSpace(x + 1 + (y << SizeShift))
			End If

			If Not((x > 0) AndAlso (y < Size - 1)) Then
			Else
				inNeighbors(5) = m_SearchSpace(x - 1 + ((y + 1) << SizeShift))
			End If

			'if (!(y < Size - 1)) { }
			'else
			'    inNeighbors[6] = m_SearchSpace[x + ((y + 1) << SizeShift)];

			If Not((x < Size - 1) AndAlso (y < Size - 1)) Then
			Else
				inNeighbors(7) = m_SearchSpace(x + 1 + ((y + 1) << SizeShift))
			End If
		End Sub

	End Class
End Namespace
