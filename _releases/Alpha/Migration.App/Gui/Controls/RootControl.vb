Imports System.Reflection
Imports Migration.Rendering

Namespace Migration
	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class RootControl
		Inherits Control

		Public Sub New()
			MyBase.New()
		End Sub

		Public Sub New(ParamArray ByVal inControls() As Control)
			MyBase.New(inControls)
		End Sub

		Public Function ProcessMouseUpEvent(ByVal inMouseX As Integer, ByVal inMouseY As Integer, ByVal inMouseButton As Integer) As Boolean
			Return ProcessMouseEvent(inMouseX, inMouseY, InvalidMouseButton, inMouseButton)
		End Function

		Public Function ProcessMouseDownEvent(ByVal inMouseX As Integer, ByVal inMouseY As Integer, ByVal inMouseButton As Integer) As Boolean
			Return ProcessMouseEvent(inMouseX, inMouseY, inMouseButton, InvalidMouseButton)
		End Function

		Public Function ProcessMouseMoveEvent(ByVal inMouseX As Integer, ByVal inMouseY As Integer) As Boolean
			Return ProcessMouseEvent(inMouseX, inMouseY, InvalidMouseButton, InvalidMouseButton)
		End Function

		Private Function ProcessMouseEvent(ByVal inMouseX As Integer, ByVal inMouseY As Integer, ByVal inButtonDown As Integer, ByVal inButtonUp As Integer) As Boolean
			If Parent IsNot Nothing Then
				Throw New InvalidOperationException("For technical reasons, this call is allowed on root controls only.")
			End If

			If (Renderer.ViewportWidth = 0) OrElse (Renderer.ViewportHeight = 0) Then
				Return False
			End If

			SetupControlScales()

			' translate mouse coordinates back to internal representation
			Dim mouseX As Integer = Convert.ToInt32(CInt(Fix((inMouseX / Convert.ToDouble(Renderer.ViewportWidth)) / WidthScale)))
			Dim mouseY As Integer = Convert.ToInt32(CInt(Fix((inMouseY / Convert.ToDouble(Renderer.ViewportHeight)) / HeightScale)))

			Return ProcessMouseEventInternal(mouseX, mouseY, inButtonDown, inButtonUp)
		End Function

		Private Sub SetupControlScales()
			Dim heightRatio As Double = Renderer.ViewportHeight / Convert.ToDouble(Height)
			Control.HeightScale = 1.0 / Convert.ToDouble(Height)
			Control.WidthScale = 1.0 / Convert.ToDouble(Renderer.ViewportWidth) * heightRatio
		End Sub

		Public Overloads Sub Render()
			If Parent IsNot Nothing Then
				Throw New InvalidOperationException("For technical reasons, this call is allowed on root controls only.")
			End If

			SetupControlScales()

			MyBase.Render(0, 0)
		End Sub
	End Class
End Namespace
