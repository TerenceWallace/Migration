Imports System.IO
Imports Migration.Common
Imports Migration.Interfaces
Imports Migration.Jobs
Imports Migration.Core

Namespace Migration

	''' <summary>
	''' The base class for all movables. 
	''' </summary>
	Public Class Movable
		Implements IPositionTracker

		Friend Shared ReadOnly GlobalLock As New Object()

		Private m_Position As CyclePoint
		Private m_Job As JobBase
		Private m_Carrying? As Resource
		Private ReadOnly m_Unique As UniqueIDObject

		Friend ReadOnly Property UniqueID() As Long
			Get
				Return m_Unique.UniqueID
			End Get
		End Property

		''' <summary>
		''' The current path node. TODO: In future this should be a read-only property
		''' which efficiently determines its value instead of being set explicitly.
		''' </summary>
		Private privateCurrentNode As LinkedListNode(Of MovablePathNode)
		Friend Property CurrentNode() As LinkedListNode(Of MovablePathNode)
			Get
				Return privateCurrentNode
			End Get
			Set(ByVal value As LinkedListNode(Of MovablePathNode))
				privateCurrentNode = value
			End Set
		End Property
		''' <summary>
		''' A list of path nodes currently in the queue.
		''' </summary>
		Private privatePath As LinkedList(Of MovablePathNode)
		Friend Property Path() As LinkedList(Of MovablePathNode)
			Get
				Return privatePath
			End Get
			Private Set(ByVal value As LinkedList(Of MovablePathNode))
				privatePath = value
			End Set
		End Property
		''' <summary>
		''' The movable type. Currently ignored, but later this will provide ways to,
		''' for example, letting soldiers walk over snow while Migrants couldn't.
		''' </summary>
		Private privateMovableType As MovableType
		Public Property MovableType() As MovableType
			Get
				Return privateMovableType
			End Get
			Set(ByVal value As MovableType)
				privateMovableType = value
			End Set
		End Property

		''' <summary>
		''' The time when the engine should issue a new path. This is usually the time
		''' of the last path node.
		''' </summary>
		Friend ReadOnly Property ReplanTime() As Long
			Get
				Return Path.Last.Value.Time
			End Get
		End Property

		''' <summary>
		''' This instance will take <see cref="CycleSpeed"/> times the <see cref="MovableManager.CycleResolution"/>
		''' to pass one grid cell. Currently this value is readonly and hardcoded to one (due to lack of support
		''' of the path engine, since it would make much things unnecessary complicated which should be avoided
		''' as long as the path engine itself is not feature complete; and to be honest it is already complicated enough
		''' to understand cooperative path planning even without having to think of different speeds and sizes; to
		''' be precise in theory it is easy but we are using a highly optimized version that additionally has a discrete 
		''' space-time and is 100% deterministic).
		''' </summary>
		Private privateCycleSpeed As Integer
		Public Property CycleSpeed() As Integer
			Get
				Return privateCycleSpeed
			End Get
			Private Set(ByVal value As Integer)
				privateCycleSpeed = value
			End Set
		End Property
		''' <summary>
		''' This is the grid cell where this instance is being moved to.
		''' </summary>
		Private privatePathTarget As Point
		Friend Property PathTarget() As Point
			Get
				Return privatePathTarget
			End Get
            Set(ByVal value As Point)
                privatePathTarget = value
            End Set
		End Property
		''' <summary>
		''' The movable manager this instance is attached to (or null).
		''' </summary>
		Private privateParent As MovableManager
		Friend Property Parent() As MovableManager
			Get
				Return privateParent
			End Get
			Set(ByVal value As MovableManager)
				privateParent = value
			End Set
		End Property
		''' <summary>
		''' An optional handler being called when the movable has reached its target
		''' or is stopped.
		''' </summary>
		Private privateResultHandler As Procedure(Of Boolean)
		Friend Property ResultHandler() As Procedure(Of Boolean)
			Get
				Return privateResultHandler
			End Get
			Set(ByVal value As Procedure(Of Boolean))
				privateResultHandler = value
			End Set
		End Property

		Private privateIsInvalidated As Boolean
		Friend Property IsInvalidated() As Boolean
			Get
				Return privateIsInvalidated
			End Get
			Set(ByVal value As Boolean)
				privateIsInvalidated = value
			End Set
		End Property

		Private privateIsMarkedForRemoval As Boolean
		Public Property IsMarkedForRemoval() As Boolean
			Get
				Return privateIsMarkedForRemoval
			End Get
			Private Set(ByVal value As Boolean)
				privateIsMarkedForRemoval = value
			End Set
		End Property

#If DEBUG Then
		Private privatePathStackTrace As System.Diagnostics.StackTrace
		Friend Property PathStackTrace() As System.Diagnostics.StackTrace
			Get
				Return privatePathStackTrace
			End Get
			Set(ByVal value As System.Diagnostics.StackTrace)
				privatePathStackTrace = value
			End Set
		End Property
#End If

		Public Event OnJobChanged As DChangeHandler(Of Movable, JobBase)

		Public Property Job() As JobBase
			Get
				Return m_Job
			End Get

			Friend Set(ByVal value As JobBase)
				Dim backup As JobBase = m_Job

				If value Is m_Job Then
					Return
				End If

				m_Job = value

				If backup IsNot Nothing Then
					' discard current job
					backup.Dispose()
				End If

				Me.Stop()

				RaiseEvent OnJobChanged(Me, backup, m_Job)
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

		''' <summary>
		''' Is this instance currently carrying out a job?
		''' </summary>
		Public ReadOnly Property HasJob() As Boolean
			Get
				Return Job IsNot Nothing
			End Get
		End Property

		''' <summary>
		''' If any, the resource currently carried (an eventually dropped) by this instance.
		''' </summary>
		Friend Property Carrying() As Resource?
			Get
				Return m_Carrying
			End Get

			Set(ByVal value? As Resource)
				Dim backup? As Resource = Me.m_Carrying
				Me.m_Carrying = value
				If (Not value.Equals(backup)) Then
					RaiseEvent OnCarryingChanged(Me)
				End If

			End Set
		End Property

		Public Property Position() As CyclePoint Implements IPositionTracker.Position
			Get
				Return m_Position
			End Get
			Set(ByVal value As CyclePoint)

			End Set
		End Property

		''' <summary>
		''' Well yes, you must not do this. This method is only exported for the path planning engine and calling it
		''' outside the right context may immediately crash the game! There is simply NO way to change a movable's
		''' position without the path planning engine or by passing a proper position in the constructor. Well to be
		''' true, there are rare exceptions but you have to be extremely careful and to know what you are doing...
		''' </summary>
		''' <param name="inNewPosition"></param>
		Friend Sub SetPosition_YouMustNotDoThis(ByVal inNewPosition As CyclePoint)
			Dim backup As CyclePoint = m_Position
			m_Position = inNewPosition

			If backup.ToPoint() <> inNewPosition.ToPoint() Then
				' notify grid cell change
				RaisePositionChange(backup)
			End If
		End Sub

		''' <summary>
		''' Is movable eligable for new jobs?
		''' </summary>
		Friend ReadOnly Property IsIdle() As Boolean
			Get
				Return Not HasJob
			End Get
		End Property
		Private privateUserControlable As Boolean
		Friend Property UserControlable() As Boolean
			Get
				Return privateUserControlable
			End Get
			Private Set(ByVal value As Boolean)
				privateUserControlable = value
			End Set
		End Property

		''' <summary>
		''' Is raised by <see cref="MovableManager.ProcessCycle"/> whenever it changes the movable position.
		''' </summary>
		Public Event OnPositionChanged As DChangeHandler(Of IPositionTracker, CyclePoint) Implements IPositionTracker.OnPositionChanged
		''' <summary>
		''' Is raised whenever Stop is called.
		''' </summary>
		Public Event OnStop As Procedure(Of Movable)
		Public Event OnCarryingChanged As Procedure(Of Movable)

		Friend Sub New(ByVal inInitialPosition As CyclePoint, ByVal inUserControlable As Boolean)
			m_Unique = New UniqueIDObject(Me)
			m_Position = CyclePoint.FromGrid(New Point(Convert.ToInt32(CInt(Fix(inInitialPosition.X))), Convert.ToInt32(CInt(Fix(inInitialPosition.Y)))))
			Path = New LinkedList(Of MovablePathNode)()
			PathTarget = inInitialPosition.ToPoint()
			MovableType = MovableType.Migrant
			CycleSpeed = 1
			UserControlable = inUserControlable
		End Sub


		''' <summary>
		''' Is raised by <see cref="MovableManager.ProcessCycle"/> whenever it changes the movable position.
		''' </summary>
		Friend Sub RaisePositionChange(ByVal inOldValue As CyclePoint)
			RaiseEvent OnPositionChanged(Me, inOldValue, Position)
		End Sub

		''' <summary>
		''' An object only is considered moving if its current path node got a direction, the only case in 
		''' which true is returned.
		''' </summary>
		Public ReadOnly Property IsMoving() As Boolean
			Get
				Return (If((CurrentNode IsNot Nothing) AndAlso (CurrentNode.Value.Direction IsNot Nothing) AndAlso CurrentNode.Value.Direction.HasValue, True, False))
			End Get
		End Property

		''' <summary>
		''' Stops movable in next aligned cycle, causing it to drop resources being carried (if any),
		''' and becoming idle.
		''' </summary>
		Friend Sub [Stop]()
			SyncLock Movable.GlobalLock
				Dim handler As Procedure(Of Boolean) = ResultHandler
				PathTarget = Position.ToPoint()

				If handler IsNot Nothing Then
					ResultHandler = Nothing

					handler(False)
				End If

				IsInvalidated = True
			End SyncLock

			RaiseEvent OnStop(Me)
		End Sub

		Public Sub InterpolateMovablePosition(ByRef refDirection As Direction, ByRef outXYInterpolation As CyclePoint, ByRef outZInterpolation As Double)
			outXYInterpolation = Position
			outZInterpolation = 0

			If Parent IsNot Nothing Then
				Parent.InterpolateMovablePosition(Me, refDirection, outXYInterpolation, outZInterpolation)
			End If
		End Sub


		Friend Sub Dispose()
			IsMarkedForRemoval = True
			Me.Stop()
		End Sub
	End Class
End Namespace
