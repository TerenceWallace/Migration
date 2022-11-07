Imports Migration.Common

Namespace Migration
	Friend Structure TaskEntry
		Public Handler As Procedure(Of RectangleDouble)
		Public TexCoords As RectangleDouble
	End Structure
End Namespace
