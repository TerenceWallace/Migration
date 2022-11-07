Imports System.Reflection
Imports System.Runtime.Serialization
Imports System.Xml.Serialization
Imports Migration.Buildings
Imports Migration.Common
Imports Migration.Core

Namespace Migration.Configuration

    <ObfuscationAttribute(Feature:="renaming", ApplyToMembers:=True), Serializable(), DataContract()> _
    Public Class BuildingConfiguration

        Private privateTypeIndex As Int32
        <XmlAttribute(), DataMember()> _
        Public Property TypeIndex() As Int32
            Get
                Return privateTypeIndex
            End Get
            Set(ByVal value As Int32)
                privateTypeIndex = value
            End Set
        End Property

        Private privateMigrantCount As Int32
        <XmlAttribute(), DataMember()> _
        Public Property MigrantCount() As Int32
            Get
                Return privateMigrantCount
            End Get
            Set(ByVal value As Int32)
                privateMigrantCount = value
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

        Private privateCharacter As String
        <XmlAttribute(), DataMember()> _
        Public Property Character() As String
            Get
                Return privateCharacter
            End Get
            Set(ByVal value As String)
                privateCharacter = value
            End Set
        End Property

        Private privateNoProductionRange As Boolean
        <XmlAttribute(), DataMember()> _
        Public Property NoProductionRange() As Boolean
            Get
                Return privateNoProductionRange
            End Get
            Set(ByVal value As Boolean)
                privateNoProductionRange = value
            End Set
        End Property

        Private privateResourceStacks As List(Of ResourceStack)
        Public Property ResourceStacks() As List(Of ResourceStack)
            Get
                Return privateResourceStacks
            End Get
            Set(ByVal value As List(Of ResourceStack))
                privateResourceStacks = value
            End Set
        End Property

        Private privateDamageResistance As Int32
        <XmlAttribute(), DataMember()> _
        Public Property DamageResistance() As Int32
            Get
                Return privateDamageResistance
            End Get
            Set(ByVal value As Int32)
                privateDamageResistance = value
            End Set
        End Property

        Private privateWoodCount As Int32
        <XmlAttribute(), DataMember()> _
        Public Property WoodCount() As Int32
            Get
                Return privateWoodCount
            End Get
            Set(ByVal value As Int32)
                privateWoodCount = value
            End Set
        End Property

        Private privateStoneCount As Int32
        <XmlAttribute(), DataMember()> _
        Public Property StoneCount() As Int32
            Get
                Return privateStoneCount
            End Get
            Set(ByVal value As Int32)
                privateStoneCount = value
            End Set
        End Property

        Private privateProductionTimeMillis As Int32
        <XmlAttribute(), DataMember()> _
        Public Property ProductionTimeMillis() As Int32
            Get
                Return privateProductionTimeMillis
            End Get
            Set(ByVal value As Int32)
                privateProductionTimeMillis = value
            End Set
        End Property

        Private privateClassString As String
        <XmlAttribute("Class"), DataMember()> _
        Public Property ClassName() As String
            Get
                Return privateClassString
            End Get
            Set(ByVal value As String)
                privateClassString = value
            End Set
        End Property

        Private privateClassType As Type
        <XmlIgnore()> _
        Public Property ClassType() As Type
            Get
                Return privateClassType
            End Get
            Set(ByVal value As Type)
                privateClassType = value
            End Set
        End Property

        Private privateClass As BuildingClass
        <XmlIgnore(), DataMember()> _
        Public Property BuildingClass() As BuildingClass
            Get
                Return privateClass
            End Get
            Set(ByVal value As BuildingClass)
                privateClass = value
            End Set
        End Property

        Private privateClassParameter As String
        <XmlAttribute(), DataMember()> _
        Public Property ClassParameter() As String
            Get
                Return privateClassParameter
            End Get
            Set(ByVal value As String)
                privateClassParameter = value
            End Set
        End Property

        Private privateTabIndex As Integer
        <XmlAttribute(), DataMember()> _
        Public Property TabIndex() As Integer
            Get
                Return privateTabIndex
            End Get
            Set(ByVal value As Integer)
                privateTabIndex = value
            End Set
        End Property

        Private privateWorker As BuildingWorkerType
        <XmlAttribute(), DataMember()> _
        Public Property Worker() As BuildingWorkerType
            Get
                Return privateWorker
            End Get
            Set(ByVal value As BuildingWorkerType)
                privateWorker = value
            End Set
        End Property

        <XmlIgnore()> _
        Public ReadOnly Property WorkerTool() As Resource
            Get
                Select Case Worker
                    Case BuildingWorkerType.None, BuildingWorkerType.Migrant
                        Return Resource.Max
                    Case BuildingWorkerType.Axe
                        Return Resource.Axe
                    Case BuildingWorkerType.Hammer
                        Return Resource.Hammer
                    Case BuildingWorkerType.Hook
                        Return Resource.Hook
                    Case BuildingWorkerType.PickAxe
                        Return Resource.PickAxe
                    Case BuildingWorkerType.Saw
                        Return Resource.Saw
                    Case BuildingWorkerType.Scythe
                        Return Resource.Scythe
                    Case Else
                        Throw New ArgumentException()
                End Select
            End Get
        End Property

        ' non-serialized fields (computed by race config loader)
        Private privateGroundPlane As List(Of Rectangle)
        <XmlIgnore()> _
        Public Property GroundPlane() As List(Of Rectangle)
            Get
                Return privateGroundPlane
            End Get
            Set(ByVal value As List(Of Rectangle))
                privateGroundPlane = value
            End Set
        End Property

        Private privateReservedPlane As List(Of Rectangle)
        <XmlIgnore()> _
        Public Property ReservedPlane() As List(Of Rectangle)
            Get
                Return privateReservedPlane
            End Get
            Set(ByVal value As List(Of Rectangle))
                privateReservedPlane = value
            End Set
        End Property

        Private privateGridWidth As Int32
        <XmlIgnore(), DataMember()> _
        Public Property GridWidth() As Int32
            Get
                Return privateGridWidth
            End Get
            Set(ByVal value As Int32)
                privateGridWidth = value
            End Set
        End Property

        Private privateGridHeight As Int32
        <XmlIgnore(), DataMember()> _
        Public Property GridHeight() As Int32
            Get
                Return privateGridHeight
            End Get
            Set(ByVal value As Int32)
                privateGridHeight = value
            End Set
        End Property

        Private privateStoneSpot As Point
        <XmlIgnore(), DataMember()> _
        Public Property StoneSpot() As Point
            Get
                Return privateStoneSpot
            End Get
            Set(ByVal value As Point)
                privateStoneSpot = value
            End Set
        End Property

        Private privateTimberSpot As Point
        <XmlIgnore(), DataMember()> _
        Public Property TimberSpot() As Point
            Get
                Return privateTimberSpot
            End Get
            Set(ByVal value As Point)
                privateTimberSpot = value
            End Set
        End Property

        Private privateGroundGrid()() As Boolean
        <XmlIgnore(), DataMember()> _
        Public Property GroundGrid() As Boolean()()
            Get
                Return privateGroundGrid
            End Get
            Set(ByVal value As Boolean()())
                privateGroundGrid = value
            End Set
        End Property

        Private privateReservedGrid()() As Boolean
        <XmlIgnore(), DataMember()> _
        Public Property ReservedGrid() As Boolean()()
            Get
                Return privateReservedGrid
            End Get
            Set(ByVal value As Boolean()())
                privateReservedGrid = value
            End Set
        End Property

        Private privateConstructorSpots As List(Of Point)
        <XmlIgnore(), DataMember()> _
        Public Property ConstructorSpots() As List(Of Point)
            Get
                Return privateConstructorSpots
            End Get
            Set(ByVal value As List(Of Point))
                privateConstructorSpots = value
            End Set
        End Property

        Private privateProductionAnimTime As Int32
        <XmlIgnore(), DataMember()> _
        Public Property ProductionAnimTime() As Int32
            Get
                Return privateProductionAnimTime
            End Get
            Set(ByVal value As Int32)
                privateProductionAnimTime = value
            End Set
        End Property

        Friend Function CreateInstance(ByVal inParent As BuildingManager, ByVal inPosition As Point) As BaseBuilding
            Dim constructor As ConstructorInfo = ClassType.GetConstructor(BindingFlags.NonPublic Or BindingFlags.Public Or BindingFlags.Instance, Nothing, New Type() {GetType(BuildingManager), GetType(BuildingConfiguration), GetType(Point)}, Nothing)
            Dim constructorOpt As ConstructorInfo = ClassType.GetConstructor(BindingFlags.NonPublic Or BindingFlags.Public Or BindingFlags.Instance, Nothing, New Type() {GetType(BuildingManager), GetType(BuildingConfiguration), GetType(Point), GetType(String)}, Nothing)

            If constructorOpt IsNot Nothing Then
                Return CType(constructorOpt.Invoke(New Object() {inParent, Me, inPosition, ClassParameter}), BaseBuilding)
            Else
                Return CType(constructor.Invoke(New Object() {inParent, Me, inPosition}), BaseBuilding)
            End If
        End Function

        Public Sub New()
            ResourceStacks = New List(Of ResourceStack)()
        End Sub
    End Class

End Namespace
