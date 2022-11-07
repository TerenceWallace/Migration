Imports System.Reflection
Imports System.Runtime.Serialization
Imports System.Xml.Serialization
Imports Migration.Common
Imports Migration.Core

Namespace Migration.Configuration
	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True), Serializable(), DataContract()> _
	Public Class ResourceStack

		Private privatePosition As Point
		<XmlIgnore(), DataMember()> _
		Public Property Position() As Point
			Get
				Return privatePosition
			End Get
			Set(ByVal value As Point)
				privatePosition = value
			End Set
		End Property

		Private privateType As Resource
		<XmlAttribute(), DataMember()> _
		Public Property Type() As Resource
			Get
				Return privateType
			End Get
			Set(ByVal value As Resource)
				privateType = value
			End Set
		End Property

		Private privateTimeOffset As Int32
		<XmlAttribute(), DataMember()> _
		Public Property TimeOffset() As Int32
			Get
				Return privateTimeOffset
			End Get
			Set(ByVal value As Int32)
				privateTimeOffset = value
			End Set
		End Property

		Private privateCycleCount As Int32
		<XmlAttribute(), DataMember()> _
		Public Property CycleCount() As Int32
			Get
				Return privateCycleCount
			End Get
			Set(ByVal value As Int32)
				privateCycleCount = value
			End Set
		End Property

		Private privateMaxStackSize As Int32
		<XmlAttribute(), DataMember()> _
		Public Property MaxStackSize() As Int32
			Get
				Return privateMaxStackSize
			End Get
			Set(ByVal value As Int32)
				privateMaxStackSize = value
			End Set
		End Property

		Private privateQualityIndex As Int32
		<XmlAttribute(), DataMember()> _
		Public Property QualityIndex() As Int32
			Get
				Return privateQualityIndex
			End Get
			Set(ByVal value As Int32)
				privateQualityIndex = value
			End Set
		End Property

		Private privateDirection As StackType
		<XmlAttribute(), DataMember()> _
		Public Property Direction() As StackType
			Get
				Return privateDirection
			End Get
			Set(ByVal value As StackType)
				privateDirection = value
			End Set
		End Property
	End Class

End Namespace
