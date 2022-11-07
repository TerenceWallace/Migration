Namespace Migration
	Friend Class PerlinNoise

		Private Const SAMPLE_SIZE As Integer = 1024
		Private Const B As Integer = SAMPLE_SIZE
		Private Const BM As Integer = (SAMPLE_SIZE - 1)
		Private Const N As Integer = &H1000
		Private Const NP As Integer = 12
		Private Const NM As Integer = &HFFF

		Private ReadOnly mOctaves As Integer
		Private ReadOnly mFrequency As Double
		Private ReadOnly mAmplitude As Double
		Private ReadOnly mSeed As Integer
		Private ReadOnly mRngCtx As CrossRandom

		Private ReadOnly p(SAMPLE_SIZE + SAMPLE_SIZE + 2 - 1) As Integer
		Private ReadOnly g3(SAMPLE_SIZE + SAMPLE_SIZE + 2 - 1)() As Double
		Private ReadOnly g2(SAMPLE_SIZE + SAMPLE_SIZE + 2 - 1)() As Double
		Private ReadOnly g1(SAMPLE_SIZE + SAMPLE_SIZE + 2 - 1) As Double

		Friend Sub New(ByVal octaves As Integer, ByVal freq As Double, ByVal amp As Double, ByVal seed As Integer)
			Dim i As Integer = 0
			Dim j As Integer = 0
			Dim k As Integer = 0

			mOctaves = octaves
			mFrequency = freq
			mAmplitude = amp
			mSeed = seed
			mRngCtx = New CrossRandom(mSeed)

			For i = 0 To g3.Length - 1
				g3(i) = New Double(2){}
				g2(i) = New Double(1){}
			Next i

			For i = 0 To B - 1
				p(i) = i
				g1(i) = Convert.ToDouble((mRngCtx.Next() Mod (B + B)) - B) / B
				For j = 0 To 1
					g2(i)(j) = Convert.ToDouble((mRngCtx.Next() Mod (B + B)) - B) / B
				Next j
				Normalize2(g2(i))
				For j = 0 To 2
					g3(i)(j) = Convert.ToDouble((mRngCtx.Next() Mod (B + B)) - B) / B
				Next j
				Normalize3(g3(i))
			Next i

			i -= 1
			Do While i > 0
				k = p(i)
				Dim mNext As Integer = mRngCtx.Next()
				j = mNext Mod B
				p(i) = p(j)
				p(j) = k
				i -= 1
			Loop

			For i = 0 To B + 2 - 1
				p(B + i) = p(i)
				g1(B + i) = g1(i)

				For j = 0 To 1
					g2(B + i)(j) = g2(i)(j)
				Next j

				For j = 0 To 2
					g3(B + i)(j) = g3(i)(j)
				Next j
			Next i

		End Sub

		Friend Sub FillNoiseArray2D(ByVal inOctaves As Integer, ByVal inFrequency As Double, ByVal inAmplitude As Double, ByVal inSeed As Integer, ByVal inWidth As Integer, ByVal inHeight As Integer, ByVal outArray() As Double)
			Dim noise As New PerlinNoise(inOctaves, inFrequency, inAmplitude, inSeed)
			Dim vec(1) As Double

			Dim y As Integer = 0
			Dim offset As Integer = 0
			Do While y < inHeight
				For x As Integer = 0 To inWidth - 1
					vec(0) = x
					vec(1) = y

					outArray(offset) = noise.Perlin_noise_2D(vec)
					offset += 1
				Next x
				y += 1
			Loop

		End Sub

		Friend Sub FillNoiseArray3D(ByVal inOctaves As Integer, ByVal inFrequency As Double, ByVal inAmplitude As Double, ByVal inSeed As Integer, ByVal inWidth As Integer, ByVal inHeight As Integer, ByVal inDepth As Integer, ByVal outArray() As Double)
			Dim noise As New PerlinNoise(inOctaves, inFrequency, inAmplitude, inSeed)
			Dim vec(2) As Double

			For z As Integer = 0 To inDepth - 1
				Dim y As Integer = 0
				Dim offset As Integer = 0
				Do While y < inHeight
					For x As Integer = 0 To inWidth - 1
						vec(0) = x
						vec(1) = y
						vec(2) = z

						outArray(offset) = noise.Perlin_noise_3D(vec)
						offset += 1
					Next x
					y += 1
				Loop
			Next z
		End Sub

		Private Shared Function s_curve(ByVal t As Double) As Double
			Return (t * t * (3.0F - 2.0F * t))
		End Function

		Private Shared Function lerp(ByVal t As Double, ByVal a As Double, ByVal b As Double) As Double
			Return (a + t * (b - a))
		End Function

		Private Function Noise1(ByVal arg As Double) As Double
			Dim bx0 As Integer = 0
			Dim bx1 As Integer = 0
			Dim rx0 As Double = 0
			Dim rx1 As Double = 0
			Dim sx As Double = 0
			Dim t As Double = 0
			Dim u As Double = 0
			Dim v As Double = 0
			Dim vec(0) As Double

			vec(0) = arg

			t = vec(0) + N
			bx0 = (Convert.ToInt32(CInt(Fix(t)))) And BM
			bx1 = (bx0 + 1) And BM
			rx0 = t - Convert.ToInt32(CInt(Fix(t)))
			rx1 = rx0 - 1.0F

			sx = s_curve(rx0)

			u = rx0 * g1(p(bx0))
			v = rx1 * g1(p(bx1))

			Return lerp(sx, u, v)
		End Function

		Private Function Noise2(ByVal vec() As Double) As Double
			Dim bx0 As Integer = 0
			Dim bx1 As Integer = 0
			Dim by0 As Integer = 0
			Dim by1 As Integer = 0
			Dim b00 As Integer = 0
			Dim b10 As Integer = 0
			Dim b01 As Integer = 0
			Dim b11 As Integer = 0
			Dim rx0 As Double = 0
			Dim rx1 As Double = 0
			Dim ry0 As Double = 0
			Dim ry1 As Double = 0
			Dim sx As Double = 0
			Dim sy As Double = 0
			Dim a As Double = 0

			Dim  m_b As Double = 0
			Dim t As Double = 0
			Dim u As Double = 0
			Dim v As Double = 0
			Dim q() As Double = Nothing
			Dim i As Integer = 0
			Dim j As Integer = 0

			t = vec(0) + N
			bx0 = (Convert.ToInt32(CInt(Fix(t)))) And BM
			bx1 = (bx0 + 1) And BM
			rx0 = t - Convert.ToInt32(CInt(Fix(t)))
			rx1 = rx0 - 1.0F

			t = vec(1) + N
			by0 = (Convert.ToInt32(CInt(Fix(t)))) And BM
			by1 = (by0 + 1) And BM
			ry0 = t - Convert.ToInt32(CInt(Fix(t)))
			ry1 = ry0 - 1.0F

			i = p(bx0)
			j = p(bx1)

			b00 = p(i + by0)
			b10 = p(j + by0)
			b01 = p(i + by1)
			b11 = p(j + by1)

			sx = s_curve(rx0)
			sy = s_curve(ry0)

			q = g2(b00)
			u = (rx0 * q(0) + ry0 * q(1))
			q = g2(b10)
			v = (rx1 * q(0) + ry0 * q(1))
			a = lerp(sx, u, v)

			q = g2(b01)
			u = (rx0 * q(0) + ry1 * q(1))
			q = g2(b11)
			v = (rx1 * q(0) + ry1 * q(1))
			 m_b = lerp(sx, u, v)

			Return lerp(sy, a,  m_b)
		End Function

		Private Function Noise3(ByVal vec() As Double) As Double
			Dim bx0 As Integer = 0
			Dim bx1 As Integer = 0
			Dim by0 As Integer = 0
			Dim by1 As Integer = 0
			Dim bz0 As Integer = 0
			Dim bz1 As Integer = 0
			Dim b00 As Integer = 0
			Dim b10 As Integer = 0
			Dim b01 As Integer = 0
			Dim b11 As Integer = 0
			Dim rx0 As Double = 0
			Dim rx1 As Double = 0
			Dim ry0 As Double = 0
			Dim ry1 As Double = 0
			Dim rz0 As Double = 0
			Dim rz1 As Double = 0
			Dim sy As Double = 0
			Dim sz As Double = 0
			Dim a As Double = 0

			Dim  m_b As Double = 0
			Dim c As Double = 0
			Dim d As Double = 0
			Dim t As Double = 0
			Dim u As Double = 0
			Dim v As Double = 0
			Dim q() As Double = Nothing
			Dim i As Integer = 0
			Dim j As Integer = 0

			t = vec(0) + N
			bx0 = (Convert.ToInt32(CInt(Fix(t)))) And BM
			bx1 = (bx0 + 1) And BM
			rx0 = t - Convert.ToInt32(CInt(Fix(t)))
			rx1 = rx0 - 1.0F

			t = vec(1) + N
			by0 = (Convert.ToInt32(CInt(Fix(t)))) And BM
			by1 = (by0 + 1) And BM
			ry0 = t - Convert.ToInt32(CInt(Fix(t)))
			ry1 = ry0 - 1.0F

			t = vec(2) + N
			bz0 = (Convert.ToInt32(CInt(Fix(t)))) And BM
			bz1 = (bz0 + 1) And BM
			rz0 = t - Convert.ToInt32(CInt(Fix(t)))
			rz1 = rz0 - 1.0F

			i = p(bx0)
			j = p(bx1)

			b00 = p(i + by0)
			b10 = p(j + by0)
			b01 = p(i + by1)
			b11 = p(j + by1)

			t = s_curve(rx0)
			sy = s_curve(ry0)
			sz = s_curve(rz0)

			q = g3(b00 + bz0)
			u = (rx0 * q(0) + ry0 * q(1) + rz0 * q(2))
			q = g3(b10 + bz0)
			v = (rx1 * q(0) + ry0 * q(1) + rz0 * q(2))
			a = lerp(t, u, v)

			q = g3(b01 + bz0)
			u = (rx0 * q(0) + ry1 * q(1) + rz0 * q(2))
			q = g3(b11 + bz0)
			v = (rx1 * q(0) + ry1 * q(1) + rz0 * q(2))
			 m_b = lerp(t, u, v)

			c = lerp(sy, a,  m_b)

			q = g3(b00 + bz1)
			u = (rx0 * q(0) + ry0 * q(1) + rz1 * q(2))
			q = g3(b10 + bz1)
			v = (rx1 * q(0) + ry0 * q(1) + rz1 * q(2))
			a = lerp(t, u, v)

			q = g3(b01 + bz1)
			u = (rx0 * q(0) + ry1 * q(1) + rz1 * q(2))
			q = g3(b11 + bz1)
			v = (rx1 * q(0) + ry1 * q(1) + rz1 * q(2))
			 m_b = lerp(t, u, v)

			d = lerp(sy, a,  m_b)

			Return lerp(sz, c, d)
		End Function

		Private Sub Normalize2(ByVal v() As Double)
			Dim s As Double = 0

			s = Convert.ToDouble(Math.Sqrt(v(0) * v(0) + v(1) * v(1)))
			s = 1.0F / s
			v(0) = v(0) * s
			v(1) = v(1) * s
		End Sub

		Private Sub Normalize3(ByVal v() As Double)
			Dim s As Double = 0

			s = Convert.ToDouble(Math.Sqrt(v(0) * v(0) + v(1) * v(1) + v(2) * v(2)))
			s = 1.0F / s

			v(0) = v(0) * s
			v(1) = v(1) * s
			v(2) = v(2) * s
		End Sub

		Friend Function Perlin_noise_3D(ByVal vec() As Double) As Double
			Dim terms As Integer = mOctaves
			Dim freq As Double = mFrequency
			Dim result As Double = 0.0F
			Dim amp As Double = mAmplitude

			vec(0) *= mFrequency
			vec(1) *= mFrequency
			vec(2) *= mFrequency

			For i As Integer = 0 To terms - 1
				result += Noise3(vec) * amp
				vec(0) *= 2.0F
				vec(1) *= 2.0F
				vec(2) *= 2.0F
				amp *= 0.5F
			Next i


			Return result
		End Function

		Friend Function Perlin_noise_2D(ByVal x As Double, ByVal y As Double) As Double
			Dim vec(1) As Double

			vec(0) = x
			vec(1) = y

			Return Perlin_noise_2D(vec)
		End Function

		Friend Function Perlin_noise_2D(ByVal vec() As Double) As Double
			Dim terms As Integer = mOctaves
			Dim freq As Double = mFrequency
			Dim result As Double = 0.0F
			Dim amp As Double = mAmplitude

			vec(0) *= mFrequency
			vec(1) *= mFrequency

			For i As Integer = 0 To terms - 1
				result += Noise2(vec) * amp
				vec(0) *= 2.0F
				vec(1) *= 2.0F
				amp *= 0.5F
			Next i

			Return result
		End Function
	End Class
End Namespace
