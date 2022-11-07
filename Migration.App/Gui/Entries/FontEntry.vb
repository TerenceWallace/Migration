Namespace Migration
	Friend Class FontEntry
		Implements IComparable(Of FontEntry), IComparer(Of FontEntry)

		Private ReadOnly m_Glyphs As New SortedList(Of Char, Glyph)()

		Private privateFamily As String
		Public Property Family() As String
			Get
				Return privateFamily
			End Get
			Private Set(ByVal value As String)
				privateFamily = value
			End Set
		End Property
		Private privateSize As Single
		Public Property Size() As Single
			Get
				Return privateSize
			End Get
			Private Set(ByVal value As Single)
				privateSize = value
			End Set
		End Property
		Private privateStyle As FontStyle
		Public Property Style() As FontStyle
			Get
				Return privateStyle
			End Get
			Private Set(ByVal value As FontStyle)
				privateStyle = value
			End Set
		End Property
		Private privateColor As Color
		Public Property Color() As Color
			Get
				Return privateColor
			End Get
			Private Set(ByVal value As Color)
				privateColor = value
			End Set
		End Property
		Private privateFont As Font
		Public Property Font() As Font
			Get
				Return privateFont
			End Get
			Private Set(ByVal value As Font)
				privateFont = value
			End Set
		End Property
		Private privateImageID As Integer
		Public Property ImageID() As Integer
			Get
				Return privateImageID
			End Get
			Private Set(ByVal value As Integer)
				privateImageID = value
			End Set
		End Property

		Private Sub New()
		End Sub

		Public Sub New(ByVal inFamily As String, ByVal inSize As Single, ByVal inStyle As FontStyle, ByVal inColor As Color)
			Family = inFamily
			Size = inSize
			Style = inStyle
			Color = inColor
			Font = New Font(Family, Size, Style, GraphicsUnit.Pixel)
		End Sub

		Public Function GetGlyph(ByVal inChar As Char) As Glyph
            Return m_Glyphs(ChrW(AscW(inChar)))
		End Function

		Public Sub CreateTextureFont()
			Dim availableGlyphs As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!""§$%&/()=?{[]}\*+~'#-_.:,;<>|ßäöüÜÖÄ@€ "

			' prepare and measure glyphs
			Dim maxHeight As Integer = 0
			Dim maxWidth As Integer = 0

			For i As Integer = 0 To availableGlyphs.Length - 1
				Dim glyph As New Glyph(availableGlyphs.Chars(i), Font)

				m_Glyphs.Add(availableGlyphs.Chars(i), glyph)
				maxHeight = Math.Max(maxHeight, glyph.Size.Height)
				maxWidth = Math.Max(maxWidth, glyph.Size.Width)
			Next i

			' render glyphs to texture and record their atlas coordinates
			Dim glyphsPerLine As Integer = Convert.ToInt32(CInt(Fix(Math.Ceiling(Math.Sqrt(availableGlyphs.Length)))))
			Dim fontImage As New Bitmap(glyphsPerLine * maxWidth, glyphsPerLine * maxHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
			Dim graphics As Graphics = System.Drawing.Graphics.FromImage(fontImage)

			Dim y As Integer = 0
			Dim offset As Integer = 0
			Do While y < glyphsPerLine
				For x As Integer = 0 To glyphsPerLine - 1
					If offset >= m_Glyphs.Count Then
						Exit For
					End If

					Dim glyph As Glyph = m_Glyphs.Values(offset)
					offset += 1

					glyph.TexFrame = New Rectangle(x * maxWidth, y * maxHeight, glyph.Size.Width, glyph.Size.Height)

					graphics.DrawString(glyph.Char.ToString(), Font, New SolidBrush(Color), x * maxWidth, y * maxHeight)
				Next x
				y += 1
			Loop

			ImageID = Gui.Loader.GUIConfig.RegisterImage(fontImage)
		End Sub

		Public Function Compare(ByVal x As FontEntry, ByVal y As FontEntry) As Integer Implements IComparer(Of FontEntry).Compare
			If x.Size < y.Size Then
				Return -1
			End If

			If x.Size > y.Size Then
				Return 1
			End If

			If x.Style > y.Style Then
				Return 1
			End If

			If x.Style < y.Style Then
				Return -1
			End If

			Dim res As Integer = 0

			res = x.Family.CompareTo(y.Family)
			If res <> 0 Then
				Return res
			End If

			If x.Color.ToArgb() > y.Color.ToArgb() Then
				Return 1
			End If

			If x.Color.ToArgb() < y.Color.ToArgb() Then
				Return -1
			End If

			Return 0
		End Function

		Public Function CompareTo(ByVal other As FontEntry) As Integer Implements IComparable(Of FontEntry).CompareTo
			Return Compare(Me, other)
		End Function
	End Class

End Namespace
