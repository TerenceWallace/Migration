Imports Migration.Common
Imports Migration.Core


#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else
Imports OpenTK.Graphics.OpenGL
#End If

Namespace Migration.Rendering


	''' <summary>
	''' A native texture currently does nothing more than providing a convenient way
	''' to load bitmap images into the rendering pipeline. 
	''' </summary>
	Public Class NativeTexture
		Implements IDisposable

		Private m_ID? As Int32 = Nothing
		''' <summary>
		''' If a renderer is used, it will store a texture reference here for
		''' fast list operations.
		''' </summary>
		Private privateNode As LinkedListNode(Of NativeTexture)
		Friend Property Node() As LinkedListNode(Of NativeTexture)
			Get
				Return privateNode
			End Get
			Set(ByVal value As LinkedListNode(Of NativeTexture))
				privateNode = value
			End Set
		End Property
		''' <summary>
		''' The internal OpenGL texture ID.
		''' </summary>
		Public ReadOnly Property ID() As Int32
			Get
				Return m_ID.Value
			End Get
		End Property

		Private privateWidth As Int32
		Public Property Width() As Int32
			Get
				Return privateWidth
			End Get
			Private Set(ByVal value As Int32)
				privateWidth = value
			End Set
		End Property
		Private privateHeight As Int32
		Public Property Height() As Int32
			Get
				Return privateHeight
			End Get
			Private Set(ByVal value As Int32)
				privateHeight = value
			End Set
		End Property
		Private privateClipRect As RectangleDouble
		Public Property ClipRect() As RectangleDouble
			Get
				Return privateClipRect
			End Get
			Private Set(ByVal value As RectangleDouble)
				privateClipRect = value
			End Set
		End Property
		Private privateIsCompressed As Boolean
		Public Property IsCompressed() As Boolean
			Get
				Return privateIsCompressed
			End Get
			Private Set(ByVal value As Boolean)
				privateIsCompressed = value
			End Set
		End Property

		''' <summary>
		''' Checks whether all unmanaged resources were released properly.
		''' </summary>
		Protected Overrides Sub Finalize()
			' TODO:
			'if (m_ID.HasValue)
			'throw new ApplicationException("Texture has not been released before GC.");
		End Sub

		''' <summary>
		''' Properly releases all unmanaged resources.
		''' </summary>
		Public Sub Dispose() Implements IDisposable.Dispose
			If Not m_ID.HasValue Then
				Return
			End If

			Dim backup As Integer = ID

			m_ID = Nothing

			GL.DeleteTexture(backup)

			Renderer.CheckError()
		End Sub


		''' <summary>
		''' If a renderer is given, the texture is registered for automatic 
		''' release. If you pass null, it's your duty to call <see cref="Dispose"/> before GC.
		''' </summary>
		Public Sub New(ByVal inImage As Bitmap)
			Me.New(TextureOptions.None, inImage)
		End Sub

		Public Sub New(ByVal inOptions As TextureOptions)
			Dim tmpID() As Integer = { -1 }

			GL.GenTextures(1, tmpID)

			If tmpID(0) = -1 Then
				Throw New ApplicationException("Unable to allocate texture name.")
			End If

			Renderer.CheckError()

			m_ID = tmpID(0)

			Bind(0)

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, Convert.ToInt32(CInt(TextureMinFilter.Linear)))
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, Convert.ToInt32(CInt(TextureMagFilter.Linear)))
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, Convert.ToInt32(CInt(0)))
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Convert.ToInt32(CInt(0)))

			If inOptions.IsSet(TextureOptions.Repeat) Then
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, Convert.ToInt32(CInt(TextureWrapMode.Repeat)))
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, Convert.ToInt32(CInt(TextureWrapMode.Repeat)))
			Else
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, Convert.ToInt32(CInt(TextureWrapMode.ClampToEdge)))
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, Convert.ToInt32(CInt(TextureWrapMode.ClampToEdge)))
			End If
		End Sub

		Public Sub New(ByVal inOptions As TextureOptions, ByVal inWidth As Integer, ByVal inHeight As Integer, ByVal inPixels() As Integer)
			Me.New(inOptions)
			Width = inWidth
			Height = inHeight

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, inWidth, inHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, inPixels)

			Renderer.CheckError()
		End Sub


		Public Sub New(ByVal inOptions As TextureOptions, ByVal inImage As Bitmap)
			Me.New(inOptions)
			Width = inImage.Width
			Height = inImage.Height

			SetPixels(inImage)
		End Sub

		Public Sub SetPixels(ByVal inImage As Bitmap)
			Dim bmp_data As System.Drawing.Imaging.BitmapData = Nothing

			If (inImage.Width <> Width) OrElse (inImage.Height <> Height) Then
				Throw New ArgumentException("Image dimensions do match.")
			End If

			Try
				bmp_data = inImage.LockBits(New System.Drawing.Rectangle(0, 0, inImage.Width, inImage.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb)

				Bind(0)

				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, bmp_data.Scan0)

				Renderer.CheckError()
			Finally
				If bmp_data IsNot Nothing Then
					inImage.UnlockBits(bmp_data)
				End If
			End Try
		End Sub

		Public ReadOnly Property IsCompressionSupported() As Boolean
			Get
				Dim texFormatCount As Integer = 0
				Dim texFormatInts() As Integer = Nothing

				GL.GetInteger(GetPName.NumCompressedTextureFormats, texFormatCount)
				Renderer.CheckError()

				texFormatInts = New Integer(texFormatCount - 1){}
				GL.GetInteger(GetPName.CompressedTextureFormats, texFormatInts)
				Renderer.CheckError()
				'                
				'                 * We are using compression for animation library frames only, and therefore we need DXT3,
				'                 * since it is best suited for alpha masking...
				'                 
				Return texFormatInts.Contains(Convert.ToInt32(CInt(PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext)))
			End Get
		End Property

		''' <summary>
		''' If image is already compressed, the following method will load it directly, omitting any unneccessary internal
		''' postprocessing in the OpenGL implementation/driver. But since compression is system dependent, you must also
		''' provide the original image in case the given compression is not supported. If this is the case, the method
		''' will behave exactly as SetPixel.
		''' </summary>
		Public Sub SetPixels(ByVal inImage As Bitmap, ByVal inCompressedPixels() As Byte)
			If Not IsCompressionSupported Then
				SetPixels(inImage)

				Return
			End If

			Bind(0)

			GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.CompressedSrgbAlphaS3tcDxt3Ext, inImage.Width, inImage.Height, 0, inCompressedPixels.Length, inCompressedPixels)

			If GL.GetError() <> ErrorCode.NoError Then
				SetPixels(inImage)

				Return
			End If
		End Sub

		Public Sub SetPixels(ByVal inOffsetX As Integer, ByVal inOffsetY As Integer, ByVal inWidth As Integer, ByVal inHeight As Integer, ByVal inPixels() As Integer)
			Dim pixelCount As Integer = inWidth * inHeight

			If (inOffsetX + inWidth > Width) OrElse (inOffsetY + inHeight > Height) OrElse (pixelCount > inPixels.Length) Then
				Throw New ArgumentOutOfRangeException()
			End If

			Bind(0)

			GL.TexSubImage2D(TextureTarget.Texture2D, 0, inOffsetX, inOffsetY, inWidth, inHeight, PixelFormat.Rgba, PixelType.UnsignedByte, inPixels)
			Renderer.CheckError()
		End Sub

		''' <summary>
		''' Binds the texture to the rendering pipeline.
		''' </summary>
		Public Sub Bind()
			Bind(0)
		End Sub

		Public Sub Bind(ByVal inStage As Integer)
			'if (Renderer.CurrentTextureID == ID)
			'    return;

			Select Case inStage
				Case 0
					GL.ActiveTexture(TextureUnit.Texture0)
				Case 1
					GL.ActiveTexture(TextureUnit.Texture1)
				Case 2
					GL.ActiveTexture(TextureUnit.Texture2)
				Case 3
					GL.ActiveTexture(TextureUnit.Texture3)
				Case 4
					GL.ActiveTexture(TextureUnit.Texture4)
				Case 5
					GL.ActiveTexture(TextureUnit.Texture5)
				Case 6
					GL.ActiveTexture(TextureUnit.Texture6)
				Case 7
					GL.ActiveTexture(TextureUnit.Texture7)
				Case Else
					Throw New ApplicationException()
			End Select

			GL.Enable(EnableCap.Texture2D)
			GL.BindTexture(TextureTarget.Texture2D, ID)
		End Sub

		Public Sub Unbind(ByVal inStage As Integer)
			Select Case inStage
				Case 0
					GL.ActiveTexture(TextureUnit.Texture0)
				Case 1
					GL.ActiveTexture(TextureUnit.Texture1)
				Case 2
					GL.ActiveTexture(TextureUnit.Texture2)
				Case 3
					GL.ActiveTexture(TextureUnit.Texture3)
				Case 4
					GL.ActiveTexture(TextureUnit.Texture4)
				Case 5
					GL.ActiveTexture(TextureUnit.Texture5)
				Case 6
					GL.ActiveTexture(TextureUnit.Texture6)
				Case 7
					GL.ActiveTexture(TextureUnit.Texture7)
				Case Else
					Throw New ApplicationException()
			End Select

			GL.BindTexture(TextureTarget.Texture2D, 0)
			GL.Disable(EnableCap.Texture2D)
		End Sub

		Public Sub Save(ByVal inFileName As String)
			Dim image As New Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
			Dim bmp_data As System.Drawing.Imaging.BitmapData = Nothing

			Using image
				Try
					bmp_data = image.LockBits(New System.Drawing.Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb)

					Bind(0)

					GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, bmp_data.Scan0)

					Renderer.CheckError()
				Finally
					If bmp_data IsNot Nothing Then
						image.UnlockBits(bmp_data)
					End If
				End Try

				image.Save(inFileName)
			End Using
		End Sub
	End Class
End Namespace
