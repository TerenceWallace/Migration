Imports System.ComponentModel
Imports System.Drawing.Imaging
Imports System.IO
Imports Migration.Common
Imports Migration.Rendering

Namespace Migration.Editor
    ''' <summary>
    ''' Interaction logic for AnimLibraryTab.xaml
    ''' </summary>
    Partial Public Class AnimLibraryTab
        Inherits System.Windows.Controls.UserControl

        Public ReadOnly Property Library() As Object
            Get
                Return TLibrary
            End Get
        End Property
        Public ReadOnly Property CurrentClass() As Object
            Get
                Return TCurrentClass
            End Get
        End Property
        Public ReadOnly Property CurrentSet() As Object
            Get
                Return TCurrentSet
            End Get
        End Property
        Public ReadOnly Property CurrentAnim() As Object
            Get
                Return TCurrentAnim
            End Get
        End Property

        Private ReadOnly Property AmbientPreview() As System.Drawing.Bitmap
            Get
                If (TCurrentClass Is Nothing) OrElse (TCurrentClass.Sets.Count = 0) OrElse (TCurrentClass.Sets(0).Animations.Count = 0) OrElse (TCurrentClass.Sets(0).Animations(0).Frames.Count = 0) Then
                    Return Nothing
                End If

                Return TCurrentClass.Sets(0).Animations(0).Frames(0).Source
            End Get
        End Property

        Private privateTLibrary As AnimationLibrary
        Friend Property TLibrary() As AnimationLibrary
            Get
                Return privateTLibrary
            End Get
            Private Set(ByVal value As AnimationLibrary)
                privateTLibrary = value
            End Set
        End Property

        Friend ReadOnly Property TCurrentClass() As Character
            Get
                If TAB_ClassDetails Is Nothing Then
                    Return Nothing
                End If

                If TypeOf TAB_ClassDetails.DataContext Is Character Then
                    Return CType(TAB_ClassDetails.DataContext, Character)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Friend ReadOnly Property TCurrentSet() As AnimationSet
            Get
                If TAB_AnimSetDetails Is Nothing Then
                    Return Nothing
                End If

                If TypeOf TAB_AnimSetDetails.DataContext Is AnimationSet Then
                    Return CType(TAB_AnimSetDetails.DataContext, AnimationSet)
                Else
                    Return Nothing
                End If
            End Get
        End Property
        Friend ReadOnly Property TCurrentAnim() As Animation
            Get
                Return TryCast(TAB_AnimDetails.DataContext, Animation)
            End Get
        End Property

        Public ReadOnly Property ResourceStacks() As BindingList(Of PointDouble)
            Get
                Return Nothing
            End Get
        End Property
        Public ReadOnly Property SolidBoundaries() As BindingList(Of RectangleDouble)
            Get
                Return Nothing
            End Get
        End Property

        Private Shared ReadOnly EmptyBitmapBytes() As Byte
        Private m_AnimationPlayer As Renderer

        Public Sub New()
            InitializeComponent()

            If Not (DesignerProperties.GetIsInDesignMode(Me)) Then
                InitializeAnimationPlayer()

                TLibrary = AnimationLibrary.OpenOrCreate("./Resources/Animations")
                DataContext = Me
            End If

            COMBO_AnimResource.ItemsSource = System.Enum.GetValues(GetType(Resource))
            COMBO_AnimResource.SelectedIndex = 0
        End Sub

        Shared Sub New()
            Dim empty As New Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb)

            empty.SetPixel(0, 0, System.Drawing.Color.FromArgb(0, 0, 0, 0))

            Dim stream As New MemoryStream()

            empty.Save(stream, ImageFormat.Png)

            EmptyBitmapBytes = stream.ToArray()
        End Sub

        Public Sub Close()
            TLibrary.Save()
            m_AnimationPlayer.Dispose()
        End Sub

        Private Sub CHECK_UseAmbientSet_Checked(ByVal sender As Object, ByVal e As RoutedEventArgs)
            If Not CHECK_UseAmbientSet.IsChecked.Value Then
                COMBO_ClassAmbientSet.SelectedItem = Nothing
            Else
                If TCurrentClass.Sets.Count = 0 Then
                    CHECK_UseAmbientSet.IsChecked = False
                Else
                    COMBO_ClassAmbientSet.SelectedItem = TCurrentClass.Sets.First()
                End If
            End If
        End Sub

        Private Sub COMBO_ClassAmbientSet_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
            If COMBO_ClassAmbientSet.SelectedItem IsNot Nothing Then
                CHECK_UseAmbientSet.IsChecked = True
            End If
        End Sub

        Private Sub BTN_AnimZoomDefault_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            SLIDER_AnimZoom.Value = 1
        End Sub

        Private Sub EVENT_UpdateAnimationPlayer(ByVal sender As Object, ByVal e As RoutedEventArgs)
            UpdateAnimationPlayer()

            e.Handled = True
        End Sub

        Private Sub LIST_AnimFrames_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
            Dim selFrame = TryCast(LIST_AnimFrames.SelectedValue, AnimationFrame)

            If selFrame Is Nothing Then
                EDIT_FrameX.Text = ""
                EDIT_FrameY.Text = ""
                EDIT_FrameX.IsEnabled = False
                EDIT_FrameY.IsEnabled = False
            Else
                EDIT_FrameX.Text = selFrame.OffsetX.ToString()
                EDIT_FrameY.Text = selFrame.OffsetY.ToString()
                EDIT_FrameX.IsEnabled = True
                EDIT_FrameY.IsEnabled = True
            End If
        End Sub

        Private Sub EDIT_FrameX_TextChanged(ByVal sender As Object, ByVal e As TextChangedEventArgs)
            If LIST_AnimFrames.SelectedValue Is Nothing Then
                Return
            End If

            TryCast(LIST_AnimFrames.SelectedValue, AnimationFrame).OffsetX = Int32.Parse(EDIT_FrameX.Text)
            TCurrentAnim.ComputeDimension()
        End Sub

        Private Sub EDIT_FrameY_TextChanged(ByVal sender As Object, ByVal e As TextChangedEventArgs)
            If LIST_AnimFrames.SelectedValue Is Nothing Then
                Return
            End If

            TryCast(LIST_AnimFrames.SelectedValue, AnimationFrame).OffsetY = Int32.Parse(EDIT_FrameY.Text)
            TCurrentAnim.ComputeDimension()
        End Sub

        Private Sub BTN_SaveLibrary_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles BTN_SaveLibrary.Click
            TLibrary.Save()
        End Sub
    End Class
End Namespace
