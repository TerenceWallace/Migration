Imports Migration.Common


Namespace Migration.Jobs
	Friend Class JobRecruiting
		Inherits JobOnce

		Private privateProfession As MigrantProfessions
		Friend Property Profession() As MigrantProfessions
			Get
				Return privateProfession
			End Get
			Private Set(ByVal value As MigrantProfessions)
				privateProfession = value
			End Set
		End Property
		Private privateTool As GenericResourceStack
		Friend Property Tool() As GenericResourceStack
			Get
				Return privateTool
			End Get
			Private Set(ByVal value As GenericResourceStack)
				privateTool = value
			End Set
		End Property

		Friend Sub New(ByVal inMigrant As Movable, ByVal inToolProvider As GenericResourceStack, ByVal inProfession As MigrantProfessions)
			MyBase.New(inMigrant)
			If inToolProvider Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Profession = inProfession
			Tool = inToolProvider
			Character = "MigrantWalking"

			Tool.AddJob(Movable)
		End Sub

		Friend Overrides Function Prepare() As Boolean
			If Not(MyBase.Prepare()) Then
				Return False
			End If

			' pick up resource
			' transform movable to profession
			AddAnimationStep(Tool.Position.ToPoint(), Nothing, AddressOf AnonymousMethod1)

			Return True
		End Function

		Private Function AnonymousMethod1(ByVal successProv As Boolean) As Boolean
			Tool.RemoveJob(Movable)
			If Not successProv Then
				Movable.Stop()
				RaiseCompletion(False)
				Return False
			End If
			Tool.RemoveResource()
			TransformMovable(Movable, Profession)
			Movable.Job = Nothing
			RaiseCompletion(True)
			Return True
		End Function

		Friend Shared Sub TransformMovable(ByVal inMovable As Movable, ByVal inTargetProfession As MigrantProfessions)
			' transform movable to profession
			Select Case inTargetProfession
				Case MigrantProfessions.Constructor
					inMovable.MovableType = MovableType.Constructor
				Case MigrantProfessions.Grader
					inMovable.MovableType = MovableType.Grader

					'Case MigrantProfessions.Geologist
					'    inMovable.MovableType = MovableType.Migrant

				Case Else
					Throw New ApplicationException()
			End Select

			VisualUtilities.Animate(inMovable, inTargetProfession & "Walking", False, True)
		End Sub
	End Class
End Namespace
