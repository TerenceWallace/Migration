Imports System.IO
Imports System.Reflection
Imports System.Xml
Imports System.Xml.Serialization
Imports Migration.Rendering

Namespace Migration.Configuration
	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True), XmlRoot("Migration")> _
	Public Class MarkupLanguage

		Private privateConfigPath As String
		<XmlIgnore()> _
		Public Property ConfigPath() As String
			Get
				Return privateConfigPath
			End Get
			Private Set(ByVal value As String)
				privateConfigPath = value
			End Set
		End Property

		Private privateOriginalResourcePath As String
		<XmlElement("ResourcePath")> _
		Public Property OriginalResourcePath() As String
			Get
				Return privateOriginalResourcePath
			End Get
			Set(ByVal value As String)
				privateOriginalResourcePath = value
			End Set
		End Property

		Private privateResourcePath As String
		<XmlIgnore()> _
		Public Property ResourcePath() As String
			Get
				Return privateResourcePath
			End Get
			Set(ByVal value As String)
				privateResourcePath = value
			End Set
		End Property

		Private privateS3CheckOverride As String
		Public Property S3CheckOverride() As String
			Get
				Return privateS3CheckOverride
			End Get
			Set(ByVal value As String)
				privateS3CheckOverride = value
			End Set
		End Property

		Private privateS3InstallPath As String
		Public Property S3InstallPath() As String
			Get
				Return privateS3InstallPath
			End Get
			Set(ByVal value As String)
				privateS3InstallPath = value
			End Set
		End Property

		Private privateConfiguration As RenderConfiguration
		Public Property Configuration() As RenderConfiguration
			Get
				Return privateConfiguration
			End Get
			Set(ByVal value As RenderConfiguration)
				privateConfiguration = value
			End Set
		End Property

		Private privateUseMinimalBounds As Boolean
		Public Property UseMinimalBounds() As Boolean
			Get
				Return privateUseMinimalBounds
			End Get
			Set(ByVal value As Boolean)
				privateUseMinimalBounds = value
			End Set
		End Property

		Public Shared Function Load(ByVal inFileName As String) As MarkupLanguage
			' load config XML
			Dim format As New XmlSerializer(GetType(MarkupLanguage))
			Dim stream As Stream = File.OpenRead(inFileName)
			Dim configXML As MarkupLanguage = Nothing

			Using stream
				configXML = CType(format.Deserialize(stream), MarkupLanguage)

				configXML.ConfigPath = (New FileInfo(inFileName)).FullName
				configXML.ResourcePath = configXML.OriginalResourcePath
			End Using

			' validate config
			configXML.ProcessResourcePath()
			configXML.ProcessGLRenderer()

			Return configXML
		End Function

		Public Shared Sub Save(ByVal inConfig As MarkupLanguage, ByVal inFileName As String)
			' load config XML
			Dim format As New XmlSerializer(GetType(MarkupLanguage))
			Dim stream As Stream = File.OpenWrite(inFileName)

			Using stream
				stream.SetLength(0)
				format.Serialize(stream, inConfig)
			End Using
		End Sub

		Private Sub ProcessResourcePath()
			Dim backup As String = ResourcePath

			If String.IsNullOrEmpty(ResourcePath) Then
				ResourcePath = "Resources\"
			End If

			If Not(Path.IsPathRooted(ResourcePath)) Then
				' paths are threaded relative to config file directory...
				ResourcePath = Path.GetFullPath(Path.GetDirectoryName(ConfigPath) & "\" & ResourcePath)
			End If

			If Not(Directory.Exists(ResourcePath)) Then
				Throw New DirectoryNotFoundException("Resource directory """ & ResourcePath & """ could not be found. (Original string was """ & backup & """ relative to its config file """ & ConfigPath & """).")
			End If
		End Sub

		Private Sub ProcessGLRenderer()
			If Configuration Is Nothing Then
				Configuration = New RenderConfiguration()
			End If
		End Sub
	End Class
End Namespace
