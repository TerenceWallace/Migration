Imports Migration.Interfaces

Namespace Migration
	Friend Class PathFinderNode
		Implements IQueueItem

		Private privateF As Double
		Public Property F() As Double Implements IQueueItem.F
			Get
				Return privateF
			End Get
			Set(ByVal value As Double)
				privateF = value
			End Set
		End Property
		Friend H As Double
		Friend G As Double
		Friend X As Integer
		Friend Y As Integer
		Private privateIndex As Integer
		Public Property Index() As Integer Implements IQueueItem.Index
			Get
				Return privateIndex
			End Get
			Set(ByVal value As Integer)
				privateIndex = value
			End Set
		End Property
	End Class
End Namespace