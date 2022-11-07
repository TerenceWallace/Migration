Imports System
Imports System.Windows.Controls

Namespace Migration.Editor
    ''' <summary>
    ''' Interaction logic for AnimLibraryTab.xaml
    ''' </summary>
    Partial Public Class AnimLibraryTab
        Inherits UserControl

        Private Sub BTN_GfxAppendSequence_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            GfxImportSequence(True)
        End Sub

        Private Sub BTN_GfxImportSequence_Click(ByVal sender As Object, ByVal args As RoutedEventArgs)
            GfxImportSequence(False)
        End Sub

        Private Sub GfxImportSequence(ByVal inCanAppend As Boolean)
            Dim gfxSequence = MainWindow.Instance.CurrentSequence

            If (gfxSequence Is Nothing) OrElse (gfxSequence.Frames.Count = 0) Then
                Log.LogMessageModal("No GFX sequence available or it does not contain any frame.")

                Return
            End If

            ' check for CTS-Sequence
            Dim color As GFXSequence = Nothing
            Dim torso As GFXSequence = Nothing
            Dim shadow As GFXSequence = Nothing

            Dim minX As Integer = Int32.MaxValue
            Dim minY As Integer = Int32.MaxValue
            Dim newAnims As New List(Of Animation)()

            If Not (PrepareCTSSequence(gfxSequence, color, torso, shadow)) Then
                Dim anim1 As Animation = Nothing

                If inCanAppend AndAlso (TCurrentAnim IsNot Nothing) AndAlso TCurrentSet.Animations.Contains(TCurrentAnim) Then
                    ' append to animation
                    anim1 = TCurrentAnim
                Else
                    ' allocate animation
                    Dim i As Integer = 0

                    Do
                        i += 1

                        If TCurrentSet.Animations.Any(Function(e) e.Name = "GfxAnim_" & i) Then
                            Continue Do
                        End If

                        anim1 = TCurrentSet.AddAnimation("GfxAnim_" & i)

                        Exit Do
                    Loop While True
                End If

                ' fill with frames
                For Each gfxFrame In gfxSequence.Frames
                    Dim frame = anim1.AddFrame()

                    InitGfxFrame(frame, gfxFrame)

                    minX = Math.Min(minX, frame.OffsetX)
                    minY = Math.Min(minY, frame.OffsetY)
                Next gfxFrame

                newAnims.Add(anim1)
            Else
                animNames = New String() {"Color", "Torso", "Shadow"}
                Dim seqs() As GFXSequence = {color, torso, shadow}

                For x As Integer = 0 To 2
                    anim = Nothing
                    LooperX = x

                    If TCurrentSet.Animations.Any(AddressOf AnonymousMethod5) Then
                        If Not inCanAppend Then
                            Throw New ArgumentException("The current animation set already contain an animation called """ & animNames(x) & """!")
                        End If

                        For Each frame In anim.Frames
                            minX = Math.Min(minX, frame.OriginalOffsetX)
                            minY = Math.Min(minY, frame.OriginalOffsetY)
                        Next frame
                    Else
                        anim = TCurrentSet.AddAnimation(animNames(x))
                    End If

                    newAnims.Add(anim)
                    anim.RenderIndex = 2 - x

                    For z As Integer = 0 To gfxSequence.Frames.Count - 1
                        Dim gfxFrame = seqs(x).Frames(z)
                        Dim frame = anim.AddFrame()

                        InitGfxFrame(frame, gfxFrame)

                        minX = Math.Min(minX, frame.OriginalOffsetX)
                        minY = Math.Min(minY, frame.OriginalOffsetY)
                    Next z
                Next x
            End If


            ' normalize offsets to origin
            For Each mAnimation In newAnims
                For Each frame In mAnimation.Frames
                    frame.OffsetX = frame.OriginalOffsetX - minX
                    frame.OffsetY = frame.OriginalOffsetY - minY
                Next frame

                mAnimation.ComputeDimension()
            Next mAnimation
        End Sub

        Private animNames() As String
        Private anim As Animation
        Private LooperX As Integer
        Private Function AnonymousMethod5(ByVal e As Animation) As Boolean
            If e.Name = animNames(LooperX) Then
                anim = e
                Return True
            End If
            Return False
        End Function

        Private Sub InitGfxFrame(ByVal inTarget As AnimationFrame, ByVal inSource As GFXFrame)
            inTarget.SetBitmap(inSource.Image)

            inTarget.Width = inSource.Width
            inTarget.Height = inSource.Height
            inTarget.OffsetX = inSource.OffsetX
            inTarget.OriginalOffsetX = inTarget.OffsetX
            inTarget.OffsetY = inSource.OffsetY
            inTarget.OriginalOffsetY = inTarget.OffsetY

            ' save metadata
            inTarget.GFXSequence = inSource.Sequence.Index
            inTarget.GFXFrame = inSource.Index
            inTarget.GFXFileChecksum = inSource.Sequence.File.Checksum
        End Sub

        Private Function PrepareCTSSequence(ByVal gfxSequence As GFXSequence, ByRef color As GFXSequence, ByRef torso As GFXSequence, ByRef shadow As GFXSequence) As Boolean
            color = Nothing
            torso = color
            shadow = torso

            ' validate sequences
            Dim ObjectSeqs = MainWindow.Instance.ObjectSeqs
            Dim TorsoSeqs = MainWindow.Instance.TorsoSeqs
            Dim ShadowSeqs = MainWindow.Instance.ShadowSeqs
            Dim seqIndex As Integer = ObjectSeqs.IndexOf(gfxSequence)
            Dim seqIndex1 As Integer = TorsoSeqs.IndexOf(gfxSequence)
            Dim seqIndex2 As Integer = ShadowSeqs.IndexOf(gfxSequence)

            If seqIndex >= 0 Then
            ElseIf seqIndex1 >= 0 Then
            ElseIf seqIndex2 >= 0 Then
            Else
                Return False
            End If

            If (ObjectSeqs.Count <> TorsoSeqs.Count) OrElse (ObjectSeqs.Count <> ShadowSeqs.Count) Then
                Return False
            End If

            color = ObjectSeqs(seqIndex)
            torso = TorsoSeqs(seqIndex)
            shadow = ShadowSeqs(seqIndex)

            If (color.Frames.Count <> torso.Frames.Count) OrElse (color.Frames.Count <> shadow.Frames.Count) Then
                Return False
            End If

            Return True
        End Function

        Private Sub BTN_GfxImportDirMotion_Click(ByVal sender As Object, ByVal args As RoutedEventArgs)
            Dim gfxSequence = MainWindow.Instance.CurrentSequence

            If (gfxSequence Is Nothing) OrElse (gfxSequence.Frames.Count = 0) Then
                Log.LogMessageModal("No GFX sequence available or it does not contain any frame.")

                Return
            End If

            ' validate sequences
            Dim color As GFXSequence = Nothing
            Dim torso As GFXSequence = Nothing
            Dim shadow As GFXSequence = Nothing

            If Not (PrepareCTSSequence(gfxSequence, color, torso, shadow)) Then
                Throw New ArgumentException("Selected GFX sequence is not a color-, torso- or shadow-seqence!")
            End If

            If (color.Frames.Count Mod 6) <> 0 Then
                Throw New ArgumentException("Color sequence's frame count is not dividable by six.")
            End If

            Dim angleStrings() As String = {"Angle_225", "Angle_270", "Angle_315", "Angle_045", "Angle_090", "Angle_135"}

            If TCurrentClass.Sets.Count > 0 Then
                ' check if this is a pure directed motion
                For n As Integer = 0 To 5
                    If (Not (TCurrentClass.HasSet(angleStrings(n)))) OrElse (Not (TCurrentClass.HasSet("Frozen" & angleStrings(n)))) Then
                        Throw New InvalidOperationException("The current class is not empty and also not a directed motion.")
                    End If
                Next n

                ' check if we have access to original offsets
                For Each mSet In TCurrentClass.Sets
                    For Each mAnimation In mSet.Animations
                        For Each frame In mAnimation.Frames
                            If (frame.OriginalOffsetX = Int32.MinValue) OrElse (frame.OriginalOffsetY = Int32.MinValue) Then
                                Throw New InvalidOperationException("To append direction motion, all existing frames need to have valid original offsets!")
                            End If
                        Next frame
                    Next mAnimation
                Next mSet
            End If

            ' allocate animation sets
            Dim sets(5) As AnimationSet
            Dim animNames() As String = {"Color", "Torso", "Shadow"}
            Dim seqs() As GFXSequence = {color, torso, shadow}
            Dim stride As Integer = color.Frames.Count \ 6
            Dim minX As Integer = Int32.MaxValue
            Dim minY As Integer = Int32.MaxValue

            If stride = 13 Then
                '                
                '                 * Usually there are 12 frames for each direction for settler motions.
                '                 * But sometimes the sequence is followed by another 6 frames, one for
                '                 * each direction representing its idle state.
                '                 * 
                '                 * In this step we will just ignore the last six frames...
                '                 
                stride = 12
            End If

            Dim i As Integer = 0
            Dim offset As Integer = 0
            Do While i < 6
                Dim mSet As AnimationSet = Nothing

                If TCurrentClass.HasSet(angleStrings(i)) Then
                    mSet = TCurrentClass.FindSet(angleStrings(i))
                Else
                    mSet = TCurrentClass.AddAnimationSet(angleStrings(i))
                End If

                sets(i) = mSet
                mSet.DurationMillis = 1000

                For x As Integer = 0 To 2
                    Dim anim As Animation = Nothing

                    If mSet.HasAnimation(animNames(x)) Then
                        anim = mSet.FindAnimation(animNames(x))
                    Else
                        anim = mSet.AddAnimation(animNames(x))
                    End If

                    anim.RenderIndex = 2 - x

                    For z As Integer = offset To offset + stride - 1
                        Dim gfxFrame = seqs(x).Frames(z)
                        Dim frame = anim.AddFrame()

                        InitGfxFrame(frame, gfxFrame)

                        minX = Math.Min(minX, frame.OffsetX)
                        minY = Math.Min(minY, frame.OffsetY)
                    Next z
                Next x
                i += 1
                offset += stride
            Loop

            If color.Frames.Count = 78 Then
                ' use special frames for frozen images
                For n As Integer = 0 To 5
                    Dim mSet As AnimationSet = Nothing

                    If TCurrentClass.HasSet("Frozen" & angleStrings(n)) Then
                        mSet = TCurrentClass.FindSet("Frozen" & angleStrings(n))
                    Else
                        mSet = TCurrentClass.AddAnimationSet("Frozen" & angleStrings(n))
                    End If

                    For x As Integer = 0 To 2
                        Dim anim As Animation = Nothing

                        If mSet.HasAnimation(animNames(x)) Then
                            anim = mSet.FindAnimation(animNames(x))
                        Else
                            anim = mSet.AddAnimation(animNames(x))
                        End If

                        Dim gfxFrame = seqs(x).Frames(72 + n)
                        Dim frame = anim.AddFrame()

                        anim.RenderIndex = 2 - x

                        InitGfxFrame(frame, gfxFrame)

                        minX = Math.Min(minX, frame.OffsetX)
                        minY = Math.Min(minY, frame.OffsetY)
                    Next x
                Next n
            Else
                ' use first stride image for frozen frames
                i = 0
                offset = 0
                Do While i < 6
                    Dim mSet As AnimationSet = Nothing

                    If TCurrentClass.HasSet("Frozen" & angleStrings(i)) Then
                        mSet = TCurrentClass.FindSet("Frozen" & angleStrings(i))
                    Else
                        mSet = TCurrentClass.AddAnimationSet("Frozen" & angleStrings(i))
                    End If

                    For x As Integer = 0 To 2
                        Dim anim As Animation = Nothing

                        If mSet.HasAnimation(animNames(x)) Then
                            anim = mSet.FindAnimation(animNames(x))
                        Else
                            anim = mSet.AddAnimation(animNames(x))
                        End If

                        Dim gfxFrame = seqs(x).Frames(offset)
                        Dim frame = anim.AddFrame()

                        anim.RenderIndex = 2 - x

                        InitGfxFrame(frame, gfxFrame)

                        minX = Math.Min(minX, frame.OffsetX)
                        minY = Math.Min(minY, frame.OffsetY)
                    Next x
                    i += 1
                    offset += stride
                Loop
            End If

            ' normalize offsets to origin
            For Each mSetWithinLoop As AnimationSet In TCurrentClass.Sets
                Dim mSet As AnimationSet = mSetWithinLoop
                For Each mAnimation In mSetWithinLoop.Animations
                    For Each frame In mAnimation.Frames
                        frame.OffsetX = frame.OriginalOffsetX - minX
                        frame.OffsetY = frame.OriginalOffsetY - minY
                    Next frame

                    mAnimation.ComputeDimension()
                Next mAnimation
            Next mSetWithinLoop
        End Sub

        Private Sub BTN_GenFrozenFrames_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            ' derive from existing animation frames
            Dim angleStrings() As String = {"Angle_225", "Angle_270", "Angle_315", "Angle_045", "Angle_090", "Angle_135"}

            For Each angle In angleStrings
                If TCurrentClass.ContainsSet("Angle" & angle) Then
                    Throw New InvalidOperationException("Current animation class is not a directed motion!")
                End If

                If TCurrentClass.ContainsSet("Frozen" & angle) Then
                    Throw New InvalidOperationException("Current animation class already contains frozen frames!")
                End If
            Next angle

            For Each angle In angleStrings
                Dim frozen As AnimationSet = TCurrentClass.AddAnimationSet("Frozen" & angle)

                For Each mAnimation In TCurrentClass.FindSet(angle).Animations
                    Dim frozenAnim = frozen.AddAnimation(mAnimation.Name)

                    frozenAnim.FrozenFrame = frozenAnim.AddFrame()
                    frozenAnim.IsFrozen = True
                    frozenAnim.IsVisible = True
                    frozenAnim.OffsetX = mAnimation.OffsetX
                    frozenAnim.OffsetY = mAnimation.OffsetY
                    frozenAnim.RenderIndex = mAnimation.RenderIndex

                    mAnimation.FirstFrame.Clone(frozenAnim.FrozenFrame)

                    frozenAnim.ComputeDimension(False)
                Next mAnimation

                frozen.ComputeDimension()
            Next angle
        End Sub
    End Class
End Namespace
