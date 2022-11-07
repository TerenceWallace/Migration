Imports System.Reflection
Imports System.Runtime.Serialization
Imports System.Xml.Serialization

Namespace Migration.Configuration

	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True), XmlRoot("RaceConfiguration"), DataContract()> _
	Public Class RaceConfiguration

		Private privateBuildables As List(Of BuildingConfiguration)
		<DataMember()> _
		Public Property Buildables() As List(Of BuildingConfiguration)
			Get
				Return privateBuildables
			End Get
			Set(ByVal value As List(Of BuildingConfiguration))
				privateBuildables = value
			End Set
		End Property

		Private privateName As String
		<XmlAttribute(), DataMember()> _
		Public Property Name() As String
			Get
				Return privateName
			End Get
			Set(ByVal value As String)
				privateName = value
			End Set
		End Property
	End Class

End Namespace
