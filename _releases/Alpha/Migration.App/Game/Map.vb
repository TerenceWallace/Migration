Imports System.IO
Imports System.Threading
Imports Migration.Buildings
Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core
Imports Migration.Jobs

Namespace Migration.Game
    Public Class Map

        Public Event OnGameStarted As DNotifyHandler(Of Map)
        Public Event OnAddMovable As DOnAddMovable(Of Map)
        Public Event OnRemoveMovable As DOnRemoveMovable(Of Map)
        Public Event OnAddBuilding As DOnAddBuilding(Of Map)
        Public Event OnRemoveBuilding As DOnRemoveBuilding(Of Map)
        Public Event OnAddStack As DOnAddResourceStack(Of Map)
        Public Event OnRemoveStack As DOnRemoveResourceStack(Of Map)
        Public Event OnGradingStep As DOnGradingStep(Of Map)
        Public Event OnAddBuildTask As DOnAddBuildTask(Of Map)
        Public Event OnRemoveBuildTask As DOnRemoveBuildTask(Of Map)
        Public Event OnAddFoilage As DOnAddFoilage(Of Map)
        Public Event OnRemoveFoilage As DOnRemoveFoilage(Of Map)
        Public Event OnAddStone As DOnAddStone(Of Map)
        Public Event OnRemoveStone As DOnRemoveStone(Of Map)

        Private m_Races() As RaceConfiguration
        Private m_LogFile As MapFile
        Private m_Random As New CrossRandom(0)
        Private m_UpdateConfig As Boolean = False
        Private m_ForwardOneMinute As Boolean = False
        Private m_SuspendSignal As New ManualResetEvent(False)
        Private m_ResumeSignal As New ManualResetEvent(False)
        Private m_SimulationThread As Thread

        Private privateRouting As TerrainRouting
        Friend Property Routing() As TerrainRouting
            Get
                Return privateRouting
            End Get
            Set(ByVal value As TerrainRouting)
                privateRouting = value
            End Set
        End Property

        Private privateMovableManager As MovableManager
        Friend Property MovableManager() As MovableManager
            Get
                Return privateMovableManager
            End Get
            Private Set(ByVal value As MovableManager)
                privateMovableManager = value
            End Set
        End Property

        Private privateResMgr As ResourceManager
        Friend Property ResourceManager() As ResourceManager
            Get
                Return privateResMgr
            End Get
            Private Set(ByVal value As ResourceManager)
                privateResMgr = value
            End Set
        End Property

        Private privateBuildMgr As BuildingManager
        Friend Property BuildingManager() As BuildingManager
            Get
                Return privateBuildMgr
            End Get
            Private Set(ByVal value As BuildingManager)
                privateBuildMgr = value
            End Set
        End Property

        Private privateIsSuspended As Boolean
        Public Property IsSuspended() As Boolean
            Get
                Return privateIsSuspended
            End Get
            Private Set(ByVal value As Boolean)
                privateIsSuspended = value
            End Set
        End Property

        Public ReadOnly Property CurrentCycle() As Long
            Get
                Return MovableManager.CurrentCycle
            End Get
        End Property

        Public ReadOnly Property CycleResolution() As Long
            Get
                Return MovableManager.CycleResolution
            End Get
        End Property

        Private privateAnimationTime As Long
        Public Property AnimationTime() As Long
            Get
                Return privateAnimationTime
            End Get
            Private Set(ByVal value As Long)
                privateAnimationTime = value
            End Set
        End Property

        Private privateSize As Integer
        Public Property Size() As Integer
            Get
                Return privateSize
            End Get
            Private Set(ByVal value As Integer)
                privateSize = value
            End Set
        End Property

        Private privateTerrain As TerrainDefinition
        Public Property Terrain() As TerrainDefinition
            Get
                Return privateTerrain
            End Get
            Private Set(ByVal value As TerrainDefinition)
                privateTerrain = value
            End Set
        End Property

        Private privateConfig As GameConfiguration
        Public Property Configuration() As GameConfiguration
            Get
                Return privateConfig
            End Get
            Private Set(ByVal value As GameConfiguration)
                privateConfig = value
            End Set
        End Property

        Private privateIsInitialized As Boolean
        Public Property IsInitialized() As Boolean
            Get
                Return privateIsInitialized
            End Get
            Private Set(ByVal value As Boolean)
                privateIsInitialized = value
            End Set
        End Property

        Public ReadOnly Property Race() As RaceConfiguration
            Get
                Return m_Races(0)
            End Get
        End Property

        Public ReadOnly Property AvgPlanMillis() As Long
            Get
                Return MovableManager.AvgPlanMillis
            End Get
        End Property

        Public ReadOnly Property MaxPlanMillis() As Long
            Get
                Return MovableManager.MaxPlanMillis
            End Get
        End Property

        Public Sub New(ByVal inSize As Integer, ByVal inInitialHouseSpaceCount As Integer, ByVal inRaces() As RaceConfiguration)
            m_Random = New CrossRandom(0)
            m_SuspendSignal = New ManualResetEvent(False)
            m_ResumeSignal = New ManualResetEvent(False)
            m_LogFile = MapFile.OpenWrite(Path.GetTempFileName(), Nothing)
            m_Races = inRaces.ToArray()
            Size = inSize
            Terrain = New TerrainDefinition(Me, New TerrainConfiguration())
            MovableManager = New MovableManager(Me, 0, Convert.ToInt32(CyclePoint.CYCLE_MILLIS))
            Routing = New TerrainRouting(Terrain, CurrentCycle, CycleResolution)
            ResourceManager = New ResourceManager(Me, MovableManager)
            BuildingManager = New BuildingManager(MovableManager, ResourceManager, inInitialHouseSpaceCount)
            ResourceManager.BuildMgr = BuildingManager

            If m_Races.Length = 0 Then
                Throw New ArgumentException("At least one race config must be specified.")
            End If

            InitializeHandlers()

            Configuration = New GameConfiguration(Me)
            m_SimulationThread = New Thread(New ThreadStart(AddressOf SimulationLoop))
            m_SimulationThread.IsBackground = True

        End Sub

        Private Sub InitializeHandlers()

            AddHandler MovableManager.OnAddMovable, Sub(sender As MovableManager, movable As Movable)
                                                        If OnAddMovableEvent IsNot Nothing Then
                                                            VisualUtilities.SynchronizeTask(Sub() RaiseEvent OnAddMovable(Me, movable))
                                                        End If
                                                        AddHandler movable.OnStop, AddressOf Movable_OnStop
                                                    End Sub

            AddHandler MovableManager.OnRemoveMovable, Sub(sender As MovableManager, movable As Movable)
                                                           If OnRemoveMovableEvent IsNot Nothing Then
                                                               VisualUtilities.SynchronizeTask(Sub() RaiseEvent OnRemoveMovable(Me, movable))
                                                           End If
                                                       End Sub

            AddHandler ResourceManager.OnAddStack, Sub(sender As ResourceManager, stack As GenericResourceStack)
                                                       If OnAddStackEvent IsNot Nothing Then
                                                           VisualUtilities.SynchronizeTask(Sub() RaiseEvent OnAddStack(Me, stack))
                                                       End If
                                                   End Sub

            AddHandler ResourceManager.OnRemoveStack, Sub(sender As ResourceManager, stack As GenericResourceStack)
                                                          If OnRemoveStackEvent IsNot Nothing Then
                                                              VisualUtilities.SynchronizeTask(Sub() RaiseEvent OnRemoveStack(Me, stack))
                                                          End If
                                                      End Sub

            AddHandler ResourceManager.OnAddFoilage, Sub(sender As ResourceManager, foilage As Foilage)
                                                         If OnAddBuildTaskEvent IsNot Nothing Then
                                                             VisualUtilities.SynchronizeTask(Sub() RaiseEvent OnAddFoilage(Me, foilage))
                                                         End If
                                                     End Sub

            AddHandler ResourceManager.OnRemoveFoilage, Sub(sender As ResourceManager, foilage As Foilage)
                                                            If OnRemoveBuildTaskEvent IsNot Nothing Then
                                                                VisualUtilities.SynchronizeTask(Sub() RaiseEvent OnRemoveFoilage(Me, foilage))
                                                            End If
                                                        End Sub

            AddHandler ResourceManager.OnAddStone, Sub(sender As ResourceManager, stone As Stone)
                                                       If OnAddStoneEvent IsNot Nothing Then
                                                           VisualUtilities.SynchronizeTask(Sub() RaiseEvent OnAddStone(Me, stone))
                                                       End If
                                                   End Sub

            AddHandler ResourceManager.OnRemoveStone, Sub(sender As ResourceManager, stone As Stone)
                                                          If OnRemoveStoneEvent IsNot Nothing Then
                                                              VisualUtilities.SynchronizeTask(Sub() RaiseEvent OnRemoveStone(Me, stone))
                                                          End If
                                                      End Sub

            AddHandler BuildingManager.OnAddBuilding, Sub(sender As BuildingManager, buildable As BaseBuilding)
                                                          If OnAddBuildingEvent IsNot Nothing Then
                                                              VisualUtilities.SynchronizeTask(Sub() RaiseEvent OnAddBuilding(Me, buildable))
                                                          End If
                                                      End Sub

            AddHandler BuildingManager.OnRemoveBuilding, Sub(sender As BuildingManager, buildable As BaseBuilding)
                                                             If OnRemoveBuildingEvent IsNot Nothing Then
                                                                 VisualUtilities.SynchronizeTask(Sub() RaiseEvent OnRemoveBuilding(Me, buildable))
                                                             End If
                                                         End Sub

            AddHandler BuildingManager.OnGradingStep, Sub(sender As BuildingManager, grader As Movable, completion As Procedure)
                                                          If OnGradingStepEvent IsNot Nothing Then
                                                              VisualUtilities.SynchronizeTask(Sub() RaiseEvent OnGradingStep(Me, grader, completion))
                                                          End If
                                                      End Sub

            AddHandler BuildingManager.OnAddBuildTask, Sub(sender As BuildingManager, task As BuildTask)
                                                           If OnAddBuildTaskEvent IsNot Nothing Then
                                                               VisualUtilities.SynchronizeTask(Sub() RaiseEvent OnAddBuildTask(Me, task))
                                                           End If
                                                       End Sub

            AddHandler BuildingManager.OnRemoveBuildTask, Sub(sender As BuildingManager, task As BuildTask)
                                                              If OnRemoveBuildTaskEvent IsNot Nothing Then
                                                                  VisualUtilities.SynchronizeTask(Sub() RaiseEvent OnRemoveBuildTask(Me, task))
                                                              End If
                                                          End Sub
        End Sub

        Public Sub ForwardOneMinute()
            m_ForwardOneMinute = True
        End Sub

        Public Sub UpdateConfig()
            m_UpdateConfig = True
        End Sub

        Public Sub Initialize()
            Start(Nothing)
        End Sub

        Public Sub Start(ByVal inInitProcedure As Procedure)
            If IsInitialized Then
                Throw New InvalidOperationException()
            End If

            If inInitProcedure IsNot Nothing Then
                inInitProcedure()
            End If

            IsInitialized = True

            m_SimulationThread.Start()
        End Sub

        Public Sub GenerateRandomMap(ByVal inSeed As Long)
            RandomTerrainGenerator.Run(Me, 0)
        End Sub

        Public Sub GenerateMapFromFile(ByVal inXMLFile As String, ByVal inSeed As Long)
            Dim fullXMLPath As String = Path.GetFullPath(inXMLFile)

            Dim m_mapFile As String = Path.GetDirectoryName(inXMLFile) & "/" & Path.GetFileNameWithoutExtension(inXMLFile) & ".map"
            Dim isLoaded As Boolean = False

            If File.Exists(m_mapFile) Then
                ' load from map file
                Dim stream As New FileStream(m_mapFile, FileMode.Open, FileAccess.Read, FileShare.Read)

                Try
                    Using stream
                        Routing.LoadFromStream(stream)
                        Terrain.LoadFromStream(stream)
                    End Using

                    isLoaded = True
                Catch
                    File.Delete(m_mapFile)
                End Try
            End If

            If Not isLoaded Then
                XMLTerrainGenerator.Run(Me, inSeed, inXMLFile)

                ' save map file
                Dim stream As New FileStream(m_mapFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read)

                stream.SetLength(0)

                Using stream
                    Routing.SaveToStream(stream)
                    Terrain.SaveToStream(stream)
                End Using
            End If

            ' add environmental objects, like trees and stones
            Dim count As Integer = 0

            For x As Integer = 0 To Size - 1
                For y As Integer = 0 To Size - 1
                    Dim flags As TerrainCellFlags = Terrain.GetFlagsAt(x, y)
                    'Dim flags2 As TerrainCellFlags = Terrain.GetFlagsAt(y, x)

                    If flags.IsSet(TerrainCellFlags.Tree_01) Then
                        ResourceManager.AddFoilage(New Point(x, y), FoilageType.Tree1, FoilageState.Grown)

                        count += 1
                    End If

                    If flags.IsSet(TerrainCellFlags.Stone) Then
                        ResourceManager.AddStone(New Point(x, y), m_Random.Next(0, 12))
                    End If
                Next y
            Next x
        End Sub

        Public Sub ClearLog()
            m_LogFile.Clear()
        End Sub

        Public Sub Save(ByVal inFileName As String)
            m_LogFile.Fork(CurrentCycle, inFileName)
        End Sub

        Friend Function ResolveBuildingTypeIndex(ByVal inBuildingID As Integer) As BuildingConfiguration

            For Each m_race As RaceConfiguration In m_Races
                Dim config As BuildingConfiguration = ( _
                    From e In m_race.Buildables _
                    Where e.TypeIndex = inBuildingID _
                    Select e).FirstOrDefault()

                If config IsNot Nothing Then
                    Return config
                End If
            Next m_race

            Return Nothing
        End Function

        Public Sub Synchronize(ByVal inTask As Procedure)
            MovableManager.QueueWorkItem(inTask)
        End Sub

        Public Sub Synchronize(ByVal inDueTimeMillis As Long, ByVal inTask As Procedure)
            MovableManager.QueueWorkItem(inDueTimeMillis, inTask)
        End Sub

        Friend Sub AddBuildingInternal(ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
            m_LogFile.AddBuilding(CurrentCycle, inConfig, inPosition)
            BuildingManager.BeginBuilding(inConfig, inPosition)
        End Sub

        Public Sub AddBuilding(ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
            If IsInitialized Then
                Synchronize(Sub() AddBuildingInternal(inConfig, inPosition))
            Else
                AddBuildingInternal(inConfig, inPosition)
            End If
        End Sub

        Friend Sub AddMovableInternal(ByVal inPosition As CyclePoint, ByVal inMovableType As MovableType)
            m_LogFile.AddMovable(CurrentCycle, inPosition, inMovableType)
            MovableManager.AddMovable(inPosition, inMovableType)
        End Sub

        Public Sub AddMovable(ByVal inPosition As CyclePoint, ByVal inMovableType As MovableType)
            If IsInitialized Then
                Synchronize(Sub() AddMovableInternal(inPosition, inMovableType))
            Else
                AddMovableInternal(inPosition, inMovableType)
            End If
        End Sub

        Friend Sub DropResourceInternal(ByVal inAround As Point, ByVal inResource As Resource, ByVal inCount As Integer)
            m_LogFile.DropResource(CurrentCycle, inAround, inResource, inCount)
            ResourceManager.DropResource(inAround, inResource, inCount)
        End Sub

        Public Sub DropResource(ByVal inAround As Point, ByVal inResource As Resource, ByVal inCount As Integer)
            If IsInitialized Then
                Synchronize(Sub() DropResourceInternal(inAround, inResource, inCount))
            Else
                DropResourceInternal(inAround, inResource, inCount)
            End If
        End Sub

        Friend Sub AddMarketTransportInternal(ByVal inBuildingID As Long, ByVal inResource As Resource)
            m_LogFile.AddMarketTransport(CurrentCycle, inBuildingID, inResource)

            Dim market As Market = CType(UniqueIDObject.Resolve(inBuildingID), Market)

            'If market Is Nothing Then
            '    market = market
            'End If

            market.AddResourceTransport(inResource)
        End Sub

        Public Sub AddMarketTransport(ByVal inBuilding As Market, ByVal inResource As Resource)
            If IsInitialized Then
                Synchronize(Sub() AddMarketTransportInternal(inBuilding.UniqueID, inResource))
            Else
                AddMarketTransportInternal(inBuilding.UniqueID, inResource)
            End If
        End Sub

        Friend Sub RemoveMarketTransportInternal(ByVal inBuildingID As Long, ByVal inResource As Resource)
            m_LogFile.RemoveMarketTransport(CurrentCycle, inBuildingID, inResource)
            TryCast(UniqueIDObject.Resolve(inBuildingID), Market).RemoveResourceTransport(inResource)
        End Sub

        Public Sub RemoveMarketTransport(ByVal inBuilding As Market, ByVal inResource As Resource)
            If IsInitialized Then
                Synchronize(Sub() RemoveMarketTransportInternal(inBuilding.UniqueID, inResource))
            Else
                RemoveMarketTransportInternal(inBuilding.UniqueID, inResource)
            End If
        End Sub

        Friend Sub ChangeToolSettingInternal(ByVal inTool As Resource, ByVal inNewTodo As Integer, ByVal inNewPercentage As Double)
            m_LogFile.ChangeToolSetting(CurrentCycle, inTool, inNewTodo, inNewPercentage)
            Configuration.ChangeToolSetting(inTool, inNewTodo, inNewPercentage)
        End Sub

        Public Sub ChangeToolSetting(ByVal inTool As Resource, ByVal inNewTodo As Integer, ByVal inNewPercentage As Double)
            If IsInitialized Then
                Synchronize(Sub() ChangeToolSettingInternal(inTool, inNewTodo, inNewPercentage))
            Else
                ChangeToolSettingInternal(inTool, inNewTodo, inNewPercentage)
            End If
        End Sub

        Friend Sub ChangeDistributionSettingInternal(ByVal inBuildingClass As String, ByVal inResource As Resource, ByVal inIsEnabled As Boolean)
            m_LogFile.ChangeDistributionSetting(CurrentCycle, inBuildingClass, inResource, inIsEnabled)
            Configuration.ChangeDistributionSetting(inBuildingClass, inResource, inIsEnabled)
        End Sub

        Public Sub ChangeDistributionSetting(ByVal inBuildingClass As String, ByVal inResource As Resource, ByVal inIsEnabled As Boolean)
            If IsInitialized Then
                Synchronize(Sub() ChangeDistributionSettingInternal(inBuildingClass, inResource, inIsEnabled))
            Else
                ChangeDistributionSettingInternal(inBuildingClass, inResource, inIsEnabled)
            End If
        End Sub

        Friend Sub ChangeProfessionInternal(ByVal inProfession As MigrantProfessions, ByVal inDelta As Integer)
            m_LogFile.ChangeProfession(CurrentCycle, inProfession, inDelta)

            Dim tool? As Resource = Nothing
            Dim movType As MovableType = 0

            Select Case inProfession
                Case MigrantProfessions.Constructor
                    tool = Resource.Hammer
                    movType = MovableType.Constructor
                Case MigrantProfessions.Grader
                    tool = Resource.Shovel
                    movType = MovableType.Grader

                    'Case MigrantProfessions.Geologist
                    '    tool = Resource.Coal
                    '    movType = MovableType.Migrant

                Case Else
                    Throw New ArgumentException("Profession has not yet been implemented.")
            End Select

            If Convert.ToBoolean(inDelta > 0) Then
                For i As Integer = 0 To inDelta - 1
                    If tool.HasValue Then
                        ' find tool 
                        Dim toolProv As GenericResourceStack = ResourceManager.FindResourceAround(New Point(0, 0), tool.Value, StackType.Provider)
                        If toolProv Is Nothing Then
                            Return
                        End If

                        ' find free Migrant
                        Dim Migrant As Movable = MovableManager.FindFreeMovableAround(New Point(0, 0), MovableType.Migrant)
                        If Migrant Is Nothing Then
                            Return
                        End If

                        Migrant.Job = New JobRecruiting(Migrant, toolProv, inProfession)
                    Else
                        ' find free Migrant
                        Dim Migrant As Movable = MovableManager.FindFreeMovableAround(New Point(0, 0), MovableType.Migrant)
                        If Migrant Is Nothing Then
                            Return
                        End If

                        ' directly transform movable
                        JobRecruiting.TransformMovable(Migrant, inProfession)
                    End If
                Next i
            Else
                For i As Integer = 0 To Math.Abs(inDelta) - 1
                    ' find Migrants with given profession
                    Dim Migrant As Movable = MovableManager.FindFreeMovableAround(New Point(0, 0), movType)
                    If Migrant Is Nothing Then
                        Return
                    End If

                    If tool.HasValue Then
                        ' drop tool at Migrant position and transform him back
                        ResourceManager.DropResource(Migrant.Position.ToPoint(), tool.Value, 1)

                        Migrant.MovableType = MovableType.Migrant
                        Dim inRestart As Boolean = False
                        Dim inRepeat As Boolean = True
                        VisualUtilities.Animate(Migrant, "MigrantWalking", inRestart, inRepeat)
                    End If
                Next i
            End If
        End Sub

        Public Sub ChangeProfession(ByVal inProfession As MigrantProfessions, ByVal inDelta As Integer)
            If IsInitialized Then
                Synchronize(Sub() ChangeProfessionInternal(inProfession, inDelta))
            Else
                ChangeProfessionInternal(inProfession, inDelta)
            End If
        End Sub

        Friend Sub RemoveBuildTaskInternal(ByVal inTaskID As Long)
            m_LogFile.RemoveBuildTask(CurrentCycle, inTaskID)
            BuildingManager.RemoveBuildTask((TryCast(UniqueIDObject.Resolve(inTaskID), BuildTask)))
        End Sub

        Public Sub RemoveBuildTask(ByVal inTask As BuildTask)
            If IsInitialized Then
                Synchronize(Sub() RemoveBuildTaskInternal(inTask.UniqueID))
            Else
                RemoveBuildTaskInternal(inTask.UniqueID)
            End If
        End Sub

        Friend Sub SetWorkingAreaInternal(ByVal inBuildingID As Long, ByVal inWorkingCenter As Point)
            m_LogFile.SetWorkingArea(CurrentCycle, inBuildingID, inWorkingCenter)
            TryCast(UniqueIDObject.Resolve(inBuildingID), BaseBuilding).WorkingArea = inWorkingCenter
        End Sub

        Public Sub SetWorkingArea(ByVal inBuilding As BaseBuilding, ByVal inWorkingCenter As Point)
            If IsInitialized Then
                Synchronize(Sub() SetWorkingAreaInternal(inBuilding.UniqueID, inWorkingCenter))
            Else
                SetWorkingAreaInternal(inBuilding.UniqueID, inWorkingCenter)
            End If
        End Sub

        Friend Sub RemoveBuildingInternal(ByVal inBuildingID As Long)
            m_LogFile.RemoveBuilding(CurrentCycle, inBuildingID)
            BuildingManager.RemoveBuilding(TryCast(UniqueIDObject.Resolve(inBuildingID), BaseBuilding))
        End Sub

        Public Sub RemoveBuilding(ByVal inBuilding As BaseBuilding)
            If IsInitialized Then
                Synchronize(Sub() RemoveBuildingInternal(inBuilding.UniqueID))
            Else
                RemoveBuildingInternal(inBuilding.UniqueID)
            End If
        End Sub

        Friend Sub RaiseBuildingPriorityInternal(ByVal inBuildingID As Long)
            m_LogFile.RaiseBuildingPriority(CurrentCycle, inBuildingID)
            TryCast(UniqueIDObject.Resolve(inBuildingID), BaseBuilding).RaisePriority()
        End Sub

        Public Sub RaiseBuildingPriority(ByVal inBuilding As BaseBuilding)
            If IsInitialized Then
                Synchronize(Sub() RaiseBuildingPriorityInternal(inBuilding.UniqueID))
            Else
                RaiseBuildingPriorityInternal(inBuilding.UniqueID)
            End If
        End Sub

        Friend Sub LowerBuildingPriorityInternal(ByVal inBuildingID As Long)
            m_LogFile.LowerBuildingPriority(CurrentCycle, inBuildingID)
            TryCast(UniqueIDObject.Resolve(inBuildingID), BaseBuilding).LowerPriority()
        End Sub

        Public Sub LowerBuildingPriority(ByVal inBuilding As BaseBuilding)
            If IsInitialized Then
                Synchronize(Sub() LowerBuildingPriorityInternal(inBuilding.UniqueID))
            Else
                LowerBuildingPriorityInternal(inBuilding.UniqueID)
            End If
        End Sub

        Friend Sub SetBuildingSuspendedInternal(ByVal inBuildingID As Long, ByVal isSuspended As Boolean)
            m_LogFile.SetBuildingSuspended(CurrentCycle, inBuildingID, isSuspended)
            TryCast(UniqueIDObject.Resolve(inBuildingID), BaseBuilding).IsSuspended = isSuspended
        End Sub

        Public Sub SetBuildingSuspended(ByVal inBuilding As BaseBuilding, ByVal isSuspended As Boolean)
            If IsInitialized Then
                Synchronize(Sub() SetBuildingSuspendedInternal(inBuilding.UniqueID, isSuspended))
            Else
                SetBuildingSuspendedInternal(inBuilding.UniqueID, isSuspended)
            End If
        End Sub

        Friend Sub RaiseTaskPriorityInternal(ByVal inTaskID As Long)
            m_LogFile.RaiseTaskPriority(CurrentCycle, inTaskID)
            BuildingManager.RaiseTaskPriority((TryCast(UniqueIDObject.Resolve(inTaskID), BuildTask)))
        End Sub

        Public Sub RaiseTaskPriority(ByVal inTask As BuildTask)
            If IsInitialized Then
                Synchronize(Sub() RaiseTaskPriorityInternal(inTask.UniqueID))
            Else
                RaiseTaskPriorityInternal(inTask.UniqueID)
            End If
        End Sub

        Friend Sub SetTaskSuspendedInternal(ByVal inTaskID As Long, ByVal isSuspended As Boolean)
            m_LogFile.SetTaskSuspended(CurrentCycle, inTaskID, isSuspended)
            TryCast(UniqueIDObject.Resolve(inTaskID), BuildTask).IsSuspended = isSuspended
        End Sub

        Public Sub SetTaskSuspended(ByVal inTask As BuildTask, ByVal isSuspended As Boolean)
            If IsInitialized Then
                Synchronize(Sub() SetTaskSuspendedInternal(inTask.UniqueID, isSuspended))
            Else
                SetTaskSuspendedInternal(inTask.UniqueID, isSuspended)
            End If
        End Sub

        Friend Sub AddOneCycle()
            If Not IsInitialized Then
                Throw New InvalidOperationException()
            End If

            MovableManager.CurrentCycle += 1

            BuildingManager.ProcessCycle()
            ResourceManager.ProcessCycle()
            MovableManager.ProcessCycle()

            If m_UpdateConfig Then
                m_UpdateConfig = False

                Configuration.Update()
            End If
        End Sub

        ''' <summary>
        ''' This is where updates take place and the game is saved
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub SimulationLoop()
            Try
                Dim watch As New Stopwatch()
                Dim lastElapsed As Long = 0
                Dim elapsedShift As Long = 0
                Dim cycleMillis As Long = 0

                If System.IO.File.Exists("GameLog.s3g") Then
                    Try
                        ' load game...
                        Dim cycle As Long = 0
                        Dim state As MapFile = Nothing

                        state = MapFile.OpenRead("GameLog.s3g")
                        Using state

                            Do While cycle < state.NextCycle
                                AnimationTime += Convert.ToInt64((CyclePoint.CYCLE_MILLIS))
                                AddOneCycle()
                                cycle += 1
                            Loop

                            Do While state.NextCycle >= 0
                                Dim thisCycle As Long = state.NextCycle

                                Do While state.NextCycle = thisCycle
                                    state.ReadNext(Me)
                                Loop

                                Do While cycle < state.NextCycle
                                    AnimationTime += Convert.ToInt64((CyclePoint.CYCLE_MILLIS))
                                    AddOneCycle()
                                    cycle += 1
                                Loop
                            Loop
                        End Using

                        cycleMillis = Convert.ToInt64((CyclePoint.CYCLE_MILLIS * CurrentCycle))
                        AnimationTime = cycleMillis
                        elapsedShift = cycleMillis
                        lastElapsed = cycleMillis
                    Catch e As Exception
                        Log.LogExceptionCritical(e)
                    End Try
                End If

                RaiseEvent OnGameStarted(Me)

                watch.Start()

                Do
                    Dim elapsed As Long = watch.ElapsedMilliseconds + elapsedShift

                    Try
                        If m_ForwardOneMinute Then
                            elapsedShift += Convert.ToInt64((TimeSpan.FromMinutes(1).TotalMilliseconds))
                            m_ForwardOneMinute = False
                        End If

                        If elapsed - lastElapsed < 10 Then
                            System.Threading.Thread.Sleep(10)

                            Continue Do
                        End If

                        Do While cycleMillis < elapsed
                            AnimationTime += Convert.ToInt64((CyclePoint.CYCLE_MILLIS))
                            AddOneCycle()

                            If IsSuspended Then
                                Dim backup As Long = watch.ElapsedMilliseconds

                                m_SuspendSignal.Set()
                                m_ResumeSignal.WaitOne()

                                elapsedShift -= watch.ElapsedMilliseconds - backup
                            End If
                            cycleMillis += Convert.ToInt32(CInt(Fix(CyclePoint.CYCLE_MILLIS)))
                        Loop
                    Finally
                        lastElapsed = elapsed
                    End Try
                Loop While True
            Catch e As Exception
                Log.LogExceptionCritical(e)
            End Try
        End Sub

        Private Sub Movable_OnStop(ByVal inMovable As Movable)

            If inMovable.Carrying.HasValue Then
                DropResource(inMovable.Position.ToPoint(), inMovable.Carrying.Value, 1)

                inMovable.Carrying = Nothing
            End If
        End Sub

        Public Sub SuspendGame()
            If IsSuspended Then
                Return
            End If

            m_SuspendSignal.Reset()
            m_ResumeSignal.Reset()

            IsSuspended = True

            ' wait for signal
            m_SuspendSignal.WaitOne()
        End Sub

        Public Sub ResumeGame()
            IsSuspended = False

            m_ResumeSignal.Set()
        End Sub
    End Class
End Namespace
