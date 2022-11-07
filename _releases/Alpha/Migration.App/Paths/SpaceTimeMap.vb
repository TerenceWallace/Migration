Imports Migration.Common
Imports Migration.Core

Namespace Migration
	Friend Class SpaceTimeMap
		Private m_Entries As New Map(Of PathNodeKey, MovablePathNode)(MapPolicy.None, PathNodeKey.Comparer)

		Friend Function GetPathList(ByVal inCell As Point) As IEnumerable(Of MovablePathNode)
			Dim key As New PathNodeKey(inCell.X, inCell.Y, 0)

			Return From e In m_Entries.Search(key, PathNodeKey.ComparerIgnoreTime) _
			       Select e.Value
		End Function

		Friend Sub Remove(ByVal inKey As PathNodeKey)
			m_Entries.Remove(inKey)
		End Sub

		Default Friend Property Item(ByVal key As PathNodeKey) As MovablePathNode
			Get
				Dim result As MovablePathNode = Nothing

				m_Entries.TryGetValue(key, result)

				Return result
			End Get

			Set(ByVal value As MovablePathNode)
				If m_Entries.ContainsKey(key) Then
					m_Entries(key) = value
				Else
					m_Entries.Add(key, value)
				End If
			End Set
		End Property

		Friend Sub Clear()
			m_Entries.Clear()
		End Sub
	End Class
End Namespace
