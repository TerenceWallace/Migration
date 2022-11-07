Imports Migration.Buildings

Namespace Migration.Jobs
	Friend Class JobFoilageCutting
		Inherits JobOnce

		Private privateBuilding As PlantProduce
		Friend Property Building() As PlantProduce
			Get
				Return privateBuilding
			End Get
			Private Set(ByVal value As PlantProduce)
				privateBuilding = value
			End Set
		End Property
		Private privateFoilage As Foilage
		Friend Property Foilage() As Foilage
			Get
				Return privateFoilage
			End Get
			Private Set(ByVal value As Foilage)
				privateFoilage = value
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

		Friend Sub New(ByVal inWorker As Movable, ByVal inFoilage As Foilage, ByVal inBuilding As PlantProduce)
			MyBase.New(inWorker)
			If (inWorker Is Nothing) OrElse (inFoilage Is Nothing) OrElse (inBuilding Is Nothing) Then
				Throw New ArgumentNullException()
			End If

			Building = inBuilding
			Foilage = inFoilage
			Worker = inWorker
			Character = Building.Config.Character & "ProduceWalking"

			Foilage.MarkAsBeingCut()
		End Sub

		Private buildingPrefix As String = String.Empty
		Friend Overrides Function Prepare() As Boolean
			If Not(MyBase.Prepare()) Then
				Return False
			End If

			Dim cutTreePos As New Point(Foilage.Position.XGrid + 1, Foilage.Position.YGrid + 1)
			buildingPrefix = Building.Config.Character

			' walk to foilage
			AddAnimationStep(cutTreePos, AddressOf AnonymousMethod1, Nothing)

			Dim timeCutting As Integer = VisualUtilities.GetDurationMillis(Worker, buildingPrefix & "ProduceTask")
			Dim timeFalling As Integer = 0

			If VisualUtilities.HasAnimation(Foilage, "Falling") Then
				' run cutting animation
				AddAnimationStepWithPathFollow(timeCutting, AddressOf AnonymousMethod2, Nothing)


				timeFalling = VisualUtilities.GetDurationMillis(Foilage, "Falling")

				' continue cutting animation but also animate falling foilage
				AddAnimationStepWithPathFollow(timeFalling, AddressOf AnonymousMethod3, Nothing)
			End If

			If VisualUtilities.HasAnimation(Foilage, "Fractioning") Then
				' continue cutting animation but also animate the foilage being cut
				' register foilage for removal
				AddAnimationStepWithPathFollow(timeCutting - timeFalling, AddressOf AnonymousMethod4, Nothing)

			End If

			If VisualUtilities.HasAnimation(Worker, buildingPrefix & "Pickup") Then
				' continue animating the tree... additionally we now pickup the cut foilage
				AddAnimationStepWithPathFollow(VisualUtilities.GetDurationMillis(Worker, buildingPrefix & "Pickup"), AddressOf AnonymousMethod6, Nothing)

			End If

			' walk back to building
			' switch to carrying animation
			AddAnimationStep(Building.SpawnPoint.Value.ToPoint(), AddressOf AnonymousMethod7, AddressOf AnonymousMethod7)

			If VisualUtilities.HasAnimation(Worker, buildingPrefix & "Drop") Then
				' animate dropping the cut foilage
				AddAnimationStepWithPathFollow(VisualUtilities.GetDurationMillis(Worker, buildingPrefix & "Drop"), AddressOf AnonymousMethod8, AddressOf AnonymousMethod8)
			End If

			Return True
		End Function

		Private Function AnonymousMethod1() As Boolean
			VisualUtilities.Animate(Worker, buildingPrefix & "ProduceWalking", False, True)
			Return True
		End Function

		Private Function AnonymousMethod2() As Boolean
			VisualUtilities.Animate(Worker, buildingPrefix & "ProduceTask", True, True)
			Return True
		End Function

		Private Function AnonymousMethod3() As Boolean
			VisualUtilities.Animate(Foilage, "Falling", True, False)
			Return True
		End Function

		Private Function AnonymousMethod4() As Boolean
			VisualUtilities.Animate(Foilage, "Fractioning", True, False)
			VisualUtilities.Animate(Worker, buildingPrefix & "ProduceTask", False, True)
			Building.Parent.QueueWorkItem(VisualUtilities.GetDurationMillis(Foilage, "Fractioning"), AddressOf AnonymousMethod5)
			Return True
		End Function

		Private Sub AnonymousMethod5()
			Building.Parent.ResourceManager.RemoveFoilage(Foilage)
		End Sub

		Private Function AnonymousMethod6() As Boolean
			VisualUtilities.Animate(Worker, buildingPrefix & "Pickup", True, False)
			Return True
		End Function

		Private Function AnonymousMethod7() As Boolean
			VisualUtilities.Animate(Worker, buildingPrefix & "Carrying", True, True)
			If Not(VisualUtilities.HasAnimation(Foilage, "Fractioning")) Then
				Building.Parent.ResourceManager.RemoveFoilage(Foilage)
			End If
			Return True
		End Function

		Private Function AnonymousMethod7(ByVal succeeded As Boolean) As Boolean
			If Not(VisualUtilities.HasAnimation(Worker, buildingPrefix & "Drop")) Then
				RaiseCompletion(succeeded)
			End If
			Return True
		End Function

		Private Function AnonymousMethod8() As Boolean
			VisualUtilities.Animate(Worker, buildingPrefix & "Drop", True, False)
			Return True
		End Function

		Private Function AnonymousMethod8(ByVal succeeded As Boolean) As Boolean
			RaiseCompletion(succeeded)
			Return True
		End Function
	End Class
End Namespace
