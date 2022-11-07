Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core

Namespace Migration.Buildings

	Public MustInherit Class BaseBuilding
		Inherits PositionTracker

		Protected Shared ReadOnly m_Random As New CrossRandom(0)
		Private privateNode As LinkedListNode(Of BaseBuilding)
		Friend Property Node() As LinkedListNode(Of BaseBuilding)
			Get
				Return privateNode
			End Get
			Set(ByVal value As LinkedListNode(Of BaseBuilding))
				privateNode = value
			End Set
		End Property
		Private privateProviders As List(Of GenericResourceStack)
		Friend Property Providers() As List(Of GenericResourceStack)
			Get
				Return privateProviders
			End Get
			Private Set(ByVal value As List(Of GenericResourceStack))
				privateProviders = value
			End Set
		End Property
		Public ReadOnly Property ProvidersReadonly() As IEnumerable(Of GenericResourceStack)
			Get
				Return Providers.AsReadOnly()
			End Get
		End Property
		Private privateProviderConfigs As List(Of ResourceStack)
		Friend Property ProviderConfigs() As List(Of ResourceStack)
			Get
				Return privateProviderConfigs
			End Get
			Private Set(ByVal value As List(Of ResourceStack))
				privateProviderConfigs = value
			End Set
		End Property
		Private privateQueries As List(Of GenericResourceStack)
		Friend Property Queries() As List(Of GenericResourceStack)
			Get
				Return privateQueries
			End Get
			Private Set(ByVal value As List(Of GenericResourceStack))
				privateQueries = value
			End Set
		End Property
		Public ReadOnly Property QueriesReadonly() As IEnumerable(Of GenericResourceStack)
			Get
				Return Queries.AsReadOnly()
			End Get
		End Property
		Private privateQueriesConfig As List(Of ResourceStack)
		Friend Property QueriesConfig() As List(Of ResourceStack)
			Get
				Return privateQueriesConfig
			End Get
			Private Set(ByVal value As List(Of ResourceStack))
				privateQueriesConfig = value
			End Set
		End Property
		Friend ProductionStart As Long = Int64.MinValue \ 2
		Private ReadOnly m_Unique As UniqueIDObject
		Friend ReadOnly Property UniqueID() As Long
			Get
				Return m_Unique.UniqueID
			End Get
		End Property
		Private privateQueriesPreload() As Integer
		Friend Property QueriesPreload() As Integer()
			Get
				Return privateQueriesPreload
			End Get
			Private Set(ByVal value As Integer())
				privateQueriesPreload = value
			End Set
		End Property
		Private privateProductionTime As Long
		Friend Property ProductionTime() As Long
			Get
				Return privateProductionTime
			End Get
			Private Set(ByVal value As Long)
				privateProductionTime = value
			End Set
		End Property

		Private privateIsSuspended As Boolean
		Public Property IsSuspended() As Boolean
			Get
				Return privateIsSuspended
			End Get
			Friend Set(ByVal value As Boolean)
				privateIsSuspended = value
			End Set
		End Property
		Private privateWorkingArea As Point
		Public Property WorkingArea() As Point
			Get
				Return privateWorkingArea
			End Get
			Friend Set(ByVal value As Point)
				privateWorkingArea = value
			End Set
		End Property
		Friend UseQualityIndex As Boolean = False

		Private privateIsReady As Boolean
		Public Property IsReady() As Boolean
			Get
				Return privateIsReady
			End Get
			Private Set(ByVal value As Boolean)
				privateIsReady = value
			End Set
		End Property
		Private privateParent As BuildingManager
		Friend Property Parent() As BuildingManager
			Get
				Return privateParent
			End Get
			Private Set(ByVal value As BuildingManager)
				privateParent = value
			End Set
		End Property
		Private privateConfig As BuildingConfiguration
		Public Property Config() As BuildingConfiguration
			Get
				Return privateConfig
			End Get
			Private Set(ByVal value As BuildingConfiguration)
				privateConfig = value
			End Set
		End Property
		Private privateSpawnPoint? As CyclePoint
		Friend Property SpawnPoint() As CyclePoint?
			Get
				Return privateSpawnPoint
			End Get
			Private Set(ByVal value? As CyclePoint)
				privateSpawnPoint = value
			End Set
		End Property

		Public ReadOnly Property Center() As Point
			Get
				Return New Point(Position.XGrid + Config.GridWidth \ 2, Position.YGrid + Config.GridHeight \ 2)
			End Get
		End Property

		Public Event OnStateChanged As Procedure(Of BaseBuilding)

		Friend Overridable Function Update() As Boolean
			Return Not IsSuspended
		End Function

		Friend Sub MarkAsReady()
			If IsReady Then
				Return
			End If

			IsReady = True

			RaiseStateChanged()
		End Sub

		Protected Sub RaiseStateChanged()
			RaiseEvent OnStateChanged(Me)
		End Sub

		Friend Sub RaisePriority()
			For Each query As GenericResourceStack In Queries
				If query Is Nothing Then
					Continue For
				End If

				Parent.ResourceManager.RaisePriority(query)
			Next query
		End Sub

		Friend Sub LowerPriority()
			For Each query As GenericResourceStack In Queries
				If query Is Nothing Then
					Continue For
				End If

				Parent.ResourceManager.LowerPriority(query)
			Next query
		End Sub

		Friend Sub New(ByVal inParent As BuildingManager, ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
			m_Unique = New UniqueIDObject(Me)
			Parent = inParent
			Config = inConfig
			Providers = New List(Of GenericResourceStack)()
			Queries = New List(Of GenericResourceStack)()
			Position = CyclePoint.FromGrid(inPosition)
			ProviderConfigs = New List(Of ResourceStack)()
			QueriesConfig = New List(Of ResourceStack)()
			ProductionTime = inConfig.ProductionTimeMillis

			For Each stackCfg As ResourceStack In Config.ResourceStacks
				If stackCfg.Type <> Resource.Max Then
					Continue For
				End If

				Dim stackPos As CyclePoint = CyclePoint.FromGrid(stackCfg.Position.X + Position.X, stackCfg.Position.Y + Position.Y)

				If Not SpawnPoint.HasValue Then
					SpawnPoint = stackPos
				End If
			Next stackCfg
		End Sub

		Friend Function HasFreeProvider() As Boolean
			Return (Providers.Any(AddressOf IsAvailable))
		End Function

		Private Function IsAvailable(ByVal e As GenericResourceStack) As Boolean
			If e Is Nothing Then
				Return True
			Else
				Return e.Available < e.MaxCount
			End If
		End Function

		Friend Overridable Sub CreateResourceStacks()
			' create resource stacks for building
			Dim tmpQueries As New List(Of KeyValuePair(Of ResourceStack, GenericResourceStack))()

			For Each stackCfg As ResourceStack In Config.ResourceStacks
				Dim stack As GenericResourceStack = Nothing
				Dim stackPos As CyclePoint = CyclePoint.FromGrid(stackCfg.Position.X + Position.X, stackCfg.Position.Y + Position.Y)

				If stackCfg.Type <> Resource.Max Then
					stack = Parent.ResourceManager.AddResourceStack(Me, stackPos, stackCfg.Direction, stackCfg.Type, stackCfg.MaxStackSize)

					If stackCfg.Direction = StackType.Provider Then
						Providers.Add(stack)
						ProviderConfigs.Add(stackCfg)
					End If
				End If

				If stackCfg.Direction = StackType.Query Then
					tmpQueries.Add(New KeyValuePair(Of ResourceStack, GenericResourceStack)(stackCfg, stack))
				End If
			Next stackCfg

			tmpQueries = tmpQueries.OrderBy(Function(e) e.Key.QualityIndex).ToList()
			QueriesConfig = tmpQueries.Select(Function(e) e.Key).ToList()
			Queries = tmpQueries.Select(Function(e) e.Value).ToList()
			UseQualityIndex = QueriesConfig.Any(Function(e) e.QualityIndex > 0)

			QueriesPreload = New Integer(Queries.Count - 1){}
		End Sub
	End Class
End Namespace
