Namespace Migration
	Public Class Glyph

		Private privateChar As Char
		Public Property [Char]() As Char
			Get
				Return privateChar
			End Get
			Private Set(ByVal value As Char)
				privateChar = value
			End Set
		End Property

		Private privateSize As Size
		Public Property Size() As Size
			Get
				Return privateSize
			End Get
			Private Set(ByVal value As Size)
				privateSize = value
			End Set
		End Property

		Private privateTexFrame As Rectangle
		Public Property TexFrame() As Rectangle
			Get
				Return privateTexFrame
			End Get
			Set(ByVal value As Rectangle)
				privateTexFrame = value
			End Set
		End Property

		Public Sub New(ByVal inChar As Char, ByVal inFont As Font)
			[Char] = inChar
			Size = System.Windows.Forms.TextRenderer.MeasureText(New String(inChar, 1), inFont)
		End Sub
	End Class
End Namespace