Imports Migration.Common


Namespace Migration
	Friend Class RandomTerrainGenerator
		Inherits AbstractTerrainGenerator

		Public Shared Sub Run(ByVal inMap As Migration.Game.Map, ByVal inSeed As Long)
			Dim TempRandomTerrainGenerator As New RandomTerrainGenerator(inMap, inSeed)
		End Sub

		Private Sub New(ByVal inMap As Migration.Game.Map, ByVal inSeed As Long)
			MyBase.New(inMap, inSeed)
			Dim heights(Size * Size - 1) As Double
			Dim foilages(Size * Size - 1) As Double
			Dim resources(Size * Size - 1) As Double

			GenerateLayers(Convert.ToInt32(CInt(inSeed)), Size, Size, heights, foilages, resources)

			Dim x As Integer = 0
			Dim offset As Integer = 0
			Do While x < Size
				Dim y As Integer = 0
				Do While y < Size
					Dim height As Double = heights(offset)
					Dim foilage As Double = foilages(offset)

					Terrain.SetHeightAt(x, y, Convert.ToByte(128 + 127 * height))

					If (foilage > -0.8) AndAlso (foilage < -0.5) Then
						Terrain.SetFlagsAt(x, y, TerrainCellFlags.Tree_01)
					End If
					y += 1
					offset += 1
				Loop
				x += 1
			Loop
		End Sub

		Private Sub GenerateLayers(ByVal inSeed As Integer, ByVal inWidth As Integer, ByVal inHeight As Integer, ByVal outHeightLayer() As Double, ByVal outFoilageLayer() As Double, ByVal outResourceLayer() As Double)
			Dim heightSelector As New PerlinNoise(8, 0.018, 1.4, inSeed)
			Dim treeSelector As New PerlinNoise(16, 0.28, 1.1, inSeed + 100000)
			Dim foilageSelector As New PerlinNoise(1, 0.02, 1.0, inSeed + 10000)
			Dim minHeight As Double = 1.0F
			Dim maxHeight As Double = -1.0F
			Dim minFoilage As Double = 1.0F
			Dim maxFoilage As Double = -1.0F

			Dim y As Integer = 0
			Dim offset As Integer = 0
			Do While y < inHeight
				Dim x As Integer = 0
				Do While x < inWidth
					Dim height As Double = heightSelector.Perlin_noise_2D(x, y)
					Dim foilage As Double = treeSelector.Perlin_noise_2D(x, y) * foilageSelector.Perlin_noise_2D(x, y)

					If outHeightLayer IsNot Nothing Then
						outHeightLayer(offset) = height

						minHeight = Math.Min(minHeight, height)
						maxHeight = Math.Max(maxHeight, height)
					End If

					If outFoilageLayer IsNot Nothing Then
						outFoilageLayer(offset) = foilage

						minFoilage = Math.Min(minFoilage, foilage)
						maxFoilage = Math.Max(maxFoilage, foilage)
					End If
					x += 1
					offset += 1
				Loop
				y += 1
			Loop

			Const maxDerivative As Double = 0.05

			y = 0
			offset = 0
			Do While y < inHeight
				Dim x As Integer = 0
				Do While x < inWidth - 1
					Dim diff As Double = outHeightLayer(offset + 1) - outHeightLayer(offset)

					If Math.Abs(diff) <= maxDerivative Then
						x += 1
						offset += 1
						Continue Do
					End If

					If diff < 0 Then
						outHeightLayer(offset + 1) = outHeightLayer(offset) - maxDerivative
					Else
						outHeightLayer(offset + 1) = outHeightLayer(offset) + maxDerivative
					End If
					x += 1
					offset += 1
				Loop

				offset += 1
				y += 1
			Loop

			For x As Integer = 0 To inWidth - 1
				y = 0
				offset = x
				Do While y < inHeight - 1
					Dim diff As Double = outHeightLayer(offset + inWidth) - outHeightLayer(offset)

					If Math.Abs(diff) <= maxDerivative Then
						y += 1
						offset += inWidth
						Continue Do
					End If

					If diff < 0 Then
						outHeightLayer(offset + inWidth) = outHeightLayer(offset) - maxDerivative
					Else
						outHeightLayer(offset + inWidth) = outHeightLayer(offset) + maxDerivative
					End If
					y += 1
					offset += inWidth
				Loop
			Next x

			' normalize layers to [-1,1]
			y = 0
			offset = 0
			Do While y < inHeight
				Dim x As Integer = 0
				Do While x < inWidth
					outHeightLayer(offset) = -1.0 + (outHeightLayer(offset) - minHeight) * 2 / (maxHeight - minHeight)
					outFoilageLayer(offset) = -1.0 + (outFoilageLayer(offset) - minFoilage) * 2 / (maxFoilage - minFoilage)
					x += 1
					offset += 1
				Loop
				y += 1
			Loop

		End Sub
	End Class
End Namespace