Imports Migration.Common
Imports OpenTK
Imports OpenTK.Input

Imports System.Threading

#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else
Imports OpenTK.Graphics.OpenGL
#End If

Namespace Migration.Rendering

	Partial Public Class Renderer

		Private m_Window As GameWindow

		''' <summary>
		''' One time initialization of a new GL context. Must be called from within the render thread!
		''' </summary>
		''' <param name="inPixelWidth">Pixel width of the viewport.</param>
		''' <param name="inPixelHeight">Pixel height of the viewport.</param>
		Private Sub InitializeGLContext(ByVal inPixelWidth As Integer, ByVal inPixelHeight As Integer)
			' just dump some system information into log file which will be important for bugreports.
			Dim logEntry As String = "Creating OpenGL Renderer..." & Environment.NewLine & "    => OpenGL Version: """ & GL.GetString(StringName.Version) & """" & Environment.NewLine & "    => GLSL Version: """ & GL.GetString(StringName.ShadingLanguageVersion) & """" & Environment.NewLine & "    => OpenGL Vendor: """ & GL.GetString(StringName.Vendor) & """" & Environment.NewLine & "    => OpenGL Renderer: """ & GL.GetString(StringName.Renderer) & """" & Environment.NewLine & "    => OpenGL Extensions: """ & Environment.NewLine & "        " & GL.GetString(StringName.Extensions).Replace(" ", Environment.NewLine & "        ") & """" & Environment.NewLine

			Log.LogMessage(logEntry)

			GL.ClearColor(Color.DarkGreen)
			GL.ClearDepth(1.0F)
			GL.DepthFunc(DepthFunction.Lequal)

			' load shaders
			m_2DSceneProgram = New Program(New Shader([Global].GetResourcePath("Shaders/2DScene.vert")), New Shader([Global].GetResourcePath("Shaders/2DScene.frag")), True)

			UpdateViewport(inPixelWidth, inPixelHeight)
		End Sub

		''' <summary>
		''' Should be called whenever the surface changes its size and properly
		''' updates the GL viewport and other related properties.
		''' </summary>
		''' <param name="inPixelWidth">Pixel width of the viewport.</param>
		''' <param name="inPixelHeight">Pixel height of the viewport.</param>
		Private Sub UpdateViewport(ByVal inPixelWidth As Integer, ByVal inPixelHeight As Integer)
			If (inPixelWidth <= 0) OrElse (inPixelHeight <= 0) Then
				Return
			End If

			GL.Viewport(0, 0, inPixelWidth, inPixelHeight)
			GL.MatrixMode(MatrixMode.Projection) ' Select The Projection Matrix
			GL.MatrixMode(MatrixMode.Modelview) ' Select The Modelview Matrix
			GL.LoadIdentity() ' Reset The Modelview Matrix

			ViewportWidth = inPixelWidth
			ViewportHeight = inPixelHeight
			AspectRatio = (Convert.ToDouble(inPixelWidth) / inPixelHeight)

			If TerrainRenderer IsNot Nothing Then
				TerrainRenderer.UpdateViewport()
			End If
		End Sub

		Private Sub New()
			m_Watch = New System.Diagnostics.Stopwatch()
			m_Watch.Start()
			CurrentTextureIDs = New Integer(15){}
		End Sub

		Public Sub New(ByVal inConfig As RenderConfiguration)
			Me.New()
			If inConfig Is Nothing Then
				Throw New ArgumentNullException()
			End If

			_inConfig = inConfig
			RenderThread = New System.Threading.Thread(AddressOf InitializeWindow)
			RenderThread.IsBackground = True
			RenderThread.Start()

		End Sub

		Private _inConfig As RenderConfiguration
		Private Sub InitializeWindow()
			Try
				m_Window = New GameWindow(_inConfig.ViewportWidth, _inConfig.ViewportHeight, OpenTK.Graphics.GraphicsMode.Default, "Migration MILESTONE 0", (If(_inConfig.IsFullScreen, GameWindowFlags.Fullscreen, GameWindowFlags.Default)))
				m_Window.Visible = True
				InitializeGLContext(_inConfig.ViewportWidth, _inConfig.ViewportHeight)

				AddHandler m_Window.Mouse.Move, AddressOf RaiseMouseMove
				AddHandler m_Window.Mouse.ButtonDown, AddressOf RaiseMouseButtonDown
				AddHandler m_Window.Mouse.ButtonUp, AddressOf RaiseMouseButtonUp
				AddHandler m_Window.Keyboard.KeyDown, AddressOf RaiseKeyboardKeyDown
				AddHandler m_Window.Keyboard.KeyUp, AddressOf RaiseKeyboardKeyUp
				AddHandler m_Window.Resize, Sub(sender As Object, e As EventArgs) UpdateViewport(m_Window.Width, m_Window.Height)

				InitializeStopWatch()

			Catch e As Exception
				Log.LogExceptionCritical(e)
			Finally
				IsTerminated = True
				Dispose()
			End Try
		End Sub


		''' <summary>
		''' 
		''' </summary>
		''' <remarks></remarks>
		Private Sub InitializeStopWatch()
			If m_Window Is Nothing Then
				Return
			End If

			Dim watch As New System.Diagnostics.Stopwatch()
			Dim avgMillis As Long = 0
			Dim frameCount As Long = 0
			Do
				m_Window.ProcessEvents()
				If m_Window.IsExiting Then
					Return
				End If

				RaiseKeyboardKeyRepeat()
				watch.Start()
				RunPerFrameTasks()
				m_Window.SwapBuffers()
				watch.Stop()
				frameCount += 1
				avgMillis += watch.ElapsedMilliseconds
				watch.Reset()

				If frameCount > 30 Then
					Dim fps As Single = (1000 / (avgMillis / Convert.ToSingle(frameCount)))
					If TerrainRenderer IsNot Nothing Then
						m_Window.Title = String.Format("Migration MILESTONE 0 (FPS: {0:0.##}; Screen: ({1:0.##}:{2:0.##}); Bounds: ({3}); Mouse: ({4}); PPM: {5}ms [max: {6}ms])", fps, TerrainRenderer.ScreenXY.X, TerrainRenderer.ScreenXY.Y, TerrainRenderer.ScreenBounds, TerrainRenderer.GridXY, (If(TerrainRenderer.Terrain.Map IsNot Nothing, TerrainRenderer.Terrain.Map.AvgPlanMillis, 0)), (If(TerrainRenderer.Terrain.Map IsNot Nothing, TerrainRenderer.Terrain.Map.MaxPlanMillis, 0)))
					End If
					frameCount = 0
					avgMillis = 0
				End If
				System.Threading.Thread.Sleep(20)
				Loop While True
		End Sub

		Private Sub RunPerFrameTasks()
			If (Terrain IsNot Nothing) AndAlso (TerrainRenderer Is Nothing) Then
				TerrainRenderer = New TerrainRenderer(Me, Terrain)
			End If

			GL.ClearColor(Color.Black)
			GL.Clear(ClearBufferMask.ColorBufferBit Or ClearBufferMask.DepthBufferBit)

			If (TerrainRenderer IsNot Nothing) AndAlso EnableTerrainRendering Then
				TerrainRenderer.OnRender()
			End If

			' render GUI sprites
			Dim spriteMat As New Matrix4(2F, 0F, 0F, 0F, 0F, -2F, 0F, 0F, 0F, 0F, -1F, 0F, -1F, 1F, 0F, 1F)

			m_2DSceneProgram.Bind(spriteMat, Matrix4.Identity, Matrix4.Identity)

			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha)
			GL.Enable(EnableCap.Blend)
			GL.Disable(EnableCap.DepthTest)

			RaiseEvent OnRenderSprites(Me)

			GL.Disable(EnableCap.Blend)

		End Sub

		Public Sub WaitForTermination()
			Do While Not IsTerminated
				Thread.Sleep(500)
			Loop
		End Sub

		''' <summary>
		''' This is only intended for AnimationEditor and provides limited functionality!
		''' </summary>
		''' <returns></returns>
		Public Shared Function CreateControl(ByVal inParent As System.Windows.Forms.ContainerControl) As Renderer
			_result = New Renderer()
			_glCtrl = New GLControl()

			_repaintTimer = New System.Windows.Forms.Timer()
			_repaintTimer.Interval = 30

			AddHandler _repaintTimer.Tick, AddressOf OnRepaintTimer_Tick

			_IsInitialized = False

			_glCtrl.Dock = System.Windows.Forms.DockStyle.Fill

			' also update viewport on resize
			AddHandler _glCtrl.SizeChanged, AddressOf GLControl_SizeChanged

			' is invoked whenever the control is to be repaint
			AddHandler _glCtrl.Paint, AddressOf GLControl_OnPaint
			AddHandler _glCtrl.MouseMove, AddressOf GLControl_OnMouseMove
			AddHandler _glCtrl.MouseDown, AddressOf GLControl_OnMouseDown
			AddHandler _glCtrl.MouseUp, AddressOf GLControl_OnMouseUp

			_result.RenderThread = Thread.CurrentThread
			_repaintTimer.Start()

			' finally attach the control to its parent
			inParent.Controls.Add(_glCtrl)

			Return _result
		End Function

		Private Shared _glCtrl As GLControl
		Private Shared _result As Renderer
		Private Shared _repaintTimer As System.Windows.Forms.Timer
		Private Shared _IsInitialized As Boolean

		Private Shared Sub OnRepaintTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
			_glCtrl.Invalidate()
		End Sub

		Private Shared Sub GLControl_SizeChanged(ByVal sender As Object, ByVal e As EventArgs)
			_result.UpdateViewport(_glCtrl.Width, _glCtrl.Height)
		End Sub

		Private Shared Sub GLControl_OnPaint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs)
			Try
				' stopping the timer prevents CPU freeze, if rendering takes longer than desired frame rate.
				_repaintTimer.Stop()

				If Not _IsInitialized Then
					_result.InitializeGLContext(_glCtrl.Width, _glCtrl.Height)
					_IsInitialized = True
				End If

				_result.RunPerFrameTasks()
				_glCtrl.SwapBuffers()

			Catch ex As Exception
				Log.LogExceptionCritical(ex)
			Finally
				_repaintTimer.Start()
			End Try
		End Sub

		Private Shared Sub GLControl_OnMouseMove(ByVal sender As Object, ByVal args As System.Windows.Forms.MouseEventArgs)
			_result.RaiseMouseMove(sender, New MouseMoveEventArgs(args.X, args.Y, 0, 0))
		End Sub

		Private Shared Sub GLControl_OnMouseDown(ByVal sender As Object, ByVal args As System.Windows.Forms.MouseEventArgs)
			Dim btn As MouseButton = 0

			Select Case args.Button
				Case System.Windows.Forms.MouseButtons.Left
					btn = MouseButton.Left
				Case System.Windows.Forms.MouseButtons.Right
					btn = MouseButton.Right
				Case System.Windows.Forms.MouseButtons.Middle
					btn = MouseButton.Middle
				Case Else
					Return
			End Select

			_result.RaiseMouseButtonDown(sender, New MouseButtonEventArgs(args.X, args.Y, btn, True))
		End Sub

		Private Shared Sub GLControl_OnMouseUp(ByVal sender As Object, ByVal args As System.Windows.Forms.MouseEventArgs)
			Dim btn As MouseButton = 0

			Select Case args.Button
				Case System.Windows.Forms.MouseButtons.Left
					btn = MouseButton.Left
				Case System.Windows.Forms.MouseButtons.Right
					btn = MouseButton.Right
				Case System.Windows.Forms.MouseButtons.Middle
					btn = MouseButton.Middle
				Case Else
					Return
			End Select

			_result.RaiseMouseButtonUp(sender, New MouseButtonEventArgs(args.X, args.Y, btn, False))
		End Sub

		''' <summary>
		''' In case of this method is called automatically on exit.
		''' In case of you have to call it yourself when the renderer
		''' is no longer needed.
		''' </summary>
		Public Sub Dispose()
			For Each image As RegisteredImage In m_RegisteredImages
				If (image IsNot Nothing) AndAlso (image.Texture IsNot Nothing) Then
					image.Texture.Dispose()
				End If
			Next image

			m_RegisteredImages.Clear()

			' release shaders
			If m_2DSceneProgram IsNot Nothing Then
				m_2DSceneProgram.Dispose()
			End If

			m_2DSceneProgram = Nothing
		End Sub
	End Class
End Namespace
