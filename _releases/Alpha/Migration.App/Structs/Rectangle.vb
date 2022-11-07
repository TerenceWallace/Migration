Imports System.Runtime.Serialization
Imports System.Xml.Serialization

Namespace Migration
	''' <summary>
	''' To prevent System.Drawing dependency just for this structure.
	''' </summary>
	<Serializable(), DataContract()> _
	Public Structure Rectangle
		<XmlAttribute(), DataMember()> _
		Public X As Integer
		<XmlAttribute(), DataMember()> _
		Public Y As Integer
		<XmlAttribute(), DataMember()> _
		Public Width As Integer
		<XmlAttribute(), DataMember()> _
		Public Height As Integer

		Public ReadOnly Property Top() As Integer
			Get
				Return Y
			End Get
		End Property
		Public ReadOnly Property Left() As Integer
			Get
				Return X
			End Get
		End Property
		Public ReadOnly Property Right() As Integer
			Get
				Return X + Width
			End Get
		End Property
		Public ReadOnly Property Bottom() As Integer
			Get
				Return Y + Height
			End Get
		End Property

		Public Sub Extend(ByVal inToContainPoint As Point)
			X = Math.Min(X, inToContainPoint.X)
			Y = Math.Min(Y, inToContainPoint.Y)
			Width = Math.Max(Width, inToContainPoint.X - X)
			Height = Math.Max(Height, inToContainPoint.Y - Y)
		End Sub

		Public Function Contains(ByVal inPoint As Point) As Boolean
			Return ((Left <= inPoint.X) AndAlso (Right >= inPoint.X) AndAlso (Top <= inPoint.Y) AndAlso (Bottom >= inPoint.Y))
		End Function

		Public Sub New(ByVal inX As Integer, ByVal inY As Integer, ByVal inWidth As Integer, ByVal inHeight As Integer)
            X = inX
			Y = inY
			Width = inWidth
			Height = inHeight
		End Sub

		Public Function CanEnclose(ByVal inRegion As Rectangle) As Boolean
			Return (inRegion.Width <= Width) AndAlso (inRegion.Height <= Height)
		End Function

		Public Shared Operator <>(ByVal inA As Rectangle, ByVal inB As Rectangle) As Boolean
			Return (inA.X <> inB.X) OrElse (inA.Y <> inB.Y) OrElse (inA.Width <> inB.Width) OrElse (inA.Height <> inB.Height)
		End Operator

		Public Shared Operator =(ByVal inA As Rectangle, ByVal inB As Rectangle) As Boolean
			Return (inA.X = inB.X) AndAlso (inA.Y = inB.Y) AndAlso (inA.Width = inB.Width) AndAlso (inA.Height = inB.Height)
		End Operator

		Public Overrides Function Equals(ByVal obj As Object) As Boolean
			If TypeOf obj Is Rectangle Then
				Return (CType(obj, Rectangle)) = Me
			Else
				Return False
			End If
		End Function

		Public Overrides Function ToString() As String
			Return String.Format("X:{0}, Y:{1}, W:{2}, H:{3}", X, Y, Width, Height)
		End Function
	End Structure
End Namespace
