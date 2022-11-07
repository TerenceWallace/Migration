Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization
Imports Migration.Common

Namespace Migration
	<Serializable(), XmlRoot("GUILayout")> _
	Public Class XMLGUILayout

		Private privateTemplatesPath As String
		<XmlAttribute()> _
		Public Property TemplatesPath() As String
			Get
				Return privateTemplatesPath
			End Get
			Set(ByVal value As String)
				privateTemplatesPath = value
			End Set
		End Property

		Private privateRootElement As RootControl
		Public Property RootElement() As RootControl
			Get
				Return privateRootElement
			End Get
			Set(ByVal value As RootControl)
				privateRootElement = value
			End Set
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

		<XmlIgnore()> _
		Private m_Templates As SortedDictionary(Of String, Template)

		Public Sub New()
			m_Templates = New SortedDictionary(Of String, Template)()
		End Sub

		Public Shared Function Load(ByVal inXMLSource As String, ByVal inFileInfo As FileInfo) As XMLGUILayout
			' load config XML
			Dim format As New XmlSerializer(GetType(XMLGUILayout))
			Dim layoutXML As XMLGUILayout = CType(format.Deserialize(New StringReader(inXMLSource)), XMLGUILayout)

			layoutXML.ConfigDirectory = Path.GetDirectoryName(inFileInfo.FullName) & "/"
			layoutXML.LastWriteTime = inFileInfo.LastWriteTime

			If Not(String.IsNullOrEmpty(layoutXML.TemplatesPath)) Then
				' load templates
				Dim fullPath As String = [Global].GetResourcePath("GUI/Default/" & layoutXML.TemplatesPath)
				Dim xmlContent As String = File.ReadAllText(fullPath)

				xmlContent = xmlContent.Replace("{Race}", Game.Setup.Map.Race.Name)

				Dim templateXML As XMLGUITemplates = XMLGUITemplates.Load(xmlContent, New FileInfo(fullPath))

				For Each template As Template In templateXML.Entries
					If String.IsNullOrEmpty(template.Name) Then
						Throw New ArgumentException("A template name can not be null or empty.")
					End If

					template.XMLPostProcess(layoutXML)

					layoutXML.m_Templates.Add(template.Name, template)
				Next template
			End If

			layoutXML.RootElement.XMLPostProcess(layoutXML)

			Return layoutXML
		End Function

		Public Function GetTemplate(ByVal inName As String) As Template
			If Not(m_Templates.ContainsKey(inName)) Then
				Throw New KeyNotFoundException("A template named """ & inName & """ was not found.")
			End If

			Return m_Templates(inName)
		End Function
	End Class
End Namespace
