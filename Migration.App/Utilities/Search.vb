Imports Migration.Common
Imports Migration.Core

Namespace Migration

	Public NotInheritable Class GridSearch
		''' <summary>
		''' Invokes the handler with values, walking along the whole grid in a rectangular
		''' spiral around start position. What exactly is the "grid" is within your power.
		''' </summary>
		Private Sub New()
		End Sub

		Public Shared Function GridWalkAround(ByVal inStartPos As Point, ByVal inWidth As Int32, ByVal inHeight As Int32, ByVal inHandler As Func(Of Point, WalkResult)) As WalkResult
			Dim hasField As Boolean = False
			Dim center As Point = inStartPos
			Dim radius As Int32 = 1

			If inHandler(New Point(center.X, center.Y)) = WalkResult.Success Then
				Return WalkResult.Success
			End If

			Do
				hasField = False

				Dim res As WalkResult = 0
				Dim start As New Point(center.X + radius, center.Y - radius)

				' walk down
				If (center.X + radius >= 0) AndAlso (center.X + radius < inWidth) Then
					For y As Integer = center.Y - radius To center.Y + radius
						If y < 0 Then
							Continue For
						End If
						If y >= inHeight Then
							Exit For
						End If

						res = inHandler(New Point(center.X + radius, y))
						If res = WalkResult.Success Then
							Return WalkResult.Success
						End If

						If res = WalkResult.Abort Then
							Return WalkResult.NotFound
						End If

						hasField = True
					Next y
				End If

				' walk left
				If (center.Y + radius >= 0) AndAlso (center.Y + radius < inHeight) Then
					For x As Integer = center.X + radius - 1 To center.X - radius Step -1
						If x < 0 Then
							Exit For
						End If
						If x >= inWidth Then
							Continue For
						End If

						res = inHandler(New Point(x, center.Y + radius))
						If res = WalkResult.Success Then
							Return WalkResult.Success
						End If

						If res = WalkResult.Abort Then
							Return WalkResult.NotFound
						End If

						hasField = True
					Next x
				End If

				' walk up
				If (center.X - radius >= 0) AndAlso (center.X - radius < inWidth) Then
					For y As Integer = center.Y + radius - 1 To center.Y - radius Step -1
						If y < 0 Then
							Exit For
						End If
						If y >= inHeight Then
							Continue For
						End If

						res = inHandler(New Point(center.X - radius, y))
						If res = WalkResult.Success Then
							Return WalkResult.Success
						End If

						If res = WalkResult.Abort Then
							Return WalkResult.NotFound
						End If

						hasField = True
					Next y
				End If

				' walk right
				If (center.Y - radius >= 0) AndAlso (center.Y - radius < inHeight) Then
					For x As Integer = center.X - radius + 1 To center.X + radius - 1
						If x < 0 Then
							Continue For
						End If
						If x >= inWidth Then
							Exit For
						End If

						res = inHandler(New Point(x, center.Y - radius))
						If res = WalkResult.Success Then
							Return WalkResult.Success
						End If

						If res = WalkResult.Abort Then
							Return WalkResult.NotFound
						End If

						hasField = True
					Next x
				End If

				radius += 1
				Loop While hasField

			Return WalkResult.NotFound ' was not aborted by handler
		End Function

		''' <summary>
		''' Circles around the given center while maintaining a constant distance of <paramref name="inRadius"/>
		''' to the center and a distance of <paramref name="inStepWidth"/> to the previous call of the handler,
		''' if possible. In contrast to WalkAround, this method does NOT cover an area but only a
		''' closed line (approx. a circle). 
		''' </summary>
		''' <param name="inCenter"></param>
		''' <param name="inRadius"></param>
		''' <param name="inStepWidth"></param>
		''' <param name="inHandler"></param>
		Public Shared Sub GridCircleAround(ByVal inCenter As Point, ByVal inGridWidth As Integer, ByVal inGridHeight As Integer, ByVal inRadius As Integer, ByVal inStepWidth As Integer, ByVal inHandler As Procedure(Of Point))
			Dim bounds As New Rectangle(inCenter.X - inRadius, inCenter.Y - inRadius, inRadius * 2, inRadius * 2)
			Dim prevPoint As New Point(Int32.MinValue, Int32.MinValue)

			For y As Integer = bounds.Top To bounds.Bottom - 1

				CircleStep(New Point(bounds.Left, y), prevPoint, inCenter, inGridWidth, inGridHeight, inRadius, inStepWidth, Sub(pos)
					prevPoint = pos
					inHandler(pos)
				End Sub)
			Next y

			For x As Integer = bounds.Left To bounds.Right - 1

				CircleStep(New Point(x, bounds.Bottom), prevPoint, inCenter, inGridWidth, inGridHeight, inRadius, inStepWidth, Sub(pos)
					prevPoint = pos
					inHandler(pos)
				End Sub)
			Next x

			For y As Integer = bounds.Bottom - 1 To bounds.Top Step -1

				CircleStep(New Point(bounds.Right, y), prevPoint, inCenter, inGridWidth, inGridHeight, inRadius, inStepWidth, Sub(pos)
					prevPoint = pos
					inHandler(pos)
				End Sub)
			Next y

			For x As Integer = bounds.Right - 1 To bounds.Left Step -1
				CircleStep(New Point(x, bounds.Top), prevPoint, inCenter, inGridWidth, inGridHeight, inRadius, inStepWidth, Sub(pos)
					prevPoint = pos
					inHandler(pos)
				End Sub)
			Next x
		End Sub

		Private Shared Function CircleStep(ByVal inPoint As Point, ByVal inPrevPoint As Point, ByVal inCenter As Point, ByVal inGridWidth As Integer, ByVal inGridHeight As Integer, ByVal inRadius As Integer, ByVal inStepWidth As Integer, ByVal inHandler As Procedure(Of Point)) As Boolean
			' walk from point to center until distance is smaller or equal to radius
			Dim xStep As Double = inCenter.X - inPoint.X
			Dim yStep As Double = inCenter.Y - inPoint.Y
			Dim norm As Double = Math.Sqrt(xStep * xStep + yStep * yStep)
			Dim xFlip As Boolean = inCenter.X > inPoint.X
			Dim yFlip As Boolean = inCenter.Y > inPoint.Y
			Dim last As Point = inPoint
			Dim xOnce As Boolean = inCenter.X - inPoint.X = 0
			Dim yOnce As Boolean = inCenter.Y - inPoint.Y = 0

			xStep /= norm
			yStep /= norm


			Dim x As Double = inPoint.X
			Do While (x < inCenter.X) = xFlip
				Dim y As Double = inPoint.Y
				Do While (y < inCenter.Y) = yFlip
					Dim current As New Point(Convert.ToInt32(CInt(Fix(x))), Convert.ToInt32(CInt(Fix(y))))

					If (current = last) OrElse (current.DistanceTo(inCenter) > inRadius) Then
						y += yStep
						yFlip = If(yOnce, ((Not yFlip)), yFlip)
						Continue Do
					End If

					If current.DistanceTo(inPrevPoint) < inStepWidth Then
						Return False
					End If

					inHandler(current)

					Return True
					y += yStep
					yFlip = If(yOnce, ((Not yFlip)), yFlip)
				Loop
				x += xStep
				xFlip = If(xOnce, ((Not xFlip)), xFlip)
			Loop

			Return False
		End Function
	End Class
End Namespace
