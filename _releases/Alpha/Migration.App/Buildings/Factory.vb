Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core

Namespace Migration.Buildings
    Public MustInherit Class Factory
        Inherits Business

        Private privateWorkingRadius As Integer
        Public Property WorkingRadius() As Integer
            Get
                Return privateWorkingRadius
            End Get
            Friend Set(ByVal value As Integer)
                privateWorkingRadius = value
            End Set
        End Property

        Private privateWorkerOrNull As Movable
        Friend Property WorkerOrNull() As Movable
            Get
                Return privateWorkerOrNull
            End Get
            Set(ByVal value As Movable)
                privateWorkerOrNull = value
            End Set
        End Property
        Private privateIsPlanting As Boolean
        Friend Property IsPlanting() As Boolean
            Get
                Return privateIsPlanting
            End Get
            Private Set(ByVal value As Boolean)
                privateIsPlanting = value
            End Set
        End Property
        Private privateIsProducing As Boolean
        Friend Property IsProducing() As Boolean
            Get
                Return privateIsProducing
            End Get
            Private Set(ByVal value As Boolean)
                privateIsProducing = value
            End Set
        End Property
        Friend ReadOnly Property IsBusy() As Boolean
            Get
                Return IsPlanting OrElse IsProducing
            End Get
        End Property

        Friend Sub New(ByVal inParent As BuildingManager, ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
            MyBase.New(inParent, inConfig, inPosition)
            Dim provCount As Integer = inConfig.ResourceStacks.AsEnumerable().Count(Function(e) (e.Type <> Resource.Max) AndAlso (e.Direction = StackType.Provider))
            Dim queryCount As Integer = inConfig.ResourceStacks.AsEnumerable().Count(Function(e) (e.Type <> Resource.Max) AndAlso (e.Direction = StackType.Query))

            If queryCount > 0 Then
                Throw New ArgumentException("OutputOnly can not have resource queries!")
            End If

            If provCount <> 1 Then
                Throw New ArgumentException("OutputOnly shall have exactly one resource provider!")
            End If
        End Sub

        Protected MustOverride Function Produce(ByVal onCompleted As Procedure(Of Boolean)) As Boolean
        Protected MustOverride Function Plant(ByVal onCompleted As Procedure) As Boolean

        ''' <summary>
        ''' Is not to be called every cycle, since it is an expensive function.
        ''' </summary>
        Friend Overrides Function Update() As Boolean
            If Not (MyBase.Update()) Then
                Return False
            End If

            If IsBusy Then
                Return False
            End If

            If Parent.MoveManager.CurrentCycle - ProductionStart < Config.ProductionTimeMillis Then
                Return False
            End If

            ' ### START NEW PRODUCTION CYCLE
            If Not (Plant(AddressOf OnPlantingCompleted)) Then
                ' has space for outcomes?
                If Not (HasFreeProvider()) Then
                    Return False
                End If

                If Not (Produce(AddressOf OnProductionCompleted)) Then
                    Return False
                End If

                IsProducing = True
            Else
                IsPlanting = True
            End If

            ' the derived class is now responsible for completing the task and raise our handler...

            Return True
        End Function

        Protected Sub DisposeWorker()
            ' dispose worker
            VisualUtilities.Hide(WorkerOrNull)
            Parent.MoveManager.MarkMovableForRemoval(WorkerOrNull)
            WorkerOrNull = Nothing
        End Sub

        Private Sub OnProductionCompleted(ByVal succeeded As Boolean)
            ProductionStart = Parent.MoveManager.CurrentCycle
            IsProducing = False

            DisposeWorker()

            If succeeded Then
                Providers(0).AddResource()
            End If
        End Sub

        Private Sub OnPlantingCompleted()
            ProductionStart = Parent.MoveManager.CurrentCycle
            IsPlanting = False

            DisposeWorker()
        End Sub

        Protected Function SpawnWorker() As Movable
            If WorkerOrNull IsNot Nothing Then
                Throw New InvalidOperationException()
            End If

            WorkerOrNull = Parent.MoveManager.AddMovable(SpawnPoint.Value, MovableType.Migrant)

            Return WorkerOrNull
        End Function
    End Class
End Namespace
