Imports Migration.Common

Namespace Migration.Jobs
	''' <summary>
	''' The job is able to override the movable's direction. This is necessary for construction workers
	''' and fighting soldiers for example.
	''' </summary>
	Public MustInherit Class JobOrder
		Inherits JobBase

		Private privateDirection? As Direction
		Public Property Direction() As Direction?
			Get
				Return privateDirection
			End Get
			Friend Set(ByVal value? As Direction)
				privateDirection = value
			End Set
		End Property

		Friend Sub New(ByVal inMovable As Movable)
			MyBase.New(inMovable)
		End Sub

		Friend MustOverride Overrides Function Prepare() As Boolean
	End Class
End Namespace
