Imports System.Reflection
Imports System.Xml
Imports System.Xml.Serialization
Imports Migration.Common

Namespace Migration
	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class Button
		Inherits Control

		Private m_StateCtrls(5) As Control

		Private privateIsDown As Boolean
		<XmlAttribute> _
		Public Property IsDown() As Boolean
			Get
				Return privateIsDown
			End Get
			Set(ByVal value As Boolean)
				privateIsDown = value
			End Set
		End Property
		Private privateIsToggleable As Boolean
		<XmlAttribute> _
		Public Property IsToggleable() As Boolean
			Get
				Return privateIsToggleable
			End Get
			Set(ByVal value As Boolean)
				privateIsToggleable = value
			End Set
		End Property
		Private privateImageTemplate As String
		<XmlAttribute> _
		Public Property ImageTemplate() As String
			Get
				Return privateImageTemplate
			End Get
			Set(ByVal value As String)
				privateImageTemplate = value
			End Set
		End Property

		Public Event OnClick As DNotifyHandler(Of Button)

		Public Sub New()
			MyBase.New()
			IsToggleable = True
		End Sub

		Public Overrides Sub XMLPostProcess(ByVal inLayout As XMLGUILayout)
			MyBase.XMLPostProcess(inLayout)

			If Not(String.IsNullOrEmpty(ImageTemplate)) Then
				If Children.Count > 0 Then
					Throw New ArgumentException("A button with an image template shall not have any children.")
				End If

				' check resources paths
				Dim states() As String = { "Up", "Over", "Down", "DownOver", "Disabled", "DisabledDown", "NormalState", "OverState", "DownState", "DownOverState", "NormalDisabledState", "DownDisabledState" }

				For i As Integer = 0 To 5
					Dim fileName As String = (ImageTemplate & states(i)) & ".png"

					If Gui.Loader.GUIConfig.Exists(fileName) Then
						Dim img As New Image() With {.SourceString = fileName, .Width = Me.Width, .Height = Me.Height, .Id = states(i + 6)}

						img.XMLPostProcess(inLayout)

						Children.Add(img)
					End If
				Next i
			End If

			For Each state As Control In Children
				Select Case state.Id
					Case "NormalState"
						If m_StateCtrls(0) IsNot Nothing Then
							Throw New ArgumentException()
						End If
						m_StateCtrls(0) = state
					Case "OverState"
						If m_StateCtrls(1) IsNot Nothing Then
							Throw New ArgumentException()
						End If
						m_StateCtrls(1) = state
					Case "DownState"
						If m_StateCtrls(2) IsNot Nothing Then
							Throw New ArgumentException()
						End If
						m_StateCtrls(2) = state
					Case "DownOverState"
						If m_StateCtrls(3) IsNot Nothing Then
							Throw New ArgumentException()
						End If
						m_StateCtrls(3) = state
					Case "NormalDisabledState"
						If m_StateCtrls(4) IsNot Nothing Then
							Throw New ArgumentException()
						End If
						m_StateCtrls(4) = state
					Case "DownDisabledState"
						If m_StateCtrls(5) IsNot Nothing Then
							Throw New ArgumentException()
						End If
						m_StateCtrls(5) = state
					Case Else
						Throw New ArgumentOutOfRangeException()
				End Select
			Next state

			If m_StateCtrls(0) Is Nothing Then
				Throw New ArgumentException("A button needs at least state definitions for NormalState.")
			End If

			If m_StateCtrls(2) Is Nothing Then
				m_StateCtrls(2) = m_StateCtrls(0)
			End If
		End Sub


		Protected Overridable Sub DoClick(ByVal inIsSynthetic As Boolean)
			If IsToggleable Then
				IsDown = Not IsDown
			End If

			RaiseEvent OnClick(Me)
		End Sub

		Protected Overrides Sub DoMouseButtonUp(ByVal inMouseX As Integer, ByVal inMouseY As Integer, ByVal inButton As Integer)
			If inButton = LeftMouseButton Then
				DoClick(False)
			End If
		End Sub

		Protected Overrides Sub Render(ByVal inChainLeft As Integer, ByVal inChainTop As Integer)
			For Each state As Control In m_StateCtrls
				If state Is Nothing Then
					Continue For
				End If

				state.IsVisible = False
			Next state

			If ((Not IsDown)) AndAlso IsEnabled AndAlso ((Not IsMouseOver)) Then ' NormalState
				m_StateCtrls(0).IsVisible = True
			ElseIf ((Not IsDown)) AndAlso IsEnabled AndAlso IsMouseOver AndAlso ((Not IsMouseDown)) Then ' OverState
				If m_StateCtrls(1) Is Nothing Then
					m_StateCtrls(0).IsVisible = True
				Else
					m_StateCtrls(1).IsVisible = True
				End If
			ElseIf (IsDown OrElse IsMouseDown) AndAlso IsEnabled Then ' DownState
				m_StateCtrls(2).IsVisible = True
			ElseIf IsDown AndAlso IsEnabled AndAlso IsMouseOver Then ' DownOver
				If m_StateCtrls(3) Is Nothing Then
					m_StateCtrls(2).IsVisible = True
				Else
					m_StateCtrls(3).IsVisible = True
				End If
			ElseIf ((Not IsDown)) AndAlso ((Not IsEnabled)) Then ' NormalDisabledState
				If m_StateCtrls(4) Is Nothing Then
					m_StateCtrls(0).IsVisible = True
				Else
					m_StateCtrls(4).IsVisible = True
				End If
			ElseIf IsDown AndAlso ((Not IsEnabled)) Then ' DownDisabledState
				If m_StateCtrls(5) Is Nothing Then
					m_StateCtrls(0).IsVisible = True
				Else
					m_StateCtrls(5).IsVisible = True
				End If
			Else
				m_StateCtrls(0).IsVisible = True
			End If

			MyBase.Render(inChainLeft, inChainTop)
		End Sub
	End Class
End Namespace
