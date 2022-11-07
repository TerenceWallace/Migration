Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core
Imports Migration.Jobs

Namespace Migration.Buildings
	Public Class StoneCutter
		Inherits Factory

		Friend Sub New(ByVal inParent As BuildingManager, ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
			MyBase.New(inParent, inConfig, inPosition)
			WorkingRadius = Int32.MaxValue
		End Sub

		Protected Overrides Function Plant(ByVal onCompleted As Procedure) As Boolean
			Return False
		End Function

		Protected Overrides Function Produce(ByVal onCompleted As Procedure(Of Boolean)) As Boolean
			' search for grown foialge of the given type
			Dim stone As Stone = Parent.ResourceManager.FindStoneAround(SpawnPoint.Value.ToPoint(), WorkingRadius)

			If stone Is Nothing Then
				Return False
			End If

			' cut foilage
			Dim job As New JobStoneCutting(SpawnWorker(), stone, Me)

			AddHandler job.OnCompleted, Sub(unused, succeeded)
				onCompleted.Invoke(succeeded)
				WorkerOrNull.Job = job
			End Sub

			Return True
		End Function
	End Class
End Namespace
