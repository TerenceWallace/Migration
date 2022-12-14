Imports Migration.Core

Namespace Migration
	Friend Class KeyComparer
		Implements IComparer(Of PathNodeKey)

		Public Function Compare(ByVal a As PathNodeKey, ByVal b As PathNodeKey) As Integer Implements IComparer(Of PathNodeKey).Compare
			If a.X < b.X Then
				Return -1
			ElseIf a.X > b.X Then
				Return 1
			End If

			If a.Y < b.Y Then
				Return -1
			ElseIf a.Y > b.Y Then
				Return 1
			End If

			' must be the last comparison
			If a.Time < b.Time Then
				Return -1
			ElseIf a.Time > b.Time Then
				Return 1
			End If

			Return 0
		End Function
	End Class
End Namespace
