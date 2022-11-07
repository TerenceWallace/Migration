Imports Migration.Common


Imports System.Threading

#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else
Imports OpenTK.Graphics.OpenGL
#End If

Namespace Migration.Rendering

	''' <summary>
	''' This class and the whole GLRenderer-Assembly is expected to change heavily in near future, 
	''' this is why I skip many documentation at this point.
	''' </summary>
	Partial Public Class Renderer

		Public Event OnMouseDown As DNotifyHandler(Of Renderer, Integer)
		Public Event OnMouseUp As DNotifyHandler(Of Renderer, Integer)
		Public Event OnMouseMove As DNotifyHandler(Of Renderer, Point)

		Public Event OnKeyDown As DNotifyHandler(Of Renderer, OpenTK.Input.Key)
		Public Event OnKeyUp As DNotifyHandler(Of Renderer, OpenTK.Input.Key)
		Public Event OnKeyRepeat As DNotifyHandler(Of Renderer, OpenTK.Input.Key)

		Public Event OnRenderSprites As DNotifyHandler(Of Renderer)

		Private m_Watch As New System.Diagnostics.Stopwatch()
		Private ReadOnly m_MouseState As New List(Of Integer)()
		Private ReadOnly m_KeyboardState As New List(Of OpenTK.Input.Key)()
		Private ReadOnly m_RegisteredImages As New List(Of RegisteredImage)()
		Private m_2DSceneProgram As Program

		Private privateTerrainRenderer As TerrainRenderer
		Public Property TerrainRenderer() As TerrainRenderer
			Get
				Return privateTerrainRenderer
			End Get
			Private Set(ByVal value As TerrainRenderer)
				privateTerrainRenderer = value
			End Set
		End Property

		Private privateTerrain As TerrainDefinition
		Public Property Terrain() As TerrainDefinition
			Get
				Return privateTerrain
			End Get
			Set(ByVal value As TerrainDefinition)
				privateTerrain = value
			End Set
		End Property

		Friend Const SelectionGranularity As Int32 = 5

		Private privateCurrentTextureIDs() As Int32
		Friend Property CurrentTextureIDs() As Int32()
			Get
				Return privateCurrentTextureIDs
			End Get
			Private Set(ByVal value As Int32())
				privateCurrentTextureIDs = value
			End Set
		End Property

		Private privateRenderThread As Thread
		Public Property RenderThread() As Thread
			Get
				Return privateRenderThread
			End Get
			Private Set(ByVal value As Thread)
				privateRenderThread = value
			End Set
		End Property

		Private privateAspectRatio As Double
		Public Property AspectRatio() As Double
			Get
				Return privateAspectRatio
			End Get
			Private Set(ByVal value As Double)
				privateAspectRatio = value
			End Set
		End Property

		Private privateEnableTerrainRendering As Boolean
		Public Property EnableTerrainRendering() As Boolean
			Get
				Return privateEnableTerrainRendering
			End Get
			Set(ByVal value As Boolean)
				privateEnableTerrainRendering = value
			End Set
		End Property

		Private privateViewportWidth As Integer
		Public Property ViewportWidth() As Integer
			Get
				Return privateViewportWidth
			End Get
			Private Set(ByVal value As Integer)
				privateViewportWidth = value
			End Set
		End Property

		Private privateViewportHeight As Integer
		Public Property ViewportHeight() As Integer
			Get
				Return privateViewportHeight
			End Get
			Private Set(ByVal value As Integer)
				privateViewportHeight = value
			End Set
		End Property

		Private privateIsTerminated As Boolean
		Public Property IsTerminated() As Boolean
			Get
				Return privateIsTerminated
			End Get
			Private Set(ByVal value As Boolean)
				privateIsTerminated = value
			End Set
		End Property

		Private privateMouseXY As Point
		Public Property MouseXY() As Point
			Get
				Return privateMouseXY
			End Get
			Private Set(ByVal value As Point)
				privateMouseXY = value
			End Set
		End Property

		Public ReadOnly Property MouseState() As IEnumerable(Of Integer)
			Get
				SyncLock m_MouseState
					Return m_MouseState.ToArray()
				End SyncLock
			End Get
		End Property

		Public ReadOnly Property KeyboardState() As IEnumerable(Of OpenTK.Input.Key)
			Get
				SyncLock m_KeyboardState
					Return m_KeyboardState.ToArray()
				End SyncLock
			End Get
		End Property

		Public Shared Sub CheckError()
			CheckError("OpenGL call failed unexpectedly.")
		End Sub

		Public Shared Sub CheckError(ByVal inMessage As String, ParamArray ByVal inArgs() As Object)
			Dim errCode As ErrorCode = GL.GetError()

			Select Case errCode
				Case ErrorCode.NoError
					Return

				Case Else
					Throw New RenderException(errCode, inMessage, inArgs)
			End Select
		End Sub

		Public Function LoadAmbientSnapshot(ByVal inCharacter As String) As Bitmap
			Dim Character As Character = AnimationLibrary.Instance.FindClass(inCharacter)

			If Character.AmbientSet IsNot Nothing Then
				Return Character.AmbientSet.Animations.First().Frames.First().Source
			Else
				Return Character.Sets.First().Animations.First().Frames.First().Source
			End If
		End Function

		''' <summary>
		''' Even if the parameter type might seem misleading, its just because we need to register
		''' some events, so its not enough to just have the terrain array.
		''' </summary>
		''' <param name="inTerrain"></param>
		Public Sub AttachTerrain(ByVal inTerrain As TerrainDefinition)
			If RenderThread Is Thread.CurrentThread Then
				Throw New InvalidOperationException("This method must not be called from the rendering thread.")
			End If

			If inTerrain Is Nothing Then
				Throw New ArgumentNullException()
			End If

			If TerrainRenderer IsNot Nothing Then
				TerrainRenderer.Dispose()

				Terrain = Nothing
				TerrainRenderer = Nothing
			End If

			Terrain = inTerrain

			Do While TerrainRenderer Is Nothing
				Thread.Sleep(30)
			Loop
		End Sub
	End Class
End Namespace
