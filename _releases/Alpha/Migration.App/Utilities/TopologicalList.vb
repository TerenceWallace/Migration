Imports Migration.Common
Imports Migration.Core
Imports Migration.Interfaces

Namespace Migration

	''' <summary>
	''' A topological list is meant to provide a highly optimized way of doing jobs like
	''' "give me all resources around XY sorted ascendingly by distance to XY". Since this
	''' is a very important and frequently used operation in the game, its worth to create
	''' a special list for it. Please be aware of that this list only ensurs sorting by
	''' distance in an approximative way, based on the granularity given. If you use 1 as
	''' granularity, which would definitely be stupid, you would get exact sorting by distance.
	''' If you choose 10, the same task will have an error of 10 length units on average, which
	''' doesn't matter at all for this game, but exponentially speeds up performance.
	''' </summary>
	''' <typeparam name="TValue"></typeparam>
	Public Class TopologicalList(Of TValue As IPositionTracker)

		Private m_Topology As New List(Of List(Of List(Of TValue)))()
		Private m_Entries As New List(Of TValue)()

		Private privateGranularity As Int32
		Public Property Granularity() As Int32
			Get
				Return privateGranularity
			End Get
			Private Set(ByVal value As Int32)
				privateGranularity = value
			End Set
		End Property

		Private privateWidth As Int32
		Public Property Width() As Int32
			Get
				Return privateWidth
			End Get
			Private Set(ByVal value As Int32)
				privateWidth = value
			End Set
		End Property

		Private privateHeight As Int32
		Public Property Height() As Int32
			Get
				Return privateHeight
			End Get
			Private Set(ByVal value As Int32)
				privateHeight = value
			End Set
		End Property

		Public Sub New(ByVal inGranularity As Integer, ByVal inWidth As Integer, ByVal inHeight As Integer)
			Granularity = inGranularity
			Width = Convert.ToInt32(CInt(Fix(inWidth / CDbl(inGranularity) + 1)))
			Height = Convert.ToInt32(CInt(Fix(inHeight / CDbl(inGranularity) + 1)))

			For x As Integer = 0 To Width - 1
				Dim column As New List(Of List(Of TValue))()

				m_Topology.Add(column)

				For y As Integer = 0 To Height - 1
					column.Add(New List(Of TValue)())
				Next y
			Next x
		End Sub

		Public Sub ForEach(ByVal inHandler As Func(Of TValue, Boolean))
			SyncLock m_Topology
				For Each entry As TValue In m_Entries
					If Not(inHandler(entry)) Then
						Exit For
					End If
				Next entry
			End SyncLock
		End Sub

		Public Sub CopyArea(ByVal inArea As Rectangle, ByVal outEntries As List(Of TValue))
			SyncLock m_Topology
				Dim granX1 As Integer = Convert.ToInt32(CInt(Fix(inArea.X / CDbl(Granularity))))
				Dim granY1 As Integer = Convert.ToInt32(CInt(Fix(inArea.Y / CDbl(Granularity))))
				Dim granX2 As Integer = Convert.ToInt32(CInt(Fix((inArea.X + inArea.Width + Granularity - 1) / CDbl(Granularity))))
				Dim granY2 As Integer = Convert.ToInt32(CInt(Fix((inArea.Y + inArea.Height + Granularity - 1) / CDbl(Granularity))))

				For x As Integer = granX1 To granX2 - 1
					For y As Integer = granY1 To granY2 - 1
						Dim o As List(Of TValue) = m_Topology(x)(y)
						outEntries.AddRange(o)
					Next y
				Next x
			End SyncLock
		End Sub

		Public Function EnumAt(ByVal inAtCell As Point) As IEnumerable(Of TValue)
			SyncLock m_Topology
				Dim list As List(Of TValue) = OpenList(CyclePoint.FromGrid(inAtCell))
				Dim result As New List(Of TValue)()

				For Each e As TValue In list
					If (e.Position.XGrid = inAtCell.X) AndAlso (e.Position.YGrid = inAtCell.Y) Then
						result.Add(e)
					End If
				Next e

				Return result
			End SyncLock
		End Function

		''' <summary>
		''' If you want to limit the radius of <see cref="EnumAround"/>, it is recommended using this function,
		''' since it is not obvious how to archieve this from outside the topological list!
		''' </summary>
		Public Function EnumAround(ByVal inAround As Point, ByVal inRadius As Integer, ByVal inHandler As Func(Of TValue, WalkResult)) As WalkResult
			SyncLock m_Topology
                Dim topRadius As Integer = CInt(CInt(Fix(CLng(inRadius) + Granularity - 1) / CDbl(Granularity)))
				Dim topAround As New Point(Convert.ToInt32(CInt(Fix(inAround.X / CDbl(Granularity)))), Convert.ToInt32(CInt(Fix(inAround.Y / CDbl(Granularity)))))

				Return GridSearch.GridWalkAround(topAround, Width, Height, Function(position)
					If (Math.Abs(position.X - topAround.X) > topRadius) OrElse (Math.Abs(position.Y - topAround.Y) > topRadius) Then
						Return WalkResult.Abort
					End If
					For Each entry As TValue In m_Topology(position.X)(position.Y)
						If (Math.Abs(entry.Position.X - inAround.X) > inRadius) OrElse (Math.Abs(entry.Position.Y - inAround.Y) > inRadius) Then
							Return WalkResult.NotFound
						End If
						If inHandler(entry) = WalkResult.Success Then
							Return WalkResult.Success
						End If
					Next entry
					Return WalkResult.NotFound
				End Function)
			End SyncLock
		End Function

		Public Function EnumAround(ByVal inAround As Point, ByVal inHandler As Func(Of TValue, WalkResult)) As WalkResult
			SyncLock m_Topology

				Return GridSearch.GridWalkAround(New Point(Convert.ToInt32(CInt(Fix(inAround.X / CDbl(Granularity)))), Convert.ToInt32(CInt(Fix(inAround.Y / CDbl(Granularity))))), Width, Height, Function(position)
					For Each entry As TValue In m_Topology(position.X)(position.Y)
						If inHandler(entry) = WalkResult.Success Then
							Return WalkResult.Success
						End If
					Next entry
					Return WalkResult.NotFound
				End Function)
			End SyncLock
		End Function

		Protected Function OpenList(ByVal inPosition As CyclePoint) As List(Of TValue)
			Dim granX As Integer = Convert.ToInt32(CInt(Fix(inPosition.XGrid / CDbl(Granularity))))
			Dim granY As Integer = Convert.ToInt32(CInt(Fix(inPosition.YGrid / CDbl(Granularity))))

			Return m_Topology(granX)(granY)
		End Function

		Private debugX As Integer
		Public Sub Add(ByVal inValue As TValue)
			SyncLock m_Topology
				Dim list As List(Of TValue) = OpenList(inValue.Position)

				If list.Contains(inValue) Then
					Throw New InvalidOperationException("Value has already been added.")
				End If

				AddHandler inValue.OnPositionChanged, AddressOf OnPositionChanged
				list.Add(inValue)
				m_Entries.Add(inValue)

				debugX += 1
			End SyncLock
		End Sub

		Private Sub OnPositionChanged(ByVal inTrackable As IPositionTracker, ByVal inOldPosition As CyclePoint, ByVal inNewPosition As CyclePoint)
			SyncLock m_Topology
				Dim oldList As List(Of TValue) = OpenList(inOldPosition)
				Dim newList As List(Of TValue) = OpenList(inNewPosition)

				If oldList Is newList Then
					Return
				End If

				If Not(oldList.Contains(CType(inTrackable, TValue))) Then
					Throw New ApplicationException()
				End If

				If newList.Contains(CType(inTrackable, TValue)) Then
					Throw New ApplicationException()
				End If

				oldList.Remove(CType(inTrackable, TValue))
				newList.Add(CType(inTrackable, TValue))
			End SyncLock
		End Sub

		Public Sub Remove(ByVal inValue As TValue)
			SyncLock m_Topology
				Dim list As List(Of TValue) = OpenList(inValue.Position)

				RemoveHandler inValue.OnPositionChanged, AddressOf OnPositionChanged

				If (Not(list.Remove(inValue))) OrElse (Not(m_Entries.Remove(inValue))) Then
					Throw New ApplicationException()
				End If
			End SyncLock
		End Sub
	End Class
End Namespace
