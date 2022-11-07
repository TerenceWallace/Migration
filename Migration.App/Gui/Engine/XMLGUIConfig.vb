Imports System.IO
Imports System.Xml.Serialization
Imports Migration.Rendering

Namespace Migration

	<XmlRoot("GUIConfig")> _
	Public Class XMLGUIConfig

		Private ReadOnly m_Context As New SortedDictionary(Of String, Object)()
		Friend ReadOnly Property Context() As SortedDictionary(Of String, Object)
			Get
				Return m_Context
			End Get
		End Property

		Private privateLastWriteTime As Date
		<XmlIgnore()> _
		Public Property LastWriteTime() As Date
			Get
				Return privateLastWriteTime
			End Get
			Private Set(ByVal value As Date)
				privateLastWriteTime = value
			End Set
		End Property

		Private privateConfigDirectory As String
		<XmlIgnore()> _
		Public Property ConfigDirectory() As String
			Get
				Return privateConfigDirectory
			End Get
			Private Set(ByVal value As String)
				privateConfigDirectory = value
			End Set
		End Property

		Private privateRenderer As Renderer
		<XmlIgnore()> _
		Public Property Renderer() As Renderer
			Get
				Return privateRenderer
			End Get
			Private Set(ByVal value As Renderer)
				privateRenderer = value
			End Set
		End Property

		Private privateAtlasList As List(Of AtlasEntry)
		<XmlArray("AtlasList"), XmlArrayItem("Atlas")> _
		Public Property AtlasList() As List(Of AtlasEntry)
			Get
				Return privateAtlasList
			End Get
			Set(ByVal value As List(Of AtlasEntry))
				privateAtlasList = value
			End Set
		End Property

		Private privateFrameList As List(Of FrameEntry)
		<XmlArray("FrameList"), XmlArrayItem("Frame")> _
		Public Property FrameList() As List(Of FrameEntry)
			Get
				Return privateFrameList
			End Get
			Set(ByVal value As List(Of FrameEntry))
				privateFrameList = value
			End Set
		End Property

		Public Shared Function Load(ByVal inRenderer As Renderer, ByVal inFileName As String) As XMLGUIConfig
			If inRenderer Is Nothing Then
				Throw New ArgumentNullException()
			End If

			' load config XML
			Dim format As New XmlSerializer(GetType(XMLGUIConfig))
			Dim stream As Stream = File.OpenRead(inFileName)
			Dim configXML As XMLGUIConfig = Nothing

			Using stream
				configXML = CType(format.Deserialize(stream), XMLGUIConfig)

				Dim fileInfo As New FileInfo(inFileName)
				configXML.ConfigDirectory = Path.GetDirectoryName(fileInfo.FullName) & "/"
				configXML.LastWriteTime = fileInfo.LastWriteTime

			End Using

			configXML.Process(inRenderer)

			Return configXML
		End Function

		Private Sub Process(ByVal inRenderer As Renderer)
			m_Context.Clear()

			Renderer = inRenderer

			' postprocess 
			For Each atlas As AtlasEntry In AtlasList
				atlas.Process(Me)
			Next atlas

			For Each frame As FrameEntry In FrameList
				frame.Process(Me)
			Next frame
		End Sub

		Public Function Exists(ByVal inResource As String) As Boolean
			Return File.Exists(ConfigDirectory & inResource)
		End Function

		Public Function GetFrame(ByVal inName As String) As FrameEntry
			If Not(m_Context.ContainsKey(inName)) Then
				Throw New KeyNotFoundException("A frame named """ & inName & """ does not exist.")
			End If

			Return CType(m_Context(inName), FrameEntry)
		End Function

		Public Function RegisterImage(ByVal inBitmap As System.Drawing.Bitmap) As Integer
			Return (New ImageEntry(Me, inBitmap)).ImageID
		End Function

		Public Function GetImage(ByVal inResourceName As String) As ImageEntry
			If Not(m_Context.ContainsKey(inResourceName)) Then
				If Not(Exists(inResourceName)) Then
					Throw New KeyNotFoundException("An image named """ & inResourceName & """ does not exist.")
				End If

				m_Context.Add(inResourceName, New ImageEntry(Me, inResourceName))
			End If

			Return CType(m_Context(inResourceName), ImageEntry)
		End Function

	End Class
End Namespace
