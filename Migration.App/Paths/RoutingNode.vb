Imports Migration.Core
Imports Migration.Interfaces

Namespace Migration

	''' <summary>
	''' Routing nodes represent a network graph which provides a higher granulated level
	''' 
	''' </summary>
	<Serializable()> _
	Friend Class RoutingNode
		Implements IQueueItem

		''' <summary>
		''' There are eight neighbors starting from 12:00 clockwise in 45 degree steps.
		''' If there is no such neighbor for a given direction, the entry is NULL.
		''' </summary>
		Public ReadOnly Neighbors(7) As RoutingNodeLink
		Public ReadOnly Heuristics()() As Single
		Public ReadOnly NodeX As Int32
		Public ReadOnly NodeY As Int32
		Public ReadOnly CellXY As Point
		Public ReadOnly NodeXY As Point
		Private privateF As Double
		Public Property F() As Double Implements IQueueItem.F
			Get
				Return privateF
			End Get
			Set(ByVal value As Double)
				privateF = value
			End Set
		End Property
		Public G As Double
		Public H As Double
		Private privateIndex As Integer
		Public Property Index() As Integer Implements IQueueItem.Index
			Get
				Return privateIndex
			End Get
			Set(ByVal value As Integer)
				privateIndex = value
			End Set
		End Property

		Public Sub New(ByVal inSizeInNodes As Integer, ByVal inX As Integer, ByVal inY As Integer, ByVal GranularityShift As Int32)
			Heuristics = New Single(inSizeInNodes - 1)(){}

			For i As Integer = 0 To inSizeInNodes - 1
				Heuristics(i) = New Single(inSizeInNodes - 1){}
			Next i

			NodeX = inX >> GranularityShift
			NodeY = inY >> GranularityShift
			NodeXY = New Point(NodeX, NodeY)
			CellXY = New Point(inX, inY)
		End Sub
	End Class
End Namespace
