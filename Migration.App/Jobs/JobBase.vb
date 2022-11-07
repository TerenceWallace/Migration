Imports Migration.Common

Namespace Migration.Jobs

    ''' <summary>
    ''' Jobs are used by movables to do tasks like wood cutting, carrying, building, fighting,
    ''' stealing, etc. They provide a generic way to accomplish these tasks in a synchronized,
    ''' stable and straighforward manner. Each movable can only have one job at maximum. If it
    ''' does, it is considered non-free and thus not eligable to get another job. The job itself
    ''' is permanent and thus gets repeated if return "true".
    ''' </summary>
    Public MustInherit Class JobBase
        Implements IDisposable

        Protected Shared ReadOnly m_Random As New CrossRandom(0)
        Private ReadOnly m_Steps As New LinkedList(Of AnimationStep)()
        Private m_CurrentStep As LinkedListNode(Of AnimationStep) = Nothing
        Private m_Character As String

        Private ReadOnly m_Unique As UniqueIDObject
        Friend ReadOnly Property UniqueID() As Long
            Get
                Return m_Unique.UniqueID
            End Get
        End Property
        Public Property Character() As String
            Get
                Return m_Character
            End Get
            Friend Set(ByVal value As String)
                If m_Character = value Then
                    Return
                End If

                m_Character = value

                RaiseCharacterChanged()
            End Set
        End Property
        Private privateMovable As Movable
        Public Property Movable() As Movable
            Get
                Return privateMovable
            End Get
            Private Set(ByVal value As Movable)
                privateMovable = value
            End Set
        End Property

        Friend ReadOnly Property Manager() As MovableManager
            Get
                Return Movable.Parent
            End Get
        End Property

        Public Event OnCharacterChanged As Procedure(Of JobBase)

        Protected Sub RaiseCharacterChanged()
            RaiseEvent OnCharacterChanged(Me)
        End Sub

        Friend Sub New(ByVal inMovable As Movable)
            If inMovable Is Nothing Then
                Throw New ArgumentNullException()
            End If

            m_Unique = New UniqueIDObject(Me)
            Movable = inMovable
        End Sub

        ''' <summary>
        ''' should be overwritten in a subclass. The task here is to determine whether the job
        ''' can be executed by now and if so, returns "true", "false" otherwise. Further, one
        ''' should add some initial animation steps, otherwise the job will finish immediately.
        ''' </summary>
        ''' <returns></returns>
        Friend MustOverride Function Prepare() As Boolean

        ''' <summary>
        ''' Asynchronously aborts the job on next update. 
        ''' </summary>
        Friend Sub [Stop]()
            If m_CurrentStep IsNot Nothing Then
                Dim onCompleted As Func(Of Boolean, Boolean) = Me.m_CurrentStep.Value.OnCompleted

                m_CurrentStep.Value.Dispose()

                If onCompleted IsNot Nothing Then
                    onCompleted(False)
                End If

            End If

            m_CurrentStep = Nothing
            m_Steps.Clear()
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Me.Stop()
        End Sub

        ''' <summary>
        ''' Lets the movable walk to the given destination and call <paramref name="onStepCompleted"/>
        ''' with "true" if the target has been reached and "false" otherwise. The callback should
        ''' return "true" if the job should continue and "false" if the job should be aborted. You
        ''' may add new animation steps within the callback! Animation steps are removed from the
        ''' internal list, once they were executed (regardless of success/failure).
        ''' </summary>
        Friend Sub AddAnimationStep(ByVal inTarget As Point, ByVal onStepStarted As Func(Of Boolean), ByVal onStepCompleted As Func(Of Boolean, Boolean))
            m_Steps.AddLast(New AnimationStep(inTarget, onStepStarted, onStepCompleted))
        End Sub

        ''' <summary>
        ''' Allows the caller to inject a fully customized animation step into the timeline.
        ''' The step is completed, when the given lambda expression in return raises the 
        ''' passed animation completion handler.
        ''' </summary>
        Friend Sub AddAnimationStep(ByVal inCustomAnimPlayback As Procedure(Of Procedure))
            m_Steps.AddLast(New AnimationStep(inCustomAnimPlayback))
        End Sub

        ''' <summary>
        ''' Calls <paramref name="onStepStarted"/> when animation has reached this step and if
        ''' "true" is returned by the callback it will just wait the given time in millis rounded
        ''' up to the next cycle time boundary. Afterwards <param name="onStepCompleted"/> is called.
        ''' The callback should return "true" if the job should continue and "false" if the job 
        ''' should be aborted. You may add new animation steps within the callback! Animation steps 
        ''' are removed from the internal list, once they were executed (regardless of success/failure).
        ''' </summary>
        Friend Sub AddAnimationStepWithPathFollow(ByVal inMillis As Integer, ByVal onStepStarted As Func(Of Boolean), ByVal onStepCompleted As Func(Of Boolean, Boolean))
            inMillis = Convert.ToInt32(CInt(Fix(((inMillis + Manager.CycleResolution - 1) / CDbl(Manager.CycleResolution)) * Manager.CycleResolution)))

            If inMillis <= 0 Then
                Throw New ArgumentOutOfRangeException()
            End If

            m_Steps.AddLast(New AnimationStep(inMillis, onStepStarted, onStepCompleted))
        End Sub

        ''' <summary>
        ''' Is used internally to update the job status.
        ''' </summary>
        Friend Sub Update()
            Dim wasRunning As Boolean = False

            If m_CurrentStep IsNot Nothing Then
                wasRunning = True

                ' check if animation step is completed
                Dim mStep As AnimationStep = m_CurrentStep.Value

                If mStep.Millis > 0 Then
                    If mStep.Millis < (Manager.CurrentCycle - mStep.StartCycles) * CyclePoint.CYCLE_MILLIS Then
                        Dim onCompleted As Func(Of Boolean, Boolean) = mStep.OnCompleted

                        mStep.Dispose()
                        m_CurrentStep = Nothing

                        If onCompleted IsNot Nothing Then
                            If Not (onCompleted(True)) Then
                                Me.Stop()
                                Return
                            End If
                        End If
                    End If
                ElseIf mStep.IsCompleted Then
                    m_CurrentStep = Nothing
                End If
            End If

            ' is used for both, starting a new job and selecting the next step
            If m_CurrentStep Is Nothing Then
                ' check if job can be restarted
                If m_Steps.Count = 0 Then
                    wasRunning = False
                End If

                If ((Not wasRunning)) AndAlso (Not (Prepare())) Then
                    Return
                End If

                If m_Steps.Count > 0 Then
                    m_CurrentStep = m_Steps.First
                    m_Steps.RemoveFirst()

                    Dim mStep As AnimationStep = m_CurrentStep.Value

                    If (mStep.OnStarted IsNot Nothing) AndAlso (Not (mStep.OnStarted.Invoke())) Then
                        m_CurrentStep = Nothing ' prevent onCompleted notification
                        Me.Stop()

                        Return
                    End If

                    If mStep.Millis > 0 Then
                        mStep.StartCycles = Manager.CurrentCycle

                        Manager.QueueWorkItem(mStep.Millis, AddressOf Update)
                    ElseIf mStep.CustomAnimPlayback Is Nothing Then

                        '                        
                        ' * For sake of continous animation, we should now be at a time cycle where path
                        ' * requests will immediately be dispatched. Otherwise the movable will walk or
                        ' * stand without changing its position for some noticeable time.
                        '                         
                        ' TODO:
                        'If (Not Manager.IsAlignedCycle) Then
                        '    'Throw New InvalidOperationException("For flawless animation, one must ensure that path planning animation steps are aligned to the movable manager's cycle resolution.")
                        'End If

                        ' handle completion
                        Me.Manager.SetPath(Me.Movable, mStep.Target, Sub(succeeded As Boolean)
                                                                         If Not succeeded Then
                                                                             Me.Stop()
                                                                         Else
                                                                             Dim onCompleted As Func(Of Boolean, Boolean) = mStep.OnCompleted
                                                                             mStep.Dispose()
                                                                             If (onCompleted IsNot Nothing) AndAlso Not (onCompleted.Invoke(True)) Then
                                                                                 Me.m_Steps.Clear()
                                                                             Else
                                                                                 mStep.MarkAsCompleted()
                                                                                 Me.Update()
                                                                             End If
                                                                         End If
                                                                     End Sub)
                    Else
                        mStep.CustomAnimPlayback.Invoke(Sub()
                                                            mStep.MarkAsCompleted()
                                                        End Sub)
                    End If
                End If
            End If
        End Sub
    End Class
End Namespace
