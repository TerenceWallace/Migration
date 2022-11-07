Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization

Namespace Migration
	<XmlRoot("GUITemplates")> _
	Public Class XMLGUITemplates
		Private privateEntries As List(Of Template)
		<XmlElement("Template")> _
		Public Property Entries() As List(Of Template)
			Get
				Return privateEntries
			End Get
			Set(ByVal value As List(Of Template))
				privateEntries = value
			End Set
		End Property
		Private privateLastWriteTime As Date
		<XmlIgnore> _
		Public Property LastWriteTime() As Date
			Get
				Return privateLastWriteTime
			End Get
			Private Set(ByVal value As Date)
				privateLastWriteTime = value
			End Set
		End Property
		Private privateConfigDirectory As String
		<XmlIgnore> _
		Public Property ConfigDirectory() As String
			Get
				Return privateConfigDirectory
			End Get
			Private Set(ByVal value As String)
				privateConfigDirectory = value
			End Set
		End Property

		Public Shared Function Load(ByVal inXMLContent As String, ByVal inFileInfo As FileInfo) As XMLGUITemplates
			' load config XML
			Dim format As New XmlSerializer(GetType(XMLGUITemplates))
			Dim templateXML As XMLGUITemplates = CType(format.Deserialize(New StringReader(inXMLContent)), XMLGUITemplates)

			templateXML.ConfigDirectory = Path.GetDirectoryName(inFileInfo.FullName) & "/"
			templateXML.LastWriteTime = inFileInfo.LastWriteTime

			Return templateXML
		End Function
	End Class
End Namespace
