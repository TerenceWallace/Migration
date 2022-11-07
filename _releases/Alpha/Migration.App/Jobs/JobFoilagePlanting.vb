Imports Migration.Buildings
Imports Migration.Common
Imports Migration.Core

Namespace Migration.Jobs
	Friend Class JobFoilagePlanting
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
		Private privateFoilagePosition As Point
		Friend Property FoilagePosition() As Point
			Get
				Return privateFoilagePosition
			End Get
			Private Set(ByVal value As Point)
				privateFoilagePosition = value
			End Set
		End Property
		Private privateFoilageType As FoilageType
		Friend Property FoilageType() As FoilageType
			Get
				Return privateFoilageType
			End Get
			Private Set(ByVal value As FoilageType)
				privateFoilageType = value
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

		Friend Sub New(ByVal inWorker As Movable, ByVal inFoilagePosition As Point, ByVal inBuilding As PlantProduce, ByVal inFoilageType As FoilageType)
			MyBase.New(inWorker)
			If (inWorker Is Nothing) OrElse (inBuilding Is Nothing) Then
				Throw New ArgumentNullException()
			End If

			Building = inBuilding
			FoilagePosition = inFoilagePosition
			Worker = inWorker
			FoilageType = inFoilageType
			Character = Building.Config.Character & "PlantWalking"
		End Sub

		Private buildingPrefix As String = String.Empty
		Friend Overrides Function Prepare() As Boolean
			If Not(MyBase.Prepare()) Then
				Return False
			End If

			Dim plantTreePos As New Point(FoilagePosition.X + 1, FoilagePosition.Y - 1)
			buildingPrefix = Building.Config.Character

			' walk to planting position
            AddAnimationStep(plantTreePos, AddressOf PlantWalking, Nothing)

            Dim timePlanting As Integer = VisualUtilities.GetDurationMillis(Worker, buildingPrefix & "PlantTask")

            ' run plating animation
            AddAnimationStepWithPathFollow(timePlanting, AddressOf PlantTask, AddressOf FoilageGrowing)

            ' walk back to building
            ' switch to carrying animation
            AddAnimationStep(Building.SpawnPoint.Value.ToPoint(), AddressOf RestartPlantWalking, AddressOf FoilagePlantingComplete)

            Return True
        End Function

        Private Function PlantWalking() As Boolean
            VisualUtilities.Animate(Worker, buildingPrefix & "PlantWalking", False, True)
            Return True
        End Function

        Private Function PlantTask() As Boolean
            VisualUtilities.Animate(Worker, buildingPrefix & "PlantTask", True, False)
            Return True
        End Function

        Private Function FoilageGrowing(ByVal success As Boolean) As Boolean
            Building.Parent.ResourceManager.AddFoilage(FoilagePosition, FoilageType, FoilageState.Growing)
            Return True
        End Function

        Private Function RestartPlantWalking() As Boolean
            VisualUtilities.Animate(Worker, buildingPrefix & "PlantWalking", True, True)
            Return True
        End Function

        Private Function FoilagePlantingComplete(ByVal succeeded As Boolean) As Boolean
            RaiseCompletion(succeeded)
            Return True
        End Function
	End Class
End Namespace
