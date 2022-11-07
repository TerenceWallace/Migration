Namespace Migration.Common

	Friend NotInheritable Class [Global]

		Private Sub New()
		End Sub


		Private Shared _GUILoadLock As New Object()
		Public Shared ReadOnly Property GUILoadLock() As Object
			Get
				If _GUILoadLock Is Nothing Then
					_GUILoadLock = New Object()
				End If
				Return _GUILoadLock
			End Get
		End Property

		Public Shared Function GetResourcePath(ByVal inResource As String) As String
			Return Game.Setup.Language.ResourcePath & "\" & inResource
		End Function

	End Class
End Namespace
