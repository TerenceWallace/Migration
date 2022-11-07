Imports Migration.Common
Imports Migration.Rendering
Imports Migration.Jobs

Namespace Migration.Visuals
	Public Class VisualMovable

		Private privateRenderer As TerrainRenderer
		Public Property Renderer() As TerrainRenderer
			Get
				Return privateRenderer
			End Get
			Private Set(ByVal value As TerrainRenderer)
				privateRenderer = value
			End Set
		End Property

		Private privateMovable As Movable
		Public Property Movable() As Movable
			Get
				Return privateMovable
			End Get
			Private Set(ByVal value As Movable)
				privateMovable = value
			End Set
		End Property

		Private privateVisual As MovableOrientedAnimation
		Public Property Visual() As MovableOrientedAnimation
			Get
				Return privateVisual
			End Get
			Set(ByVal value As MovableOrientedAnimation)
				privateVisual = value
			End Set
		End Property

		Private Sub Databind(ByVal inRenderer As TerrainRenderer, ByVal inMovable As Movable)
			If (inRenderer Is Nothing) OrElse (inMovable Is Nothing) Then
				Throw New ArgumentNullException()
			End If

			Movable = inMovable
			Renderer = inRenderer
			Movable.UserContext = Me

			AddHandler Movable.OnJobChanged, AddressOf Migrant_OnJobChanged

			Migrant_OnJobChanged(Movable, Nothing, Movable.Job)
		End Sub

		Public Shared Sub Assign(ByVal inRenderer As TerrainRenderer, ByVal inMovable As Movable)
			If inMovable.UserContext IsNot Nothing Then
				Throw New InvalidOperationException()
			End If

			Dim v As New VisualMovable()
			v.Databind(inRenderer, inMovable)
		End Sub

		Public Shared Sub Detach(ByVal inMovable As Movable)
			If inMovable.UserContext Is Nothing Then
				Return
			End If

			Dim visualizer As VisualMovable = TryCast(inMovable.UserContext, VisualMovable)

			If visualizer.Visual IsNot Nothing Then
				visualizer.Renderer.RemoveVisual(visualizer.Visual)
			End If

			visualizer.Visual = Nothing
			inMovable.UserContext = Nothing
		End Sub

		Private Shared Function GetDefaultCharacter(ByVal inMovable As Movable) As String
			Select Case inMovable.MovableType
				Case MovableType.Migrant
					Return "MigrantWalking"
				Case MovableType.Grader
					Return "GraderWalking"
				Case MovableType.Constructor
					Return "ConstructorWalking"
				Case Else
					Throw New ApplicationException()
			End Select
		End Function

		Private Shared Sub Migrant_OnJobChanged(ByVal inMovable As Movable, ByVal inOldJob As JobBase, ByVal inNewJob As JobBase)
			Dim defaultCharacter As String = GetDefaultCharacter(inMovable)

			If inOldJob IsNot Nothing Then
				RemoveHandler inOldJob.OnCharacterChanged, AddressOf MigrantJob_OnCharacterChanged
			End If

			If inNewJob IsNot Nothing Then
				AddHandler inNewJob.OnCharacterChanged, AddressOf MigrantJob_OnCharacterChanged

				If TypeOf inNewJob Is JobOrder Then
					Dim dirJob As JobOrder = TryCast(inNewJob, JobOrder)

					VisualUtilities.DirectedAnimation(inMovable, (If(String.IsNullOrEmpty(inNewJob.Character), defaultCharacter, inNewJob.Character)), dirJob.Direction, False, True)
				Else
					VisualUtilities.Animate(inMovable, (If(String.IsNullOrEmpty(inNewJob.Character), defaultCharacter, inNewJob.Character)), False, True)
				End If
			Else
				VisualUtilities.Animate(inMovable, defaultCharacter, False, True)
			End If
		End Sub

		Private Shared Sub MigrantJob_OnCharacterChanged(ByVal inParam As JobBase)
			Dim defaultCharacter As String = GetDefaultCharacter(inParam.Movable)

			If TypeOf inParam Is JobOrder Then
				Dim dirJob As JobOrder = TryCast(inParam, JobOrder)

				VisualUtilities.DirectedAnimation(inParam.Movable, (If(String.IsNullOrEmpty(inParam.Character), defaultCharacter, inParam.Character)), dirJob.Direction, False, True)
			Else
				VisualUtilities.Animate(inParam.Movable, (If(String.IsNullOrEmpty(inParam.Character), defaultCharacter, inParam.Character)), False, True)
			End If
		End Sub
	End Class
End Namespace
