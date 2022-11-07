Namespace Migration

	Public Class ReadonlySettableList(Of TValue)
		Implements IList(Of TValue)

		Private m_List As IList(Of TValue)

		Public Sub New(ByVal inList As IList(Of TValue))
			m_List = inList
		End Sub

		Public Sub Add(ByVal item As TValue) Implements System.Collections.Generic.ICollection(Of TValue).Add
			Throw New InvalidOperationException("The collection is readonly.")
		End Sub

		Public Sub Clear() Implements System.Collections.Generic.ICollection(Of TValue).Clear
			Throw New InvalidOperationException("The collection is readonly.")
		End Sub

		Public Function Contains(ByVal item As TValue) As Boolean Implements System.Collections.Generic.ICollection(Of TValue).Contains
			Return m_List.Contains(item)
		End Function

        Public Sub CopyTo(ByVal array() As TValue, ByVal arrayIndex As Integer) Implements IList(Of TValue).CopyTo
            m_List.CopyTo(array, arrayIndex)
        End Sub

		Public ReadOnly Property Count() As Integer Implements System.Collections.Generic.ICollection(Of TValue).Count
			Get
				Return m_List.Count
			End Get
		End Property

		Public ReadOnly Property IsReadOnly() As Boolean Implements System.Collections.Generic.ICollection(Of TValue).IsReadOnly
			Get
				Return True
			End Get
		End Property

		Public Function Remove(ByVal item As TValue) As Boolean Implements System.Collections.Generic.ICollection(Of TValue).Remove
			Throw New InvalidOperationException("The collection is readonly.")
		End Function

		Public Function GetEnumerator() As IEnumerator(Of TValue) Implements System.Collections.Generic.IEnumerable(Of TValue).GetEnumerator
			Return m_List.GetEnumerator()
		End Function

        Private Function IEnumerable_GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return Me.IEnumerable_GetEnumerator()
        End Function
		Private Function IEnumerable_GetEnumerator() As System.Collections.IEnumerator
			Return m_List.GetEnumerator()
		End Function

		Public Function IndexOf(ByVal item As TValue) As Integer Implements IList(Of TValue).IndexOf
			Return m_List.IndexOf(item)
		End Function

		Public Sub Insert(ByVal index As Integer, ByVal item As TValue) Implements IList(Of TValue).Insert
			Throw New InvalidOperationException("The collection is readonly.")
		End Sub

		Public Sub RemoveAt(ByVal index As Integer) Implements IList(Of TValue).RemoveAt
			Throw New InvalidOperationException("The collection is readonly.")
		End Sub

		Default Public Property Item(ByVal index As Integer) As TValue Implements IList(Of TValue).Item
			Get
				Return m_List(index)
			End Get
			Set(ByVal value As TValue)
				m_List(index) = value
			End Set
		End Property
	End Class

End Namespace
