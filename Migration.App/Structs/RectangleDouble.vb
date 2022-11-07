Namespace Migration

	<Serializable()> _
	Public Structure RectangleDouble
		Public X As Double
		Public Y As Double
		Public Width As Double
		Public Height As Double

		Public ReadOnly Property Top() As Double
			Get
				Return Y
			End Get
		End Property
		Public ReadOnly Property Left() As Double
			Get
				Return X
			End Get
		End Property
		Public ReadOnly Property Right() As Double
			Get
				Return X + Width
			End Get
		End Property
		Public ReadOnly Property Bottom() As Double
			Get
				Return Y + Height
			End Get
		End Property

		Public Sub New(ByVal inX As Double, ByVal inY As Double, ByVal inWidth As Double, ByVal inHeight As Double)
            X = inX
			Y = inY
			Width = inWidth
			Height = inHeight
		End Sub

		Public Overrides Function ToString() As String
			Return String.Format("X:{0:0.##}, Y:{1:0.##}, W:{2:0.##}, H:{3:0.##}", X, Y, Width, Height)
		End Function
	End Structure
End Namespace
