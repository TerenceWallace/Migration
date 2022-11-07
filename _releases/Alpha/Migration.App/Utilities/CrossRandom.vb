Namespace System
	' C# Version Copyright (C) 2001-2004 Akihilo Kramot (Takel).  
	' C# porting from a C-program for MT19937, originaly coded by 
	' Takuji Nishimura, considering the suggestions by            
	' Topher Cooper and Marc Rieffel in July-Aug. 1997.           
	' This library is free software under the Artistic license:   
	'                                                             
	' You can find the original C-program at                      
	'     http://www.math.keio.ac.jp/~matumoto/mt.html            
	''' <summary>
	''' Mersenne Twister Random Generator
	''' </summary>
	<Serializable()> _
	Public Class CrossRandom

		' Period parameters 
		Private Const N As Integer = 624
		Private Const M As Integer = 397
		Private Const MATRIX_A As UInteger = &H9908B0DFL ' constant vector a
		Private Const UPPER_MASK As UInteger = &H80000000L ' most significant w-r bits
		Private Const LOWER_MASK As UInteger = &H7FFFFFFF ' least significant r bits

		' Tempering parameters 
		Private Const TEMPERING_MASK_B As UInteger = &H9D2C5680L
		Private Const TEMPERING_MASK_C As UInteger = &HEFC60000L

		Private Shared Function TEMPERING_SHIFT_U(ByVal y As UInteger) As UInteger
			Return (y >> 11)
		End Function

		Private Shared Function TEMPERING_SHIFT_S(ByVal y As UInteger) As UInteger
			Return (y << 7)
		End Function

		Private Shared Function TEMPERING_SHIFT_T(ByVal y As UInteger) As UInteger
			Return (y << 15)
		End Function

		Private Shared Function TEMPERING_SHIFT_L(ByVal y As UInteger) As UInteger
			Return (y >> 18)
		End Function

		Private mt(N - 1) As UInteger ' the array for the state vector

		Private mti As Short

		Private Shared mag01() As UInteger = { &H0, MATRIX_A }

		' initializing the array with a NONZERO seed 
		Public Sub New(ByVal seed As Integer)
			Me.mt = New UInt32(623){}
			If seed = 0 Then
				seed = &H1105
			End If

			Me.mt(0) = Convert.ToUInt32(seed And -1)
			Me.mti = 1

			Do While Me.mti < &H270
				Me.mt(Me.mti) = Convert.ToUInt32((&H10DCD * Me.mt((Me.mti - 1))) And UInt32.MaxValue)
				Me.mti = Convert.ToInt16((Me.mti + 1))
			Loop

		End Sub

		Public Sub New() ' a default initial seed is used
			Me.New(Environment.TickCount)
		End Sub

		Public Function GenerateUInt() As UInteger
			'			unchecked
			Dim y As UInteger = 0

			' mag01[x] = x * MATRIX_A  for x=0,1 
			If mti >= N Then ' generate N words at one time
				Dim kk As Short = 0

				Do While kk < N - M
					y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
					mt(kk) = mt(kk + M) Xor (y >> 1) Xor mag01(Convert.ToInt32(y And &H1))
					kk += Convert.ToInt16(1)
				Loop

				Do While kk < N - 1
					y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
					mt(kk) = mt(kk + (M - N)) Xor (y >> 1) Xor mag01(Convert.ToInt32(y And &H1))
					kk += Convert.ToInt16(1)
				Loop

				y = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
				mt(N - 1) = mt(M - 1) Xor (y >> 1) Xor mag01(Convert.ToInt32(y And &H1))

				mti = 0
			End If

			y = mt(mti)
			mti += Convert.ToInt16(1)
			y = y Xor TEMPERING_SHIFT_U(y)
			y = y Xor TEMPERING_SHIFT_S(y) And TEMPERING_MASK_B
			y = y Xor TEMPERING_SHIFT_T(y) And TEMPERING_MASK_C
			y = y Xor TEMPERING_SHIFT_L(y)

			Return y
		End Function

		Public Overridable Function NextUInt() As UInteger
			Return Me.GenerateUInt()
		End Function

		Public Overridable Function NextUInt(ByVal maxValue As UInteger) As UInteger
			Return Convert.ToUInt32(Me.GenerateUInt() / (Convert.ToDouble(UInteger.MaxValue) / maxValue))
		End Function

		Public Overridable Function NextUInt(ByVal minValue As UInteger, ByVal maxValue As UInteger) As UInteger ' throws ArgumentOutOfRangeException
			'			unchecked
			If minValue >= maxValue Then
				Throw New ArgumentOutOfRangeException()
			End If

			Return Convert.ToUInt32(Me.GenerateUInt() / (Convert.ToDouble(UInteger.MaxValue) / (maxValue - minValue)) + minValue)
		End Function

		Public Function [Next]() As Integer
			Return Me.Next(Integer.MaxValue)
		End Function

		Public Function [Next](ByVal maxValue As Integer) As Integer ' throws ArgumentOutOfRangeException
			'			unchecked
			If maxValue <= 1 Then
				If maxValue < 0 Then
					Throw New ArgumentOutOfRangeException()
				End If

				Return 0
			End If

			Return Convert.ToInt32(CInt(Fix(Me.NextDouble() * maxValue)))
		End Function

		Public Function [Next](ByVal minValue As Integer, ByVal maxValue As Integer) As Integer
			'			unchecked
			If maxValue < minValue Then
				Throw New ArgumentOutOfRangeException()
			ElseIf maxValue = minValue Then
				Return minValue
			Else
				Return Me.Next(maxValue - minValue) + minValue
			End If
		End Function

		Public Sub NextBytes(ByVal buffer() As Byte) ' throws ArgumentNullException
			'			unchecked
			Dim bufLen As Integer = buffer.Length

			If buffer Is Nothing Then
				Throw New ArgumentNullException()
			End If

			For idx As Integer = 0 To bufLen - 1
				buffer(idx) = Convert.ToByte(Me.Next(256))
			Next idx
		End Sub

		Public Function NextDouble() As Double
			'			unchecked
			Return Convert.ToDouble(Me.GenerateUInt()) / (Convert.ToUInt64(UInteger.MaxValue) + 1)
		End Function
	End Class

End Namespace
