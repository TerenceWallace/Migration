Imports Migration.Buildings
Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core
Imports Migration.Jobs

Namespace Migration

	''' <summary>
	''' The resource manager, as its name implies, takes track of all resource stacks in the game.
	''' It also seeks for Migrants carrying resources from provider to queries in a prioritized
	''' way. This is very important, since it ensures that queries with lower VirtualCount
	''' will come first, which has a significant positive impact on the whole economy. In the original
	''' game this was not enforced, this combined with the fact that there seemed to be a limit of
	''' resources being carried, it was just a burden if not impossible to get a huge settlement
	''' running properly.
	''' </summary>
	Friend Class ResourceManager
		Inherits SynchronizedManager

		''' <summary>
		''' All resources and queries are put into this list.
		''' </summary>
		Private ReadOnly m_Resources As TopologicalList(Of GenericResourceStack)
		Private ReadOnly m_Foilage As TopologicalList(Of Foilage)
		Private ReadOnly m_Stones As TopologicalList(Of Stone)
		Private ReadOnly m_PriorityQueries As New LinkedList(Of GenericResourceStack)()

		''' <summary>
		''' All queries will also go here, as long as their VirtualCount is
		''' less than their MaxStack size. If this is the case you will find
		''' the query in the list with index VirtualCount.
		''' </summary>
		Private ReadOnly m_Queries() As List(Of GenericResourceStack)

		''' <summary>
		''' The associated movable manager.
		''' </summary>
        Private privateManager As MovableManager
        Friend Property Manager() As MovableManager
            Get
                Return privateManager
            End Get
            Private Set(ByVal value As MovableManager)
                privateManager = value
            End Set
        End Property

        Private privateBuildMgr As BuildingManager
        Friend Property BuildMgr() As BuildingManager
            Get
                Return privateBuildMgr
            End Get
            Set(ByVal value As BuildingManager)
                privateBuildMgr = value
            End Set
        End Property

        Private privateMap As Migration.Game.Map
        Friend Property Map() As Migration.Game.Map
            Get
                Return privateMap
            End Get
            Private Set(ByVal value As Migration.Game.Map)
                privateMap = value
            End Set
        End Property

        Friend ReadOnly Property Terrain() As TerrainDefinition
            Get
                Return Manager.Terrain
            End Get
        End Property

        ''' <summary>
        ''' Is raised whenever a resource stack is added.
        ''' </summary>
        Friend Event OnAddStack As DOnAddResourceStack(Of ResourceManager)
        ''' <summary>
        ''' Is raised whenever a resource stack is removed.
        ''' </summary>
        Friend Event OnRemoveStack As DOnRemoveResourceStack(Of ResourceManager)
        Friend Event OnAddFoilage As DOnAddFoilage(Of ResourceManager)
        Friend Event OnRemoveFoilage As DOnRemoveFoilage(Of ResourceManager)
        Friend Event OnAddStone As DOnAddStone(Of ResourceManager)
        Friend Event OnRemoveStone As DOnRemoveStone(Of ResourceManager)

        Friend Sub New(ByVal inMap As Migration.Game.Map, ByVal inMovMgr As MovableManager)
            MyBase.New(inMovMgr)
            If (inMovMgr Is Nothing) OrElse (inMap Is Nothing) Then
                Throw New ArgumentNullException()
            End If

            Map = inMap
            Manager = inMovMgr
            m_Resources = New TopologicalList(Of GenericResourceStack)(10, inMovMgr.Size, inMovMgr.Size)
            m_Foilage = New TopologicalList(Of Foilage)(10, inMovMgr.Size, inMovMgr.Size)
            m_Stones = New TopologicalList(Of Stone)(10, inMovMgr.Size, inMovMgr.Size)
            m_Queries = New List(Of GenericResourceStack)(GenericResourceStack.DEFAULT_STACK_SIZE - 1) {}

            For i As Integer = 0 To m_Queries.Length - 1
                m_Queries(i) = New List(Of GenericResourceStack)()
            Next i
        End Sub

        Private Sub inMovMgr_OnCellChanged(ByVal sender As MovableManager, ByVal cell As Point)
            Dim pos As New Point(cell.X, cell.Y)

            If Manager.IsWalkable(pos, 0) Then
                Return
            End If

            Dim stacks As IEnumerable(Of GenericResourceStack) = m_Resources.EnumAt(pos)

            For Each stack As GenericResourceStack In stacks
                If stack.Type = StackType.Query Then
                    Throw New InvalidOperationException("Attempt to make a cell, containing a resource query, unwalkable.")
                End If

                RemoveResource(stack)
            Next stack
        End Sub

        ''' <summary>
        ''' Adds a new resource stack with the given parameters and raises <see cref="OnAddStack"/>.
        ''' This method is also called internally and it is crucial to do all postprocessings meant to
        ''' be applied for all resource stacks, like attaching a visual for rendering, in the mentioned event.
        ''' </summary>
        ''' <exception cref="ArgumentException">There is already a resource or the given position is not walkable.</exception>
        Friend Function AddResourceStack(ByVal inPosition As CyclePoint, ByVal inType As StackType, ByVal inResource As Resource, ByVal inMaxStack As Int32) As GenericResourceStack
            Return AddResourceStack(Nothing, inPosition, inType, inResource, inMaxStack)
        End Function

        Friend Function CanPlaceResourceAt(ByVal inPosition As Point) As Boolean
            For x As Integer = -1 To 1
                For y As Integer = -1 To 1
                    Dim pos As New Point(inPosition.X + x, inPosition.Y + y)

                    If (pos.X < 0) OrElse (pos.Y < 0) OrElse (pos.X >= Terrain.Size) OrElse (pos.Y >= Terrain.Size) Then
                        Return False
                    End If

                    If m_Resources.EnumAt(pos).Count() > 0 Then
                        Return False
                    End If
                Next y
            Next x

            If Manager.Terrain.GetWallAt(inPosition.X, inPosition.Y) > WallValue.Reserved Then
                Return False
            End If

            Return True
        End Function

        Friend Function AddResourceStack(ByVal inParent As BaseBuilding, ByVal inPosition As CyclePoint, ByVal inType As StackType, ByVal inResource As Resource, ByVal inMaxStack As Int32) As GenericResourceStack
            Dim stack As New GenericResourceStack(inParent, inType, inResource, (If(Convert.ToBoolean(inMaxStack > 0), inMaxStack, GenericResourceStack.DEFAULT_STACK_SIZE)))

            stack.Position = inPosition

            If stack.Type = StackType.Query Then
                AddHandler stack.OnCountChanged, AddressOf OnStackCountChanged
                AddHandler stack.OnMaxCountChanged, AddressOf OnMaxStackCountChanged

                m_Queries(0).Add(stack)
            End If

            Manager.Terrain.InitializeWallAt(New Point(inPosition.XGrid, inPosition.YGrid), WallValue.Reserved, New Rectangle(0, 0, 1, 1))

            m_Resources.Add(stack)

            RaiseEvent OnAddStack(Me, stack)

            Return stack
        End Function

        Private Sub OnMaxStackCountChanged(ByVal inSender As GenericResourceStack, ByVal inOldMaxCount As Integer, ByVal inNewMaxCount As Integer)
            If inSender.IsRemoved Then
                Return
            End If

            If inOldMaxCount > inNewMaxCount Then
                ' max count is being decreased
                If inSender.VirtualCount >= inNewMaxCount Then
                    ' remove from queue
                    If Not (m_Queries(inSender.VirtualCount).Remove(inSender)) Then
                        Throw New ApplicationException("Resource query is not registered in queue.")
                    End If
                End If
            Else
                ' max count is being increased
                If (inSender.VirtualCount >= inOldMaxCount) AndAlso (inSender.VirtualCount < inNewMaxCount) Then
                    ' add to new queue
                    m_Queries(inSender.VirtualCount).Add(inSender)
                End If
            End If
        End Sub

        ''' <summary>
        ''' For resource queries we need to keep track of their stack size, since we have
        ''' to relocate them appropriately in the priority queue.
        ''' </summary>
        Private Sub OnStackCountChanged(ByVal inSender As GenericResourceStack, ByVal inOldValue As Integer, ByVal inNewValue As Integer)
            If inSender.IsRemoved Then
                Return
            End If

            If inOldValue < inSender.MaxCount Then
                ' remove from old queue
                If Not (m_Queries(inOldValue).Remove(inSender)) Then
                    Throw New ApplicationException("Resource query is not registered in queue.")
                End If
            End If

            If inNewValue < inSender.MaxCount Then
                ' add to new queue
                m_Queries(inNewValue).Add(inSender)
            End If
        End Sub

        ''' <summary>
        ''' If no resource exists at the given grid point, a new resource provider is added and
        ''' filled. Otherwise the existing stack is used if it is of the same resource type.
        ''' If <paramref name="inCount"/> exceeds the remaining stack capacity, the method enumerates
        ''' around the given point and repeats the task until all desired resources are placed.
        ''' </summary>
        ''' <remarks>
        ''' This is an important method also demonstrating how powerful our lightweight resource
        ''' concept is. With almost no effort we can simulate resource drops in the same way they
        ''' are done in the original game.
        ''' </remarks>
        ''' <param name="inAround">You may also pass unwalkable spots. The method enumerates until it finds one.</param>
        ''' <param name="inResource">The resource type being dropped.</param>
        ''' <param name="inCount">Amount of resources being dropped.</param>
        Friend Sub DropResource(ByVal inAround As Point, ByVal inResource As Resource, ByVal inCount As Integer)
            DropResourceInternal(inAround, inResource, inCount, Nothing)
        End Sub

        Friend Sub DropMarketResource(ByVal inMarket As Market, ByVal inAround As Point, ByVal inResource As Resource, ByVal inCount As Integer)
            DropResourceInternal(inAround, inResource, inCount, inMarket.UniqueID)
        End Sub

        Private Sub DropResourceInternal(ByVal inAround As Point, ByVal inResource As Resource, ByVal inCount As Integer, ByVal inMinProvID? As Int64)
            ' merge with existing stacks
            ' add a new resource provider

            Terrain.EnumAround(inAround, Function(pos As Point)
                                             Dim stackEnum As IEnumerable(Of GenericResourceStack) = m_Resources.EnumAt(pos)
                                             Dim stack As GenericResourceStack = Nothing
                                             If stackEnum.Count() > 0 Then
                                                 stack = stackEnum.First()
                                                 If (stack.Type = StackType.Query) OrElse (stack.Resource <> inResource) Then
                                                     Return WalkResult.NotFound
                                                 End If
                                             Else
                                                 If Not (CanPlaceResourceAt(pos)) Then
                                                     Return WalkResult.NotFound
                                                 End If
                                                 stack = AddResourceStack(CyclePoint.FromGrid(pos), StackType.Provider, inResource, 0)
                                             End If
                                             Dim freeCount As Integer = stack.MaxCount - stack.VirtualCount
                                             If freeCount <= 0 Then
                                                 Return WalkResult.NotFound
                                             End If
                                             freeCount = Math.Min(freeCount, inCount)
                                             For i As Integer = 0 To freeCount - 1
                                                 stack.AddResource()
                                             Next i
                                             If inMinProvID.HasValue Then
                                                 stack.MinProviderID = inMinProvID.Value
                                             End If
                                             inCount -= freeCount
                                             If inCount <= 0 Then
                                                 Return WalkResult.Success
                                             Else
                                                 Return WalkResult.NotFound
                                             End If
                                         End Function)
        End Sub

        Friend Function FindResourceAround(ByVal inAround As Point, ByVal inResource As Resource, ByVal inStackType As StackType) As GenericResourceStack
            Return FindResourceAroundInternal(inAround, inResource, inStackType, 0)
        End Function

        Friend Function FindResourceAroundInternal(ByVal inAround As Point, ByVal inResource As Resource, ByVal inStackType As StackType, ByVal inQueryID As Int64) As GenericResourceStack
            Dim result As GenericResourceStack = Nothing

            ' To prevent resources from being carried back to the origin (market)
            m_Resources.EnumAround(inAround, Function(stack)
                                                 If stack.Type <> inStackType Then
                                                     Return WalkResult.NotFound
                                                 End If
                                                 If stack.Resource <> inResource Then
                                                     Return WalkResult.NotFound
                                                 End If
                                                 If stack.VirtualCount <= 0 Then
                                                     Return WalkResult.NotFound
                                                 End If
                                                 If inQueryID > 0 Then
                                                     If stack.MinProviderID >= inQueryID Then
                                                         Return WalkResult.NotFound
                                                     End If
                                                 End If
                                                 result = stack
                                                 Return WalkResult.Success
                                             End Function)

            Return result
        End Function


        ''' <summary>
        ''' Removes the given stack. Please keep in mind that this method is also called
        ''' internally and it is crucial that you put ALL additional code required to release
        ''' the resource stack into the event handler <see cref="OnRemoveStack"/>, like
        ''' detaching it from the render pipeline.
        ''' </summary>
        ''' <exception cref="ArgumentNullException">No resource given.</exception>
        ''' <exception cref="ApplicationException">Resource does not exist.</exception>
        Friend Sub RemoveResource(ByVal inResource As GenericResourceStack)
            If inResource Is Nothing Then
                Throw New ArgumentNullException()
            End If

            m_Resources.Remove(inResource)

            If inResource.PriorityNode IsNot Nothing Then
                m_PriorityQueries.Remove(inResource.PriorityNode)
            End If

            inResource.PriorityNode = Nothing

            ' remove from query queue
            If inResource.Type = StackType.Query Then
                If inResource.VirtualCount < inResource.MaxCount Then
                    If Not (m_Queries(inResource.VirtualCount).Remove(inResource)) Then
                        Throw New ApplicationException("Resource query is not registered in queue.")
                    End If
                End If
            End If

            inResource.MarkAsRemoved()

            RaiseEvent OnRemoveStack(Me, inResource)
        End Sub

        Friend Sub RaisePriority(ByVal inStack As GenericResourceStack)
            If inStack.Type <> StackType.Query Then
                Throw New InvalidOperationException()
            End If

            LowerPriority(inStack)

            inStack.PriorityNode = m_PriorityQueries.AddFirst(inStack)
        End Sub

        Friend Sub LowerPriority(ByVal inStack As GenericResourceStack)
            If Not inStack.HasPriority Then
                Return
            End If

            m_PriorityQueries.Remove(inStack.PriorityNode)
            inStack.PriorityNode = Nothing
        End Sub

        ''' <summary>
        ''' Will do all the job planning.
        ''' </summary>
        Friend Overrides Sub ProcessCycle()
            MyBase.ProcessCycle()

            If Not IsAlignedCycle Then
                Return
            End If

            ' process high priorities
            For Each query As GenericResourceStack In m_PriorityQueries
                Dim i As Integer = 0
                Dim count As Integer = query.MaxCount - query.VirtualCount
                Do While i < count
                    ProcessQuery(query)
                    i += 1
                Loop
            Next query

            ' process BuildTask resources
            If BuildMgr IsNot Nothing Then
                BuildMgr.ProcessQueries(AddressOf AnonymousMethod1)
            End If

            ' process normal priorities
            For Each queue As List(Of GenericResourceStack) In m_Queries
                ' create a local modifiable copy of the collection
                Dim mLocalQueue As New List(Of GenericResourceStack)(queue)
                For Each query As GenericResourceStack In mLocalQueue
                    ProcessQuery(query)
                Next query
            Next queue

        End Sub

        Private Sub AnonymousMethod1(ByVal query As GenericResourceStack)
            If query.VirtualCount < query.MaxCount Then
                ProcessQuery(query)
            End If
        End Sub

        Private Sub ProcessQuery(ByVal query As GenericResourceStack)
            Debug.Assert(query.Type = StackType.Query)

            ' special handling for Mine queries
            If (query.Building IsNot Nothing) AndAlso (TypeOf query.Building Is Mine) Then
                Dim dist As StockDistributionArray = Map.Configuration.StockDistributions
                Dim isQueryEnabled As Boolean = False

                For i As Integer = 0 To dist.Length - 1
                    If dist(i).Building Is query.Building.Config Then
                        isQueryEnabled = dist(i).Queries(query.Resource)

                        Exit For
                    End If
                Next i

                If Not isQueryEnabled Then
                    Return
                End If
            End If

            ' special handling for Market queries
            Dim queryID As Int64 = 0

            If (query.Building IsNot Nothing) AndAlso (TypeOf query.Building Is Market) Then
                Dim market As Market = (TryCast(query.Building, Market))

                queryID = market.UniqueID
            End If

            ' search for matching provider
            Dim provider As GenericResourceStack = FindResourceAroundInternal(query.Position.ToPoint(), query.Resource, StackType.Provider, queryID)
            If provider Is Nothing Then Return

            ' find Migrant around provider
            Dim m_movable As Movable = Manager.FindFreeMovableAround(provider.Position.ToPoint(), MovableType.Migrant)
            If m_movable Is Nothing Then Return

            ' carry resource...
            Dim job As New JobCarrying(m_movable, provider, query)
            m_movable.Job = job

            AddHandler job.OnPickedUp, Sub(usused) job.Character = "Carrying_" & m_movable.Carrying.ToString()
            AddHandler job.OnCompleted, Sub(unused, succeeded)
                                            m_movable.Job = Nothing
                                            job.Update()
                                        End Sub
        End Sub

		Friend Sub CountResources(ByVal inBuffer() As Integer)
			Array.Clear(inBuffer, 0, inBuffer.Length)

			m_Resources.ForEach(Function(stack)
				inBuffer(Convert.ToInt32(stack.Resource)) += stack.VirtualCount
				Return True
			End Function)
		End Sub

		Friend Sub RemoveFoilage(ByVal inFoilage As Foilage)
			m_Foilage.Remove(inFoilage)

			Terrain.SetWallAt(inFoilage.Position.ToPoint(), WallValue.Free, New Rectangle(-1, -1, 3, 3))

			RaiseEvent OnRemoveFoilage(Me, inFoilage)
		End Sub

		Friend Sub AddFoilage(ByVal inPosition As Point, ByVal inType As FoilageType, ByVal inState As FoilageState)
			Dim around As Point = inPosition

			Dim checkResult As WalkResult = GridSearch.GridWalkAround(around, Terrain.Size, Terrain.Size, Function(pos As Point)
				If Math.Abs(pos.X - around.X) > 5 Then
					Return WalkResult.Abort
				End If
				If Not(Terrain.CanFoilageBePlacedAt(pos.X, pos.Y, inType)) Then
					Return WalkResult.NotFound
				End If
				inPosition = pos
				Return WalkResult.Success
			End Function)

			If checkResult <> WalkResult.Success Then
				Return ' it's not considred essential enough to throw an exception when foilage can not be placed around target...
			End If

			' reserve map space
			Terrain.SetWallAt(inPosition, WallValue.Building)
			'Terrain.SetFlagsAt(inPosition.X, inPosition.Y, TerrainCellFlags.Grading);
			Terrain.InitializeWallAt(inPosition, WallValue.Reserved, New Rectangle(-1, -1, 3, 3))

			Dim foilage As New Foilage(inPosition, inType, inState)

			m_Foilage.Add(foilage)

			If inState = FoilageState.Growing Then
				QueueWorkItem(VisualUtilities.GetDurationMillis(foilage, "Growing"), Sub() foilage.MarkAsGrown())
			End If

			RaiseEvent OnAddFoilage(Me, foilage)
		End Sub

		Friend Function FindFoilageAround(ByVal inAround As Point, ByVal inRadius As Integer, ByVal inTypeArg As FoilageType, ByVal inStateArg As FoilageState) As Foilage
			resultFoilage = Nothing
			inType = inTypeArg
			inState = inStateArg

			If WalkResult.Success <> m_Foilage.EnumAround(inAround, inRadius, AddressOf AnonymousMethod3) Then
				Return Nothing
			End If

			Return resultFoilage
		End Function

		Private resultFoilage As Foilage = Nothing
		Private inType As FoilageType
		Private inState As FoilageState
		Private Function AnonymousMethod3(ByVal foilage As Foilage) As WalkResult
			If (foilage.State <> inState) OrElse ((foilage.Type And inType) = 0) Then
				Return WalkResult.NotFound
			End If
			resultFoilage = foilage
			Return WalkResult.Success
		End Function

		Friend Function CountFoilageAround(ByVal inAround As Point, ByVal inRadius As Integer, ByVal inType As FoilageType, ByVal inState As FoilageState) As Integer
			Dim count As Integer = 0

			m_Foilage.EnumAround(inAround, inRadius, Function(foilage)
				If (foilage.State <> inState) OrElse ((foilage.Type And inType) = 0) Then
					Return WalkResult.NotFound
				End If
				count += 1
				Return WalkResult.NotFound
			End Function)

			Return count
		End Function

		Friend Function EnumFoilageAround(ByVal inAround As Point, ByVal inRadius As Integer, ByVal inHandler As Func(Of Foilage, WalkResult)) As WalkResult
			Return m_Foilage.EnumAround(inAround, inRadius, Function(foilage) inHandler(foilage))
		End Function

		Friend Sub RemoveStone(ByVal inStone As Stone)
			m_Stones.Remove(inStone)

			Terrain.SetWallAt(inStone.Position.ToPoint(), WallValue.Free, New Rectangle(-1, -1, 3, 3))

			RaiseEvent OnRemoveStone(Me, inStone)
		End Sub

		Friend Sub AddStone(ByVal inPosition As Point, ByVal inInitialStoneCount As Integer)
			Dim around As Point = inPosition

			Dim checkResult As WalkResult = GridSearch.GridWalkAround(around, Terrain.Size, Terrain.Size, Function(pos)
				If Math.Abs(pos.X - around.X) > 5 Then
					Return WalkResult.Abort
				End If
				If Not(Terrain.CanFoilageBePlacedAt(pos.X, pos.Y, FoilageType.Tree1)) Then
					Return WalkResult.NotFound
				End If
				inPosition = pos
				Return WalkResult.Success
			End Function)

			If checkResult <> WalkResult.Success Then
				Return ' it's not considred essential enough to throw an exception when stone can not be placed around target...
			End If

			' reserve map space
			Terrain.SetWallAt(inPosition, WallValue.Building)
			Terrain.InitializeWallAt(inPosition, WallValue.Reserved, New Rectangle(-1, -1, 3, 3))

			Dim stone As New Stone(inPosition, inInitialStoneCount)

			m_Stones.Add(stone)

			RaiseEvent OnAddStone(Me, stone)
		End Sub

		Friend Function FindStoneAround(ByVal inAround As Point, ByVal inRadius As Integer) As Stone
			resultStone = Nothing

			If WalkResult.Success <> m_Stones.EnumAround(inAround, inRadius, AddressOf AnonymousMethod4) Then
				Return Nothing
			End If

			Return resultStone
		End Function

		Private resultStone As Stone = Nothing
		Private Function AnonymousMethod4(ByVal stone As Stone) As WalkResult
			If stone.RemainingStones <= 0 Then
				Return WalkResult.NotFound
			End If
			resultStone = stone
			Return WalkResult.Success
		End Function
	End Class
End Namespace
