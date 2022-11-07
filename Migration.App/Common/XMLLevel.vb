Imports System.Xml
Imports System.Xml.Serialization

Namespace Migration.Common

	<Serializable()> _
	Public Class XMLLevel

		Private privateId As String
		<XmlAttribute()> _
		Public Property Id() As String
			Get
				Return privateId
			End Get
			Set(ByVal value As String)
				privateId = value
			End Set
		End Property

		Private privateTextureScale As Double
		<XmlAttribute()> _
		Public Property TextureScale() As Double
			Get
				Return privateTextureScale
			End Get
			Set(ByVal value As Double)
				privateTextureScale = value
			End Set
		End Property

		Private privateMargin As Double
		<XmlIgnore()> _
		Public Property Margin() As Double
			Get
				Return privateMargin
			End Get
			Set(ByVal value As Double)
				privateMargin = value
			End Set
		End Property

		Private privateRedNoiseDivisor As Double
		<XmlAttribute()> _
		Public Property RedNoiseDivisor() As Double
			Get
				Return privateRedNoiseDivisor
			End Get
			Set(ByVal value As Double)
				privateRedNoiseDivisor = value
			End Set
		End Property

		Private privateBlueNoiseDivisor As Double
		<XmlAttribute()> _
		Public Property BlueNoiseDivisor() As Double
			Get
				Return privateBlueNoiseDivisor
			End Get
			Set(ByVal value As Double)
				privateBlueNoiseDivisor = value
			End Set
		End Property

		Private privateGreenNoiseDivisor As Double
		<XmlAttribute()> _
		Public Property GreenNoiseDivisor() As Double
			Get
				Return privateGreenNoiseDivisor
			End Get
			Set(ByVal value As Double)
				privateGreenNoiseDivisor = value
			End Set
		End Property

		Public Sub New()
			BlueNoiseDivisor = 12
			RedNoiseDivisor = 12
			GreenNoiseDivisor = 12
			TextureScale = 32
		End Sub
	End Class
End Namespace
