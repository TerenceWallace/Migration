Imports Migration.Common



#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else
Imports OpenTK.Graphics.OpenGL
#End If

Namespace Migration.Rendering
	Partial Public Class TerrainRenderer

		Public Sub OnRender()
			If PrecisionTimerCallback Is Nothing Then
				Throw New InvalidOperationException("No precision timer available.")
			End If

            Dim animTime As Int64 = PrecisionTimerCallback.Invoke()

			SyncLock m_WorkItems
				Dim list As LinkedListNode(Of WorkItem) = m_WorkItems.First

				Do While list IsNot Nothing
					Dim node As LinkedListNode(Of WorkItem) = list.Next

					If list.Value.ExpirationMillis <= animTime Then
						m_WorkItems.Remove(list.Value)

						list.Value.Task()
					End If

					list = node
				Loop
			End SyncLock

			' compute visible area of the map
			GL.Disable(EnableCap.DepthTest)

			ScreenBounds = ComputeOcclusion(m_ProjMatrix, m_ViewMatrix, m_ModelMatrix)

			' collect all visible renderables
			Dim visibleObjects As New List(Of RenderableVisual)()

			SyncLock m_SceneObjects
				m_SceneObjects.CopyArea(ScreenBounds, visibleObjects)
			End SyncLock

			GL.Enable(EnableCap.AlphaTest)
			GL.AlphaFunc(AlphaFunction.Greater, 0.0F)

			UpdateObjectSelection(visibleObjects)

			GL.Disable(EnableCap.AlphaTest)

			GL.Enable(EnableCap.DepthTest)
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha)
			GL.Enable(EnableCap.Blend)

			RenderGroundPlane(m_ProjMatrix, m_ViewMatrix, m_ModelMatrix)

			GL.Enable(EnableCap.AlphaTest)
			GL.AlphaFunc(AlphaFunction.Greater, 0.0F)
			GL.DepthFunc(DepthFunction.Lequal)
			GL.Clear(ClearBufferMask.DepthBufferBit)

			m_2DSpriteProgram.Bind(m_ProjMatrix, m_ViewMatrix, m_ModelMatrix)
			RadixRenderSprites(visibleObjects)
			m_2DSceneProgram.Bind(m_ProjMatrix, m_ViewMatrix, m_ModelMatrix)

			GL.Disable(EnableCap.AlphaTest)
			GL.Disable(EnableCap.Blend)

			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha)
			GL.Enable(EnableCap.Blend)
			' render level points
			If IsBuildingGridVisible Then
				Dim w As Single = 0.7F
				Dim s As Single = (1 - w) / 2

				For Each mStack As Stack(Of Point) In m_BuildingPipeline
					mStack.Clear()
				Next mStack

				Dim y As Integer = ScreenBounds.Y
				Dim height As Integer = Math.Min(Size - 1, ScreenBounds.Y + ScreenBounds.Height)
				Do While y < height
					Dim x As Integer = ScreenBounds.X
					Dim width As Integer = Math.Min(Size - 1, ScreenBounds.X + ScreenBounds.Width)
					Do While x < width
						Dim eval As Integer = Terrain.GetBuildingExpenses(x, y)

						If eval <= 1 Then
							x += 1
							Continue Do
						End If

						m_BuildingPipeline(Math.Min(m_BuildingGridTextures.Length - 1, eval - 2)).Push(New Point(x, y))
						x += 1
					Loop
					y += 1
				Loop

				For i As Integer = 0 To m_BuildingPipeline.Length - 1
					m_BuildingGridTextures(i).Bind()

					GL.Begin(BeginMode.Quads)
					For Each pos As Point In m_BuildingPipeline(i)
						DrawAtlasStripe(Nothing, pos.X + s, pos.Y + s, Terrain.GetZShiftAt(pos.X, pos.Y), w, w, 255, 255, 255, 255, 0F, 0F, 1F, 1F)
					Next pos
					GL.End()
				Next i
			End If
			GL.Disable(EnableCap.Blend)
		End Sub

		Private Sub RadixRenderSprites(ByVal visibleObjects As IEnumerable(Of RenderableVisual))
            Dim time As Long = PrecisionTimerCallback.Invoke

			If SpriteEngine Is Nothing Then
				' fallback for AnimationEditor
				For Each visual As RenderableVisual In visibleObjects
					visual.SetAnimationTime(time)
					visual.Render(RenderPass.Pass1_Shadow)
				Next visual
			Else
				' using high performance sprite engine
				SpriteEngine.BeginRadixRender()
				For Each visual As RenderableVisual In visibleObjects
					visual.SetAnimationTime(time)
					visual.Render(RenderPass.Pass1_Shadow)
				Next visual
				SpriteEngine.EndRadixRender()
			End If
		End Sub

		Friend Sub DrawAtlas(ByVal inVisual As RenderableVisual, ByVal inLeft As Single, ByVal inTop As Single, ByVal inZShift As Single, ByVal inWidth As Single, ByVal inHeight As Single, ByVal inRed As Byte, ByVal inGreen As Byte, ByVal inBlue As Byte, ByVal inAlpha As Byte, ByVal inTexX1 As Single, ByVal inTexY1 As Single, ByVal inTexX2 As Single, ByVal inTexY2 As Single)
			GL.Begin(BeginMode.Quads)
			DrawAtlasStripe(inVisual, inLeft, inTop, inZShift, inWidth, inHeight, inRed, inGreen, inBlue, inAlpha, inTexX1, inTexY1, inTexX2, inTexY2)
			GL.End()
		End Sub

		Friend Sub DrawAtlasStripe(ByVal inVisual As RenderableVisual, ByVal inLeft As Single, ByVal inTop As Single, ByVal inZShift As Single, ByVal inWidth As Single, ByVal inHeight As Single, ByVal inRed As Byte, ByVal inGreen As Byte, ByVal inBlue As Byte, ByVal inAlpha As Byte, ByVal inTexX1 As Single, ByVal inTexY1 As Single, ByVal inTexX2 As Single, ByVal inTexY2 As Single)
			If m_IsSelectionPass Then
				Dim selectionIndex As Integer = -1

				m_SelectionPassResults.Add(inVisual)

				selectionIndex = m_SelectionPassResults.Count * SelectionGranularity
				inRed = Convert.ToByte((selectionIndex And &HFF))
				inGreen = Convert.ToByte((selectionIndex And &HFF00) >> 8)
				inBlue = Convert.ToByte((selectionIndex And &HFF0000) >> 16)
			End If

			GL.Color4(inRed, inGreen, inBlue, inAlpha)

			If (inVisual IsNot Nothing) AndAlso (inVisual.BuildingProgress < 1.0) AndAlso (inVisual.BuildingProgress >= 0.0) Then
				Dim texYSize As Single = (inTexY1 - inTexY2) * Convert.ToSingle(inVisual.BuildingProgress)
				Dim offsetX As Single = inHeight - inHeight * Convert.ToSingle(inVisual.BuildingProgress)

				inTexY1 = inTexY2 + texYSize

				GL.TexCoord2(inTexX1, inTexY1)
				GL.Vertex3(inLeft, inTop + offsetX, inZShift)
				GL.TexCoord2(inTexX2, inTexY1)
				GL.Vertex3(inLeft + inWidth, inTop + offsetX, inZShift)
				GL.TexCoord2(inTexX2, inTexY2)
				GL.Vertex3(inLeft + inWidth, inTop + inHeight, inZShift)
				GL.TexCoord2(inTexX1, inTexY2)
				GL.Vertex3(inLeft, inTop + inHeight, inZShift)
			Else
				GL.TexCoord2(inTexX1, inTexY1)
				GL.Vertex3(inLeft, inTop, inZShift)
				GL.TexCoord2(inTexX2, inTexY1)
				GL.Vertex3(inLeft + inWidth, inTop, inZShift)
				GL.TexCoord2(inTexX2, inTexY2)
				GL.Vertex3(inLeft + inWidth, inTop + inHeight, inZShift)
				GL.TexCoord2(inTexX1, inTexY2)
				GL.Vertex3(inLeft, inTop + inHeight, inZShift)
			End If
		End Sub
	End Class
End Namespace
