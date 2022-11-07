Imports Migration.Common

Imports System.Threading

#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else
Imports OpenTK.Graphics.OpenGL
#End If

Namespace Migration.Rendering
	Partial Public Class TerrainRenderer

		Private Sub UpdateObjectSelection(ByVal visibleObjects As IEnumerable(Of RenderableVisual))
			'            ######################################################
			'             * Update selection for objects
			'             *#####################################################
			m_SelectionPassResults.Clear()
			GL.ClearColor(Color.Black)
			GL.Clear(ClearBufferMask.ColorBufferBit Or ClearBufferMask.DepthBufferBit)

			' setup GLSL program
			m_2DSceneSelectProgram.Bind(m_ProjMatrix, m_ViewMatrix, m_ModelMatrix)

			' selection pass (each one is rendered with an unique color)
			m_IsSelectionPass = True
			RadixRenderSprites(visibleObjects)
			m_IsSelectionPass = False

			' process selection information
			Dim selectedPixels(0) As Integer
			Dim selValue As Integer = 0
			Dim prevMouseOver As RenderableVisual = MouseOverVisual

			GL.ReadPixels(Renderer.MouseXY.X, Renderer.ViewportHeight - Renderer.MouseXY.Y, 1, 1, PixelFormat.Rgb, PixelType.UnsignedByte, selectedPixels)

			If selectedPixels(0) > 0 Then
				selValue = (selectedPixels(0) \ SelectionGranularity) - 1

				If (selValue >= 0) AndAlso (selValue < m_SelectionPassResults.Count) Then
					MouseOverVisual = m_SelectionPassResults(selValue)
				Else
					MouseOverVisual = Nothing
				End If
			Else
				MouseOverVisual = Nothing
			End If

			If prevMouseOver IsNot MouseOverVisual Then
				' raise selection events
				Dim newOver As RenderableVisual = MouseOverVisual

				ThreadPool.QueueUserWorkItem(Sub(state As Object)
					Dim mouseLeave As DNotifyHandler(Of TerrainRenderer, RenderableVisual) = OnMouseLeaveEvent
					Dim mouseEnter As DNotifyHandler(Of TerrainRenderer, RenderableVisual) = OnMouseEnterEvent
					If (mouseLeave IsNot Nothing) AndAlso (prevMouseOver IsNot Nothing) Then
						mouseLeave(Me, prevMouseOver)
					End If
					If (mouseEnter IsNot Nothing) AndAlso (newOver IsNot Nothing) Then
						mouseEnter(Me, newOver)
					End If
				End Sub)

			End If

			'            ######################################################
			'             * Update selection for terrain
			'             *#####################################################
			Dim gridPos As New Point()

			MouseToGridPos(Renderer.MouseXY, m_ProjMatrix, m_ViewMatrix, m_ModelMatrix, gridPos)

			If GridXY <> gridPos Then
				GridXY = gridPos

				RaiseEvent OnMouseGridMove(Me, GridXY)
			End If
		End Sub

	End Class
End Namespace
