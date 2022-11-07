Imports Migration.Common
Imports Migration.Jobs
Imports Migration.Core

Namespace Migration

    ''' <summary>
    ''' The movable manager is the internal interface to pathfinding. Every movable is added here and 
    ''' the only (intended) way to change its position is by using SetPath. The most
    ''' important thing everyone should keep in mind about this path engine: It is deterministic in
    ''' the strongest way, meaning it is 100% deterministic within a discrete space-time. This is a very
    ''' important foundation for later network synchronization and saving games.
    ''' </summary>
    Friend Class MovableManager
        Inherits SynchronizedManager

        Friend Shared ReadOnly LOG_2_FACTOR As Double = 1.0 / Math.Log(2.0)

        Private ReadOnly m_MarkedForRemoval As New List(Of Movable)()
        Private m_Movables As TopologicalList(Of Movable)

        Friend ReadOnly Property Terrain() As TerrainDefinition
            Get
                Return Map.Terrain
            End Get
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

        Friend ReadOnly Property AvgPlanMillis() As Long
            Get
                Return 0 'm_AStar.AvgPlanMillis
            End Get
        End Property

        Friend ReadOnly Property MaxPlanMillis() As Long
            Get
                Return 0 'm_AStar.MaxPlanMilli
            End Get
        End Property
        ''' <summary>
        ''' Width and/or Height in TerrainCells. For performance reasons width and height will always
        ''' have the same value and must be a power of two. But don't rely on that outside the path engine.
        ''' </summary>
        Private privateSize As Int32
        Friend Property Size() As Int32
            Get
                Return privateSize
            End Get
            Private Set(ByVal value As Int32)
                privateSize = value
            End Set
        End Property

        ''' <summary>
        ''' Will be called once for every added movable.
        ''' </summary>
        Friend Event OnAddMovable As DOnAddMovable(Of MovableManager)
        ''' <summary>
        ''' Will be called once for every removed movable.
        ''' </summary>
        Friend Event OnRemoveMovable As DOnRemoveMovable(Of MovableManager)

        ''' <summary>
        ''' Can a movable of the given type walk on given cell?
        ''' </summary>
        ''' <param name="inCell">Starting at (0,0) the grid position on <see cref="Terrain"/> in TerrainCells.</param>
        ''' <param name="inMovableType">An integer movable type.</param>
        ''' <returns></returns>
        Friend Function IsWalkable(ByVal inCell As Point, ByVal inMovableType As MovableType) As Boolean
            Return Convert.ToInt32(CInt(Terrain.GetWallAt(inCell.X, inCell.Y))) <= Convert.ToInt32(CInt(inMovableType))
        End Function

        ''' <summary>
        ''' Creates a new instance.
        ''' </summary>
        ''' <param name="inMap">Desired terrain grid width (height) in TerrainCells. Must be a power of two.</param>
        ''' <param name="inCurrentCycle">An initial value for <see cref="CurrentCycle"/>.</param>
        ''' <param name="inCycleResolution">Desired value for <see cref="CycleResolution"/>.</param>
        Friend Sub New(ByVal inMap As Migration.Game.Map, ByVal inCurrentCycle As Int64, ByVal inCycleResolution As Int64)
            MyBase.New(inCurrentCycle, inCycleResolution)
            Size = inMap.Size
            Map = inMap
            m_Movables = New TopologicalList(Of Movable)(10, Size, Size)
        End Sub

        ''' <summary>
        ''' The only place where new paths are acquired. 
        ''' </summary>
        Private Sub AcquirePath(ByVal inMovable As Movable)
            If Not IsAlignedCycle Then
                Throw New ApplicationException("Path can not be acquired in current cycle.")
            End If

            If (inMovable.Position.XCycles Mod Convert.ToInt32(CInt(Fix(CyclePoint.CYCLE_MILLIS)))) <> 0 Then
                Throw New ApplicationException("XCycles is not a multiple of node time.")
            End If

            If (inMovable.Position.YCycles Mod Convert.ToInt32(CInt(Fix(CyclePoint.CYCLE_MILLIS)))) <> 0 Then
                Throw New ApplicationException("YCycles is not a multiple of node time.")
            End If

            ' acquire new path
            'if (inMovable.HasJob)
            Map.Routing.SetDynamicPath(inMovable, inMovable.PathTarget, inMovable.MovableType)
            '            else
            '            {
            '                m_AStar.SetIdlePath(
            '                    inMovable,
            '                    0);
            '            }
        End Sub

        Friend Overrides Sub ProcessCycle()
            MyBase.ProcessCycle()

            Map.Routing.AddOneCycle()

            If IsAlignedCycle Then
                SyncLock Movable.GlobalLock
                    m_Movables.ForEach(AddressOf UpdateJob)

                    m_Movables.ForEach(AddressOf SetPath)


                    For Each m_movable As Movable In m_MarkedForRemoval
                        Map.Routing.ClearPath(m_movable)

                        m_Movables.Remove(m_movable)

                        m_movable.CurrentNode = Nothing
                        m_movable.Parent = Nothing
                    Next m_movable

                    m_MarkedForRemoval.Clear()
                End SyncLock
            End If
        End Sub

        Private Function UpdateJob(ByVal m_movable As Movable) As Boolean
            If m_movable.Job Is Nothing Then
                Return True
            End If
            m_movable.Job.Update()
            Return True
        End Function

        Private Function SetPath(ByVal m_movable As Movable) As Boolean
            If ((Not m_movable.IsInvalidated)) AndAlso (m_movable.ReplanTime >= CurrentCycle) Then
                Dim currentNode As MovablePathNode = m_movable.CurrentNode.Value
                Dim mNext As LinkedListNode(Of MovablePathNode) = m_movable.CurrentNode.Next

                If mNext IsNot Nothing Then
                    If mNext.Value.Time <= CurrentCycle Then
                        Dim newPos As CyclePoint = CyclePoint.FromGrid(mNext.Value.Position.X, mNext.Value.Position.Y)
                        Dim backupPos As CyclePoint = m_movable.Position

                        m_movable.SetPosition_YouMustNotDoThis(newPos) ' this is one of the rare exceptions, where we just have to do it...
                        m_movable.CurrentNode = mNext

                        If newPos.ToPoint() = m_movable.PathTarget Then
                            Dim handler As Procedure(Of Boolean) = m_movable.ResultHandler
                            m_movable.ResultHandler = Nothing

                            If handler IsNot Nothing Then
                                handler(True)
                            End If

                        End If
                    End If
                End If
            End If

            If m_movable.IsInvalidated OrElse (m_movable.ReplanTime <= CurrentCycle) Then
                AcquirePath(m_movable)
            End If

            Return True
        End Function

        ''' <summary>
        ''' Even though position interpolation does not really belong here, and instead should be
        ''' kept in a renderer or whatever, it requires a bunch of internal knowledge about path
        ''' planning if you want to implement it efficiently. And thus it is much cleaner to
        ''' put this method here instead of exposing the path planning internals to the public!
        ''' </summary>
        ''' <param name="inMovable"></param>
        ''' <param name="refDirection"></param>
        ''' <param name="outXYInterpolation"></param>
        ''' <param name="outZInterpolation"></param>
        Friend Sub InterpolateMovablePosition(ByVal inMovable As Movable, ByRef refDirection As Direction, ByRef outXYInterpolation As CyclePoint, ByRef outZInterpolation As Double)
            SyncLock Movable.GlobalLock
                outXYInterpolation = inMovable.Position

                If inMovable.CurrentNode IsNot Nothing Then
                    Dim currentNodeRef As LinkedListNode(Of MovablePathNode) = inMovable.CurrentNode
                    Dim currentNode As MovablePathNode = currentNodeRef.Value

                    If currentNode.Direction.HasValue Then
                        Dim nextNode As MovablePathNode = currentNodeRef.Next.Value
                        Dim nodeStartTime As Long = currentNode.Time
                        Dim nodeEndTime As Long = nextNode.Time
                        Dim nodeDuration As Integer = Convert.ToInt32(CInt(nodeEndTime - nodeStartTime))
                        Dim timeOffset As Integer = Convert.ToInt32(CInt(CurrentCycle - nodeStartTime))
                        Dim nextZ As Single = Terrain.GetZShiftAt(nextNode.Position.X, nextNode.Position.Y)
                        Dim currentZ As Single = Terrain.GetZShiftAt(currentNode.Position.X, currentNode.Position.Y)

                        If timeOffset >= nodeDuration Then
                            timeOffset = nodeDuration
                        End If


                        ' compute animation progress
                        Dim dirVec As Point = DirectionUtils.GetDirectionVector(currentNode.Direction.Value)

                        '                        
                        '                         * This is a point where I first made a big mistake and tried to set the movable position directly
                        '                         * to the interpolated value. This is also the reason for the "YouMustNotDoThis" suffix.
                        '                         * It will lead to race conditions (caused by multi-threading) which are extremly difficult to track down. 
                        '                         * Also the interpolation has just nothing to do with path planning but is only interesting
                        '                         * for the visualization so it should stay there...
                        '                         
                        refDirection = currentNode.Direction.Value
                        outXYInterpolation = CyclePoint.FromGrid(currentNode.Position.X, currentNode.Position.Y).AddCycleVector(New Point(dirVec.X * timeOffset, dirVec.Y * timeOffset))
                        outZInterpolation = currentZ + timeOffset * (nextZ - currentZ) / nodeDuration
                    Else
                        Dim pos As CyclePoint = inMovable.Position
                        outZInterpolation = Terrain.GetZShiftAt(pos.XGrid, pos.YGrid)

                    End If
                Else
                    Dim pos As CyclePoint = inMovable.Position
                    outZInterpolation = Terrain.GetZShiftAt(pos.XGrid, pos.YGrid)

                End If
            End SyncLock
        End Sub

        ''' <summary>
        ''' Enumerates movables around the given grid position with increasing distance on average (meaning
        ''' is is not strongly sorted by distance, for performance reasons).
        ''' </summary>
        ''' <param name="inAround"></param>
        ''' <param name="inHandler"></param>
        ''' <returns></returns>
        Friend Function EnumMovablesAround(ByVal inAround As Point, ByVal inHandler As Func(Of Movable, WalkResult)) As WalkResult
            Return m_Movables.EnumAround(inAround, Function(movable) inHandler(movable))
        End Function

        Friend Function FindFreeMovableAround(ByVal inAround As Point, ByVal inType As MovableType) As Movable
            _inType = inType

            If WalkResult.Success <> EnumMovablesAround(inAround, AddressOf GetWalkResult) Then
                Return Nothing
            End If

            Return result
        End Function

        Private result As Movable = Nothing
        Private _inType As MovableType

        Private Function GetWalkResult(ByVal m_movable As Movable) As WalkResult
            If (m_movable.MovableType <> _inType) OrElse m_movable.HasJob Then
                Return WalkResult.NotFound
            Else
                result = m_movable
                Return WalkResult.Success
            End If
        End Function

        ''' <summary>
        ''' It is not checked whether the movable
        ''' can be placed/walk where it is. If it can't walk or is to be placed on a non-movable object, it will 
        ''' immediately die. Allocated movables have to be released with ReleaseMovable.
        ''' On success, <see cref="OnAddMovable"/> will be raised.
        ''' </summary>
        Friend Function AddMovable(ByVal inPosition As CyclePoint, ByVal inMovable As MovableType) As Movable

            Dim m_movable As New Movable(inPosition, False) With {.MovableType = inMovable}

            m_Movables.Add(m_movable)

            m_movable.Parent = Me

            If IsAlignedCycle Then
                m_movable.Path.AddLast(New MovablePathNode() With {.Movable = m_movable, .Position = New Point(m_movable.Position.XGrid, m_movable.Position.YGrid), .Time = CurrentCycle})
            End If

            m_movable.Path.AddLast(New MovablePathNode() With {.Movable = m_movable, .Position = New Point(m_movable.Position.XGrid, m_movable.Position.YGrid), .Time = NextAlignedCycle})

            m_movable.CurrentNode = m_movable.Path.First

            RaiseEvent OnAddMovable(Me, m_movable)

            Return m_movable
        End Function

        ''' <summary>
        ''' Marks the given movable for removal. The manager may decide the specific point of removal on
        ''' his convenience. In contrast, the <see cref="OnRemoveMovable"/> event is called BEFORE this method
        ''' returns!
        ''' </summary>
        Friend Sub MarkMovableForRemoval(ByVal inMovable As Movable)
            If inMovable Is Nothing Then
                Return
            End If

            If inMovable.Parent IsNot Me Then
                Throw New InvalidOperationException("Movable is not attached to this path engine instance.")
            End If

            inMovable.Dispose()

            m_MarkedForRemoval.Add(inMovable)

            RaiseEvent OnRemoveMovable(Me, inMovable)

            ' must be executed after! remove event
            inMovable.Job = Nothing
        End Sub

        ''' <summary>
        ''' See <see cref="SetPath"/>.
        ''' </summary>
        Friend Sub SetPath(ByVal inMovable As Movable, ByVal inTarget As Point)
            SetPath(inMovable, inTarget, Nothing)
        End Sub

        ''' <summary>
        ''' Sets a path with an optional result handler. It is not guaranteed that a path can be
        ''' found, neither that it will be reached. What is guaranteed is that the result handler
        ''' is called in any case and only with "true" as parameter if the movable has reached its
        ''' destination. Further, a movable with a pending result handler is considered to be non-idle.
        ''' If you try to set a path for a movable with a pending result handler ("doing a job"), the 
        ''' call will immediately throw an exception!
        ''' </summary>
        ''' <param name="inMovable">Movable to plan a path for.</param>
        ''' <param name="inTarget">Destination for the movable.</param>
        ''' <param name="inResultHandler">An optional result handler.</param>
        ''' <exception cref="InvalidOperationException">The movable is already doing a job. Call Stop() first.</exception>
        Friend Sub SetPath(ByVal inMovable As Movable, ByVal inTarget As Point, ByVal inResultHandler As Procedure(Of Boolean))

            Dim m_movable As Movable = CType(inMovable, Movable)

            If (m_movable.ResultHandler IsNot Nothing) AndAlso ((Not m_movable.UserControlable)) Then
                Throw New InvalidOperationException("The movable is already doing a job.")
            End If

            If m_movable.PathTarget = inTarget Then
                If inResultHandler IsNot Nothing Then
                    inResultHandler(True)
                End If

                Return
            End If

            m_movable.ResultHandler = inResultHandler
#If DEBUG Then
            m_movable.PathStackTrace = New System.Diagnostics.StackTrace(True)
#End If

            ' update path information
            m_movable.PathTarget = inTarget
            m_movable.IsInvalidated = True
        End Sub

        ''' <summary>
        ''' See <see cref="SetPath"/>.
        ''' </summary>
        Friend Sub SetPath(ByVal inMovables As IEnumerable(Of Movable), ByVal inTarget As Point)
            SetPath(inMovables, inTarget, Nothing)
        End Sub

        ''' <summary>
        ''' See <see cref="SetPath"/>.
        ''' </summary>
        Friend Sub SetPath(ByVal inMovables As IEnumerable(Of Movable), ByVal inTarget As Point, ByVal inResultHandler As Procedure(Of Boolean))

            For Each m_movable As Movable In inMovables
                SetPath(m_movable, inTarget, inResultHandler)
            Next m_movable
        End Sub

        Friend Function CountIdleMigrants() As Integer
            Dim tempCountIdleMigrants As Integer = 0

            m_Movables.ForEach(AddressOf CountIdleMigrants)

            Return idleMigrantCount
        End Function

        Private idleMigrantCount As Integer

        Private Function CountIdleMigrants(ByVal m_movable As Movable) As Boolean
            Select Case m_movable.MovableType
                Case MovableType.Migrant
                    If Not m_movable.HasJob Then
                        idleMigrantCount += 1
                    End If
            End Select
            Return True
        End Function

        Friend Sub CountMovables(ByVal inBuffer() As Integer)
            _inBuffer = inBuffer
            Array.Clear(inBuffer, 0, _inBuffer.Length)

            m_Movables.ForEach(AddressOf CountMovables)

        End Sub

        Private _inBuffer() As Integer

        Private Function CountMovables(ByVal m_movable As Movable) As Boolean
            Select Case m_movable.MovableType
                Case MovableType.Grader
                    _inBuffer(Convert.ToInt32(MigrantStatisticTypes.Grader)) += 1

                Case MovableType.Constructor
                    _inBuffer(Convert.ToInt32(MigrantStatisticTypes.Constructor)) += 1

                Case MovableType.Migrant
                    If m_movable.HasJob Then
                        Dim job As JobBase = m_movable.Job
                        If job.GetType() Is GetType(JobCarrying) Then
                            _inBuffer(Convert.ToInt32(MigrantStatisticTypes.Migrant)) += 1

                        Else
                            _inBuffer(Convert.ToInt32(MigrantStatisticTypes.Worker)) += 1

                        End If
                    Else
                        _inBuffer(Convert.ToInt32(MigrantStatisticTypes.Migrant)) += 1
                    End If
                Case Else
                    Throw New NotImplementedException("Add counting for this type of movable!")
            End Select
            Return True
        End Function
    End Class
End Namespace
