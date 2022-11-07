Imports Migration.Common
Imports System.IO
Imports Migration.Core

Imports OpenTK


#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else
Imports OpenTK.Graphics.OpenGL
#End If

Namespace Migration.Rendering

	Partial Public Class TerrainRenderer

		Private m_BuildingPipeline() As Stack(Of Point)
		Private m_IsSelectionPass As Boolean = False
		Private m_ProjMatrix As Matrix4
		Private m_ViewMatrix As Matrix4
		Private m_ModelMatrix As Matrix4
		Private m_2DSceneProgram As Program
		Private m_2DSceneSelectProgram As Program
		Private m_2DSpriteProgram As Program
		Private m_Mesh As TerrainMesh
		Private m_Shader As Program
		Private m_WaterShader As Program
		Private m_SelectionShader As Program
		Private m_OcclusionShader As Program
		Private m_GroundTextures(6) As NativeTexture

		Private m_TimeMillisID As Integer = -1
		Private m_SelectionOffsetXID As Integer = -1
		Private m_SelectionOffsetYID As Integer = -1
		Private m_BuildingGridTextures() As NativeTexture

		Private m_ScreenXY As New PointDouble(0, 0)
		Friend Const SelectionGranularity As Integer = 5

		Private ReadOnly m_WorkItems As New LinkedList(Of WorkItem)()
		Private ReadOnly m_SceneObjects As TopologicalList(Of RenderableVisual)
		Private ReadOnly m_SelectionPassResults As New List(Of RenderableVisual)()

		Public Event OnMouseEnter As DNotifyHandler(Of TerrainRenderer, RenderableVisual)
		Public Event OnMouseLeave As DNotifyHandler(Of TerrainRenderer, RenderableVisual)
		Public Event OnMouseGridMove As DNotifyHandler(Of TerrainRenderer, Point)
		Public Event OnScreenMove As DNotifyHandler(Of TerrainRenderer)

		Private privatePrecisionTimerCallback As DPrecisionTimerMillis
		Public Property PrecisionTimerCallback() As DPrecisionTimerMillis
			Get
				Return privatePrecisionTimerCallback
			End Get
			Set(ByVal value As DPrecisionTimerMillis)
				privatePrecisionTimerCallback = value
			End Set
		End Property

		Private privateSpriteEngine As SpriteEngine
		Public Property SpriteEngine() As SpriteEngine
			Get
				Return privateSpriteEngine
			End Get
			Private Set(ByVal value As SpriteEngine)
				privateSpriteEngine = value
			End Set
		End Property

		Private privateIsBuildingGridVisible As Boolean
		Public Property IsBuildingGridVisible() As Boolean
			Get
				Return privateIsBuildingGridVisible
			End Get
			Set(ByVal value As Boolean)
				privateIsBuildingGridVisible = value
			End Set
		End Property

		Private privateGridXY As Point
		Public Property GridXY() As Point
			Get
				Return privateGridXY
			End Get
			Private Set(ByVal value As Point)
				privateGridXY = value
			End Set
		End Property

		Private privateMouseOverVisual As RenderableVisual
		Public Property MouseOverVisual() As RenderableVisual
			Get
				Return privateMouseOverVisual
			End Get
			Private Set(ByVal value As RenderableVisual)
				privateMouseOverVisual = value
			End Set
		End Property

		Private privateScreenBounds As Rectangle
		Public Property ScreenBounds() As Rectangle
			Get
				Return privateScreenBounds
			End Get
			Private Set(ByVal value As Rectangle)
				privateScreenBounds = value
			End Set
		End Property

		Private privateHeightScale As Double
		Public Property HeightScale() As Double
			Get
				Return privateHeightScale
			End Get
			Private Set(ByVal value As Double)
				privateHeightScale = value
			End Set
		End Property

		Private privateRenderer As Renderer
		Public Property Renderer() As Renderer
			Get
				Return privateRenderer
			End Get
			Private Set(ByVal value As Renderer)
				privateRenderer = value
			End Set
		End Property

		Private privateTerrain As TerrainDefinition
		Public Property Terrain() As TerrainDefinition
			Get
				Return privateTerrain
			End Get
			Private Set(ByVal value As TerrainDefinition)
				privateTerrain = value
			End Set
		End Property

		Private privateSize As Integer
		Public Property Size() As Integer
			Get
				Return privateSize
			End Get
			Private Set(ByVal value As Integer)
				privateSize = value
			End Set
		End Property

		Public Property ScreenXY() As PointDouble
			Get
				Return m_ScreenXY
			End Get
			Set(ByVal value As PointDouble)
				'Point newPos = new Point(
				'    Math.Max(0, Math.Min(Size - 1, value.X)),
				'    Math.Max(0, Math.Min(Size - 1, value.Y)));
				Dim newPos As PointDouble = value

				If newPos = m_ScreenXY Then
					Return
				End If

				m_ScreenXY = newPos
				m_ViewMatrix = Matrix4.CreateTranslation(Convert.ToSingle(-m_ScreenXY.X), Convert.ToSingle(-m_ScreenXY.Y), 0F)

				RaiseEvent OnScreenMove(Me)
			End Set
		End Property

		Private Function LoadImageResource(ByVal inNameWithoutExtension As String) As Bitmap
			Dim path As String = [Global].GetResourcePath(inNameWithoutExtension)

			If Not(File.Exists(path & ".png")) Then
				path = path & ".jpg"
			Else
				path = path & ".png"
			End If

			If Not(File.Exists(path)) Then
				Throw New FileNotFoundException(path)
			End If

			Return CType(Bitmap.FromFile(path), Bitmap)
		End Function

		Public Sub SynchronizeTask(ByVal inTask As Procedure)
			SynchronizeTask(0, inTask)
		End Sub

		Public Sub SynchronizeTask(ByVal DurationMillis As Long, ByVal inTask As Procedure)
			If inTask Is Nothing Then
				Throw New ArgumentNullException()
			End If

            Dim item As New WorkItem() With {.Task = inTask, .ExpirationMillis = PrecisionTimerCallback.Invoke() + DurationMillis}

			SyncLock m_WorkItems
				Dim list As LinkedListNode(Of WorkItem) = m_WorkItems.First

				Do While list IsNot Nothing
					If list.Value.ExpirationMillis > item.ExpirationMillis Then
						' insert work item
						m_WorkItems.AddBefore(list, item)

						Exit Do
					End If

					list = list.Next
				Loop

				If list Is Nothing Then
					' insert work item
					m_WorkItems.AddLast(item)
				End If
			End SyncLock
		End Sub

		Public Sub New(ByVal inRenderer As Renderer, ByVal inTerrain As TerrainDefinition)
			Renderer = inRenderer
			Terrain = inTerrain
			Size = Terrain.Size
			HeightScale = Terrain.Config.HeightScale
			m_SceneObjects = New TopologicalList(Of RenderableVisual)(TerrainMesh.BlockSize, Terrain.Size, Terrain.Size)
			m_ModelMatrix = Matrix4.Identity
			m_ViewMatrix = m_ModelMatrix
			m_ProjMatrix = m_ViewMatrix

			If AnimationLibrary.Instance.IsReadonly Then
				SpriteEngine = Migration.Rendering.SpriteEngine.OpenOrCreate("Textures.cache", 2048)

				' are cached textures outdated?
				If Not(SpriteEngine.CustomChecksums.Contains(AnimationLibrary.Instance.Checksum)) Then
					If MessageBox.Show("Texture cache either seems to be missing or outdated. To proceed (run the game), this cache must be updated which can take up to minutes, depending on your system performance. Do you want to proceed?", "Generating Texture Cache...", MessageBoxButtons.YesNo) <> DialogResult.Yes Then
						System.Diagnostics.Process.GetCurrentProcess().Kill()
					End If

					SpriteEngine.BeginUpdate()
					' add all animation libraries...
					For Each Character As Character In AnimationLibrary.Instance.Classes
						Dim name As String = Character.Name

						SpriteEngine.UpdateClass(Character)
					Next Character

					SpriteEngine.CustomChecksums.Add(AnimationLibrary.Instance.Checksum)
					SpriteEngine.EndUpdate()
				End If
			End If

			' instantiate shaders
			Dim shader_TerrainVert As String = ParameterizeFragmentShader(File.ReadAllText([Global].GetResourcePath("Shaders/Terrain.vert")), inTerrain.Config)
			Dim shader_TerrainFrag As String = ParameterizeFragmentShader(File.ReadAllText([Global].GetResourcePath("Shaders/Terrain.frag")), inTerrain.Config)
			Dim shader_WaterFrag As String = ParameterizeFragmentShader(File.ReadAllText([Global].GetResourcePath("Shaders/Water.frag")), inTerrain.Config)
			Dim shader_WaterVert As String = ParameterizeFragmentShader(File.ReadAllText([Global].GetResourcePath("Shaders/Water.vert")), inTerrain.Config)
			Dim shader_SelectionVert As String = ParameterizeFragmentShader(File.ReadAllText([Global].GetResourcePath("Shaders/TerrainSelection.vert")), inTerrain.Config)

			m_Shader = New Program(New Shader([Global].GetResourcePath("Shaders/Terrain.vert"), shader_TerrainVert), New Shader([Global].GetResourcePath("Shaders/Terrain.frag"), shader_TerrainFrag), True)
			m_WaterShader = New Program(New Shader([Global].GetResourcePath("Shaders/Water.vert"), shader_WaterVert), New Shader([Global].GetResourcePath("Shaders/Water.frag"), shader_WaterFrag), True)
			m_SelectionShader = New Program(New Shader([Global].GetResourcePath("Shaders/TerrainSelection.vert"), shader_SelectionVert), New Shader([Global].GetResourcePath("Shaders/TerrainSelection.frag")), True)
			m_OcclusionShader = New Program(New Shader([Global].GetResourcePath("Shaders/TerrainOcclusion.vert"), shader_SelectionVert), New Shader([Global].GetResourcePath("Shaders/TerrainOcclusion.frag")), True)
			m_2DSceneSelectProgram = New Program(New Shader([Global].GetResourcePath("Shaders/2DSceneSelect.vert")), New Shader([Global].GetResourcePath("Shaders/2DSceneSelect.frag")), True)
			m_2DSceneProgram = New Program(New Shader([Global].GetResourcePath("Shaders/2DScene.vert")), New Shader([Global].GetResourcePath("Shaders/2DScene.frag")), True)
			m_2DSpriteProgram = New Program(New Shader([Global].GetResourcePath("Shaders/2DSprite.vert")), New Shader([Global].GetResourcePath("Shaders/2DScene.frag")), True)

			m_TimeMillisID = GL.GetUniformLocation(m_WaterShader.ProgramID, "timeMillis")
			If m_TimeMillisID < 0 Then
				Throw New ArgumentException("Terrain shader does not export uniform ""timeMillis"".")
			End If

			m_SelectionOffsetXID = GL.GetUniformLocation(m_SelectionShader.ProgramID, "SelectionOffsetXID")
			If m_SelectionOffsetXID < 0 Then
				Throw New ArgumentException("Terrain selection shader does not export uniform ""SelectionOffsetXID"".")
			End If

			m_SelectionOffsetYID = GL.GetUniformLocation(m_SelectionShader.ProgramID, "SelectionOffsetYID")
			If m_SelectionOffsetYID < 0 Then
				Throw New ArgumentException("Terrain selection shader does not export uniform ""SelectionOffsetYID"".")
			End If

			m_Mesh = New TerrainMesh(Me)

			' load textures for ground plane
			m_GroundTextures(0) = New NativeTexture(TextureOptions.Repeat, LoadImageResource("Terrain/Highland/High/Level_00"))
			m_GroundTextures(1) = New NativeTexture(TextureOptions.Repeat, LoadImageResource("Terrain/Highland/High/Level_01"))
			m_GroundTextures(2) = New NativeTexture(TextureOptions.Repeat, LoadImageResource("Terrain/Highland/High/Level_02"))
			m_GroundTextures(3) = New NativeTexture(TextureOptions.Repeat, LoadImageResource("Terrain/Highland/High/Level_03"))
			m_GroundTextures(4) = New NativeTexture(TextureOptions.Repeat, LoadImageResource("Terrain/Highland/High/Level_04"))
			m_GroundTextures(5) = m_Mesh.HeightMap
			m_GroundTextures(6) = New NativeTexture(TextureOptions.Repeat, LoadImageResource("Terrain/Highland/High/Water"))

			' load textures for building grid
			m_BuildingGridTextures = New NativeTexture(5){}
			m_BuildingPipeline = New Stack(Of Point)(5){}

			For i As Integer = 0 To m_BuildingGridTextures.Length - 1
				m_BuildingGridTextures(i) = New NativeTexture(CType(Bitmap.FromFile([Global].GetResourcePath("Icons/BuildingGrid/Level_0" & i.ToString() & ".png")), Bitmap))
				m_BuildingPipeline(i) = New Stack(Of Point)()
			Next i

			UpdateViewport()
		End Sub

		Public Sub UpdateViewport()
			Dim CellWidth As Double = 45.0 / 900 * Renderer.ViewportWidth

			m_ProjMatrix = Matrix4.CreateOrthographicOffCenter(0F, Convert.ToSingle(CellWidth), Convert.ToSingle(CellWidth / Renderer.AspectRatio), 0F, -1000F, 1000F)
			m_ModelMatrix = Matrix4.CreateRotationX(Convert.ToSingle(Math.Asin(Math.PI / 4)))

		End Sub

		Public Sub Dispose()
			If m_Shader IsNot Nothing Then
				m_Shader.Dispose()
			End If

			If m_WaterShader IsNot Nothing Then
				m_WaterShader.Dispose()
			End If

			If m_OcclusionShader IsNot Nothing Then
				m_OcclusionShader.Dispose()
			End If

			If m_SelectionShader IsNot Nothing Then
				m_SelectionShader.Dispose()
			End If

			If m_GroundTextures IsNot Nothing Then
				For Each tex As NativeTexture In m_GroundTextures
					tex.Dispose()
				Next tex
			End If

			If m_Mesh IsNot Nothing Then
				m_Mesh.Dispose()
			End If

			m_Mesh = Nothing
			m_GroundTextures = Nothing
			Renderer = Nothing
			m_Shader = Nothing
			m_WaterShader = Nothing
			m_SelectionShader = Nothing
			m_OcclusionShader = Nothing
		End Sub

	End Class
End Namespace
