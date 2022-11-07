Imports Migration.Interfaces

Namespace Migration


	Friend Class PriorityQueue(Of PathNodeKey As IQueueItem)
		Private InnerList As New List(Of PathNodeKey)()

		Friend Sub New()
		End Sub

		Protected Sub SwitchElements(ByVal i As Integer, ByVal j As Integer)
			Dim ik As PathNodeKey = InnerList(i)
			Dim jk As PathNodeKey = InnerList(j)

			ik.Index = j
			jk.Index = i

			InnerList(i) = jk
			InnerList(j) = ik
		End Sub

		Protected Overridable Function OnCompare(ByVal i As Integer, ByVal j As Integer) As Integer
			Dim ik As PathNodeKey = InnerList(i)
			Dim jk As PathNodeKey = InnerList(j)

			If ik.F < jk.F Then
				Return -1
			ElseIf ik.F > jk.F Then
				Return 1
			Else
				Return 0
			End If
		End Function

		''' <summary>
		''' Push an object onto the PQ
		''' </summary>
		''' <returns>The index in the list where the object is _now_. This will change when objects are taken from or put onto the PQ.</returns>
		Friend Function Push(ByVal item As PathNodeKey) As Integer
			Dim p As Integer = InnerList.Count
			Dim p2 As Integer = 0
			item.Index = InnerList.Count
			InnerList.Add(item) ' E[p] = O

			Do
				If p = 0 Then
					Exit Do
				End If
				p2 = (p - 1) \ 2
				If OnCompare(p, p2) < 0 Then
					SwitchElements(p, p2)
					p = p2
				Else
					Exit Do
				End If
				Loop While True
			Return p
		End Function

		''' <summary>
		''' Get the smallest object and remove it.
		''' </summary>
		''' <returns>The smallest object</returns>
		Friend Function Pop() As PathNodeKey
			Dim result As PathNodeKey = InnerList(0)
			Dim p As Integer = 0
			Dim p1 As Integer = 0
			Dim p2 As Integer = 0
			Dim pn As Integer = 0
			Dim i0 As PathNodeKey = InnerList(InnerList.Count - 1)

			i0.Index = 0
			InnerList(0) = i0

			InnerList.RemoveAt(InnerList.Count - 1)

			result.Index = -1

			Do
				pn = p
				p1 = 2 * p + 1
				p2 = 2 * p + 2
				If InnerList.Count > p1 AndAlso OnCompare(p, p1) > 0 Then ' links kleiner
					p = p1
				End If
				If InnerList.Count > p2 AndAlso OnCompare(p, p2) > 0 Then ' rechts noch kleiner
					p = p2
				End If

				If p = pn Then
					Exit Do
				End If
				SwitchElements(p, pn)
				Loop While True

			Return result
		End Function

		''' <summary>
		''' Notify the PQ that the object at position i has changed
		''' and the PQ needs to restore order.
		''' </summary>
		Friend Sub Update(ByVal item As PathNodeKey)

			Dim  m_count As Integer = InnerList.Count

			' usually we only need to switch some elements, since estimation won't change that much.
			Do While (item.Index - 1 >= 0) AndAlso (OnCompare(item.Index - 1, item.Index) > 0)
				SwitchElements(item.Index - 1, item.Index)
			Loop

			Do While (item.Index + 1 <  m_count) AndAlso (OnCompare(item.Index + 1, item.Index) < 0)
				SwitchElements(item.Index + 1, item.Index)
			Loop
		End Sub

		''' <summary>
		''' Get the smallest object without removing it.
		''' </summary>
		''' <returns>The smallest object</returns>
		Friend Function Peek() As PathNodeKey
			If InnerList.Count > 0 Then
				Return InnerList(0)
			End If
			Return Nothing
		End Function

		Friend Sub Clear()
			InnerList.Clear()
		End Sub

		Friend ReadOnly Property Count() As Integer
			Get
				Return InnerList.Count
			End Get
		End Property
	End Class

End Namespace
