Imports System.ComponentModel
Imports System.IO
Imports Migration.Common
Imports Migration.Core

Namespace Migration

    Public Class Animation

        Public Event OnDimensionChanged As DDimensionChangedHandler

        Private m_OffsetX As Int32
        Private m_OffsetY As Int32
        Private m_IsFrozen As Boolean
        Private m_FrozenFrame As AnimationFrame
        Private m_SoundStartDelay As Int32
        Private m_SoundRepeatDelay As Int32
        Private m_PlaySound As Boolean
        Private m_RepeatSound As Boolean
        Private m_Sound As AudioObject

        Private m_Frames As New InternalBinding(Of AnimationFrame)()

        Public ReadOnly Property FirstFrame() As AnimationFrame
            Get
                Return (If(Frames.Count = 0, Nothing, Frames(0)))
            End Get
        End Property

        Public ReadOnly Property Frames() As BindingList(Of AnimationFrame)
            Get
                Return m_Frames
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

        Public ReadOnly Property Path() As String
            Get
                If SetOrNull IsNot Nothing Then
                    Return SetOrNull.Character.Library.Directory & "\" & SetOrNull.Character.Name & "\" & SetOrNull.Name & "\" & Name
                Else
                    Return Library.Directory & "\" & Name
                End If
            End Get
        End Property

        Private privateRenderIndex As Int32
        Public Property RenderIndex() As Int32
            Get
                Return privateRenderIndex
            End Get
            Set(ByVal value As Int32)
                privateRenderIndex = value
            End Set
        End Property

        Private privateSetOrNull As AnimationSet
        Public Property SetOrNull() As AnimationSet
            Get
                Return privateSetOrNull
            End Get
            Private Set(ByVal value As AnimationSet)
                privateSetOrNull = value
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

        Public Property SoundStartDelay() As Int32
            Get
                Return m_SoundStartDelay
            End Get
            Set(ByVal value As Int32)
                ForceWriteable()
                m_SoundStartDelay = value
            End Set
        End Property

        Public Property SoundRepeatDelay() As Int32
            Get
                Return m_SoundRepeatDelay
            End Get
            Set(ByVal value As Int32)
                ForceWriteable()
                m_SoundRepeatDelay = value
            End Set
        End Property

        Public Property PlaySound() As Boolean
            Get
                Return m_PlaySound
            End Get
            Set(ByVal value As Boolean)
                ForceWriteable()
                m_PlaySound = value
            End Set
        End Property

        Public Property RepeatSound() As Boolean
            Get
                Return m_RepeatSound
            End Get
            Set(ByVal value As Boolean)
                ForceWriteable()
                m_RepeatSound = value
            End Set
        End Property

        Public Property Sound() As AudioObject
            Get
                Return m_Sound
            End Get
            Set(ByVal value As AudioObject)
                ForceWriteable()
                m_Sound = value
            End Set
        End Property

        Public Property IsFrozen() As Boolean
            Get
                Return m_IsFrozen
            End Get
            Set(ByVal value As Boolean)
                ForceWriteable()

                m_IsFrozen = value
                If value AndAlso (FrozenFrame Is Nothing) AndAlso (Frames.Count > 0) Then
                    FrozenFrame = Frames(0)
                End If
            End Set
        End Property

        Public Property OffsetX() As Int32
            Get
                Return m_OffsetX
            End Get
            Set(ByVal value As Int32)
                ForceWriteable()
                m_OffsetX = value
                NotifyDimensionChange()
            End Set
        End Property

        Public Property OffsetY() As Int32
            Get
                Return m_OffsetY
            End Get
            Set(ByVal value As Int32)
                ForceWriteable()
                m_OffsetY = value
                NotifyDimensionChange()
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

        Private privateIsVisible As Boolean
        Public Property IsVisible() As Boolean
            Get
                Return privateIsVisible
            End Get
            Set(ByVal value As Boolean)
                privateIsVisible = value
            End Set
        End Property

        Public Property FrozenFrame() As AnimationFrame
            Get
                Return m_FrozenFrame
            End Get
            Set(ByVal value As AnimationFrame)
                ForceWriteable()

                If value IsNot Nothing Then
                    If Not (Frames.Contains(value)) Then
                        Throw New ArgumentException("Given frozen frame is not contained in animation.")
                    End If
                End If

                m_FrozenFrame = value
            End Set
        End Property


        Friend Sub Save(ByVal inWriter As BinaryWriter)
            ' write animation to stream
            inWriter.Write(Convert.ToByte(4)) ' animation type ID
            inWriter.Write(Convert.ToUInt16(&H1000)) ' animation version

            inWriter.Write(Convert.ToString(Name))
            inWriter.Write(Convert.ToInt32(CInt(m_OffsetX)))
            inWriter.Write(Convert.ToInt32(CInt(m_OffsetY)))
            inWriter.Write(Convert.ToInt32(CInt(RenderIndex)))
            inWriter.Write(Convert.ToBoolean(m_IsFrozen))
            inWriter.Write(Convert.ToInt32(CInt(SoundStartDelay)))
            inWriter.Write(Convert.ToInt32(CInt(SoundRepeatDelay)))
            inWriter.Write(Convert.ToBoolean(PlaySound))
            inWriter.Write(Convert.ToBoolean(RepeatSound))

            If Sound IsNot Nothing Then
                inWriter.Write(Convert.ToString(Sound.Name))
            Else
                inWriter.Write(Convert.ToString(""))
            End If

            inWriter.Write(Convert.ToInt32(CInt(m_Frames.Count)))

            For Each frame As AnimationFrame In m_Frames
                frame.Save(inWriter)
            Next frame

            If m_FrozenFrame IsNot Nothing Then
                inWriter.Write(Convert.ToInt64(CInt(m_FrozenFrame.Checksum)))
            Else
                inWriter.Write(Convert.ToInt64(CInt(0)))
            End If
        End Sub

        Friend Shared Function Load(ByVal inLibrary As AnimationLibrary, ByVal inSetOrNull As AnimationSet, ByVal inReader As BinaryReader) As Animation
            Dim result As Animation = Nothing

            If inReader.ReadByte() <> 4 Then
                Throw New InvalidDataException()
            End If

            Select Case inReader.ReadUInt16()
                Case &H1000
                    If inSetOrNull IsNot Nothing Then
                        result = New Animation(inReader.ReadString(), inSetOrNull)
                    Else
                        result = New Animation(inReader.ReadString(), inLibrary)
                    End If

                    result.OffsetX = inReader.ReadInt32()
                    result.OffsetY = inReader.ReadInt32()
                    result.RenderIndex = inReader.ReadInt32()
                    result.IsFrozen = inReader.ReadBoolean()
                    result.SoundStartDelay = inReader.ReadInt32()
                    result.SoundRepeatDelay = inReader.ReadInt32()
                    result.PlaySound = inReader.ReadBoolean()
                    result.RepeatSound = inReader.ReadBoolean()

                    Dim sndName As String = inReader.ReadString()

                    If Not (String.IsNullOrEmpty(sndName)) Then
                        result.Sound = result.Library.FindAudio(sndName)
                    End If

                    Dim i As Integer = 0
                    Dim count As Integer = inReader.ReadInt32()
                    Do While i < count
                        result.m_Frames.Add(AnimationFrame.Load(result, inReader))
                        i += 1
                    Loop

                    Dim frozenID As Int64 = inReader.ReadInt64()

                    If frozenID <> 0 Then
                        For Each frame As AnimationFrame In result.Frames
                            'If frame.Checksum = frozenID Then
                            result.m_FrozenFrame = frame
                            Exit For
                            'End If
                        Next frame

                        If result.m_FrozenFrame Is Nothing Then
                            Throw New InvalidDataException()
                        End If
                    End If

                Case Else
                    Throw New InvalidDataException()
            End Select

            Return result
        End Function

        Public Sub ComputeDimension()
            ComputeDimension(True)
        End Sub

        Friend Sub NotifyDimensionChange()
            RaiseEvent OnDimensionChanged()
        End Sub

        Public Sub ComputeDimension(ByVal inNotifyParent As Boolean)
            ForceWriteable()

            Dim newWidth As Int32 = 0
            Dim newHeight As Int32 = 0

            For Each frame As AnimationFrame In Frames
                newWidth = Math.Max(newWidth, frame.OffsetX + frame.Width)
                newHeight = Math.Max(newHeight, frame.OffsetY + frame.Height)
            Next frame

            Width = newWidth
            Height = newHeight

            If inNotifyParent Then
                NotifyDimensionChange()
            End If
        End Sub

        Friend Sub New(ByVal inName As String, ByVal inSet As AnimationSet)
            Name = inName
            SetOrNull = inSet
            Library = inSet.Character.Library
        End Sub

        Friend Sub New(ByVal inName As String, ByVal inLibrary As AnimationLibrary)
            Name = inName
            Library = inLibrary
        End Sub

        Public Function AddFrame() As AnimationFrame
            ForceWriteable()

            Dim result As New AnimationFrame(Me)

            m_Frames.AddInternal(result)

            If IsFrozen AndAlso (FrozenFrame Is Nothing) Then
                FrozenFrame = result
            End If

            result.Index = Library.FrameCount
            Library.FrameCount += 1

            ComputeDimension()

            Return result
        End Function

        Public Sub RemoveFrame(ByVal inFrame As AnimationFrame)
            ForceWriteable()

            If Not (m_Frames.Contains(inFrame)) Then
                Throw New ArgumentException("Given frame is not contained in the current animation.")
            End If

            m_Frames.RemoveInternal(inFrame)

            If m_FrozenFrame Is inFrame Then
                If Frames.Count > 0 Then
                    m_FrozenFrame = Frames(0)
                Else
                    m_FrozenFrame = Nothing
                End If
            End If

            ComputeDimension()
        End Sub

        Public Sub MoveFrameLeft(ByVal inFrame As AnimationFrame)
            ForceWriteable()

            Dim pos As Integer = m_Frames.IndexOf(inFrame)

            If pos < 0 Then
                Throw New ArgumentException("Given frame is not contained in the current animation.")
            End If

            If pos = 0 Then
                Return
            End If

            m_Frames.RemoveAtInternal(pos)
            m_Frames.InsertInternal(pos - 1, inFrame)
        End Sub

        Public Sub MoveFrameRight(ByVal inFrame As AnimationFrame)
            ForceWriteable()

            Dim pos As Integer = m_Frames.IndexOf(inFrame)

            If pos < 0 Then
                Throw New ArgumentException("Given frame is not contained in the current animation.")
            End If

            If pos = m_Frames.Count - 1 Then
                Return
            End If

            m_Frames.RemoveAtInternal(pos)
            m_Frames.InsertInternal(pos + 1, inFrame)
        End Sub

        Private Sub ForceWriteable()
            Library.ForceWriteable()
        End Sub
    End Class


End Namespace
