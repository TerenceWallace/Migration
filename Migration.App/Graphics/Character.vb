Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.Serialization
Imports Migration.Common
Imports Migration.Core

Namespace Migration

    ''' <summary>
    ''' A Character represents the topmost animation object. For example it may contains
    ''' all animations available for a given object class like a "sawmill". There is an ambient
    ''' animation, always visible as animation background. This might be the sawmill itself.
    ''' Then there are animation sets. Each set represents another "animation type". Such a type
    ''' may consist of several animations, blended together, constructing the final animation
    ''' for a given type. There can only be one active type per Character. Of course you could
    ''' just ignore animation types and put the whole thing as single animation with little variations
    ''' into the class again and again, but this will just blow up the file size. The Character
    ''' provides a way to split animations into little pieces, saving a lot of file space.
    ''' </summary>
    Public Class Character
        Private m_Sets As New UniqueMap(Of String, AnimationSet)()
        Private m_Frames As UniqueMap(Of Int64, Byte()) = Nothing

        Public Event OnChanged As DDimensionChangedHandler

        Private m_AmbientSet As AnimationSet
        ''' <summary>
        ''' The ambient set is always visible in the background (can be null). The animation will be auto-repeated.
        ''' </summary>
        Public Property AmbientSet() As AnimationSet
            Get
                Return m_AmbientSet
            End Get
            Set(ByVal value As AnimationSet)
                ForceWriteable()

                If (value IsNot Nothing) AndAlso (Not (m_Sets.ContainsKey(value.Name))) Then
                    Throw New InvalidOperationException("The given ambient set does not belong to this Character!")
                End If

                m_AmbientSet = value
            End Set
        End Property

        Public ReadOnly Property UseAmbientSet() As Boolean
            Get
                Return m_AmbientSet IsNot Nothing
            End Get
        End Property

        Private privateResourceStacks As List(Of ResourceStackEntry)
        Public Property ResourceStacks() As List(Of ResourceStackEntry)
            Get
                Return privateResourceStacks
            End Get
            Private Set(ByVal value As List(Of ResourceStackEntry))
                privateResourceStacks = value
            End Set
        End Property

        Private privateGroundPlane As List(Of Rectangle)
        Public Property GroundPlane() As List(Of Rectangle)
            Get
                Return privateGroundPlane
            End Get
            Private Set(ByVal value As List(Of Rectangle))
                privateGroundPlane = value
            End Set
        End Property

        Private privateReservedPlane As List(Of Rectangle)
        Public Property ReservedPlane() As List(Of Rectangle)
            Get
                Return privateReservedPlane
            End Get
            Private Set(ByVal value As List(Of Rectangle))
                privateReservedPlane = value
            End Set
        End Property

        Private privateShiftX As Int32
        Public Property ShiftX() As Int32
            Get
                Return privateShiftX
            End Get
            Set(ByVal value As Int32)
                privateShiftX = value
            End Set
        End Property

        Private privateShiftY As Int32
        Public Property ShiftY() As Int32
            Get
                Return privateShiftY
            End Get
            Set(ByVal value As Int32)
                privateShiftY = value
            End Set
        End Property

        Private Sub ForceWriteable()
            Library.ForceWriteable()
        End Sub

        Friend Sub Save(ByVal inWriter As BinaryWriter)
            LoadFrames()

            ' swap all bitmap fields to prevent serialization
            Dim deferred As New DeferredContainer()

            For Each mSet As AnimationSet In Sets
                For Each anim As Animation In mSet.Animations
                    For Each Frame As AnimationFrame In anim.Frames
                        If Frame.m_Bitmap Is Nothing Then
                            Frame.m_Bitmap = m_Frames(Frame.Checksum)
                        End If

                        deferred.AddFrame(Frame)

                        Frame.m_Bitmap = Nothing
                    Next Frame
                Next anim
            Next mSet

            m_Frames = deferred.Frames

            ' serialize frames
            Using target As Stream = File.OpenWrite(Library.Directory & "/" & Name & ".gfx.tmp")

                deferred.Store(target)
            End Using

            File.Delete(Library.Directory & "/" & Name & ".gfx")
            File.Move(Library.Directory & "/" & Name & ".gfx.tmp", Library.Directory & "/" & Name & ".gfx")

            ' write class object to stream
            inWriter.Write(Convert.ToByte(2)) ' class type ID
            inWriter.Write(Convert.ToUInt16(&H1001)) ' class version

            inWriter.Write(Name)
            inWriter.Write(m_Sets.Count)
            inWriter.Write(ShiftX)
            inWriter.Write(ShiftY)

            For Each mSet As AnimationSet In m_Sets.Values
                mSet.Save(inWriter)
            Next mSet

            If AmbientSet IsNot Nothing Then
                inWriter.Write(Convert.ToString(AmbientSet.Name))
            Else
                inWriter.Write(Convert.ToString(""))
            End If

            inWriter.Write(Convert.ToInt32(CInt(ResourceStacks.Count)))
            For Each stack As ResourceStackEntry In ResourceStacks
                inWriter.Write(Convert.ToSByte(stack.Position.X))
                inWriter.Write(Convert.ToSByte(stack.Position.Y))
                inWriter.Write(Convert.ToByte(stack.Resource))
            Next stack

            inWriter.Write(Convert.ToInt32(CInt(GroundPlane.Count)))
            For Each bound As Rectangle In GroundPlane
                inWriter.Write(Convert.ToSByte(bound.X))
                inWriter.Write(Convert.ToSByte(bound.Y))
                inWriter.Write(Convert.ToByte(bound.Width))
                inWriter.Write(Convert.ToByte(bound.Height))
            Next bound

            inWriter.Write(Convert.ToInt32(CInt(ReservedPlane.Count)))
            For Each bound As Rectangle In ReservedPlane
                inWriter.Write(Convert.ToSByte(bound.X))
                inWriter.Write(Convert.ToSByte(bound.Y))
                inWriter.Write(Convert.ToByte(bound.Width))
                inWriter.Write(Convert.ToByte(bound.Height))
            Next bound
        End Sub

        Friend Shared Function Load(ByVal inLibrary As AnimationLibrary, ByVal inReader As BinaryReader) As Character
            Dim result As Character = Nothing

            If inReader.ReadByte() <> 2 Then
                Throw New InvalidDataException()
            End If

            Dim version As UInt16 = inReader.ReadUInt16()
            Select Case version
                Case &H1000, &H1001
                    result = New Character(inReader.ReadString(), inLibrary)

                    Dim setCount As Integer = inReader.ReadInt32()

                    result.ShiftX = inReader.ReadInt32()
                    result.ShiftY = inReader.ReadInt32()

                    For i As Integer = 0 To setCount - 1
                        Dim mSet As AnimationSet = AnimationSet.Load(result, inReader)

                        result.m_Sets.Add(mSet.Name, mSet)
                    Next i

                    Dim tmp As String = inReader.ReadString()

                    If Not (String.IsNullOrEmpty(tmp)) Then
                        result.AmbientSet = result.m_Sets(tmp)
                    End If

                    If version = &H1001 Then
                        Dim i As Integer = 0
                        Dim count As Integer = inReader.ReadInt32()
                        Do While i < count
                            Dim x As SByte = inReader.ReadSByte()
                            Dim y As SByte = inReader.ReadSByte()
                            Dim res As Resource = CType(inReader.ReadByte(), Resource)

                            result.ResourceStacks.Add(New ResourceStackEntry() With {.Position = New Point(x, y), .Resource = res})
                            i += 1
                        Loop

                        i = 0
                        count = inReader.ReadInt32()
                        Do While i < count
                            Dim x As SByte = inReader.ReadSByte()
                            Dim y As SByte = inReader.ReadSByte()

                            Dim m_width As Byte = inReader.ReadByte()

                            Dim m_height As Byte = inReader.ReadByte()

                            result.GroundPlane.Add(New Rectangle(x, y, m_width, m_height))
                            i += 1
                        Loop

                        i = 0
                        count = inReader.ReadInt32()
                        Do While i < count
                            Dim x As SByte = inReader.ReadSByte()
                            Dim y As SByte = inReader.ReadSByte()

                            Dim m_width As Byte = inReader.ReadByte()

                            Dim m_height As Byte = inReader.ReadByte()

                            result.ReservedPlane.Add(New Rectangle(x, y, m_width, m_height))
                            i += 1
                        Loop
                    End If

                    Exit Select
                Case Else
                    Throw New InvalidDataException()
            End Select

            For Each mSet As AnimationSet In result.Sets
                For Each anim As Animation In mSet.Animations
                    anim.ComputeDimension(False)
                Next anim

                mSet.ComputeDimension(False)
            Next mSet

            result.ComputeDimension()

            Return result
        End Function

        <OnDeserialized()> _
        Private Sub OnDeserialized(ByVal ctx As StreamingContext)
            For Each mSet As AnimationSet In Sets
                For Each anim As Animation In mSet.Animations
                    anim.ComputeDimension(False)
                Next anim

                mSet.ComputeDimension(False)
            Next mSet

            ComputeDimension()
        End Sub

        Friend Sub ComputeDimension()
            ForceWriteable()

            Dim newWidth As Int32 = 0
            Dim newHeight As Int32 = 0

            For Each mSet As AnimationSet In Sets
                newWidth = Math.Max(newWidth, mSet.Width)
                newHeight = Math.Max(newHeight, mSet.Height)
            Next mSet

            Width = newWidth
            Height = newHeight

            RaiseEvent OnChanged()
        End Sub

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

        Public ReadOnly Property Sets() As BindingList(Of AnimationSet)
            Get
                Return m_Sets.GetValueBinding()
            End Get
        End Property

        Public ReadOnly Property Children() As BindingList(Of AnimationSet)
            Get
                Return Sets
            End Get
        End Property

        Private privateName As String
        Public Property Name() As String
            Get
                Return privateName
            End Get
            Friend Set(ByVal value As String)
                privateName = value
            End Set
        End Property

        Private privateLibrary As AnimationLibrary
        Public Property Library() As AnimationLibrary
            Get
                Return privateLibrary
            End Get
            Private Set(ByVal value As AnimationLibrary)
                privateLibrary = value
            End Set
        End Property

        Friend Sub New(ByVal inName As String, ByVal inParent As AnimationLibrary)
            Name = inName
            Library = inParent
            ResourceStacks = New List(Of ResourceStackEntry)()
            GroundPlane = New List(Of Rectangle)()
            ReservedPlane = New List(Of Rectangle)()
        End Sub

        Public Sub Rename(ByVal inSet As AnimationSet, ByVal inNewName As String)
            ForceWriteable()

            Library.ValidateName(inNewName)

            If m_Sets.ContainsKey(inNewName) Then
                Throw New ArgumentException("An animation set named """ & inNewName & """ does already exist!")
            End If

            Dim pos As Integer = 0

            pos = m_Sets.Values.IndexOf(inSet)
            If pos < 0 Then
                Throw New ApplicationException("Animation set does not belong to this set.")
            End If

            m_Sets.Remove(inSet.Name)
            inSet.Name = inNewName
            m_Sets.Add(inSet.Name, inSet)
        End Sub

        Public Function AddAnimationSet(ByVal inName As String) As AnimationSet
            ForceWriteable()

            Dim result As New AnimationSet(inName, Me)

            Library.ValidateName(inName)

            If m_Sets.ContainsKey(inName) Then
                Throw New ArgumentException("An animation set named """ & inName & """ does already exist!")
            End If

            m_Sets.Add(inName, result)
            ComputeDimension()

            Return result
        End Function

        Public Function ContainsSet(ByVal inName As String) As Boolean
            Return m_Sets.ContainsKey(inName)
        End Function

        Public Function HasSet(ByVal inName As String) As Boolean
            Return m_Sets.ContainsKey(inName)
        End Function

        Public Function FindSet(ByVal inName As String) As AnimationSet
            Try
                Return m_Sets(inName)
            Catch e As Exception
                Throw New ArgumentException("An animation set named """ & inName & """ does not exist!", e)
            End Try
        End Function

        Public Sub RemoveAnimationSet(ByVal inAnimationSet As AnimationSet)
            ForceWriteable()

            m_Sets.Remove(inAnimationSet.Name)

            If (AmbientSet IsNot Nothing) AndAlso (AmbientSet.Name.CompareTo(inAnimationSet.Name) = 0) Then
                AmbientSet = Nothing
            End If

            ComputeDimension()
        End Sub

        Friend Sub LoadFrames()
            If m_Frames Is Nothing Then
                If Not (File.Exists(Library.Directory & "/" & Name & ".gfx")) Then
                    m_Frames = New UniqueMap(Of Long, Byte())()
                Else
                    ' deferred load
                    Dim source As Stream = File.OpenRead(Library.Directory & "/" & Name & ".gfx")

                    Using source
                        Dim deferred As DeferredContainer = DeferredContainer.Load(source)

                        m_Frames = deferred.Frames
                    End Using
                End If
            End If
        End Sub

        Friend Function LoadFrame(ByVal inFrame As AnimationFrame) As Byte()
            Dim result() As Byte = Nothing

            LoadFrames()

            If Not (m_Frames.TryGetValue(inFrame.Checksum, result)) Then
                Throw New FileNotFoundException("Failed to load shared frame bitmap (Checksum: " & inFrame.Checksum & ") in animation """ & inFrame.AnimationOrNull.Path & """.")
            End If

            Return result
        End Function
    End Class
End Namespace
