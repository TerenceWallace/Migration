Namespace Migration
	<Serializable()> _
	Public Structure PointDouble
		Public X As Double
		Public Y As Double

		Public Sub New(ByVal inX As Double, ByVal inY As Double)
            X = inX
			Y = inY
		End Sub

		Public Overrides Function ToString() As String
			Return String.Format("X:{0:0.##}, Y:{1:0.##}", X, Y)
		End Function

		Public Shared Operator <>(ByVal inA As PointDouble, ByVal inB As PointDouble) As Boolean
			Return (inA.X <> inB.X) OrElse (inA.Y <> inB.Y)
		End Operator

		Public Shared Operator =(ByVal inA As PointDouble, ByVal inB As PointDouble) As Boolean
			Return (inA.X = inB.X) AndAlso (inA.Y = inB.Y)
		End Operator

		Public Function DistanceTo(ByVal inPoint As Point) As Double
			Return Math.Sqrt((inPoint.X - X) * (inPoint.X - X) + (inPoint.Y - Y) * (inPoint.Y - Y))
		End Function

		Public Overrides Function Equals(ByVal obj As Object) As Boolean
			If TypeOf obj Is PointDouble Then
				Return (CType(obj, PointDouble)) = Me
			Else
				Return False
			End If
		End Function
	End Structure
End Namespace
