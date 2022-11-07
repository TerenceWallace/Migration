Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Rendering
Imports System.Windows.Controls

Namespace Migration.Editor
	''' <summary>
	''' Interaction logic for AnimLibraryTab.xaml
	''' </summary>
	Partial Public Class AnimLibraryTab
		Inherits UserControl

		Private m_RenderableClass As RenderableCharacter = Nothing
		Private m_Terrain As TerrainDefinition
		Private m_AnimIsMouseDown As Boolean = False
		Private m_RenderableResource As RenderableCharacter
		Private m_BuildingEditor As RuntimeBuildingEditor
		Private m_Watch As New System.Diagnostics.Stopwatch()

		Private Sub InitializeAnimationPlayer()
			Game.Setup.Language = MarkupLanguage.Load(My.Resources.ConfigurationFile)

			Dim playerContainer = New System.Windows.Forms.ContainerControl()

			m_AnimationPlayer = Renderer.CreateControl(playerContainer)
			m_Terrain = New TerrainDefinitionEditor()
			m_Watch.Start()

			System.Threading.ThreadPool.QueueUserWorkItem(AddressOf AnonymousMethod1)

			AddHandler m_AnimationPlayer.OnMouseDown, AddressOf AnonymousMethod2
			AddHandler m_AnimationPlayer.OnMouseUp, AddressOf AnonymousMethod3

			HOST_AnimationPlayer.Child = playerContainer
		End Sub

		Private Sub AnonymousMethod1(ByVal unused As Object)
			m_AnimationPlayer.AttachTerrain(m_Terrain)
			m_AnimationPlayer.TerrainRenderer.PrecisionTimerCallback = Function() m_Watch.ElapsedMilliseconds
			m_AnimationPlayer.EnableTerrainRendering = True
			AddHandler m_AnimationPlayer.TerrainRenderer.OnMouseGridMove, AddressOf AnonymousMethod4
		End Sub

		Private Sub AnonymousMethod4(ByVal sender As TerrainRenderer, ByVal grid As Point)
			If Not m_AnimIsMouseDown Then
				Return
			End If
			Dim renderable As RenderableCharacter = m_RenderableClass
			If renderable IsNot Nothing Then
				If RADIO_AnimMove.IsChecked.Value Then
					m_BuildingEditor.SetPosition(grid)
				End If
			End If
			If RADIO_AnimResource.IsChecked.Value Then
				m_RenderableResource.Position = CyclePoint.FromGrid(grid)
			End If
		End Sub

		Private Sub AnonymousMethod2(ByVal sender As Renderer, ByVal btn As Integer)
			m_AnimIsMouseDown = True
			If btn = Convert.ToInt32(OpenTK.Input.MouseButton.Middle) Then
				m_AnimationPlayer.TerrainRenderer.IsBuildingGridVisible = False

				If RADIO_AnimZPlane.IsChecked.Value Then
					For Each spot In m_BuildingEditor.GetSurroundingSettlersSpots()
						AnimationLibrary.Instance.ForcePositionShift = True
						Dim anim As RenderableCharacter = m_AnimationPlayer.TerrainRenderer.CreateAnimation("SettlerWalking", New PositionTracker(), 1)
						AnimationLibrary.Instance.ForcePositionShift = False
						m_BuildingEditor.SurroundingSettlers.Add(anim)
						anim.Position = CyclePoint.FromGrid(spot.X + m_BuildingEditor.Position.X, spot.Y + m_BuildingEditor.Position.Y)
						anim.Play("FrozenAngle_045")
					Next spot
				End If

				If RADIO_AnimBuild.IsChecked.Value Then
					For Each spot In m_BuildingEditor.GetSurroundingBuildingsSpots()
						Dim anim As RenderableCharacter = m_AnimationPlayer.TerrainRenderer.CreateAnimation(m_BuildingEditor.Character, New PositionTracker(), 1)
						m_BuildingEditor.SurroundingBuildings.Add(anim)
						anim.Position = CyclePoint.FromGrid(spot)
					Next spot
				End If
			End If
		End Sub

		Private Sub AnonymousMethod3(ByVal sender As Renderer, ByVal btn As Integer)
			Dim gridPos = m_AnimationPlayer.TerrainRenderer.GridXY
			Dim relPos = New Point(gridPos.X - m_BuildingEditor.Position.X, gridPos.Y - m_BuildingEditor.Position.Y)
			m_AnimIsMouseDown = False

			If btn = Convert.ToInt32(OpenTK.Input.MouseButton.Right) Then
				If RADIO_AnimResource.IsChecked.Value Then
					m_BuildingEditor.ResourceStacks.Add(m_RenderableResource)
					m_RenderableResource = Nothing
					RADIO_AnimMove.IsChecked = True
				End If
				If RADIO_AnimZPlane.IsChecked.Value Then
					m_BuildingEditor.GroundPlane.Remove(relPos)
				End If
				If RADIO_AnimBuild.IsChecked.Value Then
					m_BuildingEditor.ReservedPlane.Remove(relPos)
				End If

			ElseIf btn = Convert.ToInt32(OpenTK.Input.MouseButton.Left) Then
				If RADIO_AnimZPlane.IsChecked.Value Then
					If Not(m_BuildingEditor.GroundPlane.Contains(relPos)) Then
						m_BuildingEditor.GroundPlane.Add(relPos)
					End If
				End If
				If RADIO_AnimBuild.IsChecked.Value Then
					If Not(m_BuildingEditor.ReservedPlane.Contains(relPos)) Then
						m_BuildingEditor.ReservedPlane.Add(relPos)
					End If
				End If

			ElseIf btn = Convert.ToInt32(OpenTK.Input.MouseButton.Middle) Then
				If RADIO_AnimZPlane.IsChecked.Value Then
					m_AnimationPlayer.TerrainRenderer.IsBuildingGridVisible = True
					ClearRendableList(m_BuildingEditor.SurroundingSettlers)
				End If
				If RADIO_AnimBuild.IsChecked.Value Then
					m_AnimationPlayer.TerrainRenderer.IsBuildingGridVisible = True
					ClearRendableList(m_BuildingEditor.SurroundingBuildings)
				End If
				If RADIO_AnimResource.IsChecked.Value Then
					ClearRendableList(m_BuildingEditor.ResourceStacks)
				End If
			End If
		End Sub

		Private Sub UpdateAnimationPlayer()
			Dim Character = TCurrentClass
			Dim animSet = TCurrentSet

			If (m_RenderableClass IsNot Nothing) AndAlso (Character IsNot m_RenderableClass.Character) Then
				If m_BuildingEditor IsNot Nothing Then
					m_BuildingEditor.Save()
				End If

				' remove previous renderable
				m_AnimationPlayer.TerrainRenderer.RemoveVisual(m_RenderableClass)

				m_RenderableClass = Nothing
			End If

			If m_RenderableClass IsNot Nothing Then
				' change animation set
				If animSet Is Nothing Then
					m_RenderableClass.Stop()
				Else
					m_RenderableClass.Play(animSet)
				End If
			Else
				If Character Is Nothing Then
					Return
				End If

				' create new class renderer
				m_RenderableClass = m_AnimationPlayer.TerrainRenderer.CreateAnimation(Character, New PositionTracker(), 1)

				If animSet IsNot Nothing Then
					m_RenderableClass.Play(animSet)
				End If

				m_AnimationPlayer.TerrainRenderer.ScreenXY = New PointDouble(0, 0)
				InitBuildingEditor(m_RenderableClass)
			End If

			If RADIO_AnimResource.IsChecked.Value Then
				Dim resource As Resource = CType(COMBO_AnimResource.SelectedValue, Resource)

				If m_RenderableResource Is Nothing Then
					m_RenderableResource = m_AnimationPlayer.TerrainRenderer.CreateAnimation("ResourceStacks", New PositionTracker(), 1)
					m_RenderableResource.FrozenFrameIndex = 7
				End If

				m_RenderableResource.Play(resource.ToString())
			Else
				If m_RenderableResource IsNot Nothing Then
					m_AnimationPlayer.TerrainRenderer.RemoveVisual(m_RenderableResource)
				End If

				m_RenderableResource = Nothing
			End If

			If RADIO_AnimBuild.IsChecked.Value OrElse RADIO_AnimZPlane.IsChecked.Value Then
				m_AnimationPlayer.TerrainRenderer.IsBuildingGridVisible = True
			Else
				m_AnimationPlayer.TerrainRenderer.IsBuildingGridVisible = False
			End If

			If m_BuildingEditor IsNot Nothing Then
				m_BuildingEditor.ShowBuildings = RADIO_AnimBuild.IsChecked.Value
				m_BuildingEditor.ShowSettlers = RADIO_AnimZPlane.IsChecked.Value
			End If
		End Sub

		Private Sub ClearRendableList(ByVal inList As List(Of RenderableCharacter))
			For Each stack In inList
				m_AnimationPlayer.TerrainRenderer.RemoveVisual(stack)
			Next stack

			inList.Clear()
		End Sub

		Private Sub InitBuildingEditor(ByVal inClass As RenderableCharacter)
			If m_BuildingEditor IsNot Nothing Then
				ClearRendableList(m_BuildingEditor.ResourceStacks)
			End If

			m_BuildingEditor = New RuntimeBuildingEditor(inClass, m_Terrain)
			m_Terrain.ResetBuildingGrid(m_BuildingEditor.Config)

			For Each stack In inClass.Character.ResourceStacks
				Dim anim As RenderableCharacter = m_AnimationPlayer.TerrainRenderer.CreateAnimation("ResourceStacks", New PositionTracker() With {.Position = CyclePoint.FromGrid(stack.Position)}, 1)

				anim.FrozenFrameIndex = 7
				anim.Play(stack.Resource.ToString())

				m_BuildingEditor.ResourceStacks.Add(anim)
			Next stack
		End Sub
	End Class
End Namespace
