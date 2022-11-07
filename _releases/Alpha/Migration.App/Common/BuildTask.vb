Imports Migration.Buildings
Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core
Imports Migration.Jobs

Namespace Migration.Common

    Public Class BuildTask
        Private Shared ReadOnly m_Random As New CrossRandom(0)
        Private Shared ReadOnly m_Brush As TerrainBrush = TerrainBrush.CreateSphereBrush(3, 3)
        Private Const StepsPerTimber As Integer = 20
        Private Const StepsPerStone As Integer = 30

        Private m_StoneQuery As GenericResourceStack
        Private m_TimberQuery As GenericResourceStack
        Private m_Graders As New List(Of Movable)()
        Private m_GradingInfo As GradingInfo
        Private m_RemainingGradingSteps As Integer
        Private m_InitialGradingSteps As Integer
        Private m_RemainingConstructorSteps As Integer
        Private m_InitialConstructorSteps As Integer
        Private m_ConstructorSpots() As ConstructorSpot
        Private m_ConstructorCount As Integer = 0
        Private m_LastConstructorSteps As Integer
        Private m_PrevState As BuildStates = BuildStates.Started
        Private m_Progress As Double = 0
        Private ReadOnly m_Unique As UniqueIDObject

        Private privateUsedTimberCount As Integer
        Friend Property UsedTimberCount() As Integer
            Get
                Return privateUsedTimberCount
            End Get
            Private Set(ByVal value As Integer)
                privateUsedTimberCount = value
            End Set
        End Property
        Private privateUsedStoneCount As Integer
        Friend Property UsedStoneCount() As Integer
            Get
                Return privateUsedStoneCount
            End Get
            Private Set(ByVal value As Integer)
                privateUsedStoneCount = value
            End Set
        End Property

        Friend ReadOnly Property UniqueID() As Long
            Get
                Return m_Unique.UniqueID
            End Get
        End Property
        Private privateAreGradersMarkedForRelease As Boolean
        Friend Property AreGradersMarkedForRelease() As Boolean
            Get
                Return privateAreGradersMarkedForRelease
            End Get
            Set(ByVal value As Boolean)
                privateAreGradersMarkedForRelease = value
            End Set
        End Property
        Private privateAreConstructorsMarkedForRelease As Boolean
        Friend Property AreConstructorsMarkedForRelease() As Boolean
            Get
                Return privateAreConstructorsMarkedForRelease
            End Get
            Set(ByVal value As Boolean)
                privateAreConstructorsMarkedForRelease = value
            End Set
        End Property
        Private privateQueries As IEnumerable(Of GenericResourceStack)
        Public Property Queries() As IEnumerable(Of GenericResourceStack)
            Get
                Return privateQueries
            End Get
            Private Set(ByVal value As IEnumerable(Of GenericResourceStack))
                privateQueries = value
            End Set
        End Property
        Private privateUserContext As Object
        Public Property UserContext() As Object
            Get
                Return privateUserContext
            End Get
            Set(ByVal value As Object)
                privateUserContext = value
            End Set
        End Property
        Private privateNode As LinkedListNode(Of BuildTask)
        Friend Property Node() As LinkedListNode(Of BuildTask)
            Get
                Return privateNode
            End Get
            Set(ByVal value As LinkedListNode(Of BuildTask))
                privateNode = value
            End Set
        End Property
        Private privateBuilding As BaseBuilding
        Public Property Building() As BaseBuilding
            Get
                Return privateBuilding
            End Get
            Private Set(ByVal value As BaseBuilding)
                privateBuilding = value
            End Set
        End Property
        Public ReadOnly Property Terrain() As TerrainDefinition
            Get
                Return Building.Parent.Terrain
            End Get
        End Property
        Public ReadOnly Property Configuration() As BuildingConfiguration
            Get
                Return Building.Config
            End Get
        End Property
        Private privateManager As BuildingManager
        Friend Property Manager() As BuildingManager
            Get
                Return privateManager
            End Get
            Private Set(ByVal value As BuildingManager)
                privateManager = value
            End Set
        End Property

        Friend ReadOnly Property ResourceManager() As ResourceManager
            Get
                Return Manager.ResourceManager
            End Get
        End Property

        Friend ReadOnly Property MoveManager() As MovableManager
            Get
                Return Manager.MoveManager
            End Get
        End Property

        Public Property Progress() As Double
            Get
                Return m_Progress
            End Get
            Private Set(ByVal value As Double)
                If m_Progress = value Then
                    Return
                End If

                m_Progress = value

                RaiseEvent OnProgressChanged(Me, value)
            End Set
        End Property
        Public ReadOnly Property IsGraded() As Boolean
            Get
                Return m_RemainingGradingSteps <= 0
            End Get
        End Property
        Public ReadOnly Property IsBuilt() As Boolean
            Get
                Return m_RemainingConstructorSteps <= 0
            End Get
        End Property
        Private privateHasWorker As Boolean
        Public Property HasWorker() As Boolean
            Get
                Return privateHasWorker
            End Get
            Private Set(ByVal value As Boolean)
                privateHasWorker = value
            End Set
        End Property
        Private privateMaximumGraders As Integer
        Public Property MaximumGraders() As Integer
            Get
                Return privateMaximumGraders
            End Get
            Private Set(ByVal value As Integer)
                privateMaximumGraders = value
            End Set
        End Property
        Private privateIsCompleted As Boolean
        Public Property IsCompleted() As Boolean
            Get
                Return privateIsCompleted
            End Get
            Private Set(ByVal value As Boolean)
                privateIsCompleted = value
            End Set
        End Property
        Private privateHasGrader As Boolean
        Public Property HasGrader() As Boolean
            Get
                Return privateHasGrader
            End Get
            Private Set(ByVal value As Boolean)
                privateHasGrader = value
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
        Public ReadOnly Property State() As BuildStates
            Get
                If Not IsGraded Then
                    Return BuildStates.Started
                End If
                If Not IsBuilt Then
                    Return BuildStates.Graded
                End If
                If Not IsCompleted Then
                    Return BuildStates.Built
                End If
                Return BuildStates.Completed
            End Get
        End Property

        Public Event OnStateChanged As Procedure(Of BuildTask)
        Public Event OnProgressChanged As Procedure(Of BuildTask, Double)

        Public Sub Dispose()
            If m_TimberQuery IsNot Nothing Then
                Manager.ResourceManager.RemoveResource(m_TimberQuery)
                m_TimberQuery = Nothing
            End If

            If m_StoneQuery IsNot Nothing Then
                Manager.ResourceManager.RemoveResource(m_StoneQuery)
                m_StoneQuery = Nothing
            End If

            ReleaseConstructors()
            ReleaseGraders()
        End Sub

        Friend Sub New(ByVal inManager As BuildingManager, ByVal inBuilding As BaseBuilding)
            If (inBuilding Is Nothing) OrElse (inManager Is Nothing) Then
                Throw New ArgumentNullException()
            End If

            m_Unique = New UniqueIDObject(Me)
            Manager = inManager
            Building = inBuilding
            Queries = New GenericResourceStack() {}
            MaximumGraders = 5
            m_GradingInfo = Terrain.GetGradingInfo(Building.Position.XGrid, Building.Position.YGrid, Configuration)
            m_RemainingGradingSteps = m_GradingInfo.Variance \ m_Brush.Variance
            m_InitialGradingSteps = m_RemainingGradingSteps
            m_RemainingConstructorSteps = (Configuration.WoodCount * StepsPerTimber + Configuration.StoneCount * StepsPerStone)
            m_InitialConstructorSteps = m_RemainingConstructorSteps

            m_LastConstructorSteps = m_InitialConstructorSteps + Math.Min(StepsPerTimber, StepsPerStone) \ 2

            ' prepare constructor spots
            Dim constSpots As New List(Of ConstructorSpot)()

            For Each pos As Point In Configuration.ConstructorSpots
                constSpots.Add(New ConstructorSpot() With {.Position = New Point(pos.X + Building.Position.XGrid, pos.Y + Building.Position.YGrid)})
            Next pos

            ' compute constructor directions
            Dim maxY As Integer = constSpots.Max(Function(e) e.Position.Y)
            Dim maxYSpot As ConstructorSpot = constSpots.First(Function(e) e.Position.Y = maxY)
            Dim iSpot As Integer = 0

            Do While iSpot < constSpots.Count
                If maxYSpot Is constSpots(iSpot) Then
                    Exit Do
                End If

                constSpots(iSpot).Direction = Direction._045
                iSpot += 1
            Loop

            Do While iSpot < constSpots.Count
                constSpots(iSpot).Direction = Direction._315
                iSpot += 1
            Loop

            m_ConstructorSpots = constSpots.ToArray()

            ' mines are not graded
            If Building.Config.ClassType Is GetType(Mine) Then
                m_RemainingGradingSteps = 0

                AddResourceQueries()
            End If
        End Sub

        Private Sub AddResourceQueries()
            m_TimberQuery = Manager.ResourceManager.AddResourceStack(CyclePoint.FromGrid(Building.Position.XGrid + Configuration.TimberSpot.X, Building.Position.YGrid + Configuration.TimberSpot.Y), StackType.Query, Resource.Timber, Math.Min(8, Configuration.WoodCount))

            m_StoneQuery = Manager.ResourceManager.AddResourceStack(CyclePoint.FromGrid(Building.Position.XGrid + Configuration.StoneSpot.X, Building.Position.YGrid + Configuration.StoneSpot.Y), StackType.Query, Resource.Stone, Math.Min(8, Configuration.StoneCount))

            AddHandler m_TimberQuery.OnCountChanged, AddressOf Query_OnCountChanged
            AddHandler m_StoneQuery.OnCountChanged, AddressOf Query_OnCountChanged

            Queries = New GenericResourceStack() {m_StoneQuery, m_TimberQuery}
        End Sub

        Private Sub Query_OnCountChanged(ByVal inSender As GenericResourceStack, ByVal inOldValue As Integer, ByVal inNewValue As Integer)
            If inSender.Resource = Resource.Timber Then
                inSender.MaxCount = Math.Min(8, Configuration.WoodCount - UsedTimberCount)
            Else
                inSender.MaxCount = Math.Min(8, Configuration.StoneCount - UsedStoneCount)
            End If
        End Sub


        Friend Sub ReleaseConstructors()
            m_ConstructorCount = 0

            For Each spot As ConstructorSpot In m_ConstructorSpots
                If spot.Constructor Is Nothing Then
                    Continue For
                End If

                spot.Constructor.Job = Nothing
                spot.Constructor.Stop()
                spot.Constructor = Nothing
            Next spot

            AreConstructorsMarkedForRelease = False
        End Sub

        Friend Sub ReleaseGraders()
            For Each grader As Movable In m_Graders
                grader.Job = Nothing
                grader.Stop()
            Next grader

            m_Graders.Clear()

            AreGradersMarkedForRelease = False
        End Sub

        Friend Sub Update()
            If Not IsGraded Then
                ' update progress
                Progress = Math.Max(0, Math.Min(1.0, 1.0 - (m_RemainingGradingSteps / Convert.ToDouble(m_InitialGradingSteps))))

                If IsSuspended Then
                    Return
                End If

                ' look for graders if necessary
                Do While MaximumGraders - m_Graders.Count > 0
                    Dim grader As Movable = MoveManager.FindFreeMovableAround(Building.Position.ToPoint(), MovableType.Grader)

                    If grader Is Nothing Then
                        Exit Do
                    End If

                    If Not HasGrader Then
                        HasGrader = True

                        AddResourceQueries()
                    End If

                    m_Graders.Add(grader)

                    grader.Job = New JobGrading(grader, Me)
                Loop
            ElseIf Not IsBuilt Then
                AreGradersMarkedForRelease = True

                ' update resource queries
                Dim hasResources As Boolean = (m_TimberQuery.Available > 0) OrElse (m_StoneQuery.Available > 0)
                Dim stepDiff As Integer = m_LastConstructorSteps - m_RemainingConstructorSteps

                If m_TimberQuery.Available > 0 Then
                    If stepDiff > StepsPerTimber Then
                        m_TimberQuery.RemoveResource()
                        m_LastConstructorSteps -= StepsPerTimber
                        UsedTimberCount += 1

                        m_TimberQuery.MaxCount = Math.Min(8, Configuration.WoodCount - UsedTimberCount)
                    End If
                ElseIf m_StoneQuery.Available > 0 Then
                    If stepDiff > StepsPerStone Then
                        m_StoneQuery.RemoveResource()
                        m_LastConstructorSteps -= StepsPerStone
                        UsedStoneCount += 1

                        m_StoneQuery.MaxCount = Math.Min(8, Configuration.StoneCount - UsedStoneCount)
                    End If
                End If

                If hasResources AndAlso (Not IsSuspended) Then
                    ' look for constructors if necessary 
                    Do While m_ConstructorSpots.Length - m_ConstructorCount > 0
                        Dim constructor As Movable = MoveManager.FindFreeMovableAround(Building.Position.ToPoint(), MovableType.Constructor)
                        Dim freeSpot As ConstructorSpot = m_ConstructorSpots.First(Function(e) e.Constructor Is Nothing)

                        If constructor Is Nothing Then
                            Exit Do
                        End If

                        m_ConstructorCount += 1

                        freeSpot.Constructor = constructor

                        If constructor.Job IsNot Nothing Then
                            Throw New ApplicationException()
                        End If

                        constructor.Job = New JobConstructing(constructor, Me, freeSpot.Position, freeSpot.Direction)
                    Loop
                Else
                    AreConstructorsMarkedForRelease = True
                End If

                If (UsedTimberCount >= Configuration.WoodCount) AndAlso (UsedStoneCount >= Configuration.StoneCount) Then
                    m_RemainingConstructorSteps = 0
                End If

                ' update progress
                Progress = Math.Max(0, Math.Min(1.0, 1.0 - (m_RemainingConstructorSteps / Convert.ToDouble(m_InitialConstructorSteps))))
            ElseIf Not HasWorker Then
                AreConstructorsMarkedForRelease = True

                Progress = 1.0

                If IsSuspended Then
                    Return
                End If

                If Configuration.Worker = BuildingWorkerType.None Then
                    IsCompleted = True
                    HasWorker = True

                    Building.MarkAsReady()

                    Return
                End If

                Migrant = Nothing
                Dim job As JobOnce = Nothing

                If Configuration.Worker <> BuildingWorkerType.Migrant Then
                    ' find tool around building
                    Dim resource As GenericResourceStack = ResourceManager.FindResourceAround(Building.Position.ToPoint(), Configuration.WorkerTool, StackType.Provider)

                    If resource IsNot Nothing Then
                        ' find free Migrant around tool and recruit him
                        Migrant = MoveManager.FindFreeMovableAround(resource.Position.ToPoint(), MovableType.Migrant)

                        If Migrant IsNot Nothing Then
                            job = New JobWorkerRecruiting(Migrant, resource, Building)
                            Migrant.Job = job

                            HasWorker = True
                        End If
                    End If
                Else
                    Migrant = MoveManager.FindFreeMovableAround(Building.Position.ToPoint(), MovableType.Migrant)

                    If Migrant IsNot Nothing Then
                        job = New JobWorkerAssignment(Migrant, Me.Building)
                        Migrant.Job = job

                        HasWorker = True
                    End If
                End If

                If job IsNot Nothing Then
                    AddHandler job.OnCompleted, AddressOf JobCompleted
                End If
            End If

            RaiseEvent OnStateChanged(Me)

            m_PrevState = State
        End Sub

        Private Migrant As Movable = Nothing
        Private Sub JobCompleted(ByVal unused As JobOnce, ByVal succeeded As Boolean)
            If Not succeeded Then
                Migrant.Job = Nothing

                Return
            End If

            Building.MarkAsReady()

            MoveManager.MarkMovableForRemoval(Migrant)
            IsCompleted = True

        End Sub

        Friend Function DoConstruct() As Boolean
            m_RemainingConstructorSteps -= 1

            Return Not IsBuilt
        End Function

        Friend Sub DoGrade(ByVal inCell As Point)
            m_RemainingGradingSteps -= 1

            Terrain.SetFlagsAt(inCell.X, inCell.Y, TerrainCellFlags.Grading)
            Terrain.AverageDrawToHeightmap(inCell.X, inCell.Y, m_GradingInfo.AvgHeight, m_Brush)
        End Sub
    End Class
End Namespace
