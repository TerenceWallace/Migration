Imports System.Xml
Imports System.Xml.Serialization
Imports Migration.Common

Namespace Migration.Configuration
	<XmlRoot("TerrainConfiguration"), Serializable()> _
	Public Class TerrainConfiguration

		Private privateHeightScale As Double
		<XmlAttribute()> _
		Public Property HeightScale() As Double
			Get
				Return privateHeightScale
			End Get
			Set(ByVal value As Double)
				privateHeightScale = value
			End Set
		End Property

		Private privateNormalZScale As Double
		<XmlAttribute()> _
		Public Property NormalZScale() As Double
			Get
				Return privateNormalZScale
			End Get
			Set(ByVal value As Double)
				privateNormalZScale = value
			End Set
		End Property

		Private privateRedNoiseFrequency As Double
		<XmlAttribute()> _
		Public Property RedNoiseFrequency() As Double
			Get
				Return privateRedNoiseFrequency
			End Get
			Set(ByVal value As Double)
				privateRedNoiseFrequency = value
			End Set
		End Property

		Private privateBlueNoiseFrequency As Double
		<XmlAttribute()> _
		Public Property BlueNoiseFrequency() As Double
			Get
				Return privateBlueNoiseFrequency
			End Get
			Set(ByVal value As Double)
				privateBlueNoiseFrequency = value
			End Set
		End Property

		Private privateGreenNoiseFrequency As Double
		<XmlAttribute()> _
		Public Property GreenNoiseFrequency() As Double
			Get
				Return privateGreenNoiseFrequency
			End Get
			Set(ByVal value As Double)
				privateGreenNoiseFrequency = value
			End Set
		End Property

		Private privateWater As XMLWater
		<XmlElement()> _
		Public Property Water() As XMLWater
			Get
				Return privateWater
			End Get
			Set(ByVal value As XMLWater)
				privateWater = value
			End Set
		End Property

		Private privateLevels As List(Of XMLLevel)
		<XmlElement("LevelList"), XmlArrayItem("Level")> _
		Public Property Levels() As List(Of XMLLevel)
			Get
				Return privateLevels
			End Get
			Set(ByVal value As List(Of XMLLevel))
				privateLevels = value
			End Set
		End Property

		Public Sub New()
			RedNoiseFrequency = 20
			GreenNoiseFrequency = 20
			BlueNoiseFrequency = 12
			HeightScale = 8
			NormalZScale = 3

			Water = New XMLWater() With {.Speed = 500, .Height = -0.3, .Amplitude = 0.5}

			Levels = New List(Of XMLLevel)()
			Levels.Add(New XMLLevel() With {.Id = "Level_00", .Margin = -1.0})
			Levels.Add(New XMLLevel() With {.Id = "Level_01", .Margin = -0.66})
			Levels.Add(New XMLLevel() With {.Id = "Level_02", .TextureScale = 64, .Margin = -0.33})
			Levels.Add(New XMLLevel() With {.Id = "Level_03", .Margin = 0})
			Levels.Add(New XMLLevel() With {.Id = "Level_04", .Margin = 0.33})
			Levels.Add(New XMLLevel() With {.Id = "Level_05", .Margin = 0.66})
		End Sub
	End Class
End Namespace
