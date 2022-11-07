Imports Migration.Common


Namespace Migration.Jobs
	Friend Class JobGrading
		Inherits JobBase

		Private privateTask As BuildTask
		Friend Property Task() As BuildTask
			Get
				Return privateTask
			End Get
			Private Set(ByVal value As BuildTask)
				privateTask = value
			End Set
		End Property

		Friend Sub New(ByVal inMovable As Movable, ByVal inTask As BuildTask)
			MyBase.New(inMovable)
			If inMovable.MovableType <> MovableType.Grader Then
				Throw New ArgumentOutOfRangeException()
			End If

			If inTask Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Task = inTask
		End Sub

		Friend Overrides Function Prepare() As Boolean
			' job will repeat until building is graded
			If Task.IsGraded Then
				' free grader from his occupation
				Dispose()

				Return False
			End If

            Dim target As New Point(Task.Building.Position.XGrid + m_Random.Next(0, Task.Configuration.GridWidth), Task.Building.Position.YGrid + m_Random.Next(0, Task.Configuration.GridHeight))

			' walk to next, randomly choosen grading spot
			Character = "GraderWalking"

			AddAnimationStep(target, Nothing, Nothing)

			' run grading animation
			AddAnimationStepWithPathFollow(1000, Function()
				Character = "GraderWorking"
				Return True
			End Function, Function(succeeded As Boolean)
				Character = "GraderWalking"
				Me.Task.DoGrade(target)
				Return True
End Function)


			Return True
		End Function
	End Class
End Namespace
