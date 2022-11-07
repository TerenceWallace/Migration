Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core
Imports Migration.Jobs

Namespace Migration.Buildings
	Public Class Waterworks
		Inherits Factory

		Friend Sub New(ByVal inParent As BuildingManager, ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
			MyBase.New(inParent, inConfig, inPosition)
			WorkingRadius = Int32.MaxValue
		End Sub

		Protected Overrides Function Plant(ByVal onCompleted As Procedure) As Boolean
			Return False
		End Function

		Protected Overrides Function Produce(ByVal onCompleted As Procedure(Of Boolean)) As Boolean
			Dim waterspot As New Point()

			If Not(Parent.Terrain.FindWater(SpawnPoint.Value.ToPoint(), 2, waterspot)) Then
				Return False
			End If

			' collect water
			Dim job As New JobCollectingWater(SpawnWorker(), waterspot, Me)

			AddHandler job.OnCompleted, Sub(unused, succeeded)
				onCompleted.Invoke(succeeded)
				WorkerOrNull.Job = job
			End Sub

			Return True
		End Function
	End Class
End Namespace
