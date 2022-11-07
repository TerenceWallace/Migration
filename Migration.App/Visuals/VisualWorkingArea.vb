Imports Migration.Common
Imports Migration.Core
Imports Migration.Interfaces
Imports Migration.Rendering

Namespace Migration.Visuals
	Public Class VisualWorkingArea
		Inherits PositionTracker

		Private m_Visuals As New List(Of RenderableCharacter)()
		Private m_VisualPositions As New List(Of CyclePoint)()

		Private privateRenderer As TerrainRenderer
		Public Property Renderer() As TerrainRenderer
			Get
				Return privateRenderer
			End Get
			Private Set(ByVal value As TerrainRenderer)
				privateRenderer = value
			End Set
		End Property

		Private privateRadius As Int32
		Public Property Radius() As Int32
			Get
				Return privateRadius
			End Get
			Private Set(ByVal value As Int32)
				privateRadius = value
			End Set
		End Property

		Public Sub Databind(ByVal inRenderer As TerrainRenderer, ByVal inCenter As Point, ByVal inRadius As Int32)
			If inRenderer Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Position = CyclePoint.FromGrid(inCenter)
			Radius = inRadius
			Renderer = inRenderer

			Dim handler As New Procedure(Of Point, String)(AddressOf Me.RenderProcedure)

			Dim markerRadius() As Integer = Nothing
			Dim [step] As Double = 0
			Dim mRadius As Double = 0

			If inRadius > 4 Then
				If inRadius > 8 Then
					If inRadius > 12 Then
						markerRadius = New Integer(3){}
					Else
						markerRadius = New Integer(2){}
					End If
				Else
					markerRadius = New Integer(1){}
				End If
			Else
				markerRadius = New Integer(0){}
			End If

			[step] = inRadius / Convert.ToDouble(markerRadius.Length)
			mRadius = [step]

			For i As Integer = 0 To markerRadius.Length - 1
				Dim iLoop As Integer = i
				GridSearch.GridCircleAround(inCenter, Renderer.Size, Renderer.Size, Convert.ToInt32(CInt(Fix(mRadius))), i + 1, Sub(pos)
					handler(pos, "AreaMarker" & (markerRadius.Length - iLoop))
					mRadius += [step]
				End Sub)
			Next i

			AddHandler OnPositionChanged, AddressOf PositionChanged
		End Sub

		Private Sub RenderProcedure(ByVal pos As Point, ByVal animSet As String)
			Dim anim As RenderableCharacter = Renderer.CreateAnimation("Other", New PositionTracker() With {.Position = CyclePoint.FromGrid(pos)})
			anim.Play(animSet)
			m_Visuals.Add(anim)
			m_VisualPositions.Add(anim.Position)
			If (pos.X < 0) OrElse (pos.Y < 0) OrElse (pos.X >= Renderer.Size) OrElse (pos.Y >= Renderer.Size) Then
				anim.IsVisible = False
			End If
		End Sub

		Private Sub PositionChanged(ByVal sender As IPositionTracker, ByVal oldValue As CyclePoint, ByVal newValue As CyclePoint)
			For i As Integer = 0 To m_Visuals.Count - 1
				Dim visual As RenderableCharacter = m_Visuals(i)
				Dim newPos As CyclePoint = CyclePoint.FromGrid(m_VisualPositions(i).X + newValue.X - oldValue.X, m_VisualPositions(i).Y + newValue.Y - oldValue.Y)
				If (newPos.X < 0) OrElse (newPos.Y < 0) OrElse (newPos.X >= Renderer.Size) OrElse (newPos.Y >= Renderer.Size) Then
					visual.IsVisible = False
				Else
					visual.IsVisible = True
					visual.Position = newPos
				End If
				m_VisualPositions(i) = newPos
			Next i
		End Sub

		Public Sub Dispose()
			For Each visual As RenderableCharacter In m_Visuals
				Renderer.RemoveVisual(visual)
			Next visual

			m_Visuals.Clear()
		End Sub
	End Class
End Namespace
