Imports Migration.Common
Imports Migration.Core

Namespace Migration.Jobs
	Friend Class JobConstructing
		Inherits JobOrder

		Private m_WorkingDirection As Direction

		Private privateTask As BuildTask
		Friend Property Task() As BuildTask
			Get
				Return privateTask
			End Get
			Private Set(ByVal value As BuildTask)
				privateTask = value
			End Set
		End Property
		Private privatePosition As Point
		Friend Property Position() As Point
			Get
				Return privatePosition
			End Get
			Private Set(ByVal value As Point)
				privatePosition = value
			End Set
		End Property

		Friend Sub New(ByVal inMovable As Movable, ByVal inTask As BuildTask, ByVal inPosition As Point, ByVal inDirection As Direction)
			MyBase.New(inMovable)
			If inMovable.MovableType <> MovableType.Constructor Then
				Throw New ArgumentOutOfRangeException()
			End If

			If inTask Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Task = inTask
			m_WorkingDirection = inDirection
			Position = inPosition
		End Sub

		Friend Overrides Function Prepare() As Boolean

			' job will repeat until building is built
			If Task.IsBuilt Then
				' free constructor from his occupation
				Dispose()

				Return False
			End If

			' walk to next, randomly choosen grading spot
			Direction = Nothing

			If Movable.Position.ToPoint() <> Position Then
				AddAnimationStep(Position, Nothing, Nothing)
			End If

			' run grading animation
            AddAnimationStepWithPathFollow(1000, AddressOf ConstructorWorking, AddressOf ConstructorWalking)
            Return True
        End Function

        Private Function ConstructorWorking() As Boolean
            Direction = m_WorkingDirection
            Character = "ConstructorWorking"
            Return True
        End Function

        Private Function ConstructorWalking(ByVal succeeded As Boolean) As Boolean
            Direction = Nothing
            If Not (Task.DoConstruct()) Then
                Character = "ConstructorWalking"
            End If
            Return True
        End Function

	End Class
End Namespace
