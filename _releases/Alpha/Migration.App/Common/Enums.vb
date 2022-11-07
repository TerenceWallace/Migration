Imports System.Reflection
Imports System.Runtime.Serialization

Namespace Migration.Common

    Public Enum LibraryMode
        Filesystem
        [Assembly]
    End Enum

    <ObfuscationAttribute(Feature:="renaming", ApplyToMembers:=True), Serializable(), DataContract()> _
    Public Enum StackType
        ''' <summary>
        ''' A query is provided with resources.
        ''' </summary>
        <EnumMember()> _
        Query
        ''' <summary>
        ''' A provider serves resource queries.
        ''' </summary>
        <EnumMember()> _
        Provider
    End Enum

    <ObfuscationAttribute(Feature:="renaming", ApplyToMembers:=True), Serializable(), DataContract()> _
    Public Enum BuildingWorkerType
        <EnumMember()> _
        None
        <EnumMember()> _
        Migrant
        <EnumMember()> _
        Axe
        <EnumMember()> _
        Hammer
        <EnumMember()> _
        Hook
        <EnumMember()> _
        PickAxe
        <EnumMember()> _
        Saw
        <EnumMember()> _
        Scythe
    End Enum

    <ObfuscationAttribute(Feature:="renaming", ApplyToMembers:=True), Serializable(), DataContract()> _
    Public Enum BuildingClass
        <EnumMember()> _
        Angler
        <EnumMember()> _
        Workshop
        <EnumMember()> _
        Market
        <EnumMember()> _
        Mine
        <EnumMember()> _
        PlantProduce
        <EnumMember()> _
        Home
        <EnumMember()> _
        StoneCutter
        <EnumMember()> _
        ToolSmith
        <EnumMember()> _
        Waterworks
        <EnumMember()> _
        Barracks
    End Enum

    Public Enum MigrantProfessions
        Geologist
        Pioneer
        Agent
        Grader
        Constructor

        Max ' must be the last entry
    End Enum

    Public Enum MigrantStatisticTypes As Integer
        Migrant
        Geologist
        Pioneer
        Agent
        Grader
        Constructor
        Speer
        Swordsman
        Archer
        Priest
        Worker

        Max ' must be the last entry
    End Enum

    Friend Enum GameMapCommand As Byte
        DropResource = 0
        AddBuilding = 1
        AddMovable = 2
        Idle = 3
        ChangeProfession = 4
        ChangeDistributionSetting = 5
        ChangeToolSetting = 6
        AddMarketTransport = 7
        RemoveMarketTransport = 8
        SetBuildingSuspended = 9
        LowerBuildingPriority = 10
        RaiseBuildingPriority = 11
        RemoveBuilding = 12
        RaiseTaskPriority = 13
        RemoveBuildTask = 14
        SetTaskSuspended = 15
        SetWorkingArea = 16
    End Enum

    Public Enum BuildStates
        Started
        Graded
        Built
        Completed
    End Enum

    Public Enum Direction As Integer
        ' _000,
        _045
        _090
        _135
        '_180,
        _225
        _270
        _315

        ' must be the last element!
        Count
    End Enum

    ''' <summary>
    ''' Currently there are only Migrants.
    ''' </summary>
    Public Enum MovableType As Integer
        Migrant = 1
        Constructor = 7
        Soldier = 9
        Grader = 10
    End Enum

    <Serializable()> _
    Public Enum RoutingDirection
        _000
        _045
        _090
        _135
        _180
        _225
        _270
        _315
    End Enum

    Public Enum FoilageState
        Growing
        Grown
        BeingCut
    End Enum

    Public Enum FoilageType
        Tree1 = 1
        'Tree2 = 2,
        'Tree3 = 4,
        'Tree4 = 8,
        'Tree = Tree1 | Tree2, // | Tree2 | Tree3 | Tree4,

        Grain = 128
        Wine = 256
        Honey = 512
        Rice = 1024
    End Enum

    ' Correlates with MovableType 
    Public Enum WallValue
        Free = 0
        Reserved = 1
        MigrantUnwalkable = 9
        SoldierUnwalkable = 10
        Building = 20

        WaterBorder = 231
    End Enum

    <Flags()> _
    Public Enum TerrainCellFlags
        None = &H0

        Tree_01 = &H1
        Tree_02 = &H2
        Tree_03 = &H3
        Tree_04 = &H4
        Tree_05 = &H5
        TreeMask = &H7

        Stone = &H8

        Grading = &H40
        Snow = &H80

        ' collects flags that prevents a cell from being used for buildings
        WallMask = TreeMask Or Snow Or Stone
    End Enum

    Friend Enum GroundType
        Desert = 0
        Grass = 1
        Mud = 2
        Rock = 3
        Sand = 4
        Spot = 5
        Stone = 6
        Swamp = 7
        Water = 8
        Wood = 9
        Snow = 10
    End Enum

    <Flags()> _
    Public Enum MapPolicy As Integer
        None = 0
        AllowDuplicates = 1
        DefaultForNonExisting = 2
        CreateNonExisting = 4
        SortManually = 8
    End Enum

    Public Enum WalkResult
        Success
        NotFound
        Abort
    End Enum


    <Serializable()> _
    Public Enum Resource As Integer
        Axe = 0
        Bow = 1
        Bread = 2
        Coal = 3
        Corn = 4
        Fish = 5
        Gold = 6
        GoldOre = 7
        Grain = 8
        Hammer = 9
        Hook = 10
        Iron = 11
        IronOre = 12
        Meat = 13
        PickAxe = 14
        Saw = 15
        Scythe = 16
        Sheep = 17
        Shovel = 18
        Spear = 19
        Sword = 20
        Stone = 21
        Timber = 22
        Water = 23
        Wine = 24
        Wood = 25

        ' must be the last item!
        Max
    End Enum

    <Flags()> _
    Public Enum TextureOptions
        None = 0
        Repeat = 1
    End Enum

    Public Enum RenderPass
        Pass1_Shadow
        Pass2_Texture
        Pass3_PlayerColor
    End Enum

    <ObfuscationAttribute(Feature:="renaming", ApplyToMembers:=True), Flags()> _
    Public Enum FrameBorders
        None = 0
        Top = 1
        Left = 8
        Right = 16
        Bottom = 32
        Middle = 256
    End Enum

    <ObfuscationAttribute(Feature:="renaming", ApplyToMembers:=True)> _
    Public Enum ImageAlignment
        Left
        Center
        Right
    End Enum


End Namespace