Imports System.Drawing.Imaging
Imports System.IO
Imports System.Xml.Serialization
Imports Migration.Common

Namespace Migration

    Friend Class XMLTerrainGenerator
        Inherits AbstractTerrainGenerator

        Private privateDirectory As String
        Public Property Directory() As String
            Get
                Return privateDirectory
            End Get
            Private Set(ByVal value As String)
                privateDirectory = value
            End Set
        End Property

        Private privateXMLFile As String
        Public Property XMLFile() As String
            Get
                Return privateXMLFile
            End Get
            Private Set(ByVal value As String)
                privateXMLFile = value
            End Set
        End Property

        Public Shared Sub Run(ByVal inMap As Migration.Game.Map, ByVal inSeed As Long, ByVal inMapFile As String)
            Dim TempXMLTerrainGenerator As New XMLTerrainGenerator(inMap, inSeed, inMapFile)
        End Sub

        Private Sub New(ByVal inMap As Migration.Game.Map, ByVal inSeed As Long, ByVal inMapFile As String)
            MyBase.New(inMap, inSeed)
            XMLFile = Path.GetFullPath(inMapFile)
            Directory = Path.GetDirectoryName(XMLFile)

            ' create and analyze map file
            Dim format As New XmlSerializer(GetType(XMLMapFile))
            Using stream As New FileStream(XMLFile, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim xmlMap As XMLMapFile = Nothing

                xmlMap = CType(format.Deserialize(stream), XMLMapFile)

                If String.IsNullOrEmpty(xmlMap.Name) Then
                    Throw New ArgumentException()
                End If

                If xmlMap.ImageFile IsNot Nothing Then
                    GenerateFromImageFile(xmlMap.ImageFile)
                Else
                    ' currently only ImageFile is supported...
                    Throw New ArgumentException()
                End If
            End Using

            Map.Routing.Analyze()
        End Sub

        Private Sub GenerateFromImageFile(ByVal xmlImgFile As XMLImageFile)
            xmlImgFile.Source = Path.GetFullPath((Me.Directory & "/" & xmlImgFile.Source))
            Try
                If Not (File.Exists(xmlImgFile.Source)) Then
                    Throw New FileNotFoundException("Unable to locate map image file.")
                End If

                Dim bmp As Bitmap = CType(System.Drawing.Image.FromFile(xmlImgFile.Source), Bitmap)
                Dim groundLayer(MyBase.Size - 1)() As GroundType
                Dim resourceLayer(MyBase.Size - 1)() As GroundType
                Dim waterBorders(MyBase.Size - 1)() As Boolean

                Dim heightMap((MyBase.Size * MyBase.Size) - 1) As Double
                Dim blurBuffer((MyBase.Size * MyBase.Size) - 1) As Double
                Dim groundMap((MyBase.Size * MyBase.Size) - 1) As Double
                Dim mSize As Integer = MyBase.Size

                Try
                    Dim imgWidth As Integer = bmp.Width
                    Dim xImgStep As Double = (Convert.ToDouble(bmp.Width) / Convert.ToDouble(MyBase.Map.Size))
                    Dim yImgStep As Double = (Convert.ToDouble(bmp.Height) / Convert.ToDouble(MyBase.Map.Size))

                    Dim xImg As Double = xImgStep
                    Dim yImg As Double = 0
                    Dim knownColors() As Integer = {xmlImgFile.Desert.ColorARGB, xmlImgFile.Grass.ColorARGB, xmlImgFile.Mud.ColorARGB, xmlImgFile.Rock.ColorARGB, xmlImgFile.Sand.ColorARGB, xmlImgFile.Spot.ColorARGB, xmlImgFile.Stone.ColorARGB, xmlImgFile.Swamp.ColorARGB, xmlImgFile.Water.ColorARGB, xmlImgFile.Wood.ColorARGB}

                    ' infere ground type layer by replacing resources with nearest ground type
                    Dim prevGroundType As GroundType = GroundType.Water
                    Dim colorARGB As Integer = xmlImgFile.Water.ColorARGB

                    For x As Integer = 0 To Size - 1
                        If x >= 511 Then
                            xImg -= 1
                        End If
                        yImg = 0

                        groundLayer(x) = New GroundType(mSize - 1) {}
                        resourceLayer(x) = New GroundType(mSize - 1) {}
                        waterBorders(x) = New Boolean(mSize - 1) {}

                        For y As Integer = 0 To Size - 1
                            Dim ground As GroundType = 0
                            Dim pixel As Integer = bmp.GetPixel(Convert.ToInt32(CInt(Fix(xImg))), Convert.ToInt32(CInt(Fix(yImg)))).ToArgb()
                            Dim colorIndex As Integer = knownColors.IndexOf(pixel)
                            Dim resType As GroundType = GroundType.Water

                            If colorIndex < 0 Then
                                ground = prevGroundType

                            ElseIf colorIndex = CInt(GroundType.Wood) OrElse colorIndex = CInt(GroundType.Stone) OrElse colorIndex = CInt(GroundType.Spot) Then
                                ground = prevGroundType
                                resType = CType(colorIndex, GroundType)

                            Else
                                prevGroundType = CType(colorIndex, GroundType)
                                ground = prevGroundType

                                If ground = GroundType.Rock Then
                                    resType = GroundType.Rock
                                End If

                            End If

                            resourceLayer(x)(y) = resType
                            groundLayer(x)(y) = ground

                            yImg += yImgStep
                        Next y

                        xImg += xImgStep
                    Next x

                    For x As Integer = 1 To mSize - 2
                        For y As Integer = 1 To mSize - 2
                            'if cell has non-water neightbors, we need to mark it as water border!
                            If (groundLayer(x)(y) = GroundType.Water) AndAlso ((((groundLayer((x + 1))(y) <> GroundType.Water) OrElse (groundLayer((x + 1))((y + 1)) <> GroundType.Water)) OrElse ((groundLayer(x)((y + 1)) <> GroundType.Water) OrElse (groundLayer(x)((y - 1)) <> GroundType.Water))) OrElse (((groundLayer((x - 1))((y - 1)) <> GroundType.Water) OrElse (groundLayer((x - 1))(y) <> GroundType.Water)) OrElse ((groundLayer((x - 1))((y + 1)) <> GroundType.Water) OrElse (groundLayer((x + 1))((y - 1)) <> GroundType.Water)))) Then
                                waterBorders(x)(y) = True
                            End If
                        Next y
                    Next x
                Finally
                    bmp.Dispose()
                    bmp = Nothing
                End Try

                ' derive height map from ground type layer
                Dim waterHeight As Double = -1.0 '(Terrain.Config.Levels[0].Margin + Terrain.Config.Levels[1].Margin) / 2.0;
                Dim sandHeight As Double = (MyBase.Terrain.Config.Levels(1).Margin + MyBase.Terrain.Config.Levels(2).Margin) / 2.0
                Dim grassHeight As Double = (MyBase.Terrain.Config.Levels(2).Margin + MyBase.Terrain.Config.Levels(3).Margin) / 2.0
                Dim rockHeight As Double = (MyBase.Terrain.Config.Levels(3).Margin + MyBase.Terrain.Config.Levels(4).Margin) / 2.0
                Dim snowHeight As Double = (MyBase.Terrain.Config.Levels(4).Margin + MyBase.Terrain.Config.Levels(5).Margin) / 2.0

                Dim offset As Integer = 0
                Dim yLoop As Integer = 0

                ' Set base level heightMap
                Do While yLoop < mSize
                    For x As Integer = 0 To mSize - 1
                        Dim height As Double = 0

                        Select Case groundLayer(x)(yLoop)
                            Case GroundType.Desert, GroundType.Grass, GroundType.Mud, GroundType.Swamp
                                height = grassHeight

                            Case GroundType.Rock
                                height = rockHeight

                            Case GroundType.Sand
                                height = sandHeight

                            Case GroundType.Snow
                                height = snowHeight

                            Case Else
                                height = waterHeight

                        End Select

                        blurBuffer(offset) = height
                        heightMap(offset) = waterHeight
                        offset += 1
                    Next x

                    yLoop += 1
                Loop

                ' Generate a groundMap based on the prior level base heightMap
                Me.Blur2D(blurBuffer, heightMap, New Double() {0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.6, 0.5, 0.4, 0.3, 0.2, 0.1})
                Array.Copy(heightMap, groundMap, groundMap.Length)

                'further postprocess heightmap
                Dim selector As New PerlinNoise(16, 0.18, 0.1, 100000)
                'Dim rockSelector As New PerlinNoise(16, 0.10, 0.2, 100000)
                Dim vec(1) As Double

                ' Generate random higher ground surfaces based on prior level ground  heightMap
                yLoop = 0
                offset = 0
                Do While yLoop < mSize
                    For x1 As Integer = 0 To mSize - 1
                        Dim height As Double = heightMap(offset)
                        vec(0) = x1
                        vec(1) = yLoop

                        'If height > rockMargin Then
                        '    height = Math.Max(rockMargin, height - rockSelector.Perlin_noise_2D(vec))
                        'Else
                        Dim value As Double = selector.Perlin_noise_2D(vec)
                        height = Math.Max(-1.0, Math.Min(1.0, height + value))
                        blurBuffer(offset) = height
                        offset += 1

                        'End If
                    Next x1

                    yLoop += 1
                Loop

                ' Copy the new randomly generated higher ground surfaces into the heightMap
                Array.Copy(blurBuffer, heightMap, heightMap.Length - 1)

                Dim xLoop As Integer = 0
                offset = 0

                Do While xLoop < MyBase.Size

                    For yLoop = 0 To MyBase.Size - 1
                        blurBuffer(offset) = Convert.ToDouble(If(waterBorders(xLoop)(yLoop), 1, 0))
                        offset += 1
                    Next yLoop

                    xLoop += 1
                Loop

                XMLTerrainGenerator.Save(blurBuffer)
                yLoop = 0
                offset = 0

                Do While yLoop < MyBase.Size
                    xLoop = 0

                    Do While xLoop < MyBase.Size
                        Dim height As Double = heightMap(offset)

                        Dim mByteVal As Byte = Convert.ToByte(128 + 127 * height)
                        MyBase.Terrain.SetHeightAt(xLoop, yLoop, mByteVal)

                        mByteVal = Convert.ToByte(128 + 127 * groundMap(offset))
                        MyBase.Terrain.SetGroundAt(xLoop, yLoop, mByteVal)

                        Dim IsWaterBorder As Boolean = waterBorders(xLoop)(yLoop)
                        If IsWaterBorder Then
                            MyBase.Terrain.SetWallAt(xLoop, yLoop, WallValue.WaterBorder)
                        End If

                        Dim mGround As GroundType = resourceLayer(xLoop)(yLoop)
                        Select Case mGround
                            Case GroundType.Spot
                                MyBase.Terrain.AddSpot(New Point(xLoop, yLoop))

                            Case GroundType.Stone
                                MyBase.Terrain.SetFlagsAt(xLoop, yLoop, TerrainCellFlags.Stone)

                            Case GroundType.Wood
                                MyBase.Terrain.SetFlagsAt(xLoop, yLoop, TerrainCellFlags.Tree_01)
                        End Select

                        xLoop += 1
                        offset += 1
                    Loop

                    yLoop += 1
                Loop

                If MyBase.Terrain.Spots.Count() = 0 Then
                    Throw New ArgumentException("A map needs to provide at least one valid spot.")
                End If

            Catch e As Exception
                Throw New ArgumentException(("Unable to load map file """ & Me.XMLFile & """. Inner exception: " & e.ToString()))
            End Try
        End Sub

        Public Shared Sub Save(ByVal inData() As Byte)
            Dim data(inData.Length - 1) As Double

            For x As Integer = 0 To data.Length - 1
                data(x) = inData(x)
            Next x

            Save(data)
        End Sub

        ''' <summary>
        ''' This procedure just simply manipulates image colors and then saves the image file
        ''' </summary>
        ''' <param name="inData"></param>
        ''' <remarks></remarks>
        Public Shared Sub Save(ByVal inData() As Double)

            Dim m_size As Integer = Convert.ToInt32(Math.Ceiling(Math.Sqrt(Convert.ToDouble(inData.Length))))
            If inData.Length <> (m_size * m_size) Then
                Throw New ArgumentException()
            End If
            Using image As New Bitmap(m_size, m_size, PixelFormat.Format32bppArgb)
                Try
                    'Dim width As Integer = image.Width
                    Dim mData() As Double = inData

                    Try
                        Dim minData As Double = Double.PositiveInfinity
                        Dim maxData As Double = Double.NegativeInfinity

                        Dim y As Integer = 0
                        Dim offset As Integer = 0

                        Do While y < m_size

                            Dim x As Integer

                            For x = 0 To m_size - 1
                                minData = Math.Min(minData, mData(offset))
                                maxData = Math.Max(maxData, mData(offset))
                                offset += 1
                            Next x

                            y += 1
                        Loop


                        offset = 0
                        y = 0
                        Do While y < m_size

                            For x As Integer = 0 To m_size - 1
                                Dim value As Double = mData(offset)
                                Dim rgb As Integer = Convert.ToInt32(CInt(Fix((value - minData) / (maxData - minData))) * 255)
                                Dim pixel As Color = Color.FromArgb((((rgb Or (rgb << 8)) Or (rgb << &H10)) Or -16777216))

                                image.SetPixel(x, y, pixel)

                                offset += 1
                            Next x

                            y += 1
                        Loop

                    Finally
                        mData = Nothing
                    End Try
                Finally
                    'image.UnlockBits(imageData)
                End Try
                image.Save("./test.png")
            End Using
        End Sub

        Private Sub Blur2D(ByVal input() As Double, ByVal output() As Double, ByVal filter() As Double)
            If (input.Length <> output.Length) OrElse (input.Length <> Size * Size) Then
                Throw New ArgumentException()
            End If

            ' compute filter matrix 
            Dim filterMat(filter.Length * filter.Length - 1) As Double
            Dim matSum As Double = 0

            Dim y As Integer = 0
            Dim offset As Integer = 0

            Do While y < filter.Length

                For x As Integer = 0 To filter.Length - 1
                    filterMat(offset) = filter(x) * filter(y)
                    matSum += filterMat(offset)
                    offset += 1
                Next x

                y += 1
            Loop

            For i As Integer = 0 To filterMat.Length - 1
                filterMat(i) /= matSum
            Next i

            ' using unmanaged arrays gives a speedup about 50%...
            Dim pFilterMat() As Double = filterMat
            Dim pInput() As Double = input
            Dim pOutput() As Double = output

            ' apply filter to input
            Dim filterStride As Integer = Convert.ToInt32(CInt(Fix(Math.Floor(filter.Length / 2.0))))
            Dim mSize As Integer = Me.Size

            For y1 As Integer = 0 To mSize - filter.Length - 1
                For x As Integer = 0 To mSize - filter.Length - 1
                    Dim pixValue As Double = 0

                    Dim iy As Integer = 0
                    Dim ifilter As Integer = 0
                    Do While iy < filter.Length
                        For ix As Integer = 0 To filter.Length - 1
                            pixValue += pInput((x + ix) + (y1 + iy) * mSize) * pFilterMat(ifilter)
                            ifilter += 1
                        Next ix
                        iy += 1
                    Loop

                    Dim Index As Integer = (x + filterStride) + (y1 + filterStride) * mSize
                    pOutput(Index) = pixValue
                Next x
            Next y1
        End Sub
    End Class
End Namespace
