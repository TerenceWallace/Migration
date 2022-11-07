Namespace Migration
	Friend Class RoutingNodeMap
		Private m_Map(,) As RoutingNode
		Private privateWidth As Integer
		Public Property Width() As Integer
			Get
				Return privateWidth
			End Get
			Private Set(ByVal value As Integer)
				privateWidth = value
			End Set
		End Property
		Private privateHeight As Integer
		Public Property Height() As Integer
			Get
				Return privateHeight
			End Get
			Private Set(ByVal value As Integer)
				privateHeight = value
			End Set
		End Property
		Private privateCount As Integer
		Public Property Count() As Integer
			Get
				Return privateCount
			End Get
			Private Set(ByVal value As Integer)
				privateCount = value
			End Set
		End Property

		Default Public ReadOnly Property Item(ByVal x As Int32, ByVal y As Int32) As RoutingNode
			Get
				Return m_Map(x, y)
			End Get
		End Property

		Default Public ReadOnly Property Item(ByVal Node As RoutingNode) As RoutingNode
			Get
				Return m_Map(Node.CellXY.X, Node.CellXY.Y)
			End Get

		End Property

		Public ReadOnly Property IsEmpty() As Boolean
			Get
				Return Count = 0
			End Get
		End Property

		Public Sub New(ByVal inWidth As Integer, ByVal inHeight As Integer)
			m_Map = New RoutingNode(inWidth - 1, inHeight - 1){}
			Width = inWidth
			Height = inHeight
		End Sub

		Public Sub Add(ByVal inValue As RoutingNode)
			Dim item As RoutingNode = m_Map(inValue.CellXY.X, inValue.CellXY.Y)

#If DEBUG Then
			If item IsNot Nothing Then
				Throw New ApplicationException()
			End If
#End If

			Count += 1
			m_Map(inValue.CellXY.X, inValue.CellXY.Y) = inValue
		End Sub

		Public Function Contains(ByVal inValue As RoutingNode) As Boolean
			Dim item As RoutingNode = m_Map(inValue.CellXY.X, inValue.CellXY.Y)

			If item Is Nothing Then
				Return False
			End If

#If DEBUG Then
			If Not(inValue.Equals(item)) Then
				Throw New ApplicationException()
			End If
#End If

			Return True
		End Function

		Public Sub Remove(ByVal inValue As RoutingNode)
			Dim item As RoutingNode = m_Map(inValue.CellXY.X, inValue.CellXY.Y)

#If DEBUG Then
			If Not(inValue.Equals(item)) Then
				Throw New ApplicationException()
			End If
#End If

			Count -= 1
			m_Map(inValue.CellXY.X, inValue.CellXY.Y) = Nothing
		End Sub

		Public Sub Clear()
			Count = 0

			For x As Integer = 0 To Width - 1
				For y As Integer = 0 To Height - 1
					m_Map(x, y) = Nothing
				Next y
			Next x
		End Sub
	End Class
End Namespace
