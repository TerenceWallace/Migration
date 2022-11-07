Imports Migration.Common


Namespace Migration
    Public Class SynchronizedManager
        Private ReadOnly m_WorkItems As New LinkedList(Of WorkItem)()
        Private m_CurrentCycle As Long
        Private m_CycleResolution As Long

        Private privateSyncWith As SynchronizedManager
        Friend Property SyncWith() As SynchronizedManager
            Get
                Return privateSyncWith
            End Get
            Private Set(ByVal value As SynchronizedManager)
                privateSyncWith = value
            End Set
        End Property
        ''' <summary>
        ''' The next cycle time aligned to <see cref="CycleResolution"/>. If <see cref="CurrentCycle"/>
        ''' is already aligned, the next aligned cycle is still returned.
        ''' </summary>
        Friend ReadOnly Property NextAlignedCycle() As Long
            Get
                Return (CurrentCycle \ CycleResolution + 1) * CycleResolution
            End Get
        End Property
        ''' <summary>
        ''' The current discrete time value. By convention, everwhere else you need a discrete game time,
        ''' just return this value of the path engine.
        ''' </summary>
        Friend Property CurrentCycle() As Long
            Get
                If SyncWith Is Nothing Then
                    Return m_CurrentCycle
                Else
                    Return SyncWith.CurrentCycle
                End If
            End Get
            Set(ByVal value As Long)
                If SyncWith IsNot Nothing Then
                    Throw New InvalidOperationException()
                End If

                m_CurrentCycle = value
            End Set
        End Property
        ''' <summary>
        ''' Directly holds the discretization granularity. All movable speeds can only be multiples of
        ''' this value, but be careful, since performance goes down polynomial (?just a guess) with 
        ''' increasing resolution.
        ''' </summary>
        Friend ReadOnly Property CycleResolution() As Long
            Get
                If SyncWith Is Nothing Then
                    Return m_CycleResolution
                Else
                    Return SyncWith.CycleResolution
                End If
            End Get
        End Property

        Friend ReadOnly Property IsAlignedCycle() As Boolean
            Get
                Return Convert.ToBoolean((CurrentCycle Mod CycleResolution) = 0)
            End Get
        End Property

        Public Shared Function MillisToAlignedCycle(ByVal inMillis As Long) As Integer
            Dim value As Integer = Convert.ToInt32(CInt((inMillis / CDbl(Convert.ToInt32(CInt(Fix(CyclePoint.CYCLE_MILLIS)))) + Convert.ToInt32(CInt(Fix(CyclePoint.CYCLE_MILLIS))) - 1) / CDbl(Convert.ToInt32(CInt(Fix(CyclePoint.CYCLE_MILLIS)))))) * Convert.ToInt32(CInt(Fix(CyclePoint.CYCLE_MILLIS)))
            Return value
        End Function



        Friend Sub New(ByVal inInitialCycle As Long, ByVal inCycleResolution As Long)
            CurrentCycle = inInitialCycle
            m_CycleResolution = inCycleResolution
        End Sub

        Friend Sub New(ByVal inSyncWith As SynchronizedManager)
            If inSyncWith Is Nothing Then
                Throw New ArgumentNullException()
            End If

            SyncWith = inSyncWith
        End Sub

        Friend Sub QueueWorkItem(ByVal inTask As Procedure)
            QueueWorkItem(0, inTask)
        End Sub

        Friend Sub QueueWorkItem(ByVal inDueTimeMillis As Long, ByVal inTask As Procedure)
            If inTask Is Nothing Then
                Throw New ArgumentNullException()
            End If

            SyncLock m_WorkItems
                Dim list As LinkedListNode(Of WorkItem) = m_WorkItems.First
                Dim expirationCycle As Long = (CurrentCycle * Convert.ToInt64((CyclePoint.CYCLE_MILLIS)) + inDueTimeMillis + Convert.ToInt64((CyclePoint.CYCLE_MILLIS)) - 1) \ Convert.ToInt64((CyclePoint.CYCLE_MILLIS))

                Do While list IsNot Nothing
                    If list.Value.ExpirationCycle > expirationCycle Then
                        ' insert work item
                        m_WorkItems.AddBefore(list, New WorkItem() With {.ExpirationCycle = expirationCycle, .Handler = inTask})

                        Exit Do
                    End If

                    list = list.Next
                Loop

                If list Is Nothing Then
                    ' insert work item
                    m_WorkItems.AddLast(New WorkItem() With {.ExpirationCycle = expirationCycle, .Handler = inTask})
                End If
            End SyncLock
        End Sub

        Friend Overridable Sub ProcessCycle()
            ' execute expired work items
            SyncLock m_WorkItems
                Dim list As LinkedListNode(Of WorkItem) = m_WorkItems.First

                Do While list IsNot Nothing
                    Dim mNext As LinkedListNode(Of WorkItem) = list.Next

                    If list.Value.ExpirationCycle <= CurrentCycle Then
                        m_WorkItems.Remove(list.Value)

                        list.Value.Handler()
                    End If

                    list = mNext
                Loop
            End SyncLock
        End Sub
    End Class
End Namespace
