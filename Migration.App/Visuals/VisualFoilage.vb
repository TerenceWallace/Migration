Imports Migration.Common
Imports Migration.Rendering

Namespace Migration.Visuals
	Public Class VisualFoilage

		Private privateRenderer As TerrainRenderer
		Public Property Renderer() As TerrainRenderer
			Get
				Return privateRenderer
			End Get
			Private Set(ByVal value As TerrainRenderer)
				privateRenderer = value
			End Set
		End Property

		Private privateFoilage As Foilage
		Public Property Foilage() As Foilage
			Get
				Return privateFoilage
			End Get
			Private Set(ByVal value As Foilage)
				privateFoilage = value
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

		Private Sub Databind(ByVal inRenderer As TerrainRenderer, ByVal inFoilage As Foilage)
			If (inRenderer Is Nothing) OrElse (inFoilage Is Nothing) Then
				Throw New ArgumentNullException()
			End If

			Foilage = inFoilage
			Renderer = inRenderer
			Foilage.UserContext = Me

			Visual = Renderer.CreateAnimation(inFoilage.Type.ToString(), Foilage)
			Visual.UserContext = Me
			AddHandler Foilage.OnStateChanged, AddressOf Foilage_OnStateChanged

			Foilage_OnStateChanged(Foilage)
		End Sub

		Private Sub Foilage_OnStateChanged(ByVal inParam As Foilage)
			Dim animSet As String = Nothing
			Dim repeat As Boolean = True

			Select Case inParam.State
				Case FoilageState.Growing
					animSet = "Growing"
					repeat = False
				Case FoilageState.Grown
					animSet = "Ambient"
				Case Else
					Return
			End Select

			VisualUtilities.Animate(Foilage, animSet, True, repeat)
		End Sub

		Public Shared Sub Assign(ByVal inRenderer As TerrainRenderer, ByVal inFoilage As Foilage)
			If inFoilage.UserContext IsNot Nothing Then
				Throw New InvalidOperationException()
			End If

			Dim v As New VisualFoilage()
			v.Databind(inRenderer, inFoilage)
		End Sub

		Public Shared Sub Detach(ByVal inFoilage As Foilage)
			If inFoilage.UserContext Is Nothing Then
				Return
			End If

			Dim visualizer As VisualFoilage = TryCast(inFoilage.UserContext, VisualFoilage)

			If visualizer.Visual IsNot Nothing Then
				visualizer.Renderer.RemoveVisual(visualizer.Visual)
			End If

			inFoilage.UserContext = Nothing
		End Sub
	End Class
End Namespace
