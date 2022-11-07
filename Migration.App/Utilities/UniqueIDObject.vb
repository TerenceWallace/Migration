Imports System.Threading

Namespace Migration
	''' <summary>
	''' Since the economy is fully deterministic, it is a very handy idea to ultimately derive some important
	''' objects like jobs, movables and the like from DebugObject, providing unique IDs and debug methods
	''' for easy bug tracking.
	''' </summary>
	Friend NotInheritable Class UniqueIDObject
		Private Shared ReadOnly m_IDToObject As New SortedDictionary(Of Long, WeakReference)()
		Private Shared m_ObjectIDCounter As Long = 0
		Private Shared m_CleanupTimer As New Timer(AddressOf OnCleanupTimer, Nothing, 10000, 10000)

		Private privateUniqueID As Long
		Friend Property UniqueID() As Long
			Get
				Return privateUniqueID
			End Get
			Private Set(ByVal value As Long)
				privateUniqueID = value
			End Set
		End Property

		Private privateReference As Object
		Friend Property Reference() As Object
			Get
				Return privateReference
			End Get
			Private Set(ByVal value As Object)
				privateReference = value
			End Set
		End Property

		Private Shared Sub OnCleanupTimer(ByVal unused As Object)
			SyncLock m_IDToObject
				Dim removals As New Stack(Of Int64)()

				For Each entry As KeyValuePair(Of Long, WeakReference) In m_IDToObject
					If Not entry.Value.IsAlive Then
						removals.Push(entry.Key)
					End If
				Next entry

				For Each id As Long In removals
					m_IDToObject.Remove(id)
				Next id
			End SyncLock
		End Sub

		Friend Sub New(ByVal inReference As Object)
			If inReference Is Nothing Then
				Throw New ArgumentNullException()
			End If

			SyncLock m_IDToObject
				m_ObjectIDCounter += 1
				UniqueID = m_ObjectIDCounter
				Reference = inReference

				m_IDToObject.Add(UniqueID, New WeakReference(Me))
			End SyncLock
		End Sub

		Friend Sub DebugBreakOnID(ByVal inObjectID As Long)
			If UniqueID = inObjectID Then
				System.Diagnostics.Debugger.Break()
			End If
		End Sub

		Friend Shared Function Resolve(ByVal inUniqueID As Int64) As Object
			SyncLock m_IDToObject
				Return CType(m_IDToObject(inUniqueID).Target, UniqueIDObject).Reference
			End SyncLock
		End Function
	End Class
End Namespace
