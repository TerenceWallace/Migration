Imports Migration.Configuration

Namespace Migration.Editor
	Public Class TerrainDefinitionEditor
		Inherits TerrainDefinition

		Public Sub New()
			MyBase.New(64, New TerrainConfiguration())
		End Sub

		Public Overrides Function GetZShiftAt(ByVal inXCell As Integer, ByVal inYCell As Integer) As Single
			Return 0F
		End Function

		Public Overrides Function CanBuildingBePlacedAt(ByVal inXCell As Integer, ByVal inYCell As Integer, ByVal inConfig As BuildingConfiguration) As Boolean
			Dim editor As RuntimeBuildingEditor = CType(BuildingConfig, RuntimeBuildingConfig).Building

			For Each pos In editor.GroundPlane
				If GetBuildingExpenses(pos.X + inXCell, pos.Y + inYCell) > 2 Then
					Return False
				End If
			Next pos

			For Each pos In editor.ReservedPlane
				If GetBuildingExpenses(pos.X + inXCell, pos.Y + inYCell) > 2 Then
					Return False
				End If
			Next pos

			Return True
		End Function

		Public Overrides Function GetBuildingExpenses(ByVal inXCell As Integer, ByVal inYCell As Integer) As Byte
			Dim editor As RuntimeBuildingEditor = CType(BuildingConfig, RuntimeBuildingConfig).Building

			If editor.ShowBuildings Then
				Dim pos As New Point(inXCell - editor.Position.X, inYCell - editor.Position.Y)

				If editor.GroundPlane.Contains(pos) Then
					Return 5
				ElseIf editor.ReservedPlane.Contains(pos) Then
					Return 3
				Else
					Return 2
				End If
			ElseIf editor.ShowSettlers Then
				Dim pos As New Point(inXCell - editor.Position.X, inYCell - editor.Position.Y)

				If editor.GroundPlane.Contains(pos) Then
					Return 3
				Else
					Return 2
				End If
			Else
				Return 1
			End If
		End Function
	End Class
End Namespace
