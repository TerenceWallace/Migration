Imports Migration.Common
Imports Migration.Core

Namespace Migration

	''' <summary>
	''' Intended to provide some default behavior for all essentially non-movable objects,
	''' which still are renderable, like resources or buildings.
	''' </summary>
	Friend Class MovablePathNode
		Friend Shared Comparer As New KeyComparer()

		Friend Class KeyComparer
			Implements IComparer(Of MovablePathNode)

			Public Function Compare(ByVal a As MovablePathNode, ByVal b As MovablePathNode) As Integer Implements IComparer(Of MovablePathNode).Compare
				If a.Position.X < b.Position.X Then
					Return -1
				ElseIf a.Position.X > b.Position.X Then
					Return 1
				End If

				If a.Position.Y < b.Position.Y Then
					Return -1
				ElseIf a.Position.Y > b.Position.Y Then
					Return 1
				End If

				' must be the last comparison
				If a.Time < b.Time Then
					Return -1
				ElseIf a.Time > b.Time Then
					Return 1
				End If

				Return 0
			End Function
		End Class

		''' <summary>
		''' The corresponding key for the internal space-time map.
		''' </summary>
		Private privatePosition As Point
		Friend Property Position() As Point
			Get
				Return privatePosition
			End Get
			Set(ByVal value As Point)
				privatePosition = value
			End Set
		End Property
		Private privateTime As Long
		Friend Property Time() As Long
			Get
				Return privateTime
			End Get
			Set(ByVal value As Long)
				privateTime = value
			End Set
		End Property
		''' <summary>
		''' A direction of animation, if any.
		''' </summary>
		Private privateDirection? As Direction
		Friend Property Direction() As Direction?
			Get
				Return privateDirection
			End Get
			Set(ByVal value? As Direction)
				privateDirection = value
			End Set
		End Property
		''' <summary>
		''' A path node is idle, if it has no direction.
		''' </summary>
		Friend ReadOnly Property IsIdle() As Boolean
			Get
				Return Not Direction.HasValue
			End Get
		End Property
		''' <summary>
		''' The movable owning this path node.
		''' </summary>
		Private privateMovable As Movable
		Friend Property Movable() As Movable
			Get
				Return privateMovable
			End Get
			Set(ByVal value As Movable)
				privateMovable = value
			End Set
		End Property

		Public Overrides Function ToString() As String
			Return Position.ToString() & "; Time: " & Time & "; Dir: " & Direction
		End Function
	End Class

End Namespace
