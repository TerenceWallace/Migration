Imports System.IO


#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else
Imports OpenTK.Graphics.OpenGL
#End If

Namespace Migration.Rendering
	''' <summary>
	''' Provides convenient access to GLSL shaders. As with other OpenGL related 
	''' unmanaged resources, you MUST dispose any allocated instance explicitly,
	''' otherwise you will get an exception by the time such an leaking object is GCed.
	''' Also see <see cref="Program"/>.
	''' </summary>
	Friend Class Shader
		Implements IDisposable

		Private m_ShaderID? As Integer

		''' <summary>
		''' Vertex- or pixelshader?
		''' </summary>
		Private privateType As ShaderType
		Public Property Type() As ShaderType
			Get
				Return privateType
			End Get
			Private Set(ByVal value As ShaderType)
				privateType = value
			End Set
		End Property
		''' <summary>
		''' Source code for the shader.
		''' </summary>
		Private privateSource As String
		Public Property Source() As String
			Get
				Return privateSource
			End Get
			Private Set(ByVal value As String)
				privateSource = value
			End Set
		End Property
		''' <summary>
		''' OpenGL specific shader ID.
		''' </summary>
		Public ReadOnly Property ShaderID() As Integer
			Get
				Return m_ShaderID.Value
			End Get
		End Property

		''' <summary>
		''' A resource leak check. Due to wrong thread context, we usually can't release OpenGL resources
		''' in class destructors!
		''' </summary>
		Protected Overrides Sub Finalize()
			'if (m_ShaderID.HasValue)
			'    Log.LogExceptionCritical(new ArgumentException("Shader has not been released before GC."));
		End Sub

		''' <summary>
		''' Releases all unmanaged resources associated with this shader.
		''' </summary>
		Public Sub Dispose() Implements IDisposable.Dispose
			If Not m_ShaderID.HasValue Then
				Return
			End If

			GL.DeleteShader(ShaderID)
			Renderer.CheckError()

			m_ShaderID = Nothing
		End Sub

		''' <summary>
		''' Loads a new shader from file. For performance reasons, shaders might be
		''' cached later on, but this won't have an impact on this API.
		''' </summary>
		''' <param name="inFileName">A file containing the shader source code.</param>
		Public Sub New(ByVal inFileName As String)
			Me.New(inFileName, File.ReadAllText(inFileName))

		End Sub

		Public Sub New(ByVal inFileName As String, ByVal inSourceCode As String)
			Try
				If inFileName.EndsWith(".vert") Then
					Type = ShaderType.VertexShader
				ElseIf inFileName.EndsWith(".frag") Then
					Type = ShaderType.FragmentShader
				Else
					Throw New ArgumentException()
				End If

				Source = inSourceCode


				Dim  m_log As String = Nothing

				Try
					m_ShaderID = GL.CreateShader(Type)
					Renderer.CheckError()
					GL.ShaderSource(ShaderID, Source)
					Renderer.CheckError()
					GL.CompileShader(ShaderID)
					Renderer.CheckError()

					 m_log = GL.GetShaderInfoLog(ShaderID)
				Catch e As Exception
					Throw New ArgumentException("Shader """ & inFileName & """ failed to load.", e)
				End Try

				Dim success As Integer = 0

				GL.GetShader(ShaderID, ShaderParameter.CompileStatus, success)

				If success <> 1 Then
					Throw New ArgumentException("Shader """ & inFileName & """ failed to compile: """ &  m_log & """.")
				End If

			Catch e As Exception
				Dispose()

				Throw e
			End Try
		End Sub
	End Class
End Namespace
