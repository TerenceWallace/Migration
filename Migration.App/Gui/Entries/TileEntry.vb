Imports System.Reflection
Imports System.Xml
Imports System.Xml.Serialization

Namespace Migration

	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class TileEntry
		Private privateIndex As Integer
		<XmlAttribute()> _
		Public Property Index() As Integer
			Get
				Return privateIndex
			End Get
			Set(ByVal value As Integer)
				privateIndex = value
			End Set
		End Property

		Private privateX As Integer
		<XmlAttribute()> _
		Public Property X() As Integer
			Get
				Return privateX
			End Get
			Set(ByVal value As Integer)
				privateX = value
			End Set
		End Property

		Private privateY As Integer
		<XmlAttribute()> _
		Public Property Y() As Integer
			Get
				Return privateY
			End Get
			Set(ByVal value As Integer)
				privateY = value
			End Set
		End Property

		Private privateWidth As Integer
		<XmlAttribute()> _
		Public Property Width() As Integer
			Get
				Return privateWidth
			End Get
			Set(ByVal value As Integer)
				privateWidth = value
			End Set
		End Property

		Private privateHeight As Integer
		<XmlAttribute()> _
		Public Property Height() As Integer
			Get
				Return privateHeight
			End Get
			Set(ByVal value As Integer)
				privateHeight = value
			End Set
		End Property
	End Class
End Namespace
