Imports System.Xml.Serialization
Imports Migration.Core

Namespace Migration

	Public Class XMLImageFile

		Private privateSource As String
		<XmlAttribute()> _
		Public Property Source() As String
			Get
				Return privateSource
			End Get
			Set(ByVal value As String)
				privateSource = value
			End Set
		End Property

		Private privateGrass As XMLColorDefinition
		Public Property Grass() As XMLColorDefinition
			Get
				Return privateGrass
			End Get
			Set(ByVal value As XMLColorDefinition)
				privateGrass = value
			End Set
		End Property
		Private privateWater As XMLColorDefinition

		Public Property Water() As XMLColorDefinition
			Get
				Return privateWater
			End Get
			Set(ByVal value As XMLColorDefinition)
				privateWater = value
			End Set
		End Property

		Private privateRock As XMLColorDefinition
		Public Property Rock() As XMLColorDefinition
			Get
				Return privateRock
			End Get
			Set(ByVal value As XMLColorDefinition)
				privateRock = value
			End Set
		End Property

		Private privateDesert As XMLColorDefinition
		Public Property Desert() As XMLColorDefinition
			Get
				Return privateDesert
			End Get
			Set(ByVal value As XMLColorDefinition)
				privateDesert = value
			End Set
		End Property

		Private privateSwamp As XMLColorDefinition
		Public Property Swamp() As XMLColorDefinition
			Get
				Return privateSwamp
			End Get
			Set(ByVal value As XMLColorDefinition)
				privateSwamp = value
			End Set
		End Property

		Private privateMud As XMLColorDefinition
		Public Property Mud() As XMLColorDefinition
			Get
				Return privateMud
			End Get
			Set(ByVal value As XMLColorDefinition)
				privateMud = value
			End Set
		End Property

		Private privateSand As XMLColorDefinition
		Public Property Sand() As XMLColorDefinition
			Get
				Return privateSand
			End Get
			Set(ByVal value As XMLColorDefinition)
				privateSand = value
			End Set
		End Property

		Private privateStone As XMLColorDefinition
		Public Property Stone() As XMLColorDefinition
			Get
				Return privateStone
			End Get
			Set(ByVal value As XMLColorDefinition)
				privateStone = value
			End Set
		End Property

		Private privateWood As XMLColorDefinition
		Public Property Wood() As XMLColorDefinition
			Get
				Return privateWood
			End Get
			Set(ByVal value As XMLColorDefinition)
				privateWood = value
			End Set
		End Property

		Private privateSpot As XMLColorDefinition
		Public Property Spot() As XMLColorDefinition
			Get
				Return privateSpot
			End Get
			Set(ByVal value As XMLColorDefinition)
				privateSpot = value
			End Set
		End Property

	End Class
End Namespace
