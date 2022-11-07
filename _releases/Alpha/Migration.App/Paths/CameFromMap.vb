Imports Migration.Core

Namespace Migration
	Friend Class CameFromMap
		Private m_Entries As New SortedDictionary(Of PathNodeKey, PathNodeKey)(PathNodeKey.Comparer)

		Friend Sub Add(ByVal refFrom As PathNodeKey, ByVal refTo As PathNodeKey)
			m_Entries.Add(refFrom, refTo)
		End Sub

		Default Friend ReadOnly Property Item(ByVal key As PathNodeKey) As PathNodeKey
			Get
				Return m_Entries(key)
			End Get
		End Property

		Friend ReadOnly Property Count() As Int32
			Get
				Return m_Entries.Count
			End Get
		End Property

		Friend Sub Clear()
			m_Entries.Clear()
		End Sub

		Friend Sub GetPath(ByVal inLastNode As PathNodeKey, ByVal outResult As LinkedList(Of MovablePathNode))
			Dim item As New PathNodeKey()

			If m_Entries.TryGetValue(inLastNode, item) Then
				GetPath(item, outResult)
			End If

			outResult.AddLast(New MovablePathNode() With {.Position = New Point(inLastNode.X, inLastNode.Y), .Time = inLastNode.Time})
		End Sub
	End Class
End Namespace
