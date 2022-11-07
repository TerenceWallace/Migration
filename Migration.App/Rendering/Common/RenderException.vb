#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else
Imports OpenTK.Graphics.OpenGL
#End If

Namespace Migration.Rendering
	''' <summary>
	''' Just a wrapper for exceptions thrown by GLRenderer.Check
	''' </summary>
	Public Class RenderException
		Inherits Exception

		Public Sub New(ByVal inErrorCode As ErrorCode)
			Me.New(inErrorCode, "OpenGL call failed unexpectedly.")
		End Sub

		Public Sub New(ByVal inErrorCode As ErrorCode, ByVal inMessage As String, ParamArray ByVal inArgs() As Object)
			MyBase.New(String.Format(inMessage, inArgs) & " (ErrorCode: " & inErrorCode & ")")
		End Sub

	End Class
End Namespace
