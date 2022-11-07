Imports Migration.Buildings

Namespace Migration.Jobs
	Friend Class JobStoneCutting
		Inherits JobOnce

		Private privateBuilding As StoneCutter
		Friend Property Building() As StoneCutter
			Get
				Return privateBuilding
			End Get
			Private Set(ByVal value As StoneCutter)
				privateBuilding = value
			End Set
		End Property
		Private privateStone As Stone
		Friend Property Stone() As Stone
			Get
				Return privateStone
			End Get
			Private Set(ByVal value As Stone)
				privateStone = value
			End Set
		End Property
		Private privateWorker As Movable
		Friend Property Worker() As Movable
			Get
				Return privateWorker
			End Get
			Private Set(ByVal value As Movable)
				privateWorker = value
			End Set
		End Property

		Friend Sub New(ByVal inWorker As Movable, ByVal inStone As Stone, ByVal inBuilding As StoneCutter)
			MyBase.New(inWorker)
			If (inWorker Is Nothing) OrElse (inStone Is Nothing) OrElse (inBuilding Is Nothing) Then
				Throw New ArgumentNullException()
			End If

			Building = inBuilding
			Stone = inStone
			Worker = inWorker
			Character = Building.Config.Character & "Walking"

			Stone.MarkAsBeingCut()
		End Sub

		Friend Overrides Function Prepare() As Boolean
			If Not(MyBase.Prepare()) Then
				Return False
			End If

			Dim cutStonePos As New Point(Stone.Position.XGrid + 1, Stone.Position.YGrid + 1)
			buildingPrefix = Building.Config.Character

			' walk to stone
			AddAnimationStep(cutStonePos, AddressOf AnonymousMethod, Nothing)

			Dim timeCutting As Integer = VisualUtilities.GetDurationMillis(Worker, buildingPrefix & "Working")

			' run cutting animation
			' mark stone for removal
			AddAnimationStepWithPathFollow(timeCutting, AddressOf AnonymousMethod1, AddressOf AnonymousMethod2)

			If VisualUtilities.HasAnimation(Worker, buildingPrefix & "Pickup") Then
				AddAnimationStepWithPathFollow(VisualUtilities.GetDurationMillis(Worker, buildingPrefix & "Pickup"), AddressOf AnonymousMethod4, Nothing)
			End If

			' walk back to building
			' switch to carrying animation
			AddAnimationStep(Building.SpawnPoint.Value.ToPoint(), AddressOf AnonymousMethod5, AddressOf AnonymousMethod6)

			If VisualUtilities.HasAnimation(Worker, buildingPrefix & "Drop") Then
				' animate dropping the cut foilage
				AddAnimationStepWithPathFollow(VisualUtilities.GetDurationMillis(Worker, buildingPrefix & "Drop"), AddressOf AnonymousMethod7, AddressOf AnonymousMethod7)
			End If

			Return True
		End Function

		Private buildingPrefix As String = String.Empty
		Private Function AnonymousMethod() As Boolean
			VisualUtilities.Animate(Worker, buildingPrefix & "Walking", False, True)
			Return True
		End Function

		Private Function AnonymousMethod1() As Boolean
			VisualUtilities.Animate(Worker, buildingPrefix & "Working", True, True)
			Return True
		End Function

		Private Function AnonymousMethod2(ByVal succeeded As Boolean) As Boolean
			Stone.Cut()
			If Stone.ActualStones <= 0 Then
                Manager.QueueWorkItem(30000, AddressOf AnonymousMethod3)
			End If
			Return True
		End Function

		Private Sub AnonymousMethod3()
			Building.Parent.ResourceManager.RemoveStone(Stone)
		End Sub

		Private Function AnonymousMethod4() As Boolean
			VisualUtilities.Animate(Worker, buildingPrefix & "Pickup", True, False)
			Return True
		End Function

		Private Function AnonymousMethod5() As Boolean
			VisualUtilities.Animate(Worker, buildingPrefix & "Carrying", True, True)
			Return True
		End Function

		Private Function AnonymousMethod6(ByVal succeeded As Boolean) As Boolean
			If Not(VisualUtilities.HasAnimation(Worker, buildingPrefix & "Drop")) Then
				RaiseCompletion(succeeded)
			End If
			Return True
		End Function

		Private Function AnonymousMethod7() As Boolean
			VisualUtilities.Animate(Worker, buildingPrefix & "Drop", True, False)
			Return True
		End Function

		Private Function AnonymousMethod7(ByVal succeeded As Boolean) As Boolean
			RaiseCompletion(succeeded)
			Return True
		End Function
	End Class
End Namespace
