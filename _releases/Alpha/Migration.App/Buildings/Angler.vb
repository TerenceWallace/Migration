Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core
Imports Migration.Jobs

Namespace Migration.Buildings
	Public Class Angler
		Inherits Factory

		Friend Sub New(ByVal inParent As BuildingManager, ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
			MyBase.New(inParent, inConfig, inPosition)
			WorkingRadius = Int32.MaxValue
		End Sub

		Protected Overrides Function Plant(ByVal onCompleted As Procedure) As Boolean
			Return False
		End Function

		Protected Overrides Function Produce(ByVal onCompleted As Procedure(Of Boolean)) As Boolean
			Dim waterSpot As New Point()
			Dim waterDir As Direction = 0

			If Not(Parent.Terrain.FindWater(SpawnPoint.Value.ToPoint(), 1, waterSpot, waterDir)) Then
				Return False
			End If

			' go fishing
			Dim job As New JobFishing(SpawnWorker(), waterSpot, waterDir, Me)

			AddHandler job.OnCompleted, Sub(unused As JobOnce, succeeded As Boolean) onCompleted.Invoke(succeeded)

			WorkerOrNull.Job = job

			Return True
		End Function

	End Class
End Namespace
