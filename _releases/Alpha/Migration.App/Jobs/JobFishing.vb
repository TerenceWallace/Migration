Imports Migration.Buildings
Imports Migration.Common
Imports Migration.Core

Namespace Migration.Jobs
	Friend Class JobFishing
		Inherits JobOnce

		Private privateBuilding As Angler
		Friend Property Building() As Angler
			Get
				Return privateBuilding
			End Get
			Private Set(ByVal value As Angler)
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
		Private privateWaterDir As Direction
		Friend Property WaterDir() As Direction
			Get
				Return privateWaterDir
			End Get
			Private Set(ByVal value As Direction)
				privateWaterDir = value
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

		Friend Sub New(ByVal inWorker As Movable, ByVal inWaterSpot As Point, ByVal inWaterDir As Direction, ByVal inBuilding As Angler)
			MyBase.New(inWorker)
			If (inWorker Is Nothing) OrElse (inBuilding Is Nothing) Then
				Throw New ArgumentNullException()
			End If

			Building = inBuilding
			WaterSpot = inWaterSpot
			WaterDir = inWaterDir
			Worker = inWorker
			Character = Building.Config.Character & "Walking"
		End Sub

		Friend Overrides Function Prepare() As Boolean
			If Not(MyBase.Prepare()) Then
				Return False
			End If

			buildingPrefix = Building.Config.Character
			Dim succeeding As Boolean = m_Random.NextDouble() < 0.7
			workAnim = If(succeeding, "Succeeding", "Failing")
			carryAnim = If(succeeding, "Carrying", "Walking")

			' walk to stone
			AddAnimationStep(WaterSpot, AddressOf AnonymousMethod, Nothing)

			' run cutting animation
			AddAnimationStepWithPathFollow(VisualUtilities.GetDurationMillis(Worker, buildingPrefix & workAnim), AddressOf AnonymousMethod1, Nothing)

			' walk back to building
			' switch to carrying animation
			AddAnimationStep(Building.SpawnPoint.Value.ToPoint(), AddressOf AnonymousMethod2, AddressOf AnonymousMethod2)

			Return True
		End Function

		Private buildingPrefix As String = String.Empty
		Private workAnim As String = String.Empty
		Private carryAnim As String = String.Empty

		Private Function AnonymousMethod() As Boolean
			VisualUtilities.Animate(Worker, buildingPrefix & "Walking", False, True)
			Return True
		End Function

		Private Function AnonymousMethod1() As Boolean
			VisualUtilities.DirectedAnimation(Worker, buildingPrefix & workAnim, WaterDir, True, False)
			Return True
		End Function

		Private Function AnonymousMethod2() As Boolean
			VisualUtilities.Animate(Worker, buildingPrefix & carryAnim, True, True)
			Return True
		End Function

		Private Function AnonymousMethod2(ByVal succeeded As Boolean) As Boolean
			RaiseCompletion(succeeded)
			Return True
		End Function
	End Class
End Namespace
