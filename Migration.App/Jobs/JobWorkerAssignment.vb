Imports Migration.Buildings

Namespace Migration.Jobs
	Friend Class JobWorkerAssignment
		Inherits JobOnce

		Private privateBuilding As BaseBuilding
		Friend Property Building() As BaseBuilding
			Get
				Return privateBuilding
			End Get
			Private Set(ByVal value As BaseBuilding)
				privateBuilding = value
			End Set
		End Property

		Friend Sub New(ByVal inMigrant As Movable, ByVal inBuilding As BaseBuilding)
			MyBase.New(inMigrant)
			If inBuilding Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Building = inBuilding
			Character = Building.Config.Character & "Walking"
		End Sub

		Friend Overrides Function Prepare() As Boolean
			If Not(MyBase.Prepare()) Then
				Return False
			End If

			AddAnimationStep(Building.SpawnPoint.Value.ToPoint(), Nothing, AddressOf AnonymousMethod1)

			Return True
		End Function

		Private Function AnonymousMethod1(ByVal succeeded As Boolean) As Boolean
			RaiseCompletion(succeeded)
			Return True
		End Function
	End Class
End Namespace
