Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports System.Windows.Threading
Imports System.Globalization
Imports System.Drawing
Imports System.IO
Imports System.ComponentModel
Imports System.Drawing.Imaging
Imports Migration

Namespace Migration.Editor
    ''' <summary>
    ''' Interaction logic for AnimLibraryTab.xaml
    ''' </summary>
    Partial Public Class AnimLibraryTab
        Inherits UserControl

        Private Sub BTN_MinimizeBounds_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            Dim targetBytes As New MemoryStream()

            Dim globalLeft As Integer = TCurrentAnim.Width
            Dim globalTop As Integer = TCurrentAnim.Height

            For Each frame As AnimationFrame In TCurrentAnim.Frames
                globalLeft = Math.Min(globalLeft, frame.OffsetX)
                globalTop = Math.Min(globalTop, frame.OffsetY)
            Next frame

            For Each frame As AnimationFrame In TCurrentAnim.Frames
                Dim source As Bitmap = CType(Bitmap.FromStream(New MemoryStream(frame.ToArray())), Bitmap)
                Dim locked As New ImagePixelLock(source, False)
                Dim left As Int32 = frame.Width
                Dim top As Int32 = frame.Height
                Dim bottom As Int32 = 0
                Dim right As Int32 = 0

                targetBytes.SetLength(0)

                Using locked
                    ' compute smallest bounds of extracted animation frame
                    Dim destPtr As Integer = locked.Pixels

                    For y As Integer = 0 To frame.Height - 1
                        For x As Integer = 0 To frame.Width - 1
                            Dim pixel As Integer = destPtr
                            destPtr += 1

                            If pixel = 0 Then
                                Continue For
                            End If

                            If x < left Then
                                left = x
                            End If

                            If y < top Then
                                top = y
                            End If

                            If y > bottom Then
                                bottom = y
                            End If

                            If x > right Then
                                right = x
                            End If
                        Next x
                    Next y
                End Using

                Dim newWidth As Integer = right - left
                Dim newHeight As Integer = bottom - top

                If (newWidth <> frame.Width) OrElse (newHeight <> frame.Height) Then
                    If (newWidth > 0) AndAlso (newHeight > 0) Then
                        frame.OffsetX += left - globalLeft
                        frame.OffsetY += top - globalTop
                        frame.Height = newHeight
                        frame.Width = newWidth

                        ' create new bitmap
                        Dim target As New Bitmap(frame.Width, frame.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                        Dim gTarget As Graphics = Graphics.FromImage(target)

                        Using gTarget
                            gTarget.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None

                            gTarget.DrawImage(source, New System.Drawing.Rectangle(0, 0, frame.Width, frame.Height), New System.Drawing.Rectangle(left, top, frame.Width, frame.Height), GraphicsUnit.Pixel)
                        End Using

                        target.Save(targetBytes, ImageFormat.Png)

                        frame.SetBitmap(targetBytes.ToArray())
                    Else
                        ' normalize empty frames
                        frame.SetBitmap(EmptyBitmapBytes)
                        frame.Width = 1
                        frame.Height = 1
                    End If
                End If
            Next frame

            GROUP_AnimDetails.DataContext = New Object()
            GROUP_AnimDetails.DataContext = TCurrentAnim
        End Sub

        Private Sub BTN_RemoveDups_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            Dim dups As New UniqueMap(Of Int64, Object)()
            Dim removals As New List(Of AnimationFrame)()

            For Each frame As AnimationFrame In TCurrentAnim.Frames
                If dups.ContainsKey(frame.Checksum) Then
                    removals.Add(frame)
                Else
                    dups.Add(frame.Checksum, Nothing)
                End If
            Next frame

            For Each frame In removals
                TCurrentAnim.RemoveFrame(frame)
            Next frame

            GROUP_AnimDetails.DataContext = New Object()
            GROUP_AnimDetails.DataContext = TCurrentAnim
        End Sub

        Private Sub BTN_StoreAtlas_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            Dim maxFrameWidth As Int32 = 0
            Dim fullHeight As Int32 = 0
            Dim fullWidth As Int32 = 0

            For Each frame As AnimationFrame In TCurrentAnim.Frames
                fullHeight = Math.Max(fullHeight, frame.Height)
                maxFrameWidth = Math.Max(maxFrameWidth, frame.Width)
            Next frame

            fullHeight = (fullHeight + 10) * TCurrentAnim.Frames.Count
            fullWidth = (maxFrameWidth + 10) * TCurrentAnim.Frames.Count

            Dim target As Bitmap = Nothing

            ' align horizontal
            target = New Bitmap(fullWidth, fullHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            Dim gTarget As Graphics = Graphics.FromImage(target)

            Using gTarget
                Dim offset As Integer = 0

                For Each frame As AnimationFrame In TCurrentAnim.Frames
                    gTarget.DrawImageUnscaled(Bitmap.FromStream(New MemoryStream(frame.ToArray())), offset, 0)

                    offset += maxFrameWidth + 10
                Next frame
            End Using

            System.IO.File.Delete("FrameAtlas.png")
            target.Save("FrameAtlas.png", ImageFormat.Png)
        End Sub

        Private Sub BTN_LoadAtlas_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            Dim maxFrameWidth As Int32 = 0
            Dim fullHeight As Int32 = 0
            Dim fullWidth As Int32 = 0

            For Each frame As AnimationFrame In TCurrentAnim.Frames
                fullHeight = Math.Max(fullHeight, frame.Height)
                maxFrameWidth = Math.Max(maxFrameWidth, frame.Width)
            Next frame

            fullHeight = (fullHeight + 10) * TCurrentAnim.Frames.Count
            fullWidth = (maxFrameWidth + 10) * TCurrentAnim.Frames.Count

            If Not (File.Exists("FrameAtlas.png")) Then
                MessageBox.Show("You have to store an atlas before loading it.")

                Return
            End If

            Dim source As Bitmap = CType(Bitmap.FromFile("FrameAtlas.png"), Bitmap)

            Using source
                If (fullWidth <> source.Width) OrElse (fullHeight <> source.Height) Then
                    MessageBox.Show("Stored atlas does not match current frame alignment!")

                    Return
                End If

                'if ((fullWidth_Hor * fullHeight_Hor) < (fullHeight_Vert * fullWidth_Vert))
                ' horizontal alignment
                Dim offset As Integer = 0
                Dim targetBytes As New MemoryStream()

                For Each frame As AnimationFrame In TCurrentAnim.Frames
                    Dim target As New Bitmap(frame.Width, frame.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                    Dim gTarget As Graphics = Graphics.FromImage(target)

                    targetBytes.SetLength(0)

                    Using target
                        Using gTarget
                            gTarget.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None

                            gTarget.DrawImage(source, New System.Drawing.Rectangle(0, 0, frame.Width, frame.Height), New System.Drawing.Rectangle(offset, 0, frame.Width, frame.Height), GraphicsUnit.Pixel)
                        End Using

                        target.Save(targetBytes, ImageFormat.Png)

                        frame.SetBitmap(targetBytes.ToArray())
                    End Using

                    offset += maxFrameWidth + 10
                Next frame
                'else
                ' align vertical
            End Using

            GROUP_AnimDetails.DataContext = New Object()
            GROUP_AnimDetails.DataContext = TCurrentAnim
        End Sub

        Private Sub BTN_Clear_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            For Each frame In TCurrentAnim.Frames.ToArray()
                TCurrentAnim.RemoveFrame(frame)
            Next frame
        End Sub

        Private Sub BTN_MoveLeft_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            Dim item As AnimationFrame = TryCast(LIST_AnimFrames.SelectedItem, AnimationFrame)

            If item IsNot Nothing Then
                TCurrentAnim.MoveFrameLeft(item)
            End If

            LIST_AnimFrames.SelectedItem = item
        End Sub

        Private Sub BTN_MoveRight_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            Dim item As AnimationFrame = TryCast(LIST_AnimFrames.SelectedItem, AnimationFrame)

            If item IsNot Nothing Then
                TCurrentAnim.MoveFrameRight(item)
            End If

            LIST_AnimFrames.SelectedItem = item
        End Sub
    End Class
End Namespace
