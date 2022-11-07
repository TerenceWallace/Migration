Imports System.Reflection
Imports System.Xml
Imports System.Xml.Serialization
Imports Migration.Common
Imports Migration.Rendering

Namespace Migration

	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class Image
		Inherits Control

		Private m_Config As ImageEntry
		Private m_SourceString As String
		Private m_WidthOverride As Integer
		Private m_HeightOverride As Integer

		Private privateHorizontalAlignment As ImageAlignment
		<XmlAttribute> _
		Public Property HorizontalAlignment() As ImageAlignment
			Get
				Return privateHorizontalAlignment
			End Get
			Set(ByVal value As ImageAlignment)
				privateHorizontalAlignment = value
			End Set
		End Property

		Private privateVerticalAlignment As ImageAlignment
		<XmlAttribute> _
		Public Property VerticalAlignment() As ImageAlignment
			Get
				Return privateVerticalAlignment
			End Get
			Set(ByVal value As ImageAlignment)
				privateVerticalAlignment = value
			End Set
		End Property

		Private privateKeepAspectRatio As Boolean
		<XmlAttribute> _
		Public Property KeepAspectRatio() As Boolean
			Get
				Return privateKeepAspectRatio
			End Get
			Set(ByVal value As Boolean)
				privateKeepAspectRatio = value
			End Set
		End Property

		<XmlAttribute("Source")> _
		Public Property SourceString() As String
			Get
				Return m_SourceString
			End Get
			Set(ByVal value As String)
				m_SourceString = value

				If value.Contains("{") AndAlso value.Contains("}") Then
					Return
				End If

				m_Config = Gui.Loader.GUIConfig.GetImage(value)

				ProcessScaling()
			End Set
		End Property

		Public Overrides Sub XMLPostProcess(ByVal inLayout As XMLGUILayout)
			MyBase.XMLPostProcess(inLayout)

			ProcessScaling()
		End Sub

		Public Sub New()
			MyBase.New()
		End Sub

		Public Sub ProcessScaling()
			If m_Config Is Nothing Then
				Return
			End If

			Dim aspect As Double = m_Config.Image.Width / Convert.ToDouble(m_Config.Image.Height)
			Dim hasWidth As Boolean = Width > 0
			Dim hasHeight As Boolean = Height > 0

			If Not hasWidth Then
				m_WidthOverride = m_Config.Image.Width
			Else
				m_WidthOverride = Width
			End If

			If Not hasHeight Then
				m_HeightOverride = m_Config.Image.Height
			Else
				m_HeightOverride = Height
			End If

			If KeepAspectRatio AndAlso (hasWidth OrElse hasHeight) Then
				If hasHeight Xor hasWidth Then
					' exactly one dimension is not specified
					If hasHeight Then
						m_WidthOverride = Convert.ToInt32(CInt(Fix(Height * aspect)))
					Else ' hasWidth
						m_HeightOverride = Convert.ToInt32(CInt(Fix(Width / aspect)))
					End If
				Else
					' two dimensions are specified
					m_WidthOverride = Convert.ToInt32(CInt(Fix(Height * aspect)))
					m_HeightOverride = Convert.ToInt32(CInt(Fix(Width / aspect)))

					If m_WidthOverride > Width Then
						m_WidthOverride = Width
						m_HeightOverride = Convert.ToInt32(CInt(Fix(Width / aspect)))
					End If

					If m_HeightOverride > Height Then
						m_HeightOverride = Height
						m_WidthOverride = Convert.ToInt32(CInt(Fix(Height * aspect)))
					End If
				End If
			End If
		End Sub

		Protected Overrides Sub Render(ByVal inChainLeft As Integer, ByVal inChainTop As Integer)
			Dim mLeft As Integer = MyBase.Left
			Dim mTop As Integer = MyBase.Top

			Select Case HorizontalAlignment
				Case ImageAlignment.Left
				Case ImageAlignment.Center
					mLeft = (Parent.Width - m_WidthOverride) \ 2
				Case ImageAlignment.Right
					mLeft = Parent.Width - m_WidthOverride
			End Select

			Select Case VerticalAlignment
				Case ImageAlignment.Left
				Case ImageAlignment.Center
					mTop = (Parent.Height - m_HeightOverride) \ 2
				Case ImageAlignment.Right
					mTop = Parent.Height - m_HeightOverride
			End Select

			Dim mXArg As Double = Convert.ToDouble((inChainLeft + mLeft) * WidthScale)
			Dim mYArg As Double = Convert.ToDouble(inChainTop + mTop) * HeightScale

			Renderer.DrawSprite(mXArg, mYArg, m_WidthOverride * WidthScale, m_HeightOverride * HeightScale, m_Config.ImageID, Opacity)

			MyBase.Render(inChainLeft, inChainTop)
		End Sub
	End Class
End Namespace
