Imports Migration.Common



#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else
Imports OpenTK.Graphics.OpenGL
#End If

Namespace Migration.Rendering
    Public Class TerrainMesh
        Implements IDisposable

        Private m_VertexBufID As Integer = -1
        Private m_IndexBufID As Integer = -1
        Private m_Parent As TerrainRenderer
        Private m_IsBlockValid(,) As Boolean
        Private m_VertexBlock(FloatsPerBlock - 1) As Single

        Public Const BlockSize As Int32 = 16
        Private Const FloatsPerBlock As Int32 = BlockSize * BlockSize * 8
        Private Const IndicesPerBlock As Int32 = BlockSize * BlockSize * 6
        Private ReadOnly IndicesPerLine As Int32
        Private ReadOnly BlocksPerLine As Int32

        Private privateSize As Int32
        Public Property Size() As Int32
            Get
                Return privateSize
            End Get
            Private Set(ByVal value As Int32)
                privateSize = value
            End Set
        End Property

        Private privateHeightMap As NativeTexture
        Public Property HeightMap() As NativeTexture
            Get
                Return privateHeightMap
            End Get
            Private Set(ByVal value As NativeTexture)
                privateHeightMap = value
            End Set
        End Property

        Public Sub New(ByVal inRenderer As TerrainRenderer)
            If inRenderer Is Nothing Then
                Throw New ArgumentNullException()
            End If

            Size = inRenderer.Size
            m_Parent = inRenderer
            BlocksPerLine = (Size \ BlockSize)
            IndicesPerLine = IndicesPerBlock * BlocksPerLine
            m_IsBlockValid = New Boolean(BlocksPerLine - 1, BlocksPerLine - 1) {}

            If (Size Mod BlockSize) <> 0 Then
                Throw New ArgumentOutOfRangeException("Terrain size is not a multiple of mesh block size.")
            End If

            GL.GenBuffers(1, m_VertexBufID)
            Renderer.CheckError()
            GL.GenBuffers(1, m_IndexBufID)
            Renderer.CheckError()

            ' generate height grid
            Dim vertices(Size * Size * 8 - 1) As Single
            Dim vertexIndices(Size - 1, Size - 1) As Integer
            Dim offset As Integer = 0

            Dim yBlock As Integer = 0
            Dim index As Integer = 0
            Do While yBlock < BlocksPerLine
                For xBlock As Integer = 0 To BlocksPerLine - 1
                    For y As Integer = yBlock * BlockSize To (yBlock + 1) * BlockSize - 1
                        For x As Integer = xBlock * BlockSize To (xBlock + 1) * BlockSize - 1
                            vertexIndices(x, y) = index
                            index += 1

                            '                            
                            '                             * Currently the normals are used for additional information like rivers, snow, etc. 
                            '                             
                            offset += 3

                            vertices(offset) = 0.0F
                            offset += 1
                            vertices(offset) = 0.0F
                            offset += 1
                            vertices(offset) = x
                            offset += 1
                            vertices(offset) = y
                            offset += 1
                            vertices(offset) = 0.0F
                            offset += 1
                        Next x
                    Next y
                Next xBlock
                yBlock += 1
            Loop

            ' generate indices for full terrain
            Dim indices(IndicesPerLine * BlocksPerLine - 1) As Integer
            offset = 0

            For yBlock1 As Integer = 0 To BlocksPerLine - 1
                For xBlock As Integer = 0 To BlocksPerLine - 1
                    For y As Integer = yBlock1 * BlockSize To (yBlock1 + 1) * BlockSize - 1
                        Dim iy As Integer = y

                        If y >= Size - 1 Then
                            iy = Size - 2
                        End If

                        For x As Integer = xBlock * BlockSize To (xBlock + 1) * BlockSize - 1
                            Dim ix As Integer = x

                            If x >= Size - 1 Then
                                ix = Size - 2
                            End If

                            indices(offset) = vertexIndices(ix, iy)
                            offset += 1
                            indices(offset) = vertexIndices(ix + 1, iy)
                            offset += 1
                            indices(offset) = vertexIndices(ix + 1, iy + 1)
                            offset += 1

                            indices(offset) = vertexIndices(ix, iy)
                            offset += 1
                            indices(offset) = vertexIndices(ix + 1, iy + 1)
                            offset += 1
                            indices(offset) = vertexIndices(ix, iy + 1)
                            offset += 1
                        Next x
                    Next y
                Next xBlock
            Next yBlock1


            If offset <> indices.Length Then
                Throw New ApplicationException()
            End If

            GL.BindBuffer(BufferTarget.ArrayBuffer, m_VertexBufID)
            Renderer.CheckError()
            GL.BufferData(BufferTarget.ArrayBuffer, New IntPtr(vertices.Length * 4), vertices, BufferUsageHint.DynamicDraw)
            Renderer.CheckError()

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_IndexBufID)
            Renderer.CheckError()
            GL.BufferData(BufferTarget.ElementArrayBuffer, New IntPtr(indices.Length * 4), indices, BufferUsageHint.StaticDraw)
            Renderer.CheckError()

            HeightMap = New NativeTexture(TextureOptions.None, Size, Size, New Integer(Size * Size - 1) {})

            ' listen to terrain changes
            AddHandler m_Parent.Terrain.OnCellChanged, AddressOf CellChanged
        End Sub

        Private Sub CellChanged(ByVal cell As Point)
            InvalidateCell(cell.X, cell.Y)
        End Sub

        Protected Overrides Sub Finalize()
            'if ((m_IndexBufID != -1) || (m_VertexBufID != -1))
            '    throw new ApplicationException("Terrain mesh was not released before GC.");
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If m_VertexBufID <> -1 Then
                GL.DeleteBuffers(1, m_VertexBufID)
            End If

            If m_IndexBufID <> -1 Then
                GL.DeleteBuffers(1, m_IndexBufID)
            End If

            m_VertexBufID = -1
            m_IndexBufID = -1
        End Sub

        ''' <summary>
        ''' Does a fast and accurate occlusion query for visible terrain cells.
        ''' </summary>
        ''' <returns></returns>
        Public Function ComputeOcclusion() As Rectangle
            GL.ClearColor(Color.White)
            GL.Clear(ClearBufferMask.ColorBufferBit Or ClearBufferMask.DepthBufferBit)

            For y As Integer = 0 To Size Step BlockSize
                For x As Integer = 0 To Size Step BlockSize
                    GL.Begin(BeginMode.Quads)
                    Dim r As Single = x / 1024.0F
                    Dim g As Single = y / 1024.0F

                    GL.Color3(r, g, 0)
                    GL.Vertex3(x, y, 0.0)
                    GL.Color3(r, g, 0)
                    GL.Vertex3(x + BlockSize, y, 0.0)
                    GL.Color3(r, g, 0)
                    GL.Vertex3(x + BlockSize, y + BlockSize, 0.0)
                    GL.Color3(r, g, 0)
                    GL.Vertex3(x, y + BlockSize, 0.0)
                    GL.End()
                Next x
            Next y

            ' detect frustum
            Dim buffer(0) As Integer
            Dim leftTop As New Point()
            Dim rightTop As New Point()
            Dim leftBottom As New Point()
            Dim rightBottom As New Point()

            GL.ReadPixels(0, 0, 1, 1, PixelFormat.Rgb, PixelType.UnsignedByte, buffer)
            leftBottom = PixelToGrid(buffer(0))
            GL.ReadPixels(m_Parent.Renderer.ViewportWidth - 1, 0, 1, 1, PixelFormat.Rgb, PixelType.UnsignedByte, buffer)
            rightBottom = PixelToGrid(buffer(0))
            GL.ReadPixels(0, m_Parent.Renderer.ViewportHeight - 1, 1, 1, PixelFormat.Rgb, PixelType.UnsignedByte, buffer)
            leftTop = PixelToGrid(buffer(0))
            GL.ReadPixels(m_Parent.Renderer.ViewportWidth - 1, m_Parent.Renderer.ViewportHeight - 1, 1, 1, PixelFormat.Rgb, PixelType.UnsignedByte, buffer)
            rightTop = PixelToGrid(buffer(0))

            Dim minX As Integer = 0
            Dim maxX As Integer = Size
            Dim minY As Integer = 0
            Dim maxY As Integer = Size

            If leftTop.X >= 0 Then
                minX = leftTop.X
                minY = leftTop.Y
            End If

            If rightTop.X >= 0 Then
                maxX = rightTop.X
                minY = rightTop.Y
            End If

            If leftBottom.X >= 0 Then
                minX = leftBottom.X
                maxY = leftBottom.Y
            End If

            If rightBottom.X >= 0 Then
                maxX = rightBottom.X
                maxY = rightBottom.Y
            End If


            If Not Game.Setup.Language.UseMinimalBounds Then
                minX = Math.Max(0, minX - BlockSize)
                minY = Math.Max(0, minY - BlockSize)
                maxX = Math.Min(Size, maxX + BlockSize)
                maxY = Math.Min(Size, maxY + BlockSize)
            End If

            If (leftTop.X < 0) AndAlso (rightTop.X < 0) AndAlso (leftBottom.X < 0) AndAlso (rightBottom.X < 0) Then
                Return New Rectangle(0, 0, 0, 0)
            Else
                Return New Rectangle(minX, minY, maxX - minX + BlockSize, maxY - minY + BlockSize)
            End If
        End Function

        Private Function PixelToGrid(ByVal inPixel As Integer) As Point
            If (inPixel And &HFF0000) <> 0 Then
                Return New Point(-1, -1)
            Else
                Return New Point((inPixel And &HFF) * 4, ((inPixel And &HFF00) >> 8) * 4)
            End If
        End Function

        ''' <summary>
        ''' Renders all blocks necessary to overlay the given rectangle of terrain cells.
        ''' </summary>
        ''' <param name="inContainedCells"></param>
        Public Sub RenderBlocks(ByVal inContainedCells As Rectangle, ByVal inCanUpdateBlocks As Boolean)
            Dim blocks As New Rectangle(Math.Max(0, Math.Min(inContainedCells.X \ BlockSize, BlocksPerLine - 1)), Math.Max(0, Math.Min(inContainedCells.Y \ BlockSize, BlocksPerLine - 1)), Math.Max(0, Math.Min((inContainedCells.X + inContainedCells.Width + BlockSize - 1) \ BlockSize, BlocksPerLine)), Math.Max(0, Math.Min((inContainedCells.Y + inContainedCells.Height + BlockSize - 1) \ BlockSize, BlocksPerLine)))

            blocks.Width = Math.Max(0, blocks.Width - blocks.X)
            blocks.Height = Math.Max(0, blocks.Height - blocks.Y)

            GL.BindBuffer(BufferTarget.ArrayBuffer, m_VertexBufID)
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_IndexBufID)
            GL.InterleavedArrays(InterleavedArrayFormat.T2fN3fV3f, 0, IntPtr.Zero)
            Renderer.CheckError()

            '            
            '             * Blocks are aligned horizontally within the index buffer. Thus we can render all required blocks
            '             * on the same block line in one draw call.
            '             
            Dim yBlock As Integer = blocks.Y
            Dim offset As Integer = IndicesPerLine * blocks.Y
            Do While yBlock < blocks.Y + blocks.Height
                Dim xBlock As Integer = blocks.X
                Dim xCount As Integer = blocks.Width

                If inCanUpdateBlocks Then
                    For x As Integer = xBlock To xBlock + xCount - 1
                        ValidateBlock(x, yBlock)
                    Next x
                End If

                GL.DrawRangeElements(BeginMode.Triangles, 0, IndicesPerLine * BlocksPerLine, xCount * IndicesPerBlock, DrawElementsType.UnsignedInt, CType((offset + xBlock * IndicesPerBlock) * 4, IntPtr))
#If DEBUG Then
                Renderer.CheckError()
#End If
                yBlock += 1
                offset += IndicesPerLine
            Loop
        End Sub

        ''' <summary>
        ''' Marks a given terrain cells as invalid, resulting in the underlying vertex
        ''' block being updated on next rendering. This is a very fast operation!
        ''' </summary>
        Public Sub InvalidateCell(ByVal inCellX As Integer, ByVal inCellY As Integer)
            '            
            '             * Doesn't need to be thread-safe since it doesn't matter if terrain gets updated
            '             * one frame sooner or later...
            '             
            Dim xBlock As Integer = inCellX \ BlockSize
            Dim yBlock As Integer = inCellY \ BlockSize

            If (xBlock >= BlocksPerLine) OrElse (yBlock >= BlocksPerLine) Then
                Return
            End If

            m_IsBlockValid(xBlock, yBlock) = False
        End Sub

        ''' <summary>
        ''' Ensures that the given block is up to date and if not, fetches all required data from
        ''' the renderer and propagates changes to GPU memory.
        ''' </summary>
        Private Sub ValidateBlock(ByVal xBlock As Integer, ByVal yBlock As Integer)
            If (xBlock >= BlocksPerLine) OrElse (yBlock >= BlocksPerLine) Then
                Return
            End If

            If m_IsBlockValid(xBlock, yBlock) Then
                Return
            End If

            Dim terrain As TerrainDefinition = m_Parent.Terrain
            Dim newHeights(BlockSize * BlockSize - 1) As Integer

            Dim y As Integer = yBlock * BlockSize
            Dim offset As Integer = 0
            Dim hOffset As Integer = 0
            Do While y < (yBlock + 1) * BlockSize
                For x As Integer = xBlock * BlockSize To (xBlock + 1) * BlockSize - 1
                    offset += 3

                    Dim height As Integer = terrain.GetHeightAt(x, y)

                    m_VertexBlock(offset) = -1.0F + terrain.GetGroundAt(x, y) / 128.0F
                    offset += 1
                    m_VertexBlock(offset) = If((terrain.GetFlagsAt(x, y) And TerrainCellFlags.Grading) <> 0, 1, 0)
                    offset += 1
                    m_VertexBlock(offset) = x
                    offset += 1
                    m_VertexBlock(offset) = y
                    offset += 1
                    m_VertexBlock(offset) = (-1.0F + height / 128.0F) / 2.0F
                    offset += 1
                    newHeights(hOffset) = height Or (height << 8) Or (height << 16) Or (height << 24)
                    hOffset += 1
                Next x
                y += 1
            Loop

            HeightMap.SetPixels(xBlock * BlockSize, yBlock * BlockSize, BlockSize, BlockSize, newHeights)

            GL.BufferSubData(BufferTarget.ArrayBuffer, CType(4 * FloatsPerBlock * (yBlock * BlocksPerLine + xBlock), IntPtr), CType(4 * FloatsPerBlock, IntPtr), m_VertexBlock)

#If DEBUG Then
            Renderer.CheckError()
#End If

            m_IsBlockValid(xBlock, yBlock) = True
        End Sub
    End Class
End Namespace
