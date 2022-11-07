Imports Migration.Buildings
Imports Migration.Rendering

Namespace Migration.Visuals
	Public Class VisualBuilding

		Private privateRenderer As TerrainRenderer
		Public Property Renderer() As TerrainRenderer
			Get
				Return privateRenderer
			End Get
			Private Set(ByVal value As TerrainRenderer)
				privateRenderer = value
			End Set
		End Property

		Private privateBuilding As BaseBuilding
		Public Property Building() As BaseBuilding
			Get
				Return privateBuilding
			End Get
			Private Set(ByVal value As BaseBuilding)
				privateBuilding = value
			End Set
		End Property
		Private privateVisual As RenderableCharacter
		Public Property Visual() As RenderableCharacter
			Get
				Return privateVisual
			End Get
			Private Set(ByVal value As RenderableCharacter)
				privateVisual = value
			End Set
		End Property

		Private Sub New(ByVal inRenderer As TerrainRenderer, ByVal inBuilding As BaseBuilding)
			If (inRenderer Is Nothing) OrElse (inBuilding Is Nothing) Then
				Throw New ArgumentNullException()
			End If

			Building = inBuilding
			Renderer = inRenderer

			Visual = inRenderer.CreateAnimation(inBuilding.Config.Character, inBuilding)
			Visual.UserContext = Me

			' all resource stacks should inherit the building's ZShift
			For Each prov As GenericResourceStack In inBuilding.ProvidersReadonly
				If (prov Is Nothing) OrElse (prov.UserContext Is Nothing) Then
					Continue For
				End If

				CType(prov.UserContext, VisualStack).Visual.ZShiftOverride = Visual.GetZShift()
			Next prov

			For Each query As GenericResourceStack In inBuilding.QueriesReadonly
				If (query Is Nothing) OrElse (query.UserContext Is Nothing) Then
					Continue For
				End If

				CType(query.UserContext, VisualStack).Visual.ZShiftOverride = Visual.GetZShift()
			Next query

			AddHandler inBuilding.OnStateChanged, AddressOf AnonymousMethod1

			If inBuilding.IsReady Then
				Visual.Play("Flag")
			End If
		End Sub

		Private Sub AnonymousMethod1(ByVal building As BaseBuilding)
			If building.IsReady Then
				Visual.Play("Flag")
			End If
		End Sub

		Public Shared Sub Assign(ByVal inRenderer As TerrainRenderer, ByVal inBuilding As BaseBuilding)
			If inBuilding.UserContext IsNot Nothing Then
				Throw New InvalidOperationException()
			End If

			inBuilding.UserContext = New VisualBuilding(inRenderer, inBuilding)
		End Sub

		Public Shared Sub Detach(ByVal inBuilding As BaseBuilding)
			If inBuilding.UserContext Is Nothing Then
				Return
			End If

			Dim visualizer As VisualBuilding = TryCast(inBuilding.UserContext, VisualBuilding)

			If visualizer.Visual IsNot Nothing Then
				visualizer.Renderer.RemoveVisual(visualizer.Visual)
			End If

			inBuilding.UserContext = Nothing
		End Sub
	End Class
End Namespace
