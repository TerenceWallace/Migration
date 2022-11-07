Imports Migration.Configuration
Imports Migration.Core
Imports Migration.Interfaces

Namespace Migration.Buildings
	Public MustInherit Class Business
		Inherits BaseBuilding
		Implements ISuspendableBuilding

		Friend Sub New(ByVal inParent As BuildingManager, ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
			MyBase.New(inParent, inConfig, inPosition)
			If Not SpawnPoint.HasValue Then
				Throw New ArgumentException("Buildable """ & inConfig.Name & """ derives from ""Worker"" but has no spawn point!")
			End If
		End Sub

		Private ReadOnly Property ISuspendableBuilding_IsSuspended() As Boolean Implements ISuspendableBuilding.IsSuspended
			Get
				Return Me.IsSuspended1
			End Get
		End Property
		Public ReadOnly Property IsSuspended1() As Boolean
			Get
				Return MyBase.IsSuspended
			End Get
		End Property
	End Class

End Namespace
