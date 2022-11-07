Imports Migration.Rendering

Namespace Migration.Visuals
	Public Class VisualStack

		Private privateRenderer As TerrainRenderer
		Public Property Renderer() As TerrainRenderer
			Get
				Return privateRenderer
			End Get
			Private Set(ByVal value As TerrainRenderer)
				privateRenderer = value
			End Set
		End Property

		Private privateStack As GenericResourceStack
		Public Property Stack() As GenericResourceStack
			Get
				Return privateStack
			End Get
			Private Set(ByVal value As GenericResourceStack)
				privateStack = value
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

		Private Sub Databind(ByVal inRenderer As TerrainRenderer, ByVal inStack As GenericResourceStack)
			If (inRenderer Is Nothing) OrElse (inStack Is Nothing) Then
				Throw New ArgumentNullException()
			End If

			Stack = inStack
			Renderer = inRenderer
			Stack.UserContext = Me

			Visual = inRenderer.CreateAnimation("ResourceStacks", inStack, 2.0)
			Visual.UserContext = Me
			AddHandler inStack.OnCountChanged, AddressOf Stack_OnCountChanged

			Dim inStackResource As String = inStack.Resource.ToString()
			Visual.Play(inStackResource)

			Stack_OnCountChanged(inStack, 0, 0)
		End Sub

		Private Sub Stack_OnCountChanged(ByVal inSender As GenericResourceStack, ByVal inOldValue As Integer, ByVal inNewValue As Integer)
			Dim avail As Integer = inSender.Available

			If avail > 0 Then
				Visual.FrozenFrameIndex = avail - 1

				Visual.IsVisible = True
			Else
				Visual.IsVisible = False
			End If
		End Sub

		Public Shared Sub Assign(ByVal inRenderer As TerrainRenderer, ByVal inStack As GenericResourceStack)
			If inStack.UserContext IsNot Nothing Then
				Throw New InvalidOperationException()
			End If

			Dim v As New VisualStack()
			v.Databind(inRenderer, inStack)
		End Sub

		Public Shared Sub Detach(ByVal inStack As GenericResourceStack)
			If inStack.UserContext Is Nothing Then
				Return
			End If

			Dim visualizer As VisualStack = TryCast(inStack.UserContext, VisualStack)

			If visualizer.Visual IsNot Nothing Then
				visualizer.Renderer.RemoveVisual(visualizer.Visual)
			End If

			inStack.UserContext = Nothing
		End Sub
	End Class
End Namespace
