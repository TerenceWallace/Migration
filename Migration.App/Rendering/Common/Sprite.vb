#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else
Imports OpenTK.Graphics.OpenGL
#End If

Namespace Migration.Rendering

	Partial Public Class Renderer
		Public Sub DrawSprite(ByVal inX As Double, ByVal inY As Double, ByVal inWidth As Double, ByVal inHeight As Double, ByVal inImageID As Integer, ByVal inOpacity As Double)
			Dim mImage As RegisteredImage = m_RegisteredImages(inImageID)

			If mImage.Texture Is Nothing Then
				mImage.Texture = New NativeTexture(mImage.Image)
			End If

			mImage.Texture.Bind()

			DrawSpriteZAtlas(Convert.ToSingle(inX), Convert.ToSingle(inY), Convert.ToSingle(inWidth), Convert.ToSingle(inHeight), Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255 * inOpacity), 0F, 0F, 1F, 1F)
		End Sub

		Public Sub DrawSpriteAtlas(ByVal inX As Double, ByVal inY As Double, ByVal inWidth As Double, ByVal inHeight As Double, ByVal inImageID As Integer, ByVal inOpacity As Double, ByVal inTexX As Integer, ByVal inTexY As Integer, ByVal inTexWidth As Integer, ByVal inTexHeight As Integer)
			Dim mImage As RegisteredImage = m_RegisteredImages(inImageID)

			If mImage.Texture Is Nothing Then
				mImage.Texture = New NativeTexture(mImage.Image)
			End If

			mImage.Texture.Bind()

			Dim texScaleX As Double = 1.0 / mImage.Image.Width
			Dim texScaleY As Double = 1.0 / mImage.Image.Height

			DrawSpriteZAtlas(Convert.ToSingle(inX), Convert.ToSingle(inY), Convert.ToSingle(inWidth), Convert.ToSingle(inHeight), Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255), Convert.ToByte(255 * inOpacity), Convert.ToSingle(inTexX * texScaleX), Convert.ToSingle(inTexY * texScaleY), Convert.ToSingle((inTexX + inTexWidth) * texScaleX), Convert.ToSingle((inTexY + inTexHeight) * texScaleY))
		End Sub

		Public Sub RegisterImage(ByVal inImageID As Integer, ByVal inImage As System.Drawing.Bitmap)
			If inImageID < 0 Then
				Throw New ArgumentOutOfRangeException()
			End If

			If inImage Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Do While m_RegisteredImages.Count <= inImageID
				m_RegisteredImages.Add(Nothing)
			Loop

			If m_RegisteredImages(inImageID) Is Nothing Then
				m_RegisteredImages(inImageID) = New RegisteredImage() With {.Image = inImage}
			End If
		End Sub

		Friend Sub DrawSpriteZAtlas(ByVal inLeft As Single, ByVal inTop As Single, ByVal inWidth As Single, ByVal inHeight As Single, ByVal inRed As Byte, ByVal inGreen As Byte, ByVal inBlue As Byte, ByVal inAlpha As Byte, ByVal inTexX1 As Single, ByVal inTexY1 As Single, ByVal inTexX2 As Single, ByVal inTexY2 As Single)
			GL.Begin(BeginMode.Quads)
			GL.Color4(inRed, inGreen, inBlue, inAlpha)

			GL.TexCoord2(inTexX1, inTexY1)
			GL.Vertex3(inLeft, inTop, 0)
			GL.TexCoord2(inTexX2, inTexY1)
			GL.Vertex3(inLeft + inWidth, inTop, 0)
			GL.TexCoord2(inTexX2, inTexY2)
			GL.Vertex3(inLeft + inWidth, inTop + inHeight, 0)
			GL.TexCoord2(inTexX1, inTexY2)
			GL.Vertex3(inLeft, inTop + inHeight, 0)
			GL.End()
		End Sub
	End Class
End Namespace
