Imports Migration.Interfaces

Namespace Migration
	Friend Class PathFinderNode
        Implements IQueueItem

        Friend H As Double
        Friend G As Double
        Friend X As Integer
        Friend Y As Integer

        Public Property F() As Double Implements IQueueItem.F

		Public Property Index() As Integer Implements IQueueItem.Index

	End Class
End Namespace