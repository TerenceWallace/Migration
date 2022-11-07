Imports System.Reflection
Imports System.Xml
Imports System.Xml.Serialization

Namespace Migration

	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class FrameEntry
		Private privateName As String
		<XmlAttribute("Name")> _
		Public Property Name() As String
			Get
				Return privateName
			End Get
			Set(ByVal value As String)
				privateName = value
			End Set
		End Property

		Private privateAtlasString As String
		<XmlAttribute("Atlas")> _
		Public Property AtlasString() As String
			Get
				Return privateAtlasString
			End Get
			Set(ByVal value As String)
				privateAtlasString = value
			End Set
		End Property

		Private privateImageString As String
		<XmlAttribute("Image")> _
		Public Property ImageString() As String
			Get
				Return privateImageString
			End Get
			Set(ByVal value As String)
				privateImageString = value
			End Set
		End Property

		Private privateImageID As Integer
		<XmlIgnore()> _
		Public Property ImageID() As Integer
			Get
				Return privateImageID
			End Get
			Private Set(ByVal value As Integer)
				privateImageID = value
			End Set
		End Property

		Private privateAtlas As AtlasEntry
		<XmlIgnore()> _
		Public Property Atlas() As AtlasEntry
			Get
				Return privateAtlas
			End Get
			Private Set(ByVal value As AtlasEntry)
				privateAtlas = value
			End Set
		End Property

		Public Sub Process(ByVal config As XMLGUIConfig)
			Dim imageEntry As ImageEntry = config.GetImage(ImageString)

			ImageID = imageEntry.ImageID

			If String.IsNullOrEmpty(Name) Then
				Throw New ArgumentException("No name given for object.")
			End If

			If String.IsNullOrEmpty(AtlasString) Then
				Throw New ArgumentException("No atlas given for object.")
			End If

			If config.Context.ContainsKey(Name) Then
				Throw New ArgumentException("There is already an object with name """ & Name & """ registered in GUI config.")
			End If

			config.Context.Add(Name, Me)

			Atlas = CType(config.Context(AtlasString), AtlasEntry)

			' check range
			Dim width As Integer = imageEntry.Image.Width
			Dim height As Integer = imageEntry.Image.Height

			For Each tile As TileEntry In Atlas.Tiles
				If (tile.X < 0) OrElse (tile.Y < 0) OrElse (tile.X >= width - 1) OrElse (tile.Y >= height - 1) OrElse (tile.Width < 0) OrElse (tile.Height < 0) OrElse (tile.X + tile.Width > width) OrElse (tile.Y + tile.Height > height) Then
					Throw New ArgumentOutOfRangeException("Frame """ & Name & """ does not match the given atlas tiles.")
				End If
			Next tile
		End Sub
	End Class
End Namespace
