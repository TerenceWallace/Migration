Imports Migration.Common
Imports Migration.Core

Namespace Migration


	Friend NotInheritable Class DirectionUtils
		Friend Const DirCount As Int32 = 8

		Private Sub New()
		End Sub
		Friend Shared Function GetWalkingDirection(ByVal inFrom As Point, ByVal inTo As Point) As Direction?
			Return GetWalkingDirection(New Point(inTo.X - inFrom.X, inTo.Y - inFrom.Y))
		End Function

		Friend Shared Function GetNearestWalkingDirection(ByVal inNormedDiff As Point) As Direction?
			If inNormedDiff.X = 0 Then
				If inNormedDiff.Y = -1 Then
					Return Direction._045
				ElseIf inNormedDiff.Y = 1 Then
					Return Direction._225
				Else
					Return Nothing
				End If
			ElseIf inNormedDiff.X = 1 Then
				If inNormedDiff.Y = -1 Then
					Return Direction._045
				ElseIf inNormedDiff.Y = 0 Then
					Return Direction._090
				ElseIf inNormedDiff.Y = 1 Then
					Return Direction._135
				Else
					Return Nothing
				End If
			ElseIf inNormedDiff.X = -1 Then
				If inNormedDiff.Y = -1 Then
					Return Direction._315
				ElseIf inNormedDiff.Y = 0 Then
					Return Direction._270
				ElseIf inNormedDiff.Y = 1 Then
					Return Direction._225
				Else
					Return Nothing
				End If
			Else
				Return Nothing
			End If
		End Function

		Friend Shared Function GetWalkingDirection(ByVal inNormedDiff As Point) As Direction?
			If inNormedDiff.X = 0 Then
				'if (diffY == -1) return Direction._000;
				'else if (diffY == 1) return Direction._180;
				'else 
				Return Nothing
			ElseIf inNormedDiff.X = 1 Then
				If inNormedDiff.Y = -1 Then
					Return Direction._045
				ElseIf inNormedDiff.Y = 0 Then
					Return Direction._090
				ElseIf inNormedDiff.Y = 1 Then
					Return Direction._135
				Else
					Return Nothing
				End If
			ElseIf inNormedDiff.X = -1 Then
				If inNormedDiff.Y = -1 Then
					Return Direction._315
				ElseIf inNormedDiff.Y = 0 Then
					Return Direction._270
				ElseIf inNormedDiff.Y = 1 Then
					Return Direction._225
				Else
					Return Nothing
				End If
			Else
				Return Nothing
			End If
		End Function

		Friend Shared Function GetDirectionVector(ByVal inDirection As Direction) As Point
			Dim result As New Point()

			Select Case inDirection
				'case Direction._000: result = new Point(0, -1); break;
				Case Direction._045
					result = New Point(1, -1)
				Case Direction._090
					result = New Point(1, 0)
				Case Direction._135
					result = New Point(1, 1)
					'case Direction._180: result = new Point(0, 1); break;
				Case Direction._225
					result = New Point(-1, 1)
				Case Direction._270
					result = New Point(-1, 0)
				Case Direction._315
					result = New Point(-1, -1)
				Case Else
					Throw New ArgumentException()
			End Select

			Return result
		End Function
	End Class
End Namespace
