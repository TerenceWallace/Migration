Imports System.Threading
Imports Migration.Buildings
Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core
Imports Migration.Rendering
Imports Migration.Visuals

Namespace Migration.Game

	Public NotInheritable Class Setup

		Friend Shared Map As Migration.Game.Map

		Private Shared m_BuildingGrid As GridState

		Private Shared m_GUILoaderTimer As System.Threading.Timer
		Public Shared ReadOnly Property Timer() As System.Threading.Timer
			Get
				Return m_GUILoaderTimer
			End Get
		End Property

		Private Shared privateRenderer As Renderer
		Public Shared Property Renderer() As Renderer
			Get
				Return privateRenderer
			End Get
			Private Set(ByVal value As Renderer)
				privateRenderer = value
			End Set
		End Property

		Public Shared ReadOnly Property Terrain() As TerrainRenderer
			Get
				Return Renderer.TerrainRenderer
			End Get
		End Property

		Public Shared ReadOnly Property CurrentCycle() As Long
			Get
				Return Map.CurrentCycle
			End Get
		End Property

		Private Shared privateLanguage As MarkupLanguage
		Public Shared Property Language() As MarkupLanguage
			Get
				Return privateLanguage
			End Get
			Set(ByVal value As MarkupLanguage)
				privateLanguage = value
			End Set
		End Property

		Private Sub New()
		End Sub

		Public Shared Sub Initialize()

			' setup renderer
			Renderer = New Renderer(Language.Configuration)

			AddHandler Renderer.OnRenderSprites, AddressOf Render_RenderSprites
			AddHandler Renderer.OnMouseMove, AddressOf Render_MouseMove
			AddHandler Renderer.OnMouseDown, AddressOf Render_MouseDown
			AddHandler Renderer.OnMouseUp, AddressOf Render_MouseUp
			AddHandler Renderer.OnKeyDown, AddressOf Renderer_OnKeyDown
			AddHandler Renderer.OnKeyRepeat, AddressOf Renderer_OnKeyRepeat

			Dim mPath As String = [Global].GetResourcePath("Animations")
			AnimationLibrary.OpenFromDirectory(mPath)

			InitializeGame()
		End Sub

		Private Shared Sub Render_RenderSprites(ByVal unused As Renderer)
			SyncLock [Global].GUILoadLock
				Dim layout As XMLGUILayout = Gui.Loader.GUILayout
				If layout Is Nothing Then
					Return
				End If

				layout.RootElement.Render()
			End SyncLock
		End Sub

		Private Shared Sub Render_MouseMove(ByVal unused As Renderer, ByVal mouse As Point)
			SyncLock [Global].GUILoadLock
				Dim layout As XMLGUILayout = Gui.Loader.GUILayout
				If layout Is Nothing Then
					Return
				End If

				layout.RootElement.ProcessMouseMoveEvent(mouse.X, mouse.Y)
			End SyncLock
			Return
		End Sub

		Private Shared Sub Render_MouseDown(ByVal unused As Renderer, ByVal btn As Integer)
			SyncLock [Global].GUILoadLock
				Dim layout As XMLGUILayout = Gui.Loader.GUILayout
				If layout Is Nothing Then
					Return
				End If
				If layout.RootElement.ProcessMouseDownEvent(Renderer.MouseXY.X, Renderer.MouseXY.Y, btn) Then
					Return
				End If
				If btn = Control.LeftMouseButton Then
					Dim buildingGrid As GridState = m_BuildingGrid
					If buildingGrid IsNot Nothing Then
						Map.AddBuilding(buildingGrid.Config, buildingGrid.Position.Position.ToPoint())
					Else
						Terrain_OnSelectObject(Terrain.MouseOverVisual)
					End If
				ElseIf btn = Control.RightMouseButton Then
					HideBuildingGrid()
					BuildingInspector.Abort()
				End If
			End SyncLock
			Return
		End Sub

		Private Shared Sub Render_MouseUp(ByVal unused As Renderer, ByVal btn As Integer)
			SyncLock [Global].GUILoadLock
				Dim layout As XMLGUILayout = Gui.Loader.GUILayout
				If layout Is Nothing Then
					Return
				End If
				layout.RootElement.ProcessMouseUpEvent(Renderer.MouseXY.X, Renderer.MouseXY.Y, btn)
			End SyncLock
			Return
		End Sub

		Private Shared Sub Terrain_OnSelectObject(ByVal inObject As RenderableVisual)
			Gui.Loader.GUI.ShowObjectInspector(inObject)
		End Sub

		Private Shared Sub InitializeGame()
			Map = New Migration.Game.Map(512, 59, New RaceConfiguration() { Loader.Open([Global].GetResourcePath("Configuration\Roman.Buildings.xml")) })

			Renderer.AttachTerrain(Map.Terrain)

			m_GUILoaderTimer = New Timer(AddressOf OnTimerCallback, Nothing, 1000, 1000)

			AddHandler Map.OnRemoveBuilding, Sub(unused As Map, obj As BaseBuilding) VisualBuilding.Detach(obj)

			AddHandler Map.OnAddBuilding, Sub(unused As Map, obj As BaseBuilding) VisualBuilding.Assign(Terrain, obj)

			AddHandler Map.OnAddMovable, Sub(unused As Map, obj As Movable) VisualMovable.Assign(Terrain, obj)

			AddHandler Map.OnRemoveMovable, Sub(unused As Map, obj As Movable) VisualMovable.Detach(obj)

			AddHandler Map.OnAddStack, Sub(unused As Map, obj As GenericResourceStack) VisualStack.Assign(Terrain, obj)

			AddHandler Map.OnRemoveStack, Sub(unused As Map, obj As GenericResourceStack) VisualStack.Detach(obj)

            AddHandler Map.OnAddBuildTask, Sub(unused As Map, obj As BuildTask)
                                               VisualBuildTask.Assign(Terrain, obj)
                                               ShowBuildingGrid(obj.Configuration)
                                           End Sub

			AddHandler Map.OnRemoveBuildTask, Sub(unused As Map, obj As BuildTask) VisualBuildTask.Detach(obj)

			AddHandler Map.OnAddFoilage, Sub(unused As Map, obj As Foilage) VisualFoilage.Assign(Terrain, obj)

			AddHandler Map.OnRemoveFoilage, Sub(unused As Map, obj As Foilage) VisualFoilage.Detach(obj)

			AddHandler Map.OnAddStone, Sub(unused As Map, obj As Stone) VisualStone.Assign(Terrain, obj)

			AddHandler Map.OnRemoveStone, Sub(unused As Map, obj As Stone) VisualStone.Detach(obj)

			Terrain.PrecisionTimerCallback = Function() Map.AnimationTime

			AddHandler Terrain.OnMouseGridMove, Sub(sender As TerrainRenderer, gridPos As Point)
				If m_BuildingGrid IsNot Nothing Then
					gridPos.X = (gridPos.X - Convert.ToInt32(CInt(Fix(m_BuildingGrid.Config.GridWidth / 2.0))))
					gridPos.Y = (gridPos.Y - Convert.ToInt32(CInt(Fix(m_BuildingGrid.Config.GridHeight / 2.0))))
					If gridPos.X < 0 Then
						gridPos.X = 0
					End If
					If gridPos.Y < 0 Then
						gridPos.Y = 0
					End If
					m_BuildingGrid.Position.Position = CyclePoint.FromGrid(gridPos)
				End If
			End Sub

			' initialize map with random seed
			Map.GenerateMapFromFile([Global].GetResourcePath("Maps/User/Alien.xml"), 0)

			' add some initial stacks and movables
			Dim spot As Point = Map.Terrain.Spots.First()
			Map.Start(Sub() RenderMap(spot))

			Renderer.EnableTerrainRendering = True
			Terrain.ScreenXY = New PointDouble(Convert.ToDouble((spot.X - 20)), (Convert.ToDouble((spot.Y - 50)) / Math.Sqrt(2)))
			Map.ClearLog()

		End Sub

		Private Shared Sub OnTimerCallback(ByVal unused As Object)
			Gui.Loader.Update()
		End Sub

		Private Shared Sub MouseGridMove(ByVal sender As Object, ByVal gridPos As Point)
			If m_BuildingGrid IsNot Nothing Then
				gridPos.X -= m_BuildingGrid.Config.GridWidth \ 2
				gridPos.Y -= m_BuildingGrid.Config.GridHeight \ 2
				If gridPos.X < 0 Then
					gridPos.X = 0
				End If
				If gridPos.Y < 0 Then
					gridPos.Y = 0
				End If
				m_BuildingGrid.Position.Position = CyclePoint.FromGrid(gridPos)
			End If
		End Sub

		Private Shared Sub RenderMap(ByVal spot As Point)
			Dim start As Point = spot

			For x As Integer = 0 To 2
				For y As Integer = 0 To 2
					Map.AddMovable(CyclePoint.FromGrid(start.X + x, start.Y + y - 10), MovableType.Grader)
					Map.AddMovable(CyclePoint.FromGrid(start.X + x + 5, start.Y + y - 10), MovableType.Constructor)
				Next y
			Next x

			For x As Integer = 0 To 2
				For y As Integer = 0 To 2
					Map.AddMovable(CyclePoint.FromGrid(start.X + x, start.Y + y), MovableType.Migrant)
					Map.AddMovable(CyclePoint.FromGrid(start.X + x + 5, start.Y + y), MovableType.Migrant)
					Map.AddMovable(CyclePoint.FromGrid(start.X + x, start.Y + y + 5), MovableType.Migrant)
					Map.AddMovable(CyclePoint.FromGrid(start.X + x + 5, start.Y + y + 5), MovableType.Migrant)
				Next y
			Next x

			Map.DropResource(New Point(start.X, start.Y), Resource.Timber, 30)
			Map.DropResource(New Point(start.X, start.Y), Resource.Stone, 31)
			Map.DropResource(New Point(start.X, start.Y), Resource.Coal, 26)
			Map.DropResource(New Point(start.X, start.Y), Resource.IronOre, 12)
			Map.DropResource(New Point(start.X, start.Y), Resource.Hammer, 28)
			Map.DropResource(New Point(start.X, start.Y), Resource.Shovel, 15)
			Map.DropResource(New Point(start.X, start.Y), Resource.Axe, 8)
			Map.DropResource(New Point(start.X, start.Y), Resource.Saw, 3)
			Map.DropResource(New Point(start.X, start.Y), Resource.PickAxe, 11)
			Map.DropResource(New Point(start.X, start.Y), Resource.Hook, 2)
			Map.DropResource(New Point(start.X, start.Y), Resource.Scythe, 3)
			Map.DropResource(New Point(start.X, start.Y), Resource.Fish, 7)
			Map.DropResource(New Point(start.X, start.Y), Resource.Meat, 8)
			Map.DropResource(New Point(start.X, start.Y), Resource.Bread, 15)

		End Sub

		Private Shared Sub Renderer_OnKeyRepeat(ByVal inSender As Renderer, ByVal inButton As OpenTK.Input.Key)
			Dim diff As Double = 1

			Select Case inButton
				Case OpenTK.Input.Key.Right
					Terrain.ScreenXY = New PointDouble(Terrain.ScreenXY.X + diff, Terrain.ScreenXY.Y)
				Case OpenTK.Input.Key.Left
					Terrain.ScreenXY = New PointDouble(Terrain.ScreenXY.X - diff, Terrain.ScreenXY.Y)
				Case OpenTK.Input.Key.Down
					Terrain.ScreenXY = New PointDouble(Terrain.ScreenXY.X, Terrain.ScreenXY.Y + diff)
				Case OpenTK.Input.Key.Up
					Terrain.ScreenXY = New PointDouble(Terrain.ScreenXY.X, Terrain.ScreenXY.Y - diff)
			End Select
		End Sub

		Private Shared Sub Renderer_OnKeyDown(ByVal inSender As Renderer, ByVal inButton As OpenTK.Input.Key)
			Select Case inButton

				Case OpenTK.Input.Key.F12
					Map.ForwardOneMinute()

				Case OpenTK.Input.Key.S
					' save game state
					Map.SuspendGame()

					Try
						Program.StoreGameState(False)
					Finally
						Map.ResumeGame()
					End Try

				Case OpenTK.Input.Key.P
					If Map.IsSuspended Then
						Map.ResumeGame()
					Else
						Map.SuspendGame()
					End If

				Case OpenTK.Input.Key.R
					Renderer.EnableTerrainRendering = Not Renderer.EnableTerrainRendering

			End Select
		End Sub

		Public Shared Sub ShowGrid(ByVal inConfig As BuildingConfiguration)
			ShowBuildingGrid(inConfig)
		End Sub

		Private Shared Sub ShowBuildingGrid(ByVal inConfig As BuildingConfiguration)
			HideBuildingGrid()

			m_BuildingGrid = New GridState() With {.Config = inConfig, .Position = New PositionTracker(), .ShiftX = inConfig.GridWidth \ 2, .ShiftY = Convert.ToInt32(CInt(Fix((inConfig.GridHeight * 1 / Math.Sqrt(2)) / 2)))}

			Map.Terrain.ResetBuildingGrid(inConfig)
			m_BuildingGrid.Visual = Terrain.CreateAnimation(inConfig.Character, m_BuildingGrid.Position, 0.6)

			Terrain.IsBuildingGridVisible = True
		End Sub

		Private Shared Sub HideBuildingGrid()
			Terrain.IsBuildingGridVisible = False

			If m_BuildingGrid Is Nothing Then
				Return
			End If

			Terrain.RemoveVisual(m_BuildingGrid.Visual)

			m_BuildingGrid = Nothing
		End Sub

	End Class
End Namespace
