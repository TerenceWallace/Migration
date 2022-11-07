Namespace Migration
	Friend Class TerrainBrush
		Private privateValues(,) As Integer
		Friend Property Values() As Integer(,)
			Get
				Return privateValues
			End Get
			Private Set(ByVal value As Integer(,))
				privateValues = value
			End Set
		End Property
		Private privateWidth As Integer
		Friend Property Width() As Integer
			Get
				Return privateWidth
			End Get
			Private Set(ByVal value As Integer)
				privateWidth = value
			End Set
		End Property
		Private privateHeight As Integer
		Friend Property Height() As Integer
			Get
				Return privateHeight
			End Get
			Private Set(ByVal value As Integer)
				privateHeight = value
			End Set
		End Property
		Private privateVariance As Integer
		Friend Property Variance() As Integer
			Get
				Return privateVariance
			End Get
			Private Set(ByVal value As Integer)
				privateVariance = value
			End Set
		End Property

		Friend Sub New(ByVal inWidth As Integer, ByVal inHeight As Integer)
			Values = New Integer(inWidth - 1, inHeight - 1){}
			Width = inWidth
			Height = inHeight
		End Sub

		Friend Shared Function CreateSphereBrush(ByVal inRadius As Integer, ByVal inHeight As Integer) As TerrainBrush
			Dim brush As New TerrainBrush(inRadius * 2, inRadius * 2)
			Dim [step] As Double = Math.PI / (inRadius * 2 - 1)
			Dim dx As Double = 0
			Dim dy As Double = 0

			For x As Integer = 0 To inRadius * 2 - 1
				For y As Integer = 0 To inRadius * 2 - 1
					brush.Values(x, y) = Convert.ToInt32(CInt(Fix(Math.Sin(dy) * Math.Sin(dx) * inHeight)))
					brush.Variance += brush.Values(x, y)

					dx += [step]
				Next y
				dx = 0
				dy += [step]
			Next x

			Return brush
		End Function
	End Class
End Namespace
