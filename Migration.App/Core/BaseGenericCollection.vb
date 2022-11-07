Imports System.Collections.ObjectModel

Namespace Migration.Core
	''' <summary>
	''' A basic generic class that can be used for any type of collection
	''' </summary>
	''' <typeparam name="T"></typeparam>
	Public Class BaseGenericCollection(Of T)
		Inherits Collection(Of T)

		Friend m_deletedList As List(Of T) = Nothing

		Public Shadows Sub Add(ByVal t As T)
			If Me.Contains(t) Then
				Throw New System.ArgumentException("Cannot add duplicate entry to collection.")
			End If
			MyBase.Add(t)
		End Sub

		''' <summary>
		''' Deletes an item from the collection.
		''' If items that are being deleted from this collection, need to update a persistant store, use this list to update your persistant store.
		''' </summary>
		''' <param name="t"></param>
		Public Sub Delete(ByVal t As T)
			If t Is Nothing Then
				Throw New System.ArgumentNullException("T", "BaseGenericCollection: Cannot Delete NULL from collection.")
			End If

			If m_deletedList Is Nothing Then
				m_deletedList = New List(Of T)()
			End If

			Me.m_deletedList.Add(t)
			MyBase.Remove(t)
		End Sub

	End Class
End Namespace
