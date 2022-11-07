Imports Migration.Buildings
Imports Migration.Common

Namespace Migration

	''' <summary>
	''' A resource stack is a integral part of the whole economy. Its a simple but powerful
	''' concept with which most resource tasks seen in Migrants can be archieved.
	''' </summary>
	Public Class GenericResourceStack
		Inherits PositionTracker

		''' <summary>
		''' There is a maximum number of items supported for ALL resource stacks.
		''' </summary>
		Public Const DEFAULT_STACK_SIZE As Int32 = 8

		Private m_Stack As New List(Of Movable)()
		Private m_LastCount As Int32 = 0
		Private m_MaxCount As Int32 = 0

		Friend ReadOnly Property HasPriority() As Boolean
			Get
				Return PriorityNode IsNot Nothing
			End Get
		End Property
		Private privatePriorityNode As LinkedListNode(Of GenericResourceStack)
		Friend Property PriorityNode() As LinkedListNode(Of GenericResourceStack)
			Get
				Return privatePriorityNode
			End Get
			Set(ByVal value As LinkedListNode(Of GenericResourceStack))
				privatePriorityNode = value
			End Set
		End Property
		''' <summary>
		''' The building, if any, this stack is attached to.
		''' </summary>
		Private privateBuilding As BaseBuilding
		Friend Property Building() As BaseBuilding
			Get
				Return privateBuilding
			End Get
			Private Set(ByVal value As BaseBuilding)
				privateBuilding = value
			End Set
		End Property
		''' <summary>
		''' Kind of this stack.
		''' </summary>
		Private privateType As StackType
		Public Property Type() As StackType
			Get
				Return privateType
			End Get
			Private Set(ByVal value As StackType)
				privateType = value
			End Set
		End Property
		''' <summary>
		''' Resource typed stored here.
		''' </summary>
		Private privateResource As Resource
		Public Property Resource() As Resource
			Get
				Return privateResource
			End Get
			Private Set(ByVal value As Resource)
				privateResource = value
			End Set
		End Property
		''' <summary>
		''' Is raised whenever resources are removed or added.
		''' </summary>
		Public Event OnCountChanged As DChangeHandler(Of GenericResourceStack, Int32)
		''' <summary>
		''' Is raised whenever <see cref="MaxCount"/> changes.
		''' </summary>
		Public Event OnMaxCountChanged As DChangeHandler(Of GenericResourceStack, Int32)
		''' <summary>
		''' For each instance you can set the maximum stack size between
		''' one and <see cref="DEFAULT_STACK_SIZE"/>.
		''' </summary>
		Public Property MaxCount() As Int32
			Get
				Return m_MaxCount
			End Get
			Friend Set(ByVal value As Int32)
				Dim backup As Integer = m_MaxCount

				If (value < 0) OrElse (value > DEFAULT_STACK_SIZE) Then
					Throw New ArgumentOutOfRangeException()
				End If

				m_MaxCount = value

				If Type = StackType.Provider Then
					Do While Available > MaxCount
						RemoveResource()
					Loop
				Else ' if (Type == ResStackType.Query)
					Do While VirtualCount > MaxCount
						If Requested > 0 Then ' remove jobs first

							Dim  m_movable As Movable = m_Stack.Where(Function(e) e IsNot Nothing).First()

							RemoveInternal( m_movable)

							 m_movable.Stop()
						Else
							RemoveResource()
						End If
					Loop

				End If

				RaiseEvent OnMaxCountChanged(Me, backup, m_MaxCount)
			End Set
		End Property
		''' <summary>
		''' The virutal resource count is calculated differently for queries and
		''' providers. For queries it is the sum of <see cref="Available"/> and
		''' <see cref="Requested"/>, for providers its the difference. 
		''' </summary>
		''' <remarks>
		''' Further a query is considered to be full if this value has reached the maximum
		''' stack size, even if for now not all resources are actually added to the
		''' stack, but on their way. A provider can also be empty even if there is
		''' the maximum possible amount of resources stored.
		''' </remarks>
		Public ReadOnly Property VirtualCount() As Int32
			Get
				If Type = StackType.Query Then
					Return Available + Requested
				Else
					Return Available - Requested
				End If
			End Get
		End Property
		''' <summary>
		''' The amount of resources building this stack. This does NOT include resource
		''' jobs that will decrease or increase the stack count in near future.
		''' </summary>
		Public ReadOnly Property Available() As Int32
			Get
				Dim c As Integer = 0

				For Each e As Movable In m_Stack
					If e Is Nothing Then
						c += 1
					End If
				Next e

				Return c
			End Get
		End Property
		''' <summary>
		''' The amount of resource job regarding this stack. This does NOT include resources
		''' already placed on the stack.
		''' </summary>
		Public ReadOnly Property Requested() As Int32
			Get
				Dim c As Integer = 0

				For Each e As Movable In m_Stack
					If e IsNot Nothing Then
						c += 1
					End If
				Next e

				Return c
			End Get
		End Property

		Private privateIsRemoved As Boolean
		Friend Property IsRemoved() As Boolean
			Get
				Return privateIsRemoved
			End Get
			Private Set(ByVal value As Boolean)
				privateIsRemoved = value
			End Set
		End Property

		Private privateMinProviderID As Int64
		Friend Property MinProviderID() As Int64
			Get
				Return privateMinProviderID
			End Get
			Set(ByVal value As Int64)
				privateMinProviderID = value
			End Set
		End Property

		Friend Sub MarkAsRemoved()
			If IsRemoved Then
				Throw New InvalidOperationException()
			End If

			IsRemoved = True


			For Each  m_movable As Movable In m_Stack
				If ( m_movable Is Nothing) OrElse ( m_movable.Job Is Nothing) Then
					Continue For
				End If

				 m_movable.Job = Nothing
				 m_movable.Stop()
			Next  m_movable
		End Sub

		Friend Sub New(ByVal inType As StackType, ByVal inResource As Resource)
			Me.New(inType, inResource, DEFAULT_STACK_SIZE)
		End Sub

		Friend Sub New(ByVal inType As StackType, ByVal inResource As Resource, ByVal inMaxStack As Int32)
			Me.New(Nothing, inType, inResource, inMaxStack)
		End Sub

		Friend Sub New(ByVal inBuilding As BaseBuilding, ByVal inType As StackType, ByVal inResource As Resource, ByVal inMaxStack As Int32)
			If (inMaxStack < 1) OrElse (inMaxStack > DEFAULT_STACK_SIZE) Then
				Throw New ArgumentOutOfRangeException()
			End If

			MaxCount = inMaxStack
			Type = inType
			Resource = inResource
			Building = inBuilding
		End Sub

		Friend ReadOnly Property HasSpace() As Boolean
			Get
				If Type = StackType.Provider Then
					Return Available < MaxCount
				Else
					Return VirtualCount < MaxCount
				End If
			End Get
		End Property

		Private Sub Update()
			If m_LastCount = VirtualCount Then
				Return
			End If

			Try
				RaiseEvent OnCountChanged(Me, m_LastCount, VirtualCount)
			Finally
				m_LastCount = VirtualCount
			End Try
		End Sub

		Private Function CheckRange(ByVal inCount As Integer) As Boolean
			If (inCount < 0) OrElse (inCount > MaxCount) Then
				Return False
			End If

			Return True
		End Function

		''' <summary>
		''' Adds a resource, depending on stack type, directly, meaning it will increase
		''' <see cref="Available"/>.
		''' </summary>
		''' <exception cref="ArgumentOutOfRangeException">Adding a resource would overflow the stack.</exception>
		Friend Sub AddResource()
			AddInternal(Nothing)
		End Sub

		''' <summary>
		''' Adds a resource job, depending on stack type, meaning it will increase
		''' <see cref="Requested"/>.
		''' </summary>
		''' <exception cref="ArgumentOutOfRangeException">Adding another resource would overflow the stack.</exception>
		Friend Sub AddJob(ByVal inMovable As Movable)
			If inMovable Is Nothing Then
				Throw New ArgumentNullException()
			End If

			AddInternal(inMovable)
		End Sub

		Private Sub AddInternal(ByVal inMovable As Movable)
			Debug.Assert(CheckRange(VirtualCount) AndAlso CheckRange(Available) AndAlso CheckRange(Requested))

			m_Stack.Insert(0, inMovable)

			If (Not(CheckRange(VirtualCount))) OrElse (Not(CheckRange(Available))) OrElse (Not(CheckRange(Requested))) Then
				m_Stack.RemoveAt(0)

				Throw New ArgumentOutOfRangeException()
			End If

			Update()
		End Sub

		''' <summary>
		''' Adds a resource, depending on stack type, directly, meaning it will increase
		''' <see cref="Available"/>.
		''' </summary>
		''' <exception cref="KeyNotFoundException">No resource available to remove.</exception>
		Friend Sub RemoveResource()
			RemoveInternal(Nothing)
		End Sub

		''' <summary>
		''' Removes a resource job meaning it will decrease <see cref="Requested"/>.
		''' </summary>
		''' <exception cref="KeyNotFoundException">Given movable was not found.</exception>
		Friend Sub RemoveJob(ByVal inMovable As Movable)
			If inMovable Is Nothing Then
				Throw New ArgumentNullException()
			End If

			RemoveInternal(inMovable)
		End Sub

		Private Sub RemoveInternal(ByVal inMovable As Movable)
			Dim success As Boolean = False

			Dim i As Integer = 0
			Do While i < m_Stack.Count
				If m_Stack(i) Is inMovable Then
					m_Stack.RemoveAt(i)

					success = True
					Exit Do
				End If
				i += 1
			Loop

			If Not success Then
				Throw New KeyNotFoundException()
			End If

			Update()
		End Sub
	End Class
End Namespace
