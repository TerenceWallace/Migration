Imports Migration.Core

Namespace Migration
	Friend Class SolverMap
		Private Map() As PathFinderNode
		Private Width As Integer
		Private Height As Integer
		Private Count As Integer
		Private SizeShift As Integer

		Default Friend Property Item(ByVal x As Integer, ByVal y As Integer) As PathFinderNode
			Get
				Return Map(x + (y << SizeShift))
			End Get
			Set(ByVal value As PathFinderNode)
				Map(x + (y << SizeShift)) = value
			End Set
		End Property

		Default Friend Property Item(ByVal point As Point) As PathFinderNode
			Get
				Return Map(point.X + (point.Y << SizeShift))
			End Get
			Set(ByVal value As PathFinderNode)
				Map(point.X + (point.Y << SizeShift)) = value
			End Set
		End Property


		Friend ReadOnly Property IsEmpty() As Boolean
			Get
				Return Count = 0
			End Get
		End Property

		Friend Sub New(ByVal inSize As Integer)
			Dim log2Factor As Double = 1.0 / Math.Log(2.0)

			SizeShift = Convert.ToInt32(CInt(Fix(Math.Floor((Math.Log(Convert.ToDouble(inSize)) * log2Factor) + 0.5))))

			If Convert.ToInt32(CInt(Fix(Math.Pow(2, Convert.ToDouble(SizeShift))))) <> inSize Then
				Throw New ArgumentException()
			End If

			Map = New PathFinderNode(inSize * inSize - 1){}
			Width = inSize
			Height = inSize
		End Sub

		Friend Sub Add(ByVal inValue As PathFinderNode)
			Count += 1
			Me(inValue.X, inValue.Y) = inValue
		End Sub

		Friend Function IsSet(ByVal inValue As Point) As Boolean
			Return Me(inValue.X, inValue.Y) IsNot Nothing
		End Function

		Friend Function IsSet(ByVal inValue As PathFinderNode) As Boolean
			Return Me(inValue.X, inValue.Y) IsNot Nothing
		End Function

		Friend Sub Remove(ByVal inValue As PathFinderNode)
			Count -= 1
			Me(inValue.X, inValue.Y) = Nothing
		End Sub

		Friend Sub Clear()
			Count = 0
			Array.Clear(Map, 0, Map.Length)
		End Sub
	End Class
End Namespace