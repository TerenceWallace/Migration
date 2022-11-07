Imports Migration.Core

#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else

#End If

Namespace Migration.Rendering
	<Serializable()> _
	Friend Class TextureAtlasEntry
		Public TexRect As RectangleDouble
		Public PixRect As Rectangle
		Public Checksum As Long
		Public Atlas As TextureAtlas
		<NonSerialized()> _
		Public Image As Bitmap
		Public AreaSize As Integer
	End Class
End Namespace
