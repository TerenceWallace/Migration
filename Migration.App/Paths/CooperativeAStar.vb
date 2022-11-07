Imports Migration.Common
Imports Migration.Core

Namespace Migration
	Friend Class CooperativeAStar

		Private m_ClosedSet As New SortedDictionary(Of PathNodeKey, Object)(PathNodeKey.Comparer)
		Private m_OpenSet As New SortedDictionary(Of PathNodeKey, Object)(PathNodeKey.Comparer)
		Private m_Reservation As New SortedDictionary(Of MovablePathNode, MovablePathNode)(MovablePathNode.Comparer)
		Private m_OrderedOpenSet As PriorityQueue(Of PathNodeKey)
		Private m_CameFrom As New CameFromMap()
		Private m_ObstacleScanner As AStarSolver
		Private m_Size As Integer
		Private m_SizeShift As Integer
		Private m_UserGrid() As Byte
		Private ReadOnly m_Neighbors(6) As PathNodeKey
		Private m_StartNode As PathNodeKey
		Private Const SQRT_2 As Double = 1.41421356

		Private privateSearchSpace As TerrainRouting
		Public Property SearchSpace() As TerrainRouting
			Get
				Return privateSearchSpace
			End Get
			Private Set(ByVal value As TerrainRouting)
				privateSearchSpace = value
			End Set
		End Property

		Public Sub New(ByVal inParent As TerrainRouting)
			Dim log2Factor As Double = 1.0 / Math.Log(2.0)

			SearchSpace = inParent
			m_Size = inParent.Size
			m_Size = inParent.Size
			m_SizeShift = Convert.ToInt32(CInt(Fix(Math.Floor((Math.Log(Convert.ToDouble(m_Size)) * log2Factor) + 0.5))))
			m_OrderedOpenSet = New PriorityQueue(Of PathNodeKey)()
			'm_ObstacleScanner = new AStarSolver(SearchSpace.Terrain);
		End Sub

		Private Function GetNeighborDistance(ByVal inStart As PathNodeKey, ByVal inEnd As PathNodeKey) As Double
			Dim diffX As Integer = Math.Abs(inStart.X - inEnd.X)
			Dim diffY As Integer = Math.Abs(inStart.Y - inEnd.Y)

			Select Case diffX + diffY
				Case 1
					Return 1
				Case 2
					Return SQRT_2
				Case Else
					Return 1
			End Select
		End Function

		Public Sub UnregisterPathNode(ByVal inPathNode As MovablePathNode)
			m_Reservation.Remove(inPathNode)
		End Sub

		Public Sub UnregisterPath(ByVal inPath As LinkedList(Of MovablePathNode))
			For Each entry As MovablePathNode In inPath
				UnregisterPathNode(entry)
			Next entry
		End Sub

		''' <summary>
		''' Returns null, if no path is found. Start- and End-Node are included in returned path. The user context
		''' is passed to IsWalkable().
		''' </summary>
		Public Sub Search(ByVal inMovable As Movable, ByVal inCurrent As PathNodeKey, ByVal inTarget As PathNodeKey, ByVal inWallBreaker As WallValue)
			Dim inWithCooperation As Boolean = True
			Dim inWithWallBreaking As Boolean = False
			SearchInternal(inMovable, inCurrent, inTarget, inWallBreaker, inWithCooperation, inWithWallBreaking)
		End Sub

		Private Shared s_ProcessedNodes As Int64 = 0
		Private Shared s_ProcessCounter As Int64 = 0
		Private Shared s_CoopFallbackCounter As Int64 = 0
		Private Shared s_WallBreakFallbackCounter As Int64 = 0

		Private Sub SearchInternal(ByVal inMovable As Movable, ByVal inCurrent As PathNodeKey, ByVal inTarget As PathNodeKey, ByVal inWallBreaker As WallValue, ByVal inWithCooperation As Boolean, ByVal inWithWallBreaking As Boolean)

			s_ProcessCounter += 1

			Dim keyNode As New MovablePathNode()
			Dim cycleResolution As Long = SearchSpace.CycleResolution

			m_StartNode = inCurrent
			m_UserGrid = SearchSpace.Terrain.GetWallMap()

			m_ClosedSet.Clear()
			m_OpenSet.Clear()
			m_OrderedOpenSet.Clear()
			m_CameFrom.Clear()

			m_StartNode.Time = SearchSpace.CurrentCycle
			m_StartNode.G = 0
			m_StartNode.H = SearchSpace.GetHeuristic(New Point(inTarget.X, inTarget.Y), New Point(inCurrent.X, inCurrent.Y))
			m_StartNode.F = m_StartNode.H
			m_StartNode.Length = 0

			m_OpenSet.Add(m_StartNode, Nothing)
			m_OrderedOpenSet.Push(m_StartNode)

			Do While m_OpenSet.Count > 0
				Dim x As PathNodeKey = m_OrderedOpenSet.Pop()

				If x.Length >= 6 Then
					ReconstructPath(inCurrent, m_CameFrom, m_CameFrom(x), inMovable.Path)

					If inWithCooperation Then


						For Each entry As MovablePathNode In inMovable.Path
							entry.Movable = inMovable

							'm_Reservation.Remove(entry);
							'm_Reservation.Add(entry, entry);

							'                                 TODO: Also add an entry for the opposite direction, if not already there.
							'                                 * This will prevent two colliding movables from walking through each
							'                                 * other on a straight line, which is a common case, since well...
							'                                 * it is optimal ;) but still not desired.
							'                                 
						Next entry
					End If

					Return
				End If

				m_OpenSet.Remove(x)
				m_ClosedSet.Add(x, Nothing)

				s_ProcessedNodes += 1

				StoreNeightbors(x, cycleResolution)

				' bool isOnReservedCell = m_UserGrid[x.X + (x.Y << m_SizeShift)] >= (int)WallValue.Reserved;

				For i As Integer = 0 To m_Neighbors.Length - ((If(inWithWallBreaking, 1, 0))) - 1
					Dim y As PathNodeKey = m_Neighbors(i)
					Dim tentative_is_better As Boolean = False

					If m_ClosedSet.ContainsKey(y) Then
						Continue For
					End If

					If inWithCooperation Then

                        Dim reservation As MovablePathNode = Nothing

						keyNode.Time = y.Time
						keyNode.Position = New Point(y.X, y.Y)

						If m_Reservation.TryGetValue(keyNode, reservation) Then
							If reservation.Movable.HasJob Then
								Continue For
							Else
								If Not inMovable.HasJob Then
									Continue For
								End If

								reservation.Movable.IsInvalidated = True
							End If
						End If
					End If

					If Not inWithWallBreaking Then
						If m_UserGrid(y.X + (y.Y << m_SizeShift)) >= Convert.ToInt32(CInt(WallValue.WaterBorder)) Then
							Continue For
						End If
					End If

					Dim tentative_g_score As Double = x.G + GetNeighborDistance(x, y)
					Dim wasAdded As Boolean = False

					If Not(m_OpenSet.ContainsKey(y)) Then
						m_OpenSet.Add(y, Nothing)
						tentative_is_better = True
						wasAdded = True
					ElseIf tentative_g_score < y.G Then
						tentative_is_better = True
					Else
						tentative_is_better = False
					End If

					If tentative_is_better Then
						m_CameFrom.Add(y, x)

						y.G = tentative_g_score
						y.H = SearchSpace.GetHeuristic(New Point(inTarget.X, inTarget.Y), New Point(y.X, y.Y))
						y.F = y.G + y.H

						If wasAdded Then
							m_OrderedOpenSet.Push(y)
						Else
							m_OrderedOpenSet.Update(y)
						End If
					End If
				Next i
			Loop

			If inWithCooperation Then
				s_CoopFallbackCounter += 1
				inWithCooperation = False
				inWithWallBreaking = False
				SearchInternal(inMovable, inCurrent, inTarget, inWallBreaker, inWithCooperation, inWithWallBreaking)
			ElseIf Not inWithWallBreaking Then
				s_WallBreakFallbackCounter += 1
				inWithCooperation = False
				inWithWallBreaking = True
				SearchInternal(inMovable, inCurrent, inTarget, inWallBreaker, inWithCooperation, inWithWallBreaking)
			Else
				Throw New ArgumentException("No path could be found!")
			End If
		End Sub

		Private Sub StoreNeightbors(ByVal inAround As PathNodeKey, ByVal inCycleResolution As Long)
			Dim time As Long = inAround.Time + inCycleResolution
			Dim newLen As Integer = inAround.Length + 1

			m_Neighbors(6) = New PathNodeKey(inAround.X, inAround.Y, time, newLen)
			m_Neighbors(1) = New PathNodeKey(inAround.X + 1, inAround.Y, time, newLen)
			m_Neighbors(2) = New PathNodeKey(inAround.X + 1, inAround.Y + 1, time, newLen)
			m_Neighbors(3) = New PathNodeKey(inAround.X - 1, inAround.Y, time, newLen)
			m_Neighbors(4) = New PathNodeKey(inAround.X - 1, inAround.Y + 1, time, newLen)
			m_Neighbors(5) = New PathNodeKey(inAround.X + 1, inAround.Y - 1, time, newLen)
			m_Neighbors(0) = New PathNodeKey(inAround.X - 1, inAround.Y - 1, time, newLen)
		End Sub

		Private Sub ReconstructPath(ByVal inPosition As PathNodeKey, ByVal came_from As CameFromMap, ByVal current_node As PathNodeKey, ByVal outPath As LinkedList(Of MovablePathNode))
			Dim time As Long = current_node.Time
			Dim lastNodeKey As New PathNodeKey()

			came_from.GetPath(current_node, outPath)

			lastNodeKey = New PathNodeKey(current_node.X, current_node.Y, time)

			Dim lastDirection? As Direction = Nothing
			Dim node As LinkedListNode(Of MovablePathNode) = outPath.First

			Do While node IsNot Nothing
				Dim nextNode As LinkedListNode(Of MovablePathNode) = node.Next

				If nextNode IsNot Nothing Then
					Dim dir? As Direction = DirectionUtils.GetWalkingDirection(node.Value.Position, nextNode.Value.Position)

					node.Value.Direction = dir
					lastDirection = node.Value.Direction
				Else
					node.Value.Direction = lastDirection
				End If

				node = nextNode
			Loop
		End Sub

	End Class
End Namespace