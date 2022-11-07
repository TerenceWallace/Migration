Imports System.Reflection

Namespace Migration
	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class RadioButton
		Inherits Button

		Protected Overridable Sub CheckButton()
			' reset all other TabButtons within this control level
			For Each btn As Control In Parent.Children
				Dim radio As RadioButton = TryCast(btn, RadioButton)

				If (radio Is Nothing) OrElse (radio Is Me) Then
					Continue For
				End If

				If radio.IsDown Then
					radio.DoClick(True)
				End If
			Next btn
		End Sub

		Protected Overridable Sub UncheckButton()
		End Sub

		Protected Overrides Sub DoClick(ByVal inIsSynthetic As Boolean)
			If Not inIsSynthetic Then
				If Not IsDown Then
					' Button can only be actively pressed
					MyBase.DoClick(inIsSynthetic)

					CheckButton()
				End If
				' else: do nothing!
			Else
				If Not IsDown Then
					CheckButton()
				Else
					IsDown = False

					UncheckButton()
				End If
			End If
		End Sub
	End Class
End Namespace
