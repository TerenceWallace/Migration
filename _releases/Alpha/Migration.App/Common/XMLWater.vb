Imports System.Xml
Imports System.Xml.Serialization

Namespace Migration.Common
	<Serializable()> _
	Public Class XMLWater

		Private privateHeight As Double
		<XmlAttribute()> _
		Public Property Height() As Double
			Get
				Return privateHeight
			End Get
			Set(ByVal value As Double)
				privateHeight = value
			End Set
		End Property

		Private privateSpeed As Double
		<XmlAttribute()> _
		Public Property Speed() As Double
			Get
				Return privateSpeed
			End Get
			Set(ByVal value As Double)
				privateSpeed = value
			End Set
		End Property

		Private privateAmplitude As Double
		<XmlAttribute()> _
		Public Property Amplitude() As Double
			Get
				Return privateAmplitude
			End Get
			Set(ByVal value As Double)
				privateAmplitude = value
			End Set
		End Property
	End Class
End Namespace