Imports System.Reflection
Imports System.Xml
Imports System.Xml.Serialization

Namespace Migration

	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class AtlasEntry
		Private privateName As String
		<XmlAttribute()> _
		Public Property Name() As String
			Get
				Return privateName
			End Get
			Set(ByVal value As String)
				privateName = value
			End Set
		End Property

		Private privateTiles() As TileEntry
		<XmlArray("TileList"), XmlArrayItem("Tile")> _
		Public Property Tiles() As TileEntry()
			Get
				Return privateTiles
			End Get
			Set(ByVal value As TileEntry())
				privateTiles = value
			End Set
		End Property

		Public Sub Process(ByVal config As XMLGUIConfig)
			If String.IsNullOrEmpty(Name) Then
				Throw New ArgumentException("No name given for object.")
			End If

			If config.Context.ContainsKey(Name) Then
				Throw New ArgumentException("There is already an object with name """ & Name & """ registered in GUI config.")
			End If

			config.Context.Add(Name, Me)
		End Sub
	End Class
End Namespace
