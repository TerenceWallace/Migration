Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core
Imports Migration.Interfaces
Imports Migration.Jobs

Namespace Migration.Buildings
	Public Class PlantProduce
		Inherits Factory
		Implements IBuildingWithWorkingArea

		Private m_PlantingTypes() As FoilageType

		Private privateFoilageCuttingMask As FoilageType
		Friend Property FoilageCuttingMask() As FoilageType
			Get
				Return privateFoilageCuttingMask
			End Get
			Private Set(ByVal value As FoilageType)
				privateFoilageCuttingMask = value
			End Set
		End Property
		Private privateFoilagePlantingMask As FoilageType
		Friend Property FoilagePlantingMask() As FoilageType
			Get
				Return privateFoilagePlantingMask
			End Get
			Private Set(ByVal value As FoilageType)
				privateFoilagePlantingMask = value
			End Set
		End Property

		Private Shared Function GetResourceParameter(ByVal inParameter As String) As Resource
			Return CType(System.Enum.Parse(GetType(Resource), inParameter.Split(":"c)(0)), Resource)
		End Function

		Private Shared Function GetRadiusParameter(ByVal inParameter As String) As Integer
			Return Int32.Parse(inParameter.Split(":"c)(1))
		End Function

		Friend Sub New(ByVal inParent As BuildingManager, ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point, ByVal inParameter As String)
			Me.New(inParent, inConfig, inPosition, GetResourceParameter(inParameter), GetRadiusParameter(inParameter))
		End Sub

		Friend Sub New(ByVal inParent As BuildingManager, ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point, ByVal inResource As Resource, ByVal inSearchRadius As Integer)
			MyBase.New(inParent, inConfig, inPosition)
			If (inSearchRadius < 2) OrElse (inSearchRadius > 50) Then
				Throw New ArgumentOutOfRangeException()
			End If

			WorkingRadius = inSearchRadius
			WorkingArea = SpawnPoint.Value.ToPoint()

			Select Case inResource
				Case Resource.Wood
					FoilageCuttingMask = FoilageType.Tree1
					FoilagePlantingMask = FoilageType.Tree1

				Case Resource.Wine
					FoilageCuttingMask = FoilageType.Wine
					FoilagePlantingMask = FoilageType.Wine

				Case Resource.Grain
					FoilageCuttingMask = FoilageType.Grain
					FoilagePlantingMask = FoilageType.Grain

				Case Else
					Throw New ArgumentOutOfRangeException()
			End Select

			Dim plantingTypes As New List(Of FoilageType)()

			For Each value As FoilageType In System.Enum.GetValues(GetType(FoilageType))
				Dim bit As FoilageType = value

				If bit.BitCount() <> 1 Then
					Continue For
				End If

				If FoilagePlantingMask.IsSet(bit) Then
					plantingTypes.Add(bit)
				End If
			Next value

			m_PlantingTypes = plantingTypes.ToArray()
		End Sub

		Protected Overrides Function Produce(ByVal onCompleted As Procedure(Of Boolean)) As Boolean
			' search for grown foialge of the given type
			Dim foilage As Foilage = Parent.ResourceManager.FindFoilageAround(WorkingArea, WorkingRadius, FoilageCuttingMask, FoilageState.Grown)

			If foilage Is Nothing Then
				Return False
			End If

			' cut foilage
			Dim job As New JobFoilageCutting(SpawnWorker(), foilage, Me)

			AddHandler job.OnCompleted, Sub(unused, succeeded)
				onCompleted(succeeded)
				WorkerOrNull.Job = job
			End Sub

			Return True
		End Function

		Protected Overrides Function Plant(ByVal onCompleted As Procedure) As Boolean
			'            
			'             * In theory we could use a complex algorithm to determine a near optimal balance between
			'             * planting and producing but since we need a new foilage for every cut foilage, the following
			'             * trivial random assumption proves to be sufficient and looks nearly optimal for most scenarios...
			'             
			If m_Random.NextDouble() < 0.5 Then
				Return False
			End If

			' search for free space
			Dim plantingType As FoilageType = m_PlantingTypes(m_Random.Next(0, m_PlantingTypes.Length))
			Dim foilagePos As New Point()

			If WalkResult.Success <> GridSearch.GridWalkAround(WorkingArea, Parent.Terrain.Size, Parent.Terrain.Size, Function(pos As Point)
					If (Math.Abs(pos.X - WorkingArea.X) > WorkingRadius) OrElse (Math.Abs(pos.Y - WorkingArea.Y) > WorkingRadius) Then
						Return WalkResult.Abort
					End If
					If Not(Parent.Terrain.CanFoilageBePlacedAt(pos.X, pos.Y, plantingType)) Then
						Return WalkResult.NotFound
					End If
					foilagePos = pos
					Return WalkResult.Success
				End Function
				) Then
				Return False
			End If

			' plant new foilage
			Dim job As New JobFoilagePlanting(SpawnWorker(), foilagePos, Me, plantingType)

			AddHandler job.OnCompleted, Sub(unused, succeeded)
				onCompleted()
				WorkerOrNull.Job = job
			End Sub

			Return True
		End Function

		Private Property IBuildingWithWorkingArea_WorkingRadius() As Integer Implements IBuildingWithWorkingArea.WorkingRadius
			Get
				Return Me.WorkingRadius1
			End Get
			Set(ByVal value As Integer)
				Me.WorkingRadius1 = value
			End Set
		End Property
		Public Property WorkingRadius1() As Integer
			Get
				Return MyBase.WorkingRadius
			End Get
			Set(ByVal value As Integer)
				MyBase.WorkingRadius = value
			End Set
		End Property
	End Class
End Namespace
