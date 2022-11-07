Imports Migration.Rendering

Namespace Migration.Visuals
	Public Class VisualStone

		Private privateRenderer As TerrainRenderer
		Public Property Renderer() As TerrainRenderer
			Get
				Return privateRenderer
			End Get
			Private Set(ByVal value As TerrainRenderer)
				privateRenderer = value
			End Set
		End Property

		Private privateStone As Stone
		Public Property Stone() As Stone
			Get
				Return privateStone
			End Get
			Private Set(ByVal value As Stone)
				privateStone = value
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

		Private Sub Databind(ByVal inRenderer As TerrainRenderer, ByVal inStone As Stone)
			If (inRenderer Is Nothing) OrElse (inStone Is Nothing) Then
				Throw New ArgumentNullException()
			End If

			Stone = inStone
			Renderer = inRenderer
			Stone.UserContext = Me

			Dim anim As RenderableCharacter = Renderer.CreateAnimation("Stone", Stone)
			Visual = anim
			Visual.UserContext = Me

			AddHandler Stone.OnStateChanged, AddressOf Stone_OnStateChanged

			Stone_OnStateChanged(Stone)
		End Sub

		Private Sub Stone_OnStateChanged(ByVal inParam As Stone)
			VisualUtilities.Animate(Stone)
		End Sub

		Public Shared Sub Assign(ByVal inRenderer As TerrainRenderer, ByVal inStone As Stone)
			If inStone.UserContext IsNot Nothing Then
				Throw New InvalidOperationException()
			End If

			Dim v As New VisualStone()
			v.Databind(inRenderer, inStone)
		End Sub

		Public Shared Sub Detach(ByVal inStone As Stone)
			If inStone.UserContext Is Nothing Then
				Return
			End If

			Dim visualizer As VisualStone = TryCast(inStone.UserContext, VisualStone)

			If visualizer.Visual IsNot Nothing Then
				visualizer.Renderer.RemoveVisual(visualizer.Visual)
			End If

			inStone.UserContext = Nothing
		End Sub
	End Class
End Namespace
