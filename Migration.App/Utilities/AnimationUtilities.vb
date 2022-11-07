Imports Migration.Buildings
Imports Migration.Common
Imports Migration.Core
Imports Migration.Rendering
Imports Migration.Visuals

Namespace Migration
	Public Class AnimationUtilities
		Inherits VisualUtilities

		''' <summary>
		''' Schedules the given task for execution at the next rendering cycle. 
		''' </summary>
		Protected Overrides Sub SynchronizeTaskImpl(ByVal inTask As Procedure)
			Game.Setup.Terrain.SynchronizeTask(inTask)
		End Sub

		Protected Overrides Sub SynchronizeTaskImpl(ByVal inDurationMillis As Long, ByVal inTask As Procedure)
			Game.Setup.Terrain.SynchronizeTask(inDurationMillis, inTask)
		End Sub

		Protected Overrides Sub HideImpl(ByVal inMovable As Movable)
			If inMovable.IsMarkedForRemoval Then
				Return
			End If

			Game.Setup.Terrain.SynchronizeTask(Sub()
				Dim visualizer As VisualMovable = TryCast(inMovable.UserContext, VisualMovable)
				Game.Setup.Terrain.RemoveVisual(visualizer.Visual)
				visualizer.Visual = Nothing
			End Sub)

		End Sub

		Protected Overrides Sub DirectedAnimationImpl(ByVal inMovable As Movable, ByVal inClass As String, ByVal inDirectionOverride? As Direction, ByVal inRestart As Boolean, ByVal inRepeat As Boolean)
			If inMovable.IsMarkedForRemoval Then
				Return
			End If

			Game.Setup.Terrain.SynchronizeTask(Sub()
				Dim visualizer As VisualMovable = TryCast(inMovable.UserContext, VisualMovable)
				Dim anim As MovableOrientedAnimation = visualizer.Visual
				If visualizer.Visual IsNot Nothing Then
					If anim.Character.Name <> inClass Then
						Game.Setup.Terrain.RemoveVisual(anim)
						visualizer.Visual = Nothing
					End If
				End If
				If visualizer.Visual Is Nothing Then
					anim = Game.Setup.Terrain.CreateMovableOrientedAnimation(inClass, inMovable, 2)
					visualizer.Visual = anim
					anim.UserContext = visualizer
				End If
				If inRestart Then
					anim.ResetTime()
				End If
				anim.IsRepeated = inRepeat
				anim.DirectionOverride = inDirectionOverride
			End Sub)
		End Sub

		Protected Overrides Sub AnimateImpl(ByVal inMovable As Movable, ByVal inClass As String, ByVal inRestart As Boolean, ByVal inRepeat As Boolean)
			If inMovable.IsMarkedForRemoval Then
				Return
			End If

			Game.Setup.Terrain.SynchronizeTask(Sub()
				Dim visualizer As VisualMovable = TryCast(inMovable.UserContext, VisualMovable)
				Dim anim As MovableOrientedAnimation = visualizer.Visual
				If visualizer.Visual IsNot Nothing Then
					If anim.Character.Name <> inClass Then
						Game.Setup.Terrain.RemoveVisual(anim)
						visualizer.Visual = Nothing
					End If
				End If
				If visualizer.Visual Is Nothing Then
					visualizer.Visual = Game.Setup.Terrain.CreateMovableOrientedAnimation(inClass, inMovable, 2)
					anim = visualizer.Visual
					anim.UserContext = visualizer
				End If
				If inRestart Then
					anim.ResetTime()
				End If
				anim.IsRepeated = inRepeat
				anim.DirectionOverride = Nothing
			End Sub)
		End Sub

		Protected Overrides Sub AnimateAroundCenterImpl(ByVal inCenter As Point, ByVal inCharacter As String, ByVal inAnimationSet As String)
			Dim Character As Character = AnimationLibrary.Instance.FindClass(inCharacter)
			Dim anim As RenderableCharacter = Game.Setup.Terrain.CreateAnimation(Character, New PositionTracker() With {.Position = CyclePoint.FromGrid(inCenter)}, 1.0F)
			Dim animSet As AnimationSet = Character.FindSet(inAnimationSet)

			anim.IsCentered = True
			anim.IsRepeated = False
			anim.ResetTime()
			anim.Play(animSet)

			SynchronizeTask(animSet.DurationMillis, Sub() Game.Setup.Terrain.RemoveVisual(anim))
		End Sub

		Protected Overrides Function GetDurationMillisImpl(ByVal inMovable As Movable, ByVal inAnimClass As String) As Integer
			Return Convert.ToInt32(CInt(Fix(AnimationLibrary.Instance.FindClass(inAnimClass).Sets.Max(Function(e) e.DurationMillis))))
		End Function

		Protected Overrides Function HasAnimationImpl(ByVal inMovable As Movable, ByVal inAnimClass As String) As Boolean
			Return AnimationLibrary.Instance.HasClass(inAnimClass)
		End Function

		Protected Overrides Function HasAnimationImpl(ByVal inFoilage As Foilage, ByVal inAnimClass As String) As Boolean
			Return AnimationLibrary.Instance.FindClass(inFoilage.Type.ToString()).HasSet(inAnimClass)
		End Function

		Protected Overrides Function GetDurationMillisImpl(ByVal inFoilage As Foilage, ByVal inState As String) As Integer
			Return Convert.ToInt32(CInt(AnimationLibrary.Instance.FindClass(inFoilage.Type.ToString()).FindSet(inState).DurationMillis))
		End Function

		Protected Overrides Sub AnimateImpl(ByVal inFoilage As Foilage, ByVal inState As String, ByVal inRestart As Boolean, ByVal inRepeat As Boolean)
			Game.Setup.Terrain.SynchronizeTask(Sub()
				Dim visualizer As VisualFoilage = TryCast(inFoilage.UserContext, VisualFoilage)
				Dim anim As RenderableCharacter = visualizer.Visual
				If anim IsNot Nothing Then
					anim.Play(inState)
					anim.IsRepeated = inRepeat
					If inRestart Then
						anim.ResetTime()
					End If
				End If
			End Sub)
		End Sub

		Protected Overrides Sub AnimateImpl(ByVal inStone As Stone)
			Game.Setup.Terrain.SynchronizeTask(Sub()
				Dim visualizer As VisualStone = TryCast(inStone.UserContext, VisualStone)
				Dim anim As RenderableCharacter = visualizer.Visual
				anim.Play(inStone.ActualStones.ToString())
			End Sub)
		End Sub

		Protected Overrides Sub ShowErrorImpl(ByVal inMessage As String)
			System.Windows.Forms.MessageBox.Show(inMessage)
		End Sub

		Protected Overrides Sub ShowMessageImpl(ByVal inMessage As String)
			System.Windows.Forms.MessageBox.Show(inMessage)
		End Sub

		Protected Overrides Sub AnimateImpl(ByVal inBuilding As BaseBuilding, ByVal inClass As String)
			Game.Setup.Terrain.SynchronizeTask(Sub()
				Dim visualizer As VisualBuilding = TryCast(inBuilding.UserContext, VisualBuilding)
				Dim anim As RenderableCharacter = visualizer.Visual
				If anim IsNot Nothing Then
					If inClass IsNot Nothing Then
						anim.Play("Flag", inClass)
						anim.IsRepeated = True
						anim.ResetTime()
					Else
						anim.Play("Flag")
						anim.IsRepeated = True
					End If
				End If
			End Sub)
		End Sub

		Protected Overrides Function GetDurationMillisImpl(ByVal inBuilding As BaseBuilding, ByVal inCharacter As String) As Integer
			Return Convert.ToInt32(CInt(AnimationLibrary.Instance.FindClass(inBuilding.Config.Character).FindSet(inCharacter).DurationMillis))
		End Function

		Protected Overrides Function HasAnimationImpl(ByVal inBuilding As BaseBuilding, ByVal inCharacter As String) As Boolean
			Return AnimationLibrary.Instance.FindClass(inBuilding.Config.Character).HasSet(inCharacter)
		End Function
	End Class
End Namespace
