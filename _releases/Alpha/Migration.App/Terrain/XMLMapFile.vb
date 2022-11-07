Imports System.Xml.Serialization

Namespace Migration
	<XmlRoot("Map")> _
	Public Class XMLMapFile
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

		Private privateImageFile As XMLImageFile
		Public Property ImageFile() As XMLImageFile
			Get
				Return privateImageFile
			End Get
			Set(ByVal value As XMLImageFile)
				privateImageFile = value
			End Set
		End Property
	End Class

End Namespace
