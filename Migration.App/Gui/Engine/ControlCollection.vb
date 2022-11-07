Imports System.Xml.Serialization

Namespace Migration
	Friend Class ControlCollection
		Implements ICollection(Of Control)

		<XmlIgnore()> _
		Private ReadOnly m_Children As New List(Of Control)()

		Private privateThis As Control
		<XmlIgnore()> _
		Public Property This() As Control
			Get
				Return privateThis
			End Get
			Private Set(ByVal value As Control)
				privateThis = value
			End Set
		End Property

		Public Sub New(ByVal inThis As Control)
			This = inThis
		End Sub

		Public Sub Add(ByVal item As Control) Implements ICollection(Of Control).Add
			If item.Parent IsNot Nothing Then
				Throw New InvalidOperationException("Control is already attached.")
			End If

			m_Children.Add(item)
			item.Parent = This
		End Sub

		Public Sub Clear() Implements ICollection(Of Control).Clear
			For Each child As Control In m_Children
				child.Parent = Nothing
			Next child

			m_Children.Clear()
		End Sub

		Public Function Contains(ByVal item As Control) As Boolean Implements ICollection(Of Control).Contains
			Return m_Children.Contains(item)
		End Function

        Public Sub CopyTo(ByVal array() As Control, ByVal arrayIndex As Integer) Implements ICollection(Of Migration.Control).CopyTo
            m_Children.CopyTo(array, arrayIndex)
        End Sub

        <XmlIgnore()> _
        Public ReadOnly Property Count() As Integer Implements ICollection(Of Control).Count
            Get
                Return m_Children.Count
            End Get
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property IsReadOnly() As Boolean Implements ICollection(Of Control).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(ByVal item As Control) As Boolean Implements ICollection(Of Control).Remove
            Dim pos As Integer = m_Children.IndexOf(item)

            If pos < 0 Then
                Return False
            End If

            item.Parent = Nothing
            m_Children.RemoveAt(pos)

            Return True
        End Function

        Public Function GetEnumerator() As IEnumerator(Of Control) Implements System.Collections.Generic.IEnumerable(Of Control).GetEnumerator
            Return m_Children.GetEnumerator()
        End Function

        Private Function IEnumerable_GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return Me.IEnumerable_GetEnumerator()
        End Function

        Private Function IEnumerable_GetEnumerator() As System.Collections.IEnumerator
            Return m_Children.GetEnumerator()
        End Function
    End Class
End Namespace
