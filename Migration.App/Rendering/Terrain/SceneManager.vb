Imports Migration.Interfaces


#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else

#End If

Namespace Migration.Rendering
	Partial Public Class TerrainRenderer

		''' <summary>
		''' "Internal" method used by AnimationEditor.
		''' </summary>
		Public Function CreateMovableOrientedAnimation(ByVal inCharacter As Character, ByVal inMovable As Movable, ByVal inWidth As Double) As MovableOrientedAnimation
			Dim visual As New MovableOrientedAnimation(Me, inCharacter, inMovable)

			SyncLock m_SceneObjects
				m_SceneObjects.Add(visual)
			End SyncLock

			Return visual
		End Function

		Public Function CreateMovableOrientedAnimation(ByVal inCharacter As String, ByVal inMovable As Movable, ByVal inWidth As Double) As MovableOrientedAnimation
			Return CreateMovableOrientedAnimation(AnimationLibrary.Instance.FindClass(inCharacter), inMovable, inWidth)
		End Function

		''' <summary>
		''' "Internal" method used by AnimationEditor.
		''' </summary>
		Public Function CreateAnimation(ByVal inCharacter As Character, ByVal inPositionTracker As IPositionTracker, ByVal inOpacity As Double) As RenderableCharacter
			If inPositionTracker Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Dim visual As New RenderableCharacter(Me, inCharacter, inPositionTracker)
			visual.Opacity = inOpacity

			SyncLock m_SceneObjects
				m_SceneObjects.Add(visual)
			End SyncLock

			Return visual
		End Function

		Public Function CreateAnimation(ByVal inCharacter As String, ByVal inPositionTracker As IPositionTracker, ByVal inOpacity As Double) As RenderableCharacter
			Return CreateAnimation(AnimationLibrary.Instance.FindClass(inCharacter), inPositionTracker, inOpacity)
		End Function

		Public Function CreateAnimation(ByVal inCharacter As String, ByVal inPositionTracker As IPositionTracker) As RenderableCharacter
			Return CreateAnimation(AnimationLibrary.Instance.FindClass(inCharacter), inPositionTracker, 1.0)
		End Function

		Public Sub RemoveVisual(ByVal inVisual As RenderableVisual)
			If inVisual Is Nothing Then
				Throw New ArgumentNullException()
			End If

			SyncLock m_SceneObjects
				m_SceneObjects.Remove(inVisual)
			End SyncLock
		End Sub
	End Class
End Namespace
