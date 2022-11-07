Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO
Imports Migration.Common

Namespace Migration

    Public Class AnimationLibrary

        Private m_Classes As UniqueMap(Of String, Character)
        Private m_AudioObjects As UniqueMap(Of String, AudioObject)
        Public Const MaxAnimPerSet As Int32 = 10

        Private Shared m_Libraries As New List(Of AnimationLibrary)()
        Public Shared ReadOnly Property Libraries() As ReadOnlyCollection(Of AnimationLibrary)
            Get
                Return m_Libraries.AsReadOnly()
            End Get
        End Property

        ' contains the most recently loaded library reference
        Private Shared privateInstance As AnimationLibrary
        Public Shared Property Instance() As AnimationLibrary
            Get
                Return privateInstance
            End Get
            Private Set(ByVal value As AnimationLibrary)
                privateInstance = value
            End Set
        End Property

        Private m_Frames As UniqueMap(Of Int64, Byte())
        Public ReadOnly Property Classes() As BindingList(Of Character)
            Get
                Return m_Classes.GetValueBinding()
            End Get
        End Property

        Public ReadOnly Property AudioObjects() As BindingList(Of AudioObject)
            Get
                Return m_AudioObjects.GetValueBinding()
            End Get
        End Property

        Private privateDirectory As String
        Public Property Directory() As String
            Get
                Return privateDirectory
            End Get
            Private Set(ByVal value As String)
                privateDirectory = value
            End Set
        End Property

        Private privateSourceMode As LibraryMode
        Public Property SourceMode() As LibraryMode
            Get
                Return privateSourceMode
            End Get
            Private Set(ByVal value As LibraryMode)
                privateSourceMode = value
            End Set
        End Property

        Private privateAnimationSetCount As Int32
        Public Property AnimationSetCount() As Int32
            Get
                Return privateAnimationSetCount
            End Get
            Private Set(ByVal value As Int32)
                privateAnimationSetCount = value
            End Set
        End Property

        Private privateFrameCount As Int32
        Public Property FrameCount() As Int32
            Get
                Return privateFrameCount
            End Get
            Friend Set(ByVal value As Int32)
                privateFrameCount = value
            End Set
        End Property

        Private privateIsReadonly As Boolean
        Public Property IsReadonly() As Boolean
            Get
                Return privateIsReadonly
            End Get
            Private Set(ByVal value As Boolean)
                privateIsReadonly = value
            End Set
        End Property

        Private privateForcePositionShift As Boolean
        Public Property ForcePositionShift() As Boolean
            Get
                Return privateForcePositionShift
            End Get
            Set(ByVal value As Boolean)
                privateForcePositionShift = value
            End Set
        End Property

        Private privateChecksum As Int64
        Public Property Checksum() As Int64
            Get
                Return privateChecksum
            End Get
            Private Set(ByVal value As Int64)
                privateChecksum = value
            End Set
        End Property

        Private Sub New()
            m_Frames = New UniqueMap(Of Long, Byte())()
            m_Classes = New UniqueMap(Of String, Character)()
            m_AudioObjects = New UniqueMap(Of String, AudioObject)()
            SourceMode = LibraryMode.Filesystem
            IsReadonly = True
        End Sub

        ''' <summary>
        ''' Registers the library in a static list <see cref="AnimationLibrary.Libraries"/>. This will prevent
        ''' the library from being GCed. You are responsible to call <see cref="UnregisterAndAllowGC"/> when
        ''' you don't need this library anymore. You only need to register a library, when it should expose
        ''' shared animations.
        ''' </summary>
        Friend Sub RegisterAndPreventGC()
            If m_Libraries.Contains(Me) Then
                Throw New InvalidOperationException("This library is already registered!")
            End If

            m_Libraries.Add(Me)
        End Sub

        Friend Sub UnregisterAndAllowGC()
            m_Libraries.Remove(Me)
        End Sub

        Public Sub ForceWriteable()
            If IsReadonly Then
                Throw New InvalidOperationException("Library is read-only.")
            End If
        End Sub

        Public Sub Save()
            ForceWriteable()

            ' serialize library
            Dim target As Stream = File.OpenWrite(Directory & "\Animations.ali.tmp")
            Dim writer As New BinaryWriter(target)
            Dim gfxFiles As New List(Of String)()

            target.SetLength(0)

            Using target
                writer.Write(Convert.ToByte(1)) ' library type ID
                writer.Write(Convert.ToUInt16(&H1000)) ' library version

                ' serialize audio objects
                writer.Write(m_AudioObjects.Count)

                For Each audio As AudioObject In m_AudioObjects.Values
                    audio.Save(writer)
                Next audio

                ' serialize classes
                writer.Write(m_Classes.Count)

                For Each Character As Character In m_Classes.Values
                    Character.Save(writer)
                    gfxFiles.Add(Path.GetFullPath(Directory & "/" & Character.Name & ".gfx"))
                Next Character
            End Using

            File.Delete(Directory & "/Animations.ali")
            File.Move(Directory & "/Animations.ali.tmp", Directory & "/Animations.ali")

            ' cleanup unused gfx files
            gfxFiles.Add(Path.GetFullPath(Directory & "/Animations.gfx"))

            For Each mFile As String In System.IO.Directory.GetFiles(Directory, "*.gfx", SearchOption.TopDirectoryOnly)
                Dim fullPath As String = Path.GetFullPath(mFile)

                If Not (gfxFiles.Contains(fullPath)) Then
                    File.Delete(fullPath)
                End If
            Next mFile

            ' collect frames
            Dim newFrames As New UniqueMap(Of Long, Byte())()

            For Each audio As AudioObject In AudioObjects
                audio.Load()

                newFrames.Add(audio.Checksum, audio.m_AudioBytes)
            Next audio

            m_Frames = newFrames

            ' serialize frames
            File.Delete(Directory & "/Animations.gfx.tmp")

            target = File.OpenWrite(Directory & "/Animations.gfx.tmp")

            Using target
                writer = New BinaryWriter(target)

                writer.Write(m_Frames.Count)

                For Each entry As KeyValuePair(Of Int64, Byte()) In m_Frames
                    writer.Write(entry.Key)
                    writer.Write(entry.Value.Length)
                    writer.Write(entry.Value)
                Next entry
            End Using

            File.Delete(Directory & "/Animations.gfx")
            File.Move(Directory & "/Animations.gfx.tmp", Directory & "/Animations.gfx")
        End Sub

        Friend Shared Function Load(ByVal inReader As BinaryReader) As AnimationLibrary
            Dim result As New AnimationLibrary()

            result.IsReadonly = False

            Try
                If inReader.ReadByte() <> 1 Then
                    Throw New InvalidDataException()
                End If

                Dim version As UInt16 = inReader.ReadUInt16()
                Select Case version
                    Case &H1000
                        Dim i As Integer = 0
                        Dim count As Integer = inReader.ReadInt32()
                        Do While i < count
                            Dim audio As AudioObject = AudioObject.Load(result, inReader)

                            result.m_AudioObjects.Add(audio.Name, audio)
                            i += 1
                        Loop

                        Dim frameIndices As New SortedDictionary(Of Long, Integer)()

                        i = 0
                        count = inReader.ReadInt32()
                        Do While i < count
                            Dim Character As Character = Migration.Character.Load(result, inReader)

                            result.m_Classes.Add(Character.Name, Character)

                            For Each mSet As AnimationSet In Character.Sets
                                For Each anim As Animation In mSet.Animations
                                    For Each Frame As AnimationFrame In anim.Frames

                                        Dim index As Integer

                                        If frameIndices.TryGetValue(Frame.Checksum, index) Then
                                            Frame.Index = index
                                        Else
                                            Frame.Index = result.FrameCount
                                            result.FrameCount += 1

                                            frameIndices.Add(Frame.Checksum, Frame.Index)
                                        End If
                                    Next Frame
                                Next anim
                                mSet.Index = result.AnimationSetCount

                                result.AnimationSetCount += 1
                            Next mSet
                            i += 1
                        Loop

                    Case Else
                        Throw New InvalidDataException()
                End Select

                Return result
            Finally
                result.IsReadonly = True
            End Try
        End Function

        Public Shared Function OpenOrCreate(ByVal inRootDirectory As String) As AnimationLibrary
            Dim result As AnimationLibrary = Nothing

            If Not (System.IO.File.Exists(inRootDirectory & "\Animations.ali")) Then
                result = Create(inRootDirectory)
            Else
                result = OpenFromDirectory(inRootDirectory)
            End If

            result.IsReadonly = False

            Return result
        End Function

        Public Shared Function Create(ByVal inRootDirectory As String) As AnimationLibrary
            Dim result As New AnimationLibrary()

            If System.IO.File.Exists(inRootDirectory & "\Animations.ali") Then
                Throw New ArgumentException("The given directory """ & inRootDirectory & """ does already contain an animation library!")
            End If

            System.IO.Directory.CreateDirectory(inRootDirectory)

            result.Directory = System.IO.Path.GetFullPath(inRootDirectory)
            result.IsReadonly = False

            Return result
        End Function

        Public Shared Function OpenFromDirectory(ByVal inRootDirectory As String) As AnimationLibrary
            Dim library As AnimationLibrary = Nothing
            Dim fullPath As String = Path.GetFullPath(inRootDirectory)
            Dim source As Stream = Nothing

            ' load library
            If Not (System.IO.Directory.Exists(fullPath)) Then
                Throw New DirectoryNotFoundException("The given directory """ & fullPath & """ does not exist!")
            End If

            source = File.OpenRead(fullPath & "/Animations.ali")

            Using source
                Dim hashBytes() As Byte = System.Security.Cryptography.MD5.Create().ComputeHash(source)
                Dim hash As Long = 0

                For i As Integer = 0 To Math.Min(8, hashBytes.Length) - 1
                    hash = hash Or (Convert.ToInt64((hashBytes(i)))) << (i * 8)
                Next i

                source.Position = 0
                library = OpenFromStream(source, fullPath)
                library.Checksum = hash
            End Using
            privateInstance = library

            Return privateInstance
        End Function

        Private Shared Function OpenFromStream(ByVal source As Stream, ByVal fullPath As String) As AnimationLibrary
            Dim library As AnimationLibrary = Load(New BinaryReader(source))

            library.Directory = fullPath

            ' load shared frames
            library.m_Frames = New UniqueMap(Of Long, Byte())()

            If File.Exists(fullPath & "/Animations.gfx") Then
                source = File.OpenRead(fullPath & "/Animations.gfx")
                Dim reader As New BinaryReader(source)

                Using source
                    Dim count As Int32 = reader.ReadInt32()

                    For i As Integer = 0 To count - 1

                        Dim m_checksum As Int64 = reader.ReadInt64()

                        library.m_Frames.Add(m_checksum, reader.ReadBytes(reader.ReadInt32()))
                    Next i
                End Using
            End If

            Return library
        End Function

        Friend Sub ValidateName(ByVal inName As String)
            Dim validChars As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_/"

            Do
                If String.IsNullOrEmpty(inName) Then
                    Exit Do
                End If

                If inName.StartsWith("/") OrElse inName.EndsWith("/") Then
                    Exit Do
                End If

                Dim isValid As Boolean = True
                Dim prev As Char = " "c

                For Each c As Char In inName
                    If (Not (validChars.Contains(c))) OrElse ((c = "/"c) AndAlso (prev = "/"c)) Then
                        isValid = False

                        Exit For
                    End If

                    prev = c
                Next c

                If isValid Then
                    Return
                End If
            Loop While False

            Throw New ArgumentException("Name """ & inName & """ is invalid.")
        End Sub

        Public Sub Rename(ByVal inClass As Character, ByVal inNewName As String)
            ForceWriteable()

            ValidateName(inNewName)

            If m_Classes.ContainsKey(inNewName) Then
                Throw New ArgumentException("An Character named """ & inNewName & """ does already exist!")
            End If

            Dim pos As Integer = 0

            pos = m_Classes.Values.IndexOf(inClass)
            If pos < 0 Then
                Throw New ApplicationException("Class does not belong to this set.")
            End If

            m_Classes.Remove(inClass.Name)
            inClass.Name = inNewName
            m_Classes.Add(inClass.Name, inClass)
        End Sub

        Public Function FindAudio(ByVal inName As String) As AudioObject
            Try
                Return m_AudioObjects(inName)
            Catch e As Exception
                Throw New ArgumentException("An audio object named """ & inName & """ does not exist!", e)
            End Try
        End Function

        Public Function FindClass(ByVal inName As String) As Character
            Try
                Return m_Classes(inName)
            Catch e As Exception
                Throw New ArgumentException("An Character named """ & inName & """ does not exist!", e)
            End Try
        End Function

        Public Function HasClass(ByVal inName As String) As Boolean
            Return m_Classes.ContainsKey(inName)
        End Function

        Public Function AddClass(ByVal inName As String) As Character
            ForceWriteable()

            Dim result As New Character(inName, Me)

            ValidateName(inName)

            If m_Classes.ContainsKey(inName) Then
                Throw New ArgumentException("An Character named """ & inName & """ does already exist!")
            End If

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Directory & "/" & inName))

            m_Classes.Add(inName, result)

            Return result
        End Function

        Public Sub RemoveClass(ByVal inClass As Character)
            ForceWriteable()

            m_Classes.Remove(inClass.Name)
        End Sub

        Public Function AddAudio(ByVal inName As String, ByVal inWavBytes() As Byte) As AudioObject
            ForceWriteable()

            Dim result As New AudioObject(Me, inWavBytes)

            ValidateName(inName)

            If m_AudioObjects.ContainsKey(inName) Then
                Throw New ArgumentException("An audio object named """ & inName & """ does already exist!")
            End If

            m_AudioObjects.Add(inName, result)
            result.Name = inName

            Return result
        End Function

        Public Sub RemoveAudio(ByVal inAudio As AudioObject)
            ForceWriteable()

            If Not (m_AudioObjects.Remove(inAudio.Name)) Then
                Return
            End If

            For Each Character As Character In Classes
                For Each mSet As AnimationSet In Character.Sets
                    For Each anim As Animation In mSet.Animations
                        If anim.Sound Is inAudio Then
                            anim.Sound = Nothing
                        End If
                    Next anim
                Next mSet
            Next Character
        End Sub

        Friend Function LoadAudio(ByVal inAudio As AudioObject) As Byte()
            Dim result() As Byte = Nothing

            If Not (m_Frames.TryGetValue(inAudio.Checksum, result)) Then
                Throw New FileNotFoundException("Failed to load audio object """ & inAudio.Name & """ (Checksum: " & inAudio.Checksum & ").")
            End If

            Return result
        End Function

        Friend Function LoadFrame(ByVal inFrame As AnimationFrame) As Byte()
            Dim result() As Byte = Nothing

            If Not (m_Frames.TryGetValue(inFrame.Checksum, result)) Then
                Throw New FileNotFoundException("Failed to load shared frame bitmap (Checksum: " & inFrame.Checksum & ") in animation """ & inFrame.AnimationOrNull.Path & """.")
            End If

            Return result
        End Function

    End Class
End Namespace
