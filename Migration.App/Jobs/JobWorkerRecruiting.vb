Imports Migration.Buildings

Namespace Migration.Jobs
	Friend Class JobWorkerRecruiting
		Inherits JobCarrying

		Private privateBuilding As BaseBuilding
		Friend Property Building() As BaseBuilding
			Get
				Return privateBuilding
			End Get
			Private Set(ByVal value As BaseBuilding)
				privateBuilding = value
			End Set
		End Property

		Friend Sub New(ByVal inMigrant As Movable, ByVal inToolProvider As GenericResourceStack, ByVal inForBuilding As BaseBuilding)
			MyBase.New(inMigrant, inToolProvider, inForBuilding.SpawnPoint.Value.ToPoint())
			If inForBuilding Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Building = inForBuilding
			Character = "MigrantWalking"

			AddHandler OnPickedUp, AddressOf AnonymousMethod1
		End Sub
		Private Overloads Sub AnonymousMethod1(ByVal unused As Object)
			Character = Building.Config.Character & "Walking"
		End Sub
	End Class
End Namespace
