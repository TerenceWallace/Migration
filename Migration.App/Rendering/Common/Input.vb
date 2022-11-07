Imports OpenTK.Input



#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else

#End If

Namespace Migration.Rendering
	Partial Public Class Renderer
		Private Sub RaiseMouseMove(ByVal sender As Object, ByVal args As MouseMoveEventArgs)
			Dim inNewMouseX As Integer = args.X
			Dim inNewMouseY As Integer = args.Y

			inNewMouseX = Math.Max(0, Math.Min(inNewMouseX, ViewportWidth - 1))
			inNewMouseY = Math.Max(0, Math.Min(inNewMouseY, ViewportWidth - 1))

			MouseXY = New Point(inNewMouseX, inNewMouseY)

			RaiseEvent OnMouseMove(Me, New Point(inNewMouseX, inNewMouseY))
		End Sub

		Private Sub RaiseKeyboardKeyUp(ByVal sender As Object, ByVal e As KeyboardKeyEventArgs)
			SyncLock m_KeyboardState
				m_KeyboardState.Remove(e.Key)
			End SyncLock

			RaiseEvent OnKeyUp(Me, e.Key)
		End Sub

		Private Sub RaiseKeyboardKeyDown(ByVal sender As Object, ByVal e As KeyboardKeyEventArgs)
			Dim hasChanged As Boolean = False

			SyncLock m_KeyboardState
				hasChanged = Not(m_KeyboardState.Contains(e.Key))

				If hasChanged Then
					m_KeyboardState.Add(e.Key)
				End If
			End SyncLock

			If hasChanged AndAlso (OnKeyDownEvent IsNot Nothing) Then
				RaiseEvent OnKeyDown(Me, e.Key)
			End If
		End Sub

		Private Sub RaiseKeyboardKeyRepeat()
			If OnKeyRepeatEvent Is Nothing Then
				Return
			End If

			For Each key As OpenTK.Input.Key In m_KeyboardState
				RaiseEvent OnKeyRepeat(Me, key)
			Next key
		End Sub

		Private Sub RaiseMouseButtonUp(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
			Dim btn As Integer = Convert.ToInt32(CInt(Fix(e.Button)))

			SyncLock m_MouseState
				m_MouseState.Remove(btn)
			End SyncLock

			RaiseEvent OnMouseUp(Me, btn)
		End Sub

		Private Sub RaiseMouseButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
			Dim btn As Integer = Convert.ToInt32(CInt(Fix(e.Button)))

			SyncLock m_MouseState
				m_MouseState.Add(btn)
			End SyncLock

			RaiseEvent OnMouseDown(Me, btn)
		End Sub
	End Class
End Namespace
