Imports OpenTK



#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else
Imports OpenTK.Graphics.OpenGL
#End If

Imports Migration.Configuration

Namespace Migration.Rendering
	Partial Public Class TerrainRenderer

		Public Function ComputeOcclusion(ByVal inProjMatrix As Matrix4, ByVal inViewMatrix As Matrix4, ByVal inModelMatrix As Matrix4) As Rectangle
			m_OcclusionShader.Bind(inProjMatrix, inViewMatrix, inModelMatrix)

			Return m_Mesh.ComputeOcclusion()
		End Function

		Friend Function MouseToGridPos(ByVal inMouseXY As Point, ByVal inProjMatrix As Matrix4, ByVal inViewMatrix As Matrix4, ByVal inModelMatrix As Matrix4, ByRef outGridPos As Point) As Boolean
			outGridPos = New Point(-1, -1)

			GL.ClearColor(Color.White)
			GL.Clear(ClearBufferMask.ColorBufferBit)

			' render selection
			m_SelectionShader.Bind(inProjMatrix, inViewMatrix, inModelMatrix)

			GL.Uniform1(m_SelectionOffsetXID, Convert.ToSingle(ScreenBounds.X))
			GL.Uniform1(m_SelectionOffsetYID, Convert.ToSingle(ScreenBounds.Y))
			Migration.Rendering.Renderer.CheckError()

			m_Mesh.RenderBlocks(ScreenBounds, False)
			Migration.Rendering.Renderer.CheckError()

			' read pixel at mouse position
			Dim selectedPixels(0) As Integer
			Dim pixel As Integer = 0

			GL.ReadPixels(inMouseXY.X, Renderer.ViewportHeight - inMouseXY.Y, 1, 1, PixelFormat.Rgb, PixelType.UnsignedByte, selectedPixels)

			pixel = selectedPixels(0)

			' convert back to grid pos
			If (pixel And &HFF0000) <> 0 Then
				Return False
			End If

			outGridPos = New Point(ScreenBounds.X + (pixel And &HFF), ScreenBounds.Y + ((pixel And &HFF00) >> 8))

			Return True
		End Function

		Private Function ParameterizeFragmentShader(ByVal inShaderSource As String, ByVal inParams As TerrainConfiguration) As String
			Dim parameters As String = "#define PARAMETERIZED" & (vbTab & vbLf) & "#define HEIGHTSCALE		" & FormatDouble(inParams.HeightScale) & Environment.NewLine & "#define NORMALSCALE		" & FormatDouble(inParams.NormalZScale) & Environment.NewLine & "#define WATERHEIGHT		" & FormatDouble(inParams.Water.Height) & Environment.NewLine & "#define MAPSIZE			" & FormatDouble(Size) & Environment.NewLine & "#define RED_FREQ		" & FormatDouble(inParams.RedNoiseFrequency) & Environment.NewLine & "#define GREEN_FREQ		" & FormatDouble(inParams.RedNoiseFrequency) & Environment.NewLine & "#define BLUE_FREQ		" & FormatDouble(inParams.RedNoiseFrequency) & Environment.NewLine & Environment.NewLine & "#define TEXSCALE_00		" & FormatDouble(inParams.Levels(0).TextureScale) & Environment.NewLine & "#define TEXSCALE_01		" & FormatDouble(inParams.Levels(1).TextureScale) & Environment.NewLine & "#define TEXSCALE_02		" & FormatDouble(inParams.Levels(2).TextureScale) & Environment.NewLine & "#define TEXSCALE_03		" & FormatDouble(inParams.Levels(3).TextureScale) & Environment.NewLine & "#define TEXSCALE_04		" & FormatDouble(inParams.Levels(4).TextureScale) & Environment.NewLine & Environment.NewLine & "#define REDNOISESCALE_00			" & FormatDouble(inParams.Levels(0).RedNoiseDivisor) & Environment.NewLine & "#define GREENNOISESCALE_00			" & FormatDouble(inParams.Levels(0).GreenNoiseDivisor) & Environment.NewLine & "#define BLUENOISESCALE_00			" & FormatDouble(inParams.Levels(0).BlueNoiseDivisor) & Environment.NewLine & "#define REDNOISESCALE_01			" & FormatDouble(inParams.Levels(1).RedNoiseDivisor) & Environment.NewLine & "#define GREENNOISESCALE_01			" & FormatDouble(inParams.Levels(1).GreenNoiseDivisor) & Environment.NewLine & "#define BLUENOISESCALE_01			" & FormatDouble(inParams.Levels(1).BlueNoiseDivisor) & Environment.NewLine & "#define REDNOISESCALE_02			" & FormatDouble(inParams.Levels(2).RedNoiseDivisor) & Environment.NewLine & "#define GREENNOISESCALE_02			" & FormatDouble(inParams.Levels(2).GreenNoiseDivisor) & Environment.NewLine & "#define BLUENOISESCALE_02			" & FormatDouble(inParams.Levels(2).BlueNoiseDivisor) & Environment.NewLine & "#define REDNOISESCALE_03			" & FormatDouble(inParams.Levels(3).RedNoiseDivisor) & Environment.NewLine & "#define GREENNOISESCALE_03			" & FormatDouble(inParams.Levels(3).GreenNoiseDivisor) & Environment.NewLine & "#define BLUENOISESCALE_03			" & FormatDouble(inParams.Levels(3).BlueNoiseDivisor) & Environment.NewLine & "#define REDNOISESCALE_04			" & FormatDouble(inParams.Levels(4).RedNoiseDivisor) & Environment.NewLine & "#define GREENNOISESCALE_04			" & FormatDouble(inParams.Levels(4).GreenNoiseDivisor) & Environment.NewLine & "#define BLUENOISESCALE_04			" & FormatDouble(inParams.Levels(4).BlueNoiseDivisor) & Environment.NewLine

			Return inShaderSource.Replace("//{PARAMETERIZATION}", parameters)
		End Function

		Private Function FormatDouble(ByVal inValue As Double) As String
			Dim result As String = inValue.ToString().Replace(","c, "."c)

			If Not(result.Contains("."c)) Then
				result &= ".0"
			End If

			Return result
		End Function

		Public Sub RenderGroundPlane(ByVal inProjMatrix As Matrix4, ByVal inViewMatrix As Matrix4, ByVal inModelMatrix As Matrix4)
			GL.ClearColor(Color.Black)
			GL.Clear(ClearBufferMask.ColorBufferBit Or ClearBufferMask.DepthBufferBit)

			' setup program and textures
			For i As Integer = 0 To m_GroundTextures.Length - 1
				If m_GroundTextures(i) Is Nothing Then
					Continue For
				End If

				m_GroundTextures(i).Bind(i)
			Next i

			' render ground plane
			m_Shader.Bind(inProjMatrix, inViewMatrix, inModelMatrix)

			m_Mesh.RenderBlocks(ScreenBounds, False)
			Migration.Rendering.Renderer.CheckError()

			GL.Clear(ClearBufferMask.DepthBufferBit)

			' render water
			m_WaterShader.Bind(inProjMatrix, inViewMatrix, inModelMatrix)
            GL.Uniform1(m_TimeMillisID, Convert.ToSingle(PrecisionTimerCallback.Invoke()))

			m_Mesh.RenderBlocks(ScreenBounds, True)
			Migration.Rendering.Renderer.CheckError()

			For i As Integer = 0 To m_GroundTextures.Length - 1
				m_GroundTextures(i).Unbind(i)
			Next i
		End Sub
	End Class
End Namespace
