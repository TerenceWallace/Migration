Imports Migration.Configuration
Imports Migration.Core

Namespace Migration.Buildings
	Public Class Mine
		Inherits Workshop

		Friend Sub New(ByVal inParent As BuildingManager, ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
			MyBase.New(inParent, inConfig, inPosition)
		End Sub

		Friend Overrides Function Update() As Boolean
			Return MyBase.Update()
		End Function
	End Class
End Namespace
