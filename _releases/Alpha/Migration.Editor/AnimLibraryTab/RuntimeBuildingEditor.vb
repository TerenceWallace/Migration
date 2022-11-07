Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Rendering

Namespace Migration.Editor
	Public Class RuntimeBuildingConfig
		Inherits BuildingConfiguration

		Private privateBuilding As RuntimeBuildingEditor
		Public Property Building() As RuntimeBuildingEditor
			Get
				Return privateBuilding
			End Get
			Private Set(ByVal value As RuntimeBuildingEditor)
				privateBuilding = value
			End Set
		End Property

		Public Sub New(ByVal inBuilding As RuntimeBuildingEditor)
			Building = inBuilding
		End Sub
	End Class

	Public Class RuntimeBuildingEditor
		Private privateResourceStacks As List(Of RenderableCharacter)
		Public Property ResourceStacks() As List(Of RenderableCharacter)
			Get
				Return privateResourceStacks
			End Get
			Private Set(ByVal value As List(Of RenderableCharacter))
				privateResourceStacks = value
			End Set
		End Property
		Private privateSurroundingBuildings As List(Of RenderableCharacter)
		Public Property SurroundingBuildings() As List(Of RenderableCharacter)
			Get
				Return privateSurroundingBuildings
			End Get
			Private Set(ByVal value As List(Of RenderableCharacter))
				privateSurroundingBuildings = value
			End Set
		End Property
		Private privateSurroundingSettlers As List(Of RenderableCharacter)
		Public Property SurroundingSettlers() As List(Of RenderableCharacter)
			Get
				Return privateSurroundingSettlers
			End Get
			Private Set(ByVal value As List(Of RenderableCharacter))
				privateSurroundingSettlers = value
			End Set
		End Property
		Private privateGroundPlane As List(Of Point)
		Public Property GroundPlane() As List(Of Point)
			Get
				Return privateGroundPlane
			End Get
			Private Set(ByVal value As List(Of Point))
				privateGroundPlane = value
			End Set
		End Property
		Private privateReservedPlane As List(Of Point)
		Public Property ReservedPlane() As List(Of Point)
			Get
				Return privateReservedPlane
			End Get
			Private Set(ByVal value As List(Of Point))
				privateReservedPlane = value
			End Set
		End Property
		Private privateZMargin As Double
		Public Property ZMargin() As Double
			Get
				Return privateZMargin
			End Get
			Private Set(ByVal value As Double)
				privateZMargin = value
			End Set
		End Property
		Private privateClass As Character
		Public Property Character() As Character
			Get
				Return privateClass
			End Get
			Private Set(ByVal value As Character)
				privateClass = value
			End Set
		End Property
		Private privateRenderable As RenderableCharacter
		Public Property Renderable() As RenderableCharacter
			Get
				Return privateRenderable
			End Get
			Private Set(ByVal value As RenderableCharacter)
				privateRenderable = value
			End Set
		End Property
		Private privatePosition As Point
		Public Property Position() As Point
			Get
				Return privatePosition
			End Get
			Private Set(ByVal value As Point)
				privatePosition = value
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

		Private privateConfig As BuildingConfiguration
		Public Property Config() As BuildingConfiguration
			Get
				Return privateConfig
			End Get
			Private Set(ByVal value As BuildingConfiguration)
				privateConfig = value
			End Set
		End Property
		Private privateShowBuildings As Boolean
		Public Property ShowBuildings() As Boolean
			Get
				Return privateShowBuildings
			End Get
			Set(ByVal value As Boolean)
				privateShowBuildings = value
			End Set
		End Property
		Private privateShowSettlers As Boolean
		Public Property ShowSettlers() As Boolean
			Get
				Return privateShowSettlers
			End Get
			Set(ByVal value As Boolean)
				privateShowSettlers = value
			End Set
		End Property

		Public Sub SetPosition(ByVal inPosition As Point)
			Dim oldPosition As Point = Renderable.Position.ToPoint()
			Dim xDelta As Integer = -oldPosition.X + inPosition.X
			Dim yDelta As Integer = -oldPosition.Y + inPosition.Y

			' clamp position to boundaries
			For Each stack In ResourceStacks
				If stack.Position.XGrid + xDelta < 0 Then
					xDelta = -stack.Position.XGrid
				End If

				If stack.Position.YGrid + yDelta < 0 Then
					yDelta = -stack.Position.YGrid
				End If
			Next stack

			Position = New Point(oldPosition.X + xDelta, oldPosition.Y + yDelta)
			Renderable.Position = CyclePoint.FromGrid(Position)

			For Each stack In ResourceStacks
				stack.Position = CyclePoint.FromGrid(stack.Position.XGrid + xDelta, stack.Position.YGrid + yDelta)
			Next stack
		End Sub

		Public Sub New(ByVal inRenderable As RenderableCharacter, ByVal inTerrain As TerrainDefinition)
			Character = inRenderable.Character
			Renderable = inRenderable
			ReservedPlane = New List(Of Point)()
			ResourceStacks = New List(Of RenderableCharacter)()
			SurroundingBuildings = New List(Of RenderableCharacter)()
			SurroundingSettlers = New List(Of RenderableCharacter)()
			GroundPlane = New List(Of Point)()
			Terrain = inTerrain
			Config = New RuntimeBuildingConfig(Me)

			For Each rect In Character.GroundPlane
				For y As Integer = 0 To rect.Height - 1
					For x As Integer = 0 To rect.Width - 1
						GroundPlane.Add(New Point(x + rect.X, y + rect.Y))
					Next x
				Next y
			Next rect

			For Each rect In Character.ReservedPlane
				For y As Integer = 0 To rect.Height - 1
					For x As Integer = 0 To rect.Width - 1
						ReservedPlane.Add(New Point(x + rect.X, y + rect.Y))
					Next x
				Next y
			Next rect
		End Sub

		Public Function GetSurroundingSettlersSpots() As IEnumerable(Of Point)
			Return GetSurroundingSpots(Function(pos) GroundPlane.Contains(pos))
		End Function

		Public Function GetSurroundingBuildingsSpots() As IEnumerable(Of Point)
			Return GetSurroundingSpots(Function(pos) Terrain.CanBuildingBePlacedAt(pos.X, pos.Y, Config))
		End Function

		Private Function GetSurroundingSpots(ByVal inHandler As Func(Of Point, Boolean)) As IEnumerable(Of Point)
			Dim spots As New List(Of Point)()
			Dim minX As Integer = If(GroundPlane.Count > 0, GroundPlane.Min(Function(e) e.X), 0)
			Dim minY As Integer = If(GroundPlane.Count > 0, GroundPlane.Min(Function(e) e.Y), 0)
			Dim grid(Terrain.Size - 1, Terrain.Size - 1) As Integer

			' set all grid cells occupied by a ground cell to "1"
			For y1 As Integer = 0 To grid.GetLength(1) - 1
				For x As Integer = 0 To grid.GetLength(0) - 1
					Dim pos As New Point(x + minX - 1, y1 + minY - 1)

					If inHandler(pos) Then
						grid(x, y1) = 1
					End If
				Next x
			Next y1

			' add all grid cells to spots that have neighbors of value "1"
			Dim y As Integer = 0
			Dim yCount As Integer = grid.GetLength(1)
			Do While y < yCount
				Dim x As Integer = 0
				Dim xCount As Integer = grid.GetLength(0)
				Do While x < xCount
					Dim hasNeighbour As Boolean = False

					If grid(x, y) = 1 Then
						x += 1
						Continue Do
					End If

					For iy As Integer = -1 To 1
						For ix As Integer = -1 To 1
							Dim posX As Integer = x + ix
							Dim posY As Integer = y + iy

							If (posX < 0) OrElse (posX >= xCount) OrElse (posY < 0) OrElse (posY >= yCount) Then
								Continue For
							End If

							If grid(posX, posY) = 1 Then
								hasNeighbour = True
							End If
						Next ix
					Next iy

					If hasNeighbour Then
						spots.Add(New Point(minX - 1 + x, minY - 1 + y))
					End If
					x += 1
				Loop
				y += 1
			Loop

			Return spots
		End Function

		Public Sub Save()
			privateClass.GroundPlane.Clear()
			privateClass.ReservedPlane.Clear()
			privateClass.ResourceStacks.Clear()

			For Each pos In GroundPlane
				privateClass.GroundPlane.Add(New Rectangle(pos.X, pos.Y, 1, 1))
			Next pos

			For Each pos In ReservedPlane
				privateClass.ReservedPlane.Add(New Rectangle(pos.X, pos.Y, 1, 1))
			Next pos

			For Each stack In ResourceStacks
				privateClass.ResourceStacks.Add(New ResourceStackEntry() With {.Position = New Point(stack.Position.XGrid - Position.X, stack.Position.YGrid - Position.Y), .Resource = (CType(System.Enum.Parse(GetType(Resource), stack.PlayingSets.First().Name), Resource))})
			Next stack
		End Sub
	End Class
End Namespace
