Imports System.Reflection

Namespace Migration
	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class Content
		Inherits Control

		Public Sub New()
			MyBase.New()
		End Sub

		Public Overrides Sub XMLPostProcess(ByVal inLayout As XMLGUILayout)
			If (XMLChildren IsNot Nothing) AndAlso (XMLChildren.Count > 0) Then
				Throw New ArgumentException("Content must not have children!")
			End If
		End Sub
	End Class
End Namespace
