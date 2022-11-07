Imports System.Reflection
Imports System.Xml
Imports System.Xml.Serialization
Imports Migration.Rendering

Namespace Migration
	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class Label
		Inherits Control

		Private Shared ReadOnly m_TextureFonts As New SortedDictionary(Of FontEntry, FontEntry)()

		Private m_Font As FontEntry = Nothing

		Private privateText As String
		<XmlAttribute> _
		Public Property Text() As String
			Get
				Return privateText
			End Get
			Set(ByVal value As String)
				privateText = value
			End Set
		End Property

		Private privateFontFamily As String
		<XmlAttribute> _
		Public Property FontFamily() As String
			Get
				Return privateFontFamily
			End Get
			Set(ByVal value As String)
				privateFontFamily = value
			End Set
		End Property

		Private privateFontSize As Single
		<XmlAttribute> _
		Public Property FontSize() As Single
			Get
				Return privateFontSize
			End Get
			Set(ByVal value As Single)
				privateFontSize = value
			End Set
		End Property

		Private privateFontIndent As Integer
		<XmlAttribute> _
		Public Property FontIndent() As Integer
			Get
				Return privateFontIndent
			End Get
			Set(ByVal value As Integer)
				privateFontIndent = value
			End Set
		End Property

		Private privateFontStyle As FontStyle
		<XmlAttribute> _
		Public Property FontStyle() As FontStyle
			Get
				Return privateFontStyle
			End Get
			Set(ByVal value As FontStyle)
				privateFontStyle = value
			End Set
		End Property

		Private privateFontColorString As String
		<XmlAttribute("FontColor")> _
		Public Property FontColorString() As String
			Get
				Return privateFontColorString
			End Get
			Set(ByVal value As String)
				privateFontColorString = value
			End Set
		End Property

		Private privateFontColor As Color
		<XmlIgnore> _
		Public Property FontColor() As Color
			Get
				Return privateFontColor
			End Get
			Private Set(ByVal value As Color)
				privateFontColor = value
			End Set
		End Property

		Public Overrides Sub XMLPostProcess(ByVal inLayout As XMLGUILayout)
			MyBase.XMLPostProcess(inLayout)

			If String.IsNullOrEmpty(FontColorString) Then
				Throw New ArgumentException("No font color specified.")
			End If

			If String.IsNullOrEmpty(FontFamily) Then
				Throw New ArgumentException("No font family specified.")
			End If

			Dim mFontFamily As System.Drawing.FontFamily = Nothing

			Try
				mFontFamily = New System.Drawing.FontFamily(FontFamily)
			Catch
				Throw New ArgumentException("Font family """ & FontFamily & """ is unknown.")
			End Try

			FontColor = Color.FromName(FontColorString)

			' create texture font
			Dim fontEntry As New FontEntry(FontFamily, FontSize, FontStyle, FontColor)

			SyncLock m_TextureFonts
				If Not(m_TextureFonts.ContainsKey(fontEntry)) Then
					m_Font = fontEntry

					m_Font.CreateTextureFont()
					m_TextureFonts.Add(m_Font, m_Font)
				Else
					m_Font = m_TextureFonts(fontEntry)
				End If
			End SyncLock
		End Sub

		Public Sub New()
			MyBase.New()
			'Dim families() As FontFamily = System.Drawing.FontFamily.Families

			FontColorString = "White"
			FontStyle = System.Drawing.FontStyle.Regular
			FontSize = 12F
			FontIndent = -3
			FontFamily = "Arial"
		End Sub

		Public Function MeasureSize(ByVal inText As String) As Point
			If Not(String.IsNullOrEmpty(Text)) Then

				Dim  m_width As Integer = 0

				Dim  m_height As Integer = 0

				For i As Integer = 0 To Text.Length - 1
					Dim glyph As Glyph = m_Font.GetGlyph(Text.Chars(i))

					 m_width += glyph.Size.Width + FontIndent
					 m_height = Math.Max( m_height, glyph.Size.Height)
				Next i

				Return New Point( m_width,  m_height)
			Else
				Return New Point(0, 0)
			End If
		End Function

		Protected Overrides Sub Render(ByVal inChainLeft As Integer, ByVal inChainTop As Integer)
			If Not(String.IsNullOrEmpty(Text)) Then
				Dim i As Integer = 0

				Dim  m_left As Integer = Me.Left
				Do While i < Text.Length
					Dim glyph As Glyph = m_Font.GetGlyph(Text.Chars(i))

					Renderer.DrawSpriteAtlas((inChainLeft +  m_left) * WidthScale, (inChainTop + Top) * HeightScale, glyph.Size.Width * WidthScale, glyph.Size.Height * HeightScale, m_Font.ImageID, Opacity, glyph.TexFrame.X, glyph.TexFrame.Y, glyph.TexFrame.Width, glyph.TexFrame.Height)

					 m_left += glyph.Size.Width + FontIndent
					i += 1
				Loop
			End If

			MyBase.Render(inChainLeft + Left, inChainTop + Top)
		End Sub
	End Class
End Namespace
