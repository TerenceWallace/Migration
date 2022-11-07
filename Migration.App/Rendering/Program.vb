Imports OpenTK



#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else
Imports OpenTK.Graphics.OpenGL
#End If

Namespace Migration.Rendering
	''' <summary>
	''' Provides convenient access to GLSL programs. As with other OpenGL related 
	''' unmanaged resources, you MUST dispose any allocated instance explicitly,
	''' otherwise you will get an exception by the time such an leaking object is GCed.
	''' </summary>
	''' <remarks>
	''' Currently shaders are required for object selection, but also to stay conform
	''' to the OpenGL ES 2.0 Specification, which withdraws all fixed-functions as well
	''' as many other convenient APIs. Later, the terrain rendering engine will make
	''' heavy use of shaders.
	''' </remarks>
	Friend Class Program
		Implements IDisposable

		Private m_ProgramID? As Integer

		''' <summary>
		''' Uniform ID of world matrix.
		''' </summary>
		Private privateWorldMatrixLocation As Integer
		Public Property WorldMatrixLocation() As Integer
			Get
				Return privateWorldMatrixLocation
			End Get
			Private Set(ByVal value As Integer)
				privateWorldMatrixLocation = value
			End Set
		End Property

		''' <summary>
		''' Uniform ID of the inverse transform of the world matrix.
		''' </summary>
		Private privateWorldITMatrixLocation As Integer
		Public Property WorldITMatrixLocation() As Integer
			Get
				Return privateWorldITMatrixLocation
			End Get
			Private Set(ByVal value As Integer)
				privateWorldITMatrixLocation = value
			End Set
		End Property

		''' <summary>
		''' Uniform ID of model matrix.
		''' </summary>
		Private privateModelMatrixLocation As Integer
		Public Property ModelMatrixLocation() As Integer
			Get
				Return privateModelMatrixLocation
			End Get
			Private Set(ByVal value As Integer)
				privateModelMatrixLocation = value
			End Set
		End Property

		''' <summary>
		''' Uniform ID of view matrix.
		''' </summary>
		Private privateViewMatrixLocation As Integer
		Public Property ViewMatrixLocation() As Integer
			Get
				Return privateViewMatrixLocation
			End Get
			Private Set(ByVal value As Integer)
				privateViewMatrixLocation = value
			End Set
		End Property

		''' <summary>
		''' If used by shader, the uniform ID of stage textures.
		''' </summary>
		Private privateTextureLocation?() As Integer
		Public Property TextureLocation() As Integer?()
			Get
				Return privateTextureLocation
			End Get
            Private Set(ByVal value As Integer?())
                privateTextureLocation = value
            End Set
		End Property

		''' <summary>
		''' The GLSL program ID. Needed to set custom shader parameters for example.
		''' </summary>
		Public ReadOnly Property ProgramID() As Integer
			Get
				Return m_ProgramID.Value
			End Get
		End Property

		''' <summary>
		''' The vertex shader belonging to this program. Also see <see cref="AutoDisposeShaders"/>.
		''' </summary>
		Private privateVertexShader As Shader
		Public Property VertexShader() As Shader
			Get
				Return privateVertexShader
			End Get
			Private Set(ByVal value As Shader)
				privateVertexShader = value
			End Set
		End Property

		''' <summary>
		''' The pixel shader belonging to this program. Also see <see cref="AutoDisposeShaders"/>.
		''' </summary>
		Private privatePixelShader As Shader
		Public Property PixelShader() As Shader
			Get
				Return privatePixelShader
			End Get
			Private Set(ByVal value As Shader)
				privatePixelShader = value
			End Set
		End Property

		''' <summary>
		''' If true, then both shaders will automatically be disposed if this GLSL program
		''' is disposed.
		''' </summary>
		Private privateAutoDisposeShaders As Boolean
		Public Property AutoDisposeShaders() As Boolean
			Get
				Return privateAutoDisposeShaders
			End Get
			Private Set(ByVal value As Boolean)
				privateAutoDisposeShaders = value
			End Set
		End Property

		''' <summary>
		''' A resource leak check. Due to wrong thread context, we usually can't release OpenGL resources
		''' in class destructors!
		''' </summary>
		Protected Overrides Sub Finalize()
			'if (m_ProgramID.HasValue)
			'throw new ApplicationException("GLSL program has not been released before GC.");
		End Sub

		''' <summary>
		''' Releases all unmanaged resources associated with this program.
		''' If <see cref="AutoDisposeShaders"/> is NOT set, then you will have to
		''' dispose both shaders yourself.
		''' </summary>
		Public Sub Dispose() Implements IDisposable.Dispose
			If Not m_ProgramID.HasValue Then
				Return
			End If

			If AutoDisposeShaders Then
				If VertexShader IsNot Nothing Then
					GL.DetachShader(ProgramID, VertexShader.ShaderID)
					Renderer.CheckError()
					VertexShader.Dispose()
				End If

				If PixelShader IsNot Nothing Then
					GL.DetachShader(ProgramID, PixelShader.ShaderID)
					Renderer.CheckError()
					PixelShader.Dispose()
				End If
			End If

			GL.DeleteProgram(ProgramID)
			Renderer.CheckError()

			PixelShader = Nothing
			VertexShader = Nothing
			m_ProgramID = Nothing
		End Sub

		''' <summary>
		''' Creates a new GLSL program from shaders.
		''' </summary>
		''' <param name="inVertexShader">A valid vertex shader.</param>
		''' <param name="inPixelShader">A valid pixel shader.</param>
		''' <param name="inAutoDisposeShaders">True, if both shaders should be disposed when this program is being disposed.</param>
		Public Sub New(ByVal inVertexShader As Shader, ByVal inPixelShader As Shader, ByVal inAutoDisposeShaders As Boolean)
			Try
				AutoDisposeShaders = inAutoDisposeShaders
				VertexShader = inVertexShader
				PixelShader = inPixelShader
				TextureLocation = New Integer?(15){}

				If (inVertexShader.Type <> ShaderType.VertexShader) OrElse (inPixelShader.Type <> ShaderType.FragmentShader) Then
					Throw New ArgumentException()
				End If

				' create program (no magic here)
				m_ProgramID = GL.CreateProgram()
				Renderer.CheckError()

				GL.AttachShader(ProgramID, inVertexShader.ShaderID)
				Renderer.CheckError()
				GL.AttachShader(ProgramID, inPixelShader.ShaderID)
				Renderer.CheckError()
				GL.LinkProgram(ProgramID)
				Renderer.CheckError()

				Dim  m_log As String = GL.GetProgramInfoLog(ProgramID)

				Dim validStatus As Integer = 0

				GL.ValidateProgram(ProgramID)
				GL.GetProgram(ProgramID, ProgramParameter.ValidateStatus, validStatus)

				If validStatus <> 1 Then
					Throw New ArgumentException("GLSL program failed to link: """ &  m_log & """.")
				End If

				' we require some default uniforms for all shaders (makes no real sense without)
				WorldMatrixLocation = GL.GetUniformLocation(ProgramID, "worldMatrix")
				WorldITMatrixLocation = GL.GetUniformLocation(ProgramID, "worldITMatrix")
				ViewMatrixLocation = GL.GetUniformLocation(ProgramID, "viewMatrix")
				ModelMatrixLocation = GL.GetUniformLocation(ProgramID, "modelMatrix")

				For i As Integer = 0 To TextureLocation.Length - 1
					TextureLocation(i) = GL.GetUniformLocation(ProgramID, "tex_Stage" & (i + 1))
				Next i

				If WorldMatrixLocation < 0 Then
					Throw New ArgumentException("GLSL program does not export uniform ""worldMatrix"".")
				End If
				If ViewMatrixLocation < 0 Then
					Throw New ArgumentException("GLSL program does not export uniform ""viewMatrix"".")
				End If
				If ModelMatrixLocation < 0 Then
					Throw New ArgumentException("GLSL program does not export uniform ""modelMatrix"".")
				End If
			Catch e As Exception
				' If we get an exception in the constructor, there is no way for the caller to explicitly call Dispose()
				Dispose()

				Throw e
			End Try
		End Sub

		''' <summary>
		''' Binds this program to the pipeline, setting all three matricies.
		''' </summary>
		Public Sub Bind(ByVal inWorldMatrix As Matrix4, ByVal inViewMatrix As Matrix4, ByVal inModelMatrix As Matrix4)
			GL.UseProgram(ProgramID)

			' set uniforms
			GL.UniformMatrix4(WorldMatrixLocation, False, inWorldMatrix)
			GL.UniformMatrix4(ViewMatrixLocation, False, inViewMatrix)
			GL.UniformMatrix4(ModelMatrixLocation, False, inModelMatrix)

			If WorldITMatrixLocation >= 0 Then
				Dim worldIT As Matrix4 = Matrix4.Transpose(Matrix4.Invert(inWorldMatrix))

				GL.UniformMatrix4(WorldITMatrixLocation, False, worldIT)
			End If

			For i As Integer = 0 To TextureLocation.Length - 1
				If TextureLocation(i).HasValue Then
					GL.Uniform1(TextureLocation(i).Value, i)
				End If
			Next i
		End Sub


		''' <summary>
		''' Unbinds the program from the pipeline.
		''' </summary>
		Public Sub Unbind()
			GL.UseProgram(0)
		End Sub
	End Class
End Namespace
