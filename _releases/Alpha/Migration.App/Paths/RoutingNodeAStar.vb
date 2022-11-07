Namespace Migration
	Friend Class RoutingNodeAStar
		Private m_ClosedSet As RoutingNodeMap
		Private m_OpenSet As RoutingNodeMap
		Private m_OrderedOpenSet As PriorityQueue(Of RoutingNode)
		Private m_RuntimeGrid As RoutingNodeMap
		Private m_Size As Integer
		Private m_StartNode As RoutingNode

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
			SearchSpace = inParent
			m_Size = inParent.Size
			m_Size = inParent.Size
			m_ClosedSet = New RoutingNodeMap(m_Size, m_Size)
			m_OpenSet = New RoutingNodeMap(m_Size, m_Size)
			m_RuntimeGrid = New RoutingNodeMap(m_Size, m_Size)
			m_OrderedOpenSet = New PriorityQueue(Of RoutingNode)()
		End Sub

		Private Function Heuristic(ByVal inStart As RoutingNode, ByVal inEnd As RoutingNode) As Double
			Return inStart.CellXY.DistanceTo(inEnd.CellXY)
		End Function

		''' <summary>
		''' Returns null, if no path is found. Start- and End-Node are included in returned path. The user context
		''' is passed to IsWalkable().
		''' </summary>
		Public Sub Initialize(ByVal inStartNode As RoutingNode)
			If inStartNode Is Nothing Then
				Throw New ArgumentNullException()
			End If

			m_StartNode = inStartNode

			m_ClosedSet.Clear()
			m_OpenSet.Clear()
			m_RuntimeGrid.Clear()
			m_OrderedOpenSet.Clear()

			m_StartNode.G = 0
			m_StartNode.H = 0
			m_StartNode.F = m_StartNode.H

			m_OpenSet.Add(m_StartNode)
			m_OrderedOpenSet.Push(m_StartNode)

			m_RuntimeGrid.Add(m_StartNode)
		End Sub

		Public Function GetHeuristic(ByVal inForNode As RoutingNode, <System.Runtime.InteropServices.Out()> ByRef outHeuristic As Double) As Boolean
			If m_StartNode.CellXY = inForNode.CellXY Then
				outHeuristic = 0
				Return True
			End If

			If m_ClosedSet.Contains(inForNode) Then
				outHeuristic = m_ClosedSet(inForNode).G
				Return True
			End If

			Do While Not m_OpenSet.IsEmpty
				Dim x As RoutingNode = m_OrderedOpenSet.Pop()

				m_OpenSet.Remove(x)
				m_ClosedSet.Add(x)

				For i As Integer = 0 To x.Neighbors.Length - 1
					Dim yLink As RoutingNodeLink = x.Neighbors(i)
					Dim tentative_is_better As Boolean = False

					If yLink Is Nothing Then
						Continue For
					End If

					Dim y As RoutingNode = yLink.Node

					If m_ClosedSet.Contains(y) Then
						Continue For
					End If

					Dim tentative_g_score As Double = m_RuntimeGrid(x).G + yLink.Costs
					Dim wasAdded As Boolean = False

					If Not(m_OpenSet.Contains(y)) Then
						m_OpenSet.Add(y)
						tentative_is_better = True
						wasAdded = True
					ElseIf tentative_g_score < m_RuntimeGrid(y).G Then
						tentative_is_better = True
					Else
						tentative_is_better = False
					End If

					If tentative_is_better Then
						If Not(m_RuntimeGrid.Contains(y)) Then
							m_RuntimeGrid.Add(y)
						End If

						m_RuntimeGrid(y).G = tentative_g_score
						m_RuntimeGrid(y).H = Heuristic(y, inForNode)
						m_RuntimeGrid(y).F = m_RuntimeGrid(y).G + m_RuntimeGrid(y).H

						If wasAdded Then
							m_OrderedOpenSet.Push(y)
						Else
							m_OrderedOpenSet.Update(y)
						End If
					End If
				Next i

				If x.CellXY = inForNode.CellXY Then
					outHeuristic = inForNode.G
					Return True
				End If
			Loop

			outHeuristic = Double.NaN
			Return False
		End Function

	End Class
End Namespace