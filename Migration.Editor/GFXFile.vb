Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Threading
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text

Imports System.Drawing

Imports System.ComponentModel
Imports System.Runtime.Serialization

Imports Migration

Namespace Migration.Editor
    <Serializable()> _
    Public Enum GFXSequenceType As Integer
        Landscape = 1
        GUI = 2
        Objects = 3
        Torso = 4
        Shadow = 5
    End Enum

    <Serializable()> _
    Public Class GFXSequence
        Private m_Frames As New List(Of GFXFrame)()

        Private privateIndex As Int32
        Public Property Index() As Int32
            Get
                Return privateIndex
            End Get
            Private Set(ByVal value As Int32)
                privateIndex = value
            End Set
        End Property
        Private privateType As GFXSequenceType
        Public Property Type() As GFXSequenceType
            Get
                Return privateType
            End Get
            Private Set(ByVal value As GFXSequenceType)
                privateType = value
            End Set
        End Property
        Private privateFile As GFXFile
        Public Property File() As GFXFile
            Get
                Return privateFile
            End Get
            Private Set(ByVal value As GFXFile)
                privateFile = value
            End Set
        End Property
        Public ReadOnly Property Frames() As List(Of GFXFrame)
            Get
                Return m_Frames
            End Get
        End Property
        Public ReadOnly Property Image() As Bitmap
            Get
                If m_Frames.Count = 0 Then
                    Return Nothing
                End If

                Return m_Frames(0).Image
            End Get
        End Property

        Public Sub New(ByVal inFile As GFXFile, ByVal inType As GFXSequenceType, ByVal inIndex As Integer)
            If inFile Is Nothing Then
                Throw New ArgumentNullException()
            End If

            File = inFile
            Type = inType
            Index = inIndex
        End Sub

        Public Sub AddFrame(ByVal inFrame As GFXFrame)
            m_Frames.Add(inFrame)
        End Sub
    End Class

    <Serializable()> _
    Public Class GFXFrame
        Private privateSequence As GFXSequence
        Public Property Sequence() As GFXSequence
            Get
                Return privateSequence
            End Get
            Private Set(ByVal value As GFXSequence)
                privateSequence = value
            End Set
        End Property
        Private privateWidth As Int32
        Public Property Width() As Int32
            Get
                Return privateWidth
            End Get
            Private Set(ByVal value As Int32)
                privateWidth = value
            End Set
        End Property
        Private privateHeight As Int32
        Public Property Height() As Int32
            Get
                Return privateHeight
            End Get
            Private Set(ByVal value As Int32)
                privateHeight = value
            End Set
        End Property
        Private privateImage As Bitmap
        Public Property Image() As Bitmap
            Get
                Return privateImage
            End Get
            Private Set(ByVal value As Bitmap)
                privateImage = value
            End Set
        End Property
        Private privateOffsetX As Int32
        Public Property OffsetX() As Int32
            Get
                Return privateOffsetX
            End Get
            Set(ByVal value As Int32)
                privateOffsetX = value
            End Set
        End Property
        Private privateOffsetY As Int32
        Public Property OffsetY() As Int32
            Get
                Return privateOffsetY
            End Get
            Set(ByVal value As Int32)
                privateOffsetY = value
            End Set
        End Property
        Private privateIndex As Int32
        Public Property Index() As Int32
            Get
                Return privateIndex
            End Get
            Private Set(ByVal value As Int32)
                privateIndex = value
            End Set
        End Property

        Private Sub New()
        End Sub

        Public Shared Function FromImage(ByVal inImage As System.Drawing.Image, ByVal inWidth As Integer, ByVal inHeight As Integer, ByVal inSequence As GFXSequence, ByVal inFrameIndex As Integer) As GFXFrame
            Dim result As New GFXFrame() With {.Width = inWidth, .Height = inHeight, .Sequence = inSequence, .Index = inFrameIndex, .Image = New Bitmap(inWidth, inHeight, PixelFormat.Format32bppArgb)}

            Graphics.FromImage(result.Image).DrawImageUnscaledAndClipped(inImage, New System.Drawing.Rectangle(0, 0, inWidth, inHeight))

            Return result
        End Function
    End Class


    <Serializable()> _
    Public Class GFXFile
        Implements IDisposable

        Private ReadOnly m_LandscapeSeqs As New List(Of GFXSequence)()
        Private ReadOnly m_GUISeqs As New List(Of GFXSequence)()
        Private ReadOnly m_ObjectSeqs As New List(Of GFXSequence)()
        Private ReadOnly m_TorsoSeqs As New List(Of GFXSequence)()
        Private ReadOnly m_ShadowSeqs As New List(Of GFXSequence)()

        <NonSerialized()> _
        Private m_LoaderThread As Thread
        <NonSerialized()> _
        Private m_PixBuffer As Bitmap
        <NonSerialized()> _
        Private m_Stream As FileStream
        <NonSerialized()> _
        Private m_Reader As BinaryReader
        <NonSerialized()> _
        Private m_Lock As New Object()
        <NonSerialized()> _
        Private m_Graphics As Graphics

        Private privateChecksum As Int32
        Public Property Checksum() As Int32
            Get
                Return privateChecksum
            End Get
            Private Set(ByVal value As Int32)
                privateChecksum = value
            End Set
        End Property

        Public ReadOnly Property LandscapeSeqs() As List(Of GFXSequence)
            Get
                Return m_LandscapeSeqs
            End Get
        End Property
        Public ReadOnly Property GUISeqs() As List(Of GFXSequence)
            Get
                Return m_GUISeqs
            End Get
        End Property
        Public ReadOnly Property ObjectSeqs() As List(Of GFXSequence)
            Get
                Return m_ObjectSeqs
            End Get
        End Property
        Public ReadOnly Property TorsoSeqs() As List(Of GFXSequence)
            Get
                Return m_TorsoSeqs
            End Get
        End Property
        Public ReadOnly Property ShadowSeqs() As List(Of GFXSequence)
            Get
                Return m_ShadowSeqs
            End Get
        End Property

        Private privateFileName As String
        Public Property FileName() As String
            Get
                Return privateFileName
            End Get
            Private Set(ByVal value As String)
                privateFileName = value
            End Set
        End Property
        Private privateIsLoaded As Boolean
        Public Property IsLoaded() As Boolean
            Get
                Return privateIsLoaded
            End Get
            Private Set(ByVal value As Boolean)
                privateIsLoaded = value
            End Set
        End Property
        Private privateProgress As Double
        Public Property Progress() As Double
            Get
                Return privateProgress
            End Get
            Private Set(ByVal value As Double)
                privateProgress = value
            End Set
        End Property

        Public Sub CancelLoad()
            Dim thread = m_LoaderThread

            If thread IsNot Nothing Then
                thread.Abort()
            End If
        End Sub

        Public Sub WaitForLoad()
            Dim thread = m_LoaderThread

            If thread IsNot Nothing Then
                thread.Join()
            End If
        End Sub

        Public Sub New()
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Reset()
        End Sub

        Public Sub Reset()
            FileName = Nothing
            SyncLock m_LandscapeSeqs
                m_LandscapeSeqs.Clear()
            End SyncLock
            SyncLock m_LandscapeSeqs
                m_GUISeqs.Clear()
            End SyncLock
            SyncLock m_LandscapeSeqs
                m_ObjectSeqs.Clear()
            End SyncLock
            SyncLock m_LandscapeSeqs
                m_TorsoSeqs.Clear()
            End SyncLock
            SyncLock m_LandscapeSeqs
                m_ShadowSeqs.Clear()
            End SyncLock
        End Sub

        Public Sub Save()
            Dim stream As FileStream = File.OpenWrite(FileName & ".cache")
            Dim format As New BinaryFormatter()

            Using stream
                format.Serialize(stream, Me)
            End Using
        End Sub

        Public Sub BeginLoad(ByVal inFilename As String)
            ' look for cached extraction
            ' parse original mFile
            ' cache extraction for next load
            Dim thread As New Thread(Sub()
                                         Try
                                             Dim format As New BinaryFormatter()
                                             Dim stream As FileStream = Nothing
                                             Try
                                                 If File.Exists(inFilename & ".cache") Then
                                                     Dim mFile As GFXFile = Nothing
                                                     stream = File.OpenRead(inFilename & ".cache")
                                                     Using stream
                                                         mFile = CType(format.Deserialize(stream), GFXFile)
                                                     End Using
                                                     SyncLock m_LandscapeSeqs
                                                         Me.m_GUISeqs.AddRange(mFile.m_GUISeqs)
                                                     End SyncLock
                                                     SyncLock m_LandscapeSeqs
                                                         Me.m_LandscapeSeqs.AddRange(mFile.m_LandscapeSeqs)
                                                     End SyncLock
                                                     SyncLock m_LandscapeSeqs
                                                         Me.m_ObjectSeqs.AddRange(mFile.m_ObjectSeqs)
                                                     End SyncLock
                                                     SyncLock m_LandscapeSeqs
                                                         Me.m_ShadowSeqs.AddRange(mFile.m_ShadowSeqs)
                                                     End SyncLock
                                                     SyncLock m_LandscapeSeqs
                                                         Me.m_TorsoSeqs.AddRange(mFile.m_TorsoSeqs)
                                                     End SyncLock
                                                     IsLoaded = True
                                                     Return
                                                 End If
                                             Catch
                                             End Try
                                             InternalLoad(inFilename)
                                             stream = File.OpenWrite(inFilename & ".cache")
                                             Using stream
                                                 format.Serialize(stream, Me)
                                             End Using
                                         Catch e1 As ThreadAbortException
                                         Catch e As Exception
                                             Log.LogExceptionModal(e)
                                         Finally
                                             FileName = inFilename
                                             If Not IsLoaded Then
                                                 Reset()
                                             End If
                                             m_LoaderThread = Nothing
                                             Progress = 0
                                         End Try
                                     End Sub)

            SyncLock m_Lock
                If m_LoaderThread IsNot Nothing Then
                    Throw New InvalidOperationException("Simultaneous loads are not permitted!")
                End If

                Reset()

                IsLoaded = False
                Progress = 0

                m_PixBuffer = New Bitmap(1024, 1204, PixelFormat.Format32bppArgb)
                m_Graphics = Graphics.FromImage(m_PixBuffer)
                m_Stream = File.OpenRead(inFilename)
                m_Reader = New BinaryReader(m_Stream)

                m_LoaderThread = thread
                m_LoaderThread.IsBackground = True
                m_LoaderThread.Start()
            End SyncLock
        End Sub

        Private Sub InternalLoad(ByVal inFileName As String)
            Try
                ' compute checksum
                Dim hash() As Byte = System.Security.Cryptography.MD5.Create().ComputeHash(m_Stream)

                For i As Integer = 0 To 3
                    Checksum = Checksum Or ((Convert.ToInt32(hash(i)))) << (i * 8)
                Next i

                Dim fileSize As Integer = ReadDWordAt(48)

                If fileSize <> m_Stream.Length Then
                    Throw New InvalidDataException("File size read from file does not match actual file size.")
                End If

                ' read frame counts of sequences and their addresses
                Dim offs As Integer = 52
                Dim seqAddresses(5) As Integer
                Dim seqCounts(5) As Integer
                Dim seqCount As Integer = 0

                For i As Integer = 0 To 5
                    seqAddresses(i) = ReadDWordAndIncrement(offs) + 6
                    Dim check As Integer = seqAddresses(i)

                    If (check < 0) OrElse (check > fileSize) Then
                        Throw New InvalidDataException("Address of """ & check & """ is out of bounds.")
                    End If
                Next i

                For i As Integer = 0 To 5
                    seqCounts(i) = ReadWordAt(seqAddresses(i))
                    Dim check As Integer = seqCounts(i)

                    If (check < 0) OrElse (check > 1000) Then
                        Throw New InvalidDataException("Sequence count of """ & check & """ is not sane.")
                    End If

                    seqCount += check
                Next i

                For i As Integer = 0 To 5
                    seqAddresses(i) += 2

                Next i

                If Not (seqCounts.Any(Function(e) e > 0)) Then
                    Throw New InvalidDataException("File does not seem to contain anything useful.")
                End If

                ' process all sequences and their frames
                Dim progressStep As Double = 1.0 / seqCount

                For iType As Integer = 1 To 5
                    Dim type As GFXSequenceType = CType(iType, GFXSequenceType)

                    For iSeq As Integer = 0 To seqCounts(iType) - 1
                        Dim seqStart As Integer = ReadDWordAt(seqAddresses(iType) + iSeq * 4)
                        Dim sequence As New GFXSequence(Me, type, iSeq)

                        If (type = GFXSequenceType.GUI) OrElse (type = GFXSequenceType.Landscape) Then
                            ' no sequence info, starts right off with frames... (interpreted as sequence with one frame)
                            Dim frame As GFXFrame = ReadFrame(seqStart, sequence, 0)

                            If frame IsNot Nothing Then
                                sequence.AddFrame(frame)
                            End If
                        Else
                            ' read sequence of frames
                            'INSTANT C# TODO TASK: There is no C# equivalent to VB's implicit 'once only' variable initialization within loops:
                            Dim byteX As Integer = 0
                            'INSTANT C# TODO TASK: There is no C# equivalent to VB's implicit 'once only' variable initialization within loops:
                            Dim relOffsTableStart As Integer = 0
                            'INSTANT C# TODO TASK: There is no C# equivalent to VB's implicit 'once only' variable initialization within loops:
                            Dim frameCount As Integer = 0

                            ReadSequence(seqStart, byteX, frameCount, relOffsTableStart)

                            For iFrame As Integer = 0 To frameCount - 1
                                Dim picStart As Integer = ReadDWordAt(relOffsTableStart + 4 * iFrame) + byteX
                                Dim frame As GFXFrame = ReadFrame(picStart, sequence, iFrame)

                                If frame IsNot Nothing Then
                                    sequence.AddFrame(frame)
                                End If
                            Next iFrame
                        End If

                        If sequence.Frames.Count > 0 Then
                            Select Case type
                                Case GFXSequenceType.Landscape
                                    SyncLock m_LandscapeSeqs
                                        m_LandscapeSeqs.Add(sequence)
                                    End SyncLock
                                Case GFXSequenceType.GUI
                                    SyncLock m_GUISeqs
                                        m_GUISeqs.Add(sequence)
                                        Exit Select
                                    End SyncLock
                                Case GFXSequenceType.Objects
                                    SyncLock m_ObjectSeqs
                                        m_ObjectSeqs.Add(sequence)
                                    End SyncLock
                                Case GFXSequenceType.Torso
                                    SyncLock m_TorsoSeqs
                                        m_TorsoSeqs.Add(sequence)
                                    End SyncLock
                                Case GFXSequenceType.Shadow
                                    SyncLock m_ShadowSeqs
                                        m_ShadowSeqs.Add(sequence)
                                    End SyncLock
                            End Select
                        End If

                        Progress += progressStep
                    Next iSeq
                Next iType

                IsLoaded = True
            Finally
                If Not IsLoaded Then
                    Reset()
                End If
            End Try
        End Sub

        Private Function ReadDWordAt(ByVal inPosition As Integer) As Integer
            m_Reader.BaseStream.Position = inPosition
            Return m_Reader.ReadInt32()
        End Function

        Private Function ReadDWordAndIncrement(ByRef refPosition As Integer) As Integer
            m_Reader.BaseStream.Position = refPosition
            Dim result = m_Reader.ReadInt32()
            refPosition = Convert.ToInt32(m_Reader.BaseStream.Position)

            Return result
        End Function

        Private Function ReadWordAt(ByVal inPosition As Integer) As Integer
            m_Reader.BaseStream.Position = inPosition
            Return m_Reader.ReadInt16()
        End Function

        Private Function ReadWordAndIncrement(ByRef refPosition As Integer) As Integer
            m_Reader.BaseStream.Position = refPosition
            Dim result = m_Reader.ReadInt16()
            refPosition = Convert.ToInt32(m_Reader.BaseStream.Position)

            Return result
        End Function

        Private Function ReadSignedByteAt(ByVal inPosition As Integer) As Integer
            m_Reader.BaseStream.Position = inPosition
            Return m_Reader.ReadSByte()
        End Function

        Private Function ReadSignedByteAndIncrement(ByRef refPosition As Integer) As Integer
            m_Reader.BaseStream.Position = refPosition
            Dim result = m_Reader.ReadSByte()
            refPosition = Convert.ToInt32(m_Reader.BaseStream.Position)

            Return result
        End Function

        Private Function ReadUnsignedByteAt(ByVal inPosition As Integer) As Integer
            m_Reader.BaseStream.Position = inPosition
            Return m_Reader.ReadByte()
        End Function

        Private Function ReadUnsignedByteAndIncrement(ByRef refPosition As Integer) As Integer
            m_Reader.BaseStream.Position = refPosition
            Dim result = m_Reader.ReadByte()
            refPosition = Convert.ToInt32(m_Reader.BaseStream.Position)

            Return result
        End Function

        Private Sub ReadSequence(ByVal inSeqHeader As Integer, ByRef outByteX As Integer, ByRef outFrameCount As Integer, ByRef outRelOffsTableStart As Integer)
            Dim offs As Integer = inSeqHeader

            ' some magic
            Dim code As Integer = ReadWordAndIncrement(offs)
            Dim word As Integer = ReadWordAndIncrement(offs)
            Dim b As Integer = ReadUnsignedByteAndIncrement(offs)
            Dim word2 As Integer = ReadWordAndIncrement(offs)

            If (code <> 5122) OrElse (word <> 0) OrElse (b <> 8) OrElse (word2 <> 0) Then
                Throw New InvalidDataException()
            End If

            outByteX = inSeqHeader + 4
            outFrameCount = ReadUnsignedByteAndIncrement(offs)

            outRelOffsTableStart = offs
        End Sub

        ''' <summary>
        ''' Converts a color given in RGB565 into RGB888.
        ''' </summary>
        Private Function FromRgb565(ByVal inValue As Integer) As Color
            Dim blue As Integer = (inValue And &HF800) >> 11
            Dim red As Integer = (inValue And &H1F)
            Dim green As Integer = (inValue And &H7E0) >> 5

            Return Color.FromArgb(255, Convert.ToInt32(blue * (255.0 / (1 << 5))), Convert.ToInt32(green * (255.0 / (1 << 6))), Convert.ToInt32(red * (255.0 / (1 << 5))))
        End Function

        Private Function ReadFrame(ByVal inOffset As Integer, ByVal inSequence As GFXSequence, ByVal inFrameIndex As Integer) As GFXFrame
            m_Graphics.Clear(Color.FromArgb(0, 0, 0, 0))

            Dim startOffs As Integer = inOffset
            Dim width As Integer = ReadWordAndIncrement(inOffset)
            Dim height As Integer = ReadWordAndIncrement(inOffset)
            Dim xrel As Integer = 0
            Dim yrel As Integer = 0
            Dim y As Integer = 0
            Dim tokoffs As Integer = 0
            Dim x As Integer = 0
            Dim iPixel As Integer = 0

            Select Case inSequence.Type
                Case GFXSequenceType.Landscape
                    ' magic
                    ReadUnsignedByteAt(inOffset)
                    ReadUnsignedByteAt(inOffset + 1)
                    ReadWordAndIncrement(inOffset)
                Case GFXSequenceType.Torso
                    xrel = ReadWordAndIncrement(inOffset)
                    yrel = ReadWordAndIncrement(inOffset)
                Case Else
                    xrel = ReadWordAndIncrement(inOffset)
                    yrel = ReadWordAndIncrement(inOffset)

                    ReadUnsignedByteAndIncrement(inOffset) ' magic zero

                    If ReadUnsignedByteAt(inOffset) = 0 Then
                        ' Hack! Ignore first zero if following byte is not (-128).
                        If ReadSignedByteAt(inOffset + 1) <> -128 Then
                            ReadUnsignedByteAndIncrement(inOffset)
                        End If
                    End If
            End Select

            Dim token As Integer = 0
            Do While y < height
                Dim pixelCount As Integer = ReadUnsignedByteAndIncrement(inOffset)
                Dim dist As Integer = ReadSignedByteAndIncrement(inOffset) ' distance from last token or left image border

                If dist < 0 Then
                    tokoffs = dist + 128
                Else
                    tokoffs = dist
                End If

                x += tokoffs

                If y >= height Then
                    Throw New InvalidDataException()
                End If

                ' read pixels
                For iPixel = 0 To pixelCount - 1
                    If x >= width Then
                        Throw New InvalidDataException()
                    End If

                    Select Case inSequence.Type
                        Case GFXSequenceType.Torso
                            ' hue of player color
                            Dim pix = ReadUnsignedByteAndIncrement(inOffset)
                            m_PixBuffer.SetPixel(x, y, Color.FromArgb(Math.Max(0, Math.Min(pix << 3, 150)), 0, 0))
                            Exit Select
                        Case GFXSequenceType.Shadow
                            ' monochrome shadow
                            m_PixBuffer.SetPixel(x, y, Color.Black)
                            Exit Select
                        Case Else
                            ' colored pixels
                            Dim pix = ReadWordAndIncrement(inOffset)
                            m_PixBuffer.SetPixel(x, y, FromRgb565(pix))
                            Exit Select
                    End Select

                    x += 1
                Next iPixel

                ' reached end of pixel line?
                If dist < 0 Then
                    y += 1
                    x = 0
                End If
                token += 1
            Loop

            If (width = 0) OrElse (height = 0) Then
                Return Nothing
            End If

            Dim result As GFXFrame = GFXFrame.FromImage(m_PixBuffer, width, height, inSequence, inFrameIndex)

            result.OffsetX = xrel
            result.OffsetY = yrel

            Return result
        End Function
    End Class
End Namespace
