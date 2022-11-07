Imports Migration.Buildings
Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core

Namespace Migration

	Friend Class BuildingManager
		Inherits SynchronizedManager

		Private m_Buildings As New LinkedList(Of BaseBuilding)()
		Private m_BuildTasks As New LinkedList(Of BuildTask)()

		Friend Event OnGradingStep As DOnGradingStep(Of BuildingManager)
		Friend Event OnAddBuilding As DOnAddBuilding(Of BuildingManager)
		Friend Event OnAddBuildTask As DOnAddBuildTask(Of BuildingManager)
		Friend Event OnRemoveBuilding As DOnRemoveBuilding(Of BuildingManager)
		Friend Event OnRemoveBuildTask As DOnRemoveBuildTask(Of BuildingManager)

		Private privateMoveManager As MovableManager
		Friend Property MoveManager() As MovableManager
			Get
				Return privateMoveManager
			End Get
			Private Set(ByVal value As MovableManager)
				privateMoveManager = value
			End Set
		End Property

		Friend ReadOnly Property Terrain() As TerrainDefinition
			Get
				Return MoveManager.Terrain
			End Get
		End Property

		Friend ReadOnly Property Map() As Migration.Game.Map
			Get
				Return ResourceManager.Map
			End Get
		End Property

		Private privateResourceManager As ResourceManager
		Friend Property ResourceManager() As ResourceManager
			Get
				Return privateResourceManager
			End Get
			Private Set(ByVal value As ResourceManager)
				privateResourceManager = value
			End Set
		End Property

		Private privateInitialHouseSpaceCount As Integer
		Friend Property InitialHouseSpaceCount() As Integer
			Get
				Return privateInitialHouseSpaceCount
			End Get
			Private Set(ByVal value As Integer)
				privateInitialHouseSpaceCount = value
			End Set
		End Property

		Friend Sub RaiseGradingStep(ByVal inGrader As Movable, ByVal onCompletion As Procedure)
			RaiseEvent OnGradingStep(Me, inGrader, onCompletion)
			onCompletion()
		End Sub

		Friend Sub New(ByVal inMovMgr As MovableManager, ByVal inResMgr As ResourceManager, ByVal inInitialHouseSpaceCount As Integer)
			MyBase.New(inMovMgr)
			If (inMovMgr Is Nothing) OrElse (inResMgr Is Nothing) Then
				Throw New ArgumentNullException()
			End If

			If (inInitialHouseSpaceCount < 0) OrElse (inInitialHouseSpaceCount > 1000000) Then
				Throw New ArgumentOutOfRangeException()
			End If

			InitialHouseSpaceCount = inInitialHouseSpaceCount
			MoveManager = inMovMgr
			ResourceManager = inResMgr
		End Sub

		Friend Function ComputeHouseSpaceCount() As Integer
			Dim result As Integer = InitialHouseSpaceCount

			For Each building As BaseBuilding In m_Buildings
				Dim house As Home = TryCast(building, Home)

				If house IsNot Nothing Then
					result += house.Config.MigrantCount
				End If
			Next building

			Return result
		End Function

		Friend Sub BeginBuilding(ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
			_inConfig = inConfig
			_inPosition = inPosition

			' scan for a proper building spot closely around the specified position
			Dim checkResult As WalkResult = GridSearch.GridWalkAround(_inPosition, Terrain.Size, Terrain.Size, AddressOf BuildingSpotSearch)

			If checkResult <> WalkResult.Success Then
				Return ' don't raise an exception, since user will notice that building isn't build.
			End If

			Terrain.SetWallAt(_inPosition, WallValue.Reserved, _inConfig.GroundPlane.ToArray())
			Terrain.InitializeWallAt(_inPosition, WallValue.Reserved, _inConfig.ReservedPlane.ToArray())

			' create instance of building class
			Dim task As New BuildTask(Me, _inConfig.CreateInstance(Me, _inPosition))

			task.Node = m_BuildTasks.AddLast(task)

			RaiseEvent OnAddBuildTask(Me, task)
		End Sub

		Private _inConfig As BuildingConfiguration
		Private _inPosition As Point
		Private Function BuildingSpotSearch(ByVal pos As Point) As WalkResult
			If Math.Abs(pos.X - _inPosition.X) > 5 Then
				Return WalkResult.Abort
			End If
			If Not(Terrain.CanBuildingBePlacedAt(pos.X, pos.Y, _inConfig)) Then
				Return WalkResult.NotFound
			End If
			_inPosition = pos
			Return WalkResult.Success
		End Function

		Friend Sub EndBuilding(ByVal inTask As BuildTask)
			Dim building As BaseBuilding = inTask.Building
			Dim inKeepPlane As Boolean = True

			RemoveBuildTask(inTask, inKeepPlane)

			building.Node = m_Buildings.AddLast(building)
			building.CreateResourceStacks()

			Terrain.SetWallAt(building.Position.ToPoint(), WallValue.Building, building.Config.GroundPlane.ToArray())

			RaiseEvent OnAddBuilding(Me, building)
		End Sub

		Private Sub RemoveGroundPlane(ByVal inBuilding As BaseBuilding)
			Terrain.SetWallAt(inBuilding.Position.ToPoint(), WallValue.Free, inBuilding.Config.GroundPlane.ToArray())
			Terrain.SetWallAt(inBuilding.Position.ToPoint(), WallValue.Free, inBuilding.Config.ReservedPlane.ToArray())
		End Sub

		Friend Sub RemoveBuildTask(ByVal inTask As BuildTask)
			Dim inKeepPlane As Boolean = False

			RemoveBuildTask(inTask, inKeepPlane)

			' show building crash animation
			VisualUtilities.AnimateAroundCenter(inTask.Building.Center, "Building", "Destroy")

			' build was aborted, drop resources...
			ResourceManager.DropResource(inTask.Building.Position.ToPoint(), Resource.Timber, inTask.UsedTimberCount \ 2)
			ResourceManager.DropResource(inTask.Building.Position.ToPoint(), Resource.Stone, inTask.UsedStoneCount \ 2)
		End Sub

		Private Sub RemoveBuildTask(ByVal inTask As BuildTask, ByVal inKeepPlane As Boolean)
			m_BuildTasks.Remove(inTask.Node)
			inTask.Node = Nothing

			inTask.Dispose()

			If Not inKeepPlane Then
				RemoveGroundPlane(inTask.Building)
			End If

			RaiseEvent OnRemoveBuildTask(Me, inTask)
		End Sub

		Friend Sub RemoveBuilding(ByVal inBuilding As BaseBuilding)
			If inBuilding.Node Is Nothing Then
				Return
			End If

			m_Buildings.Remove(inBuilding)
			RemoveGroundPlane(inBuilding)

			' show building crash animation
			VisualUtilities.AnimateAroundCenter(inBuilding.Center, "Building", "Destroy")

			' drop resources...
			ResourceManager.DropResource(inBuilding.Position.ToPoint(), Resource.Timber, inBuilding.Config.WoodCount \ 2)
			ResourceManager.DropResource(inBuilding.Position.ToPoint(), Resource.Stone, inBuilding.Config.StoneCount \ 2)

			RaiseEvent OnRemoveBuilding(Me, inBuilding)
		End Sub

		Friend Sub RaiseTaskPriority(ByVal inTask As BuildTask)
			m_BuildTasks.Remove(inTask.Node)
			inTask.Node = m_BuildTasks.AddFirst(inTask)

			For Each query As GenericResourceStack In inTask.Queries
				ResourceManager.RaisePriority(query)
			Next query
		End Sub

		Friend Sub ProcessQueries(ByVal inHandler As Procedure(Of GenericResourceStack))
			Dim taskList As LinkedListNode(Of BuildTask) = m_BuildTasks.First

			Do While taskList IsNot Nothing
				Dim task As BuildTask = taskList.Value

				For Each query As GenericResourceStack In task.Queries
					inHandler(query)
				Next query

				taskList = taskList.Next
			Loop
		End Sub

		Friend Overrides Sub ProcessCycle()
			MyBase.ProcessCycle()

			Dim isAligned As Boolean = IsAlignedCycle

			For Each building As BaseBuilding In m_Buildings
				If (building.Config.ClassType IsNot GetType(Workshop)) AndAlso ((Not isAligned)) Then
					Continue For
				End If

				building.Update()
			Next building

			If isAligned Then
				Dim taskList As LinkedListNode(Of BuildTask) = m_BuildTasks.First

				Do While taskList IsNot Nothing
					Dim task As BuildTask = taskList.Value
					Dim taskNode As LinkedListNode(Of BuildTask) = taskList
					taskList = taskList.Next

					task.Update()

					If task.IsCompleted Then
						EndBuilding(taskNode.Value)
					End If
				Loop

				'                 After processing, we can now release marked constructors and graders.
				'                 * If we would have done so during task update, these movables would be
				'                 * scheduled on the convenience of the tasks following in the queue (LIFO
				'                 * on average). But this is not the desired behaviour, instead we want to 
				'                 * schedule them in FIFO order...
				'                 
				taskList = m_BuildTasks.First

				Do While taskList IsNot Nothing
					Dim task As BuildTask = taskList.Value

					If task.AreConstructorsMarkedForRelease Then
						task.ReleaseConstructors()
					End If

					If task.AreGradersMarkedForRelease Then
						task.ReleaseGraders()
					End If

					taskList = taskList.Next
				Loop
			End If
		End Sub
	End Class
End Namespace
