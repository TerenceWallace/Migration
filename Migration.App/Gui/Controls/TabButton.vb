Imports System.Reflection
Imports System.Xml
Imports System.Xml.Serialization

Namespace Migration
	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class TabButton
		Inherits RadioButton

		Private privateTabString As String
		<XmlAttribute("Tab")> _
		Public Property TabString() As String
			Get
				Return privateTabString
			End Get
			Set(ByVal value As String)
				privateTabString = value
			End Set
		End Property
		Private privateTab As Control
		<XmlIgnore> _
		Public Property Tab() As Control
			Get
				Return privateTab
			End Get
			Private Set(ByVal value As Control)
				privateTab = value
			End Set
		End Property

		Public Sub New()
			MyBase.New()

		End Sub

		Protected Overrides Sub CheckButton()
			MyBase.CheckButton()

			' make tab visible
			If Tab IsNot Nothing Then
				Tab.IsVisible = True
			End If
		End Sub

		Protected Overrides Sub UncheckButton()
			MyBase.UncheckButton()

			' make tab invisible
			If Tab IsNot Nothing Then
				Tab.IsVisible = False
			End If
		End Sub

		Public Overrides Sub XMLPostProcess(ByVal inLayout As XMLGUILayout)
			MyBase.XMLPostProcess(inLayout)

			If Not(String.IsNullOrEmpty(TabString)) Then
				Tab = RootElement.FindControl(TabString)

				If Tab Is Nothing Then
					Throw New ArgumentException("A control with name """ & TabString & """ does not exist.")
				End If
			End If
		End Sub


	End Class
End Namespace
