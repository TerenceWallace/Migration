Namespace Migration

	''' <summary>
	''' A cycle point is a key element in deterministic, discretized space-time. Even if it provides
	''' double values for rendering, it internally uses exact integers to manage its "solid" state.
	''' </summary>
	Public Structure CyclePoint
		''' <summary>
		''' The only deterministic way to change a cycle point, by raw cycle manipulation.
		''' </summary>
		Friend Property XCycles() As Int64
		''' <summary>
		''' The only deterministic way to change a cycle point, by raw cycle manipulation.
		''' </summary>
		Friend Property YCycles() As Int64

		Public Const CYCLE_MILLIS As Double = 33

		Public Shared Operator <>(ByVal inA As CyclePoint, ByVal inB As CyclePoint) As Boolean
			Return (inA.XCycles <> inB.XCycles) OrElse (inA.YCycles <> inB.YCycles)
		End Operator

		Public Shared Operator =(ByVal inA As CyclePoint, ByVal inB As CyclePoint) As Boolean
			Return (inA.XCycles = inB.XCycles) AndAlso (inA.YCycles = inB.YCycles)
		End Operator

		Public Overrides Function Equals(ByVal obj As Object) As Boolean
			If TypeOf obj Is CyclePoint Then
				Return (CType(obj, CyclePoint)) = Me
			Else
				Return False
			End If
		End Function

		''' <summary>
		''' Returns the floored X offset in terms of grid cells.
		''' </summary>
		Public ReadOnly Property XGrid() As Int32
			Get
				Return Convert.ToInt32(CInt(Fix(Math.Floor(XCycles / CYCLE_MILLIS))))
			End Get
		End Property

		''' <summary>
		''' Returns the floored Y offset in terms of grid cells.
		''' </summary>
		Public ReadOnly Property YGrid() As Int32
			Get
				Dim FlooredY As Integer = Convert.ToInt32(CInt(Fix(Math.Floor(YCycles / CYCLE_MILLIS))))
				Return FlooredY
			End Get
		End Property

		''' <summary>
		''' Returns the double precision Y offset in terms of grid cells.
		''' </summary>
		Public ReadOnly Property Y() As Double
			Get
				Return YCycles / CYCLE_MILLIS
			End Get
		End Property

		''' <summary>
		''' Returns the double precision X offset in terms of grid cells.
		''' </summary>
		Public ReadOnly Property X() As Double
			Get
				Return XCycles / CYCLE_MILLIS
			End Get
		End Property

		''' <summary>
		''' Converts a grid position into a cycle point.
		''' </summary>
		Public Shared Function FromGrid(ByVal inPoint As Point) As CyclePoint
			Return FromGrid(inPoint.X, inPoint.Y)
		End Function

		''' <summary>
		''' Converts a grid position into a cycle point.
		''' </summary>
		Public Shared Function FromGrid(ByVal inX As Double, ByVal inY As Double) As CyclePoint
			Dim result As New CyclePoint()

			result.XCycles = Convert.ToInt64(Math.Ceiling(inX * CYCLE_MILLIS))
			result.YCycles = Convert.ToInt64(Math.Ceiling(inY * CYCLE_MILLIS))

			Return result
		End Function

		''' <summary>
		''' Converts a cycle point into an integer grid position.
		''' </summary>
		Public Function ToPoint() As Point
			Return New Point(XGrid, YGrid)
		End Function

		''' <summary>
		''' Initializes a cycle point structure with raw cycle offsets. You should
		''' only use this when you know what you are doing ;).
		''' </summary>
		Public Sub New(ByVal inXCycles As Int64, ByVal inYCycles As Int64)
            XCycles = inXCycles
			YCycles = inYCycles
		End Sub

		''' <summary>
		''' The only deterministic way to change a cycle point, by raw cycle manipulation.
		''' </summary>
		Public Function AddCycleVector(ByVal inCycleVector As Point) As CyclePoint
			Return New CyclePoint(XCycles + inCycleVector.X, YCycles + inCycleVector.Y)
		End Function

		Public Overrides Function ToString() As String
			Return String.Format("XCycles: {0} ({2:0.##}); YCycles: {1} ({3:0.##})", XCycles, YCycles, X, Y)
		End Function
	End Structure
End Namespace
