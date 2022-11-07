Imports Migration.Buildings
Imports Migration.Core
Imports Migration.Rendering

Namespace Migration.Visuals
	Public Class VisualBuildTask
		Private Shared ReadOnly m_Tasks As New LinkedList(Of VisualBuildTask)()

		Private m_Renderables As New List(Of RenderableVisual)()
		Private m_Node As LinkedListNode(Of VisualBuildTask) = Nothing

		Private privateTask As BuildTask
		Public Property Task() As BuildTask
			Get
				Return privateTask
			End Get
			Private Set(ByVal value As BuildTask)
				privateTask = value
			End Set
		End Property

		Public ReadOnly Property Building() As BaseBuilding
			Get
				Return Task.Building
			End Get
		End Property

		Private privateRenderer As TerrainRenderer
		Public Property Renderer() As TerrainRenderer
			Get
				Return privateRenderer
			End Get
			Private Set(ByVal value As TerrainRenderer)
				privateRenderer = value
			End Set
		End Property

		Public Shared Function GetBuildTaskAt(ByVal inPosition As Point) As VisualBuildTask

			For Each  m_task As VisualBuildTask In m_Tasks

				Dim  m_building As BaseBuilding =  m_task.Task.Building
				Dim rect As New Rectangle( m_building.Position.XGrid,  m_building.Position.YGrid,  m_building.Config.GridWidth,  m_building.Config.GridHeight)

				If rect.Contains(inPosition) Then
					Return  m_task
				End If
			Next  m_task

			Return Nothing
		End Function

		Private Sub Databind(ByVal inRenderer As TerrainRenderer, ByVal inTask As BuildTask)
			AddHandler inTask.OnStateChanged, AddressOf Map_OnBuildTaskStateChanged
			Renderer = inRenderer
			Task = inTask

			Map_OnBuildTaskStateChanged(inTask)
		End Sub

		Public Shared Sub Assign(ByVal inRenderer As TerrainRenderer, ByVal inTask As BuildTask)
			If inTask.UserContext IsNot Nothing Then
				Throw New InvalidOperationException()
			End If

			Dim visual As New VisualBuildTask()
			visual.Databind(inRenderer, inTask)

			inTask.UserContext = visual
			visual.m_Node = m_Tasks.AddLast(visual)
		End Sub

		Public Shared Sub Detach(ByVal inTask As BuildTask)
			If inTask.UserContext Is Nothing Then
				Return
			End If

			Dim visualizer As VisualBuildTask = TryCast(inTask.UserContext, VisualBuildTask)

			visualizer.ReleaseRenderables()
			m_Tasks.Remove(visualizer.m_Node)

			visualizer.m_Node = Nothing
			inTask.UserContext = Nothing
		End Sub

		Private Function CreateBuildingMarker(ByVal inPosX As Integer, ByVal inPosY As Integer) As RenderableCharacter
			Dim anim As RenderableCharacter = Renderer.CreateAnimation("Other", New PositionTracker() With {.Position = CyclePoint.FromGrid(inPosX, inPosY)})

			anim.Play("BuildMarker")

			Return anim
		End Function

		Private Sub ReleaseRenderables()
			For Each renderable As RenderableVisual In m_Renderables
				Renderer.RemoveVisual(renderable)
			Next renderable

			m_Renderables.Clear()
		End Sub

		Private Sub Map_OnBuildTaskStateChanged(ByVal inTask As BuildTask)
			If Not inTask.IsGraded Then
				' mark grading area
				Dim pos As Point = inTask.Building.Position.ToPoint()

				m_Renderables.Add(CreateBuildingMarker(pos.X, pos.Y))
                m_Renderables.Add(CreateBuildingMarker(pos.X + inTask.Configuration.GridWidth, pos.Y))
                m_Renderables.Add(CreateBuildingMarker(pos.X + inTask.Configuration.GridWidth, pos.Y + inTask.Configuration.GridHeight))
                m_Renderables.Add(CreateBuildingMarker(pos.X, pos.Y + inTask.Configuration.GridHeight))
			ElseIf Not inTask.IsBuilt Then
				' animate build progress
				Dim renderable As RenderableCharacter = Renderer.CreateAnimation(Building.Config.Character, Building)

				renderable.BuildingProgress = 0
				ReleaseRenderables()

				m_Renderables.Add(renderable)
				AddHandler inTask.OnProgressChanged, Sub(task As BuildTask, progress As Double) renderable.BuildingProgress = progress

			ElseIf Not inTask.IsCompleted Then
				' missing Migrant or tool?
				ReleaseRenderables()

				m_Renderables.Add(Renderer.CreateAnimation(Building.Config.Character, Building))
			Else
				ReleaseRenderables()

				Detach(inTask)
			End If
		End Sub

	End Class
End Namespace
