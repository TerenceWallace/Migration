Imports System.ComponentModel
Imports System.Runtime.Serialization

Namespace Migration.Core
	Friend Class InternalBinding(Of T)
		Inherits BindingList(Of T)

		Private m_AllowChange As Boolean = True

		<OnDeserialized()> _
		Private Sub OnDeserialized(ByVal context As StreamingContext)
			m_AllowChange = True
		End Sub

		Friend Sub Protect()
			m_AllowChange = False
		End Sub

		Friend Sub AddInternal(ByVal inItem As T)
			Try
				m_AllowChange = True

				Add(inItem)
			Finally
				m_AllowChange = False
			End Try
		End Sub

		Friend Sub InsertInternal(ByVal inIndex As Int32, ByVal inItem As T)
			Try
				m_AllowChange = True

				Insert(inIndex, inItem)
			Finally
				m_AllowChange = False
			End Try
		End Sub

		Friend Sub ClearInternal()
			Try
				m_AllowChange = True

				Clear()
			Finally
				m_AllowChange = False
			End Try
		End Sub

		Friend Function RemoveInternal(ByVal inItem As T) As Boolean
			Try
				m_AllowChange = True

				Return Remove(inItem)
			Finally
				m_AllowChange = False
			End Try
		End Function

		Friend Sub RemoveAtInternal(ByVal inIndex As Int32)
			Try
				m_AllowChange = True

				RemoveAt(inIndex)
			Finally
				m_AllowChange = False
			End Try
		End Sub

		Protected Overrides Function AddNewCore() As Object
			If Not m_AllowChange Then
				Throw New InvalidOperationException("List is in read-only state and can not be modified!")
			End If

			Return MyBase.AddNewCore()
		End Function

		Protected Overrides Sub ClearItems()
			If Not m_AllowChange Then
				Throw New InvalidOperationException("List is in read-only state and can not be modified!")
			End If

			MyBase.ClearItems()
		End Sub

		Protected Overrides Sub InsertItem(ByVal index As Integer, ByVal item As T)
			If Not m_AllowChange Then
				Throw New InvalidOperationException("List is in read-only state and can not be modified!")
			End If

			MyBase.InsertItem(index, item)
		End Sub

		Protected Overrides Sub RemoveItem(ByVal index As Integer)
			If Not m_AllowChange Then
				Throw New InvalidOperationException("List is in read-only state and can not be modified!")
			End If

			MyBase.RemoveItem(index)
		End Sub
	End Class
End Namespace
