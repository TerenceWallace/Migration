Imports System.Runtime.InteropServices
Imports System.Xml.Serialization

Namespace Migration
	<StructLayout(LayoutKind.Sequential)> _
	Public Structure XMLColorDefinition
		<XmlAttribute()> _
		Public R As Byte
		<XmlAttribute()> _
		Public G As Byte
		<XmlAttribute()> _
		Public B As Byte

		Public ReadOnly Property ColorARGB() As Integer
			Get
				Return Color.FromArgb(R, G, B).ToArgb()
			End Get
		End Property
	End Structure
End Namespace