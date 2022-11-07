Imports Migration.Buildings
Imports Migration.Core

Namespace Migration.Jobs
	Friend Class JobCollectingWater
		Inherits JobOnce

		Private privateBuilding As Waterworks
		Friend Property Building() As Waterworks
			Get
				Return privateBuilding
			End Get
			Private Set(ByVal value As Waterworks)
				privateBuilding = value
			End Set
		End Property
		Private privateWaterSpot As Point
		Friend Property WaterSpot() As Point
			Get
				Return privateWaterSpot
			End Get
			Private Set(ByVal value As Point)
				privateWaterSpot = value
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

		Friend Sub New(ByVal inWorker As Movable, ByVal inWaterSpot As Point, ByVal inBuilding As Waterworks)
			MyBase.New(inWorker)
			If (inWorker Is Nothing) OrElse (inBuilding Is Nothing) Then
				Throw New ArgumentNullException()
			End If

			Building = inBuilding
			WaterSpot = inWaterSpot
			Worker = inWorker
			Character = Building.Config.Character & "Walking"
		End Sub

		Friend Overrides Function Prepare() As Boolean
			If Not(MyBase.Prepare()) Then
				Return False
			End If

			Dim buildingPrefix As String = Building.Config.Character

			AddAnimationStep(WaterSpot, Function()
				' walk to collecting position
				VisualUtilities.Animate(Worker, buildingPrefix & "Walking", inRestart:= False, inRepeat:= True)
				Return True
			End Function, Nothing)

			AddAnimationStepWithPathFollow(VisualUtilities.GetDurationMillis(Worker, buildingPrefix & "Working"), Function()
				' run plating animation
				VisualUtilities.Animate(Worker, buildingPrefix & "Working", inRestart:= True, inRepeat:= False)
				Return True
			End Function, Nothing)

			' walk back to building
			AddAnimationStep(Building.SpawnPoint.Value.ToPoint(), Function()
				' switch to carrying animation
				VisualUtilities.Animate(Worker, buildingPrefix & "Carrying", inRestart:= True, inRepeat:= True)
				Return True
			End Function, Function(succeeded)
				RaiseCompletion(succeeded)
				Return True
End Function)

			Return True
		End Function

	End Class
End Namespace
