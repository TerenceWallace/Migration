Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Xml
Imports System.Xml.Serialization

Namespace Migration
	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class Template

		Private Shared m_Serializer As New XmlSerializer(GetType(List(Of Control)))

		Private m_Children() As Control
		Private m_XMLString As String
		Private m_Layout As XMLGUILayout

		Private privateName As String
		<XmlAttribute> _
		Public Property Name() As String
			Get
				Return privateName
			End Get
			Set(ByVal value As String)
				privateName = value
			End Set
		End Property

		<XmlElement(GetType(Control)), XmlElement(GetType(Image)), XmlElement(GetType(Frame)), XmlElement(GetType(Button)), XmlElement(GetType(ListBox)), XmlElement(GetType(TabButton)), XmlElement(GetType(RadioButton)), XmlElement(GetType(Label))> _
		Public XMLChildren As List(Of Control)

		Public Overridable Sub XMLPostProcess(ByVal inLayout As XMLGUILayout)
			m_Layout = inLayout

			If XMLChildren IsNot Nothing Then
				Dim stream As New MemoryStream()

				SyncLock m_Serializer
					m_Serializer.Serialize(stream, XMLChildren)

					m_XMLString = Encoding.UTF8.GetString(stream.ToArray())
				End SyncLock

				m_Children = New Control(XMLChildren.Count - 1){}

				For i As Integer = 0 To m_Children.Length - 1
					Dim child As Control = CType(XMLChildren(i), Control)

					child.XMLPostProcess(inLayout)

					m_Children(i) = child
				Next i

				XMLChildren = Nothing
			End If
		End Sub

		Public Sub Instantiate(<System.Runtime.InteropServices.Out()> ByRef outChilds As List(Of Control), <System.Runtime.InteropServices.Out()> ByRef outPresenters As List(Of Control), ParamArray ByVal inParams() As Object)
			' parameterize template XML
			Dim paramXML As String = m_XMLString

			For i As Integer = 0 To inParams.Length - 1
				paramXML = paramXML.Replace("{" & i & "}", inParams(i).ToString())
			Next i

			SyncLock m_Serializer
				Dim stream As New MemoryStream(Encoding.UTF8.GetBytes(paramXML))
				outChilds = CType(m_Serializer.Deserialize(stream), List(Of Control))
			End SyncLock

			For Each child As Control In outChilds
				child.XMLPostProcess(m_Layout)
			Next child

			' collect presenters
			outPresenters = New List(Of Control)()

			CollectPresenters(outChilds, outPresenters)
		End Sub

		Private Sub CollectPresenters(ByVal inChilds As IEnumerable(Of Control), ByVal outPresenters As List(Of Control))
			For Each child As Control In inChilds
				If TypeOf child Is Content Then
					outPresenters.Add(TryCast(child, Content))
				Else
					CollectPresenters(child.Children, outPresenters)
				End If
			Next child
		End Sub
	End Class
End Namespace
