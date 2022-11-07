Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports Migration.Buildings
Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core

Namespace Migration
	<Serializable()> _
	Public Class TerrainDefinition

		Private ReadOnly m_SizeShift As Integer
		Private m_HeightMap() As Byte
		Private m_WaterDistMap() As Byte
		Friend m_WallMap() As Byte
		Private m_FlagMap() As Byte
		Private ReadOnly m_BuildingGrid() As Byte
		Private m_GroundMap() As Byte
		Private ReadOnly m_Random As New CrossRandom(1)
		Private ReadOnly m_WaterHeight As Integer
		Private m_Spots As New List(Of Point)()

		<NonSerialized()> _
		Private m_BuildingConfig As BuildingConfiguration

		<NonSerialized()> _
		Private m_Map As Migration.Game.Map

		Public ReadOnly Property BuildingConfig() As BuildingConfiguration
			Get
				Return m_BuildingConfig
			End Get
		End Property

		Public ReadOnly Property Spots() As IEnumerable(Of Point)
			Get
				Return m_Spots.AsReadOnly()
			End Get
		End Property

		Private privateConfig As TerrainConfiguration
		Public Property Config() As TerrainConfiguration
			Get
				Return privateConfig
			End Get
			Private Set(ByVal value As TerrainConfiguration)
				privateConfig = value
			End Set
		End Property

		Private privateSize As Integer
		Public Property Size() As Integer
			Get
				Return privateSize
			End Get
			Private Set(ByVal value As Integer)
				privateSize = value
			End Set
		End Property

		Public ReadOnly Property Map() As Migration.Game.Map
			Get
				Return m_Map
			End Get
		End Property

		Public Event OnCellChanged As Procedure(Of Point)

		Public Sub New(ByVal inMap As Migration.Game.Map, ByVal inConfig As TerrainConfiguration)
			Me.New(inMap.Size, inConfig)
			m_Map = inMap
		End Sub

		Public Sub New(ByVal inSize As Integer, ByVal inConfig As TerrainConfiguration)
			If inConfig Is Nothing Then
				Throw New ArgumentNullException()
			End If

			ValidateConfig(inConfig)

			Config = inConfig
			Size = inSize
			m_SizeShift = Convert.ToInt32(CInt(Fix(Math.Floor(Math.Log(Convert.ToDouble(Size), 2) + 0.5))))

			If Convert.ToInt32(CInt(Fix(Math.Pow(2, Convert.ToDouble(m_SizeShift))))) <> Size Then
				Throw New ApplicationException("Grid size must be a power of two.")
			End If

			m_HeightMap = New Byte(Size * Size - 1){}
			m_WallMap = New Byte(Size * Size - 1){}
			m_WaterDistMap = New Byte(Size * Size - 1){}
			m_FlagMap = New Byte(Size * Size - 1){}
			m_BuildingGrid = New Byte(Size * Size - 1){}
			m_GroundMap = New Byte(Size * Size - 1){}

			m_WaterHeight = Convert.ToInt32(CInt(Fix((1.0 + Config.Water.Height) * 128)))

			For i As Integer = 0 To m_HeightMap.Length - 1
				m_HeightMap(i) = 128
			Next i
		End Sub

		Public Sub SaveToStream(ByVal inStream As Stream)
			Dim format As New BinaryFormatter()

			format.Serialize(inStream, Me)
		End Sub

		Public Sub LoadFromStream(ByVal inStream As Stream)
			Dim format As New BinaryFormatter()
			Dim source As TerrainDefinition = CType(format.Deserialize(inStream), TerrainDefinition)

			If (source.Size <> Size) OrElse (source.m_WaterHeight <> m_WaterHeight) Then
				Throw New ArgumentException("The terrain data stored in the stream is not compatible with this instance.")
			End If

			Me.m_FlagMap = source.m_FlagMap
			Me.m_GroundMap = source.m_GroundMap
			Me.m_HeightMap = source.m_HeightMap
			Me.m_Spots = source.m_Spots
			Me.m_WallMap = source.m_WallMap
			Me.m_WaterDistMap = source.m_WaterDistMap
		End Sub

		Private Sub ValidateConfig(ByVal inConfig As TerrainConfiguration)
			If (inConfig.BlueNoiseFrequency = 0) OrElse (inConfig.RedNoiseFrequency = 0) OrElse (inConfig.GreenNoiseFrequency = 0) Then
				Throw New ArgumentOutOfRangeException()
			End If

			If (inConfig.Water.Height < -1.0) OrElse (inConfig.Water.Height > 1.0) Then
				Throw New ArgumentOutOfRangeException()
			End If

			If inConfig.Water.Speed = 0 Then
				Throw New ArgumentOutOfRangeException()
			End If

			If inConfig.Levels.Count <> 6 Then
				Throw New ArgumentOutOfRangeException()
			End If

			For Each level As XMLLevel In inConfig.Levels
				If (level.RedNoiseDivisor = 0) OrElse (level.GreenNoiseDivisor = 0) OrElse (level.BlueNoiseDivisor = 0) OrElse (level.TextureScale = 0) Then
					Throw New ArgumentOutOfRangeException()
				End If
			Next level
		End Sub

		Friend Function GetWallMap() As Byte()
			Return m_WallMap
		End Function

		Friend Function AddSpot(ByVal inNewSpot As Point) As Boolean
			For Each spot As Point In m_Spots
				If spot.DistanceTo(inNewSpot) < 20 Then
					Return False ' spots need to be a reasonable distance apart, otherwise it just makes no sense...
				End If
			Next spot

			m_Spots.Add(inNewSpot)

			Return True
		End Function

		Private Function IsValidCell(ByVal inXCell As Integer, ByVal inYCell As Integer) As Boolean
			If (inXCell < 0) OrElse (inXCell >= Size) Then
				Return False
			End If

			If (inYCell < 0) OrElse (inYCell >= Size) Then
				Return False
			End If

			Return True
		End Function

		Private Sub CheckBounds(ByVal inXCell As Integer, ByVal inYCell As Integer)
#If DEBUG Then
			If Not(IsValidCell(inXCell, inYCell)) Then
				Throw New ArgumentOutOfRangeException()
			End If
#End If
		End Sub

		Public Function GetGroundAt(ByVal inXCell As Integer, ByVal inYCell As Integer) As Byte
			CheckBounds(inXCell, inYCell)

			Return m_GroundMap(inXCell + (inYCell << m_SizeShift))
		End Function

		Friend Sub SetGroundAt(ByVal inXCell As Integer, ByVal inYCell As Integer, ByVal inGroundType As Byte)
			CheckBounds(inXCell, inYCell)

			m_GroundMap(inXCell + (inYCell << m_SizeShift)) = inGroundType

			RaiseEvent OnCellChanged(New Point(inXCell, inYCell))
		End Sub

		Public Function GetHeightAt(ByVal inXCell As Integer, ByVal inYCell As Integer) As Byte
			CheckBounds(inXCell, inYCell)
			Dim index As Integer = inXCell + (inYCell << m_SizeShift)
			Dim mValue As Byte = m_HeightMap(index)

			Return mValue
		End Function

		Friend Sub SetHeightAt(ByVal inXCell As Integer, ByVal inYCell As Integer, ByVal inHeight As Byte)
			CheckBounds(inXCell, inYCell)

			m_HeightMap(inXCell + (inYCell << m_SizeShift)) = inHeight

			RaiseEvent OnCellChanged(New Point(inXCell, inYCell))
		End Sub

		Public Function GetWallAt(ByVal inXCell As Integer, ByVal inYCell As Integer) As WallValue
			CheckBounds(inXCell, inYCell)

			Return CType(m_WallMap(inXCell + (inYCell << m_SizeShift)), WallValue)
		End Function

		Friend Sub SetWallAt(ByVal inXCell As Integer, ByVal inYCell As Integer, ByVal inWall As WallValue)
			CheckBounds(inXCell, inYCell)

			m_WallMap(inXCell + (inYCell << m_SizeShift)) = Convert.ToByte(inWall)
		End Sub

		Friend Sub SetWallAt(ByVal inCell As Point, ByVal inWall As WallValue, ParamArray ByVal inAreas() As Rectangle)
			For Each rect As Rectangle In inAreas
				For x As Integer = inCell.X + rect.X To inCell.X + rect.Width + rect.X - 1
					For y As Integer = inCell.Y + rect.Y To inCell.Y + rect.Height + rect.Y - 1
						SetWallAt(x, y, inWall)
					Next y
				Next x
			Next rect
		End Sub

		''' <summary>
		''' Only sets the wall if the target cells are free.
		''' </summary>
		Friend Sub InitializeWallAt(ByVal inCell As Point, ByVal inWall As WallValue, ParamArray ByVal inAreas() As Rectangle)
			For Each rect As Rectangle In inAreas
				For x As Integer = inCell.X + rect.X To inCell.X + rect.Width + rect.X - 1
					For y As Integer = inCell.Y + rect.Y To inCell.Y + rect.Height + rect.Y - 1
						If (x < 0) OrElse (x >= Size) Or (y < 0) OrElse (y >= Size) Then
							Continue For
						End If

						If GetWallAt(x, y) = WallValue.Free Then
							SetWallAt(x, y, inWall)
						End If
					Next y
				Next x
			Next rect
		End Sub

		Public Function GetFlagsAt(ByVal inXCell As Integer, ByVal inYCell As Integer) As TerrainCellFlags
			CheckBounds(inXCell, inYCell)

			Dim Index As Integer = inXCell + (inYCell << m_SizeShift)
			Dim Value As TerrainCellFlags = CType(m_FlagMap(Index), TerrainCellFlags)
			Return Value
		End Function

		Friend Sub SetFlagsAt(ByVal inXCell As Integer, ByVal inYCell As Integer, ByVal inFlag As TerrainCellFlags)
			CheckBounds(inXCell, inYCell)

			Dim Index As Integer = inXCell + (inYCell << m_SizeShift)
			Dim Value As Byte = Convert.ToByte(inFlag)
			m_FlagMap(Index) = Value

			RaiseEvent OnCellChanged(New Point(inXCell, inYCell))
		End Sub

		Public Overridable Function GetZShiftAt(ByVal inXCell As Integer, ByVal inYCell As Integer) As Single
			Return Convert.ToSingle((-1.0 + GetHeightAt(inXCell, inYCell) / 128.0) * Config.HeightScale)
		End Function

		Public Sub ResetBuildingGrid(ByVal inConfig As BuildingConfiguration)
			If inConfig Is Nothing Then
				Throw New ArgumentNullException()
			End If

			m_BuildingConfig = inConfig

			Array.Clear(m_BuildingGrid, 0, m_BuildingGrid.Length)
		End Sub

		Public Function GetGradingInfo(ByVal inXCell As Integer, ByVal inYCell As Integer, ByVal config As BuildingConfiguration) As GradingInfo
			' determine avergae height on building ground
			Dim result As New GradingInfo()
			Dim count As Integer = 0
			Dim avgHeight As Integer = 0

			For y As Integer = inYCell To config.GridHeight + inYCell - 1
				For x As Integer = inXCell To config.GridWidth + inXCell - 1
					count += 1
					avgHeight += GetHeightAt(x, y)
				Next x
			Next y

			avgHeight = Convert.ToInt32(CInt(Fix(avgHeight / CDbl(count))))

			' determine grading effort
			Dim variance As Integer = 0

			For y As Integer = inYCell To config.GridHeight + inYCell - 1
				For x As Integer = inXCell To config.GridWidth + inXCell - 1
					variance += Math.Abs(avgHeight - GetHeightAt(x, y))
				Next x
			Next y

			result.AvgHeight = avgHeight
			result.Variance = variance

			Return result
		End Function

		Public Overridable Function CanFoilageBePlacedAt(ByVal inXCell As Integer, ByVal inYCell As Integer, ByVal inFoilageType As FoilageType) As Boolean
			For x As Integer = inXCell - 1 To inXCell
				For y As Integer = inYCell - 1 To inYCell
					If (x < 0) OrElse (y < 0) OrElse (x >= Size) OrElse (y >= Size) Then
						Return False
					End If

					If (GetWallAt(x, y) <> WallValue.Free) OrElse IsWater(x, y) Then
						Return False
					End If
				Next y
			Next x

			For x As Integer = inXCell - 2 To inXCell + 2 - 1
				For y As Integer = inYCell - 2 To inYCell + 2 - 1
					If (x < 0) OrElse (y < 0) OrElse (x >= Size) OrElse (y >= Size) Then
						Return False
					End If

					If GetWallAt(x, y) > WallValue.Reserved Then
						Return False
					End If
				Next y
			Next x

			Return True
		End Function

		Friend Function IsWater(ByVal inPosition As Point) As Boolean
			Return IsWater(inPosition.X, inPosition.Y)
		End Function

		Friend Function IsWater(ByVal inX As Integer, ByVal inY As Integer) As Boolean
			Dim mHeight As Integer = GetHeightAt(inX, inY)
			Return Convert.ToBoolean(mHeight < m_WaterHeight)
		End Function

		Friend Function FindWater(ByVal inAround As Point, ByVal inWaterDepth As Integer, ByRef outWaterSpot As Point) As Boolean
			Dim unused As Direction = 0

			Return FindWater(inAround, inWaterDepth, outWaterSpot, unused)
		End Function

		Friend Function FindWater(ByVal inAround As Point, ByVal inWaterDepth As Integer, ByRef outWaterSpot As Point, ByRef outWaterDir As Direction) As Boolean
			Dim waterSpot As New Point(0, 0)
			Dim waterDir As Direction = Direction._045
			outWaterSpot = waterSpot
			outWaterDir = waterDir

			If Me.GetHeightAt(inAround.X, inAround.Y) < Me.m_WaterHeight Then
				Throw New InvalidOperationException("Can only query for water from land.")
			End If
			If WalkResult.Success <> GridSearch.GridWalkAround(inAround, Me.Size, Me.Size, Function(pos As Point)
					If Me.GetHeightAt(pos.X, pos.Y) >= Me.m_WaterHeight Then
						Return WalkResult.NotFound
					End If
					Dim dirX As Double = (pos.X - inAround.X)
					Dim dirY As Double = (pos.Y - inAround.Y)
					Dim normFactor As Double = Math.Sqrt(((dirX * dirX) + (dirY * dirY)))
					dirX = (dirX / normFactor)
					dirY = (dirY / normFactor)
					Dim waterCount As Integer = 1
					Dim last As Point = pos
					Dim current As Point = pos
					Dim currentX As Double = pos.X
					Dim currentY As Double = pos.Y
					Do While Me.GetHeightAt(current.X, current.Y) < Me.m_WaterHeight
						If last <> current Then
							waterCount += 1
						End If
						currentX = (currentX - dirX)
						currentY = (currentY - dirY)
						last = current
						current = New Point(Convert.ToInt32(currentX), Convert.ToInt32(currentY))
					Loop
					If waterCount <> inWaterDepth Then
						Return WalkResult.NotFound
					End If
					waterSpot = current
					waterDir = DirectionUtils.GetNearestWalkingDirection(New Point(Convert.ToInt32(Math.Round(dirX)), Convert.ToInt32(Math.Round(dirY)))).Value
					Return WalkResult.Success
				End Function
				) Then
				'walk back towards building and count water cells
				Return False
			End If

			outWaterSpot = waterSpot
			outWaterDir = waterDir
			Return True

		End Function

		Public Overridable Function CanBuildingBePlacedAt(ByVal inXCell As Integer, ByVal inYCell As Integer, ByVal config As BuildingConfiguration) As Boolean
			' check if building can be build here at all
			Dim isMine As Boolean = GetType(Mine).IsAssignableFrom(config.ClassType)

			For Each pos As Rectangle In config.GroundPlane.Concat(config.ReservedPlane)
				Dim x As Integer = pos.X + inXCell
				Dim y As Integer = pos.Y + inYCell

				If (x < 0) OrElse (y < 0) OrElse (x >= Size) OrElse (y >= Size) Then
					Return False
				End If

				Dim layer As Single = (-1.0F + GetHeightAt(x, y) / 128.0F)
				Dim height As Single = Convert.ToSingle(layer * Me.Config.HeightScale)
				Dim hasPassed As Boolean = False

				Do
					If GetWallAt(x, y) <> WallValue.Free Then
						Exit Do
					End If

					If layer <= Me.Config.Water.Height Then
						Exit Do
					End If

					Dim flags As TerrainCellFlags = GetFlagsAt(x, y)

					If (flags And TerrainCellFlags.WallMask) <> 0 Then
						Exit Do
					End If

					If isMine Then

						If layer < Me.Config.Levels(3).Margin Then
							Exit Do
						End If
					ElseIf layer >= Me.Config.Levels(3).Margin Then
						Exit Do
					End If

					hasPassed = True
					Loop While False

				If Not hasPassed Then
					Return False
				End If
			Next pos

			Return True
		End Function

		Public Overridable Function GetBuildingExpenses(ByVal inXCell As Integer, ByVal inYCell As Integer) As Byte

			Dim  m_config As BuildingConfiguration = m_BuildingConfig

			If  m_config Is Nothing Then
				Return 0
			End If

			inXCell -= Convert.ToInt32(CInt(Fix( m_config.GridWidth / 2.0)))
			inYCell -= Convert.ToInt32(CInt(Fix(( m_config.GridHeight * 1 / Math.Sqrt(2)) / 2)))

			If (inXCell < 0) OrElse (inYCell < 0) OrElse (inXCell +  m_config.GridWidth >= Size) OrElse (inYCell +  m_config.GridHeight >= Size) Then
				Return 0
			End If

			Dim result As Byte = m_BuildingGrid(inXCell + (inYCell << m_SizeShift))

			If result > 0 Then
				Return result ' already cached
			End If

			If Not(CanBuildingBePlacedAt(inXCell, inYCell,  m_config)) Then
				' not buildable here
				result = 1
			Else
				' determine avergae height on building ground
				Dim variance As Double = GetGradingInfo(inXCell, inYCell,  m_config).Variance

				variance = Math.Log(variance / GradingInfo.GradingStrength, 2.5)

				result = Convert.ToByte(variance)
			End If

			m_BuildingGrid(inXCell + (inYCell << m_SizeShift)) = result

			Return result
		End Function

		''' <summary>
		''' Changes the height of the given cell as well as its surrounding accordingly to
		''' the given brush. These painting operations are the preferred way of changing
		''' terrain properties, since they cause a more natural look and feel.
		''' </summary>
		''' <param name="inXCell"></param>
		''' <param name="inYCell"></param>
		''' <param name="inBrush"></param>
		Friend Sub DrawToHeightmap(ByVal inXCell As Integer, ByVal inYCell As Integer, ByVal inBrush As TerrainBrush)

		End Sub

		''' <summary>
		''' Instead of <see cref="DrawToHeightmap"/>, this one will use the brush to reduce
		''' the terrain variance from average. This will in fact grade the terrain to the
		''' given average height by using the brush as weight for each surrounding cell.
		''' </summary>
		''' <param name="inXCell"></param>
		''' <param name="inYCell"></param>
		''' <param name="inAvgHeight"></param>
		''' <param name="inBrush"></param>
		Friend Sub AverageDrawToHeightmap(ByVal inXCell As Integer, ByVal inYCell As Integer, ByVal inAvgHeight As Integer, ByVal inBrush As TerrainBrush)
			Dim x As Integer = inXCell - inBrush.Width \ 2
			Dim ix As Integer = 0
			Do While x < inXCell + inBrush.Width \ 2
				Dim y As Integer = inYCell - inBrush.Height \ 2
				Dim iy As Integer = 0
				Do While y < inYCell + inBrush.Height \ 2
					If Not(IsValidCell(x, y)) Then
						y += 1
						iy += 1
						Continue Do
					End If

					Dim height As Integer = GetHeightAt(x, y)
					Dim diff As Integer = inAvgHeight - height
					Dim newHeight As Integer = height + Math.Sign(diff) * Math.Abs(inBrush.Values(ix, iy))

					If (newHeight < inAvgHeight) AndAlso (height >= inAvgHeight) Then
						newHeight = inAvgHeight
					ElseIf (newHeight > inAvgHeight) AndAlso (height <= inAvgHeight) Then
						newHeight = inAvgHeight
					End If

					SetHeightAt(x, y, Convert.ToByte(newHeight))
					y += 1
					iy += 1
				Loop
				x += 1
				ix += 1
			Loop
		End Sub

		Friend Function EnumAround(ByVal inAround As Point, ByVal inHandler As Func(Of Point, WalkResult)) As WalkResult
			Return GridSearch.GridWalkAround(inAround, Size, Size, Function(pos) inHandler(pos))
		End Function

	End Class
End Namespace
