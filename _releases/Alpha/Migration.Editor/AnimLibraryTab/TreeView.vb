Imports System
Imports System.Windows.Controls

Namespace Migration.Editor
    ''' <summary>
    ''' Interaction logic for AnimLibraryTab.xaml
    ''' </summary>
    Partial Public Class AnimLibraryTab
        Inherits UserControl

        Private Sub TreeView_SelectedItemChanged(ByVal sender As Object, ByVal e As RoutedPropertyChangedEventArgs(Of Object))
            Dim item As Object = Nothing

            If e IsNot Nothing Then
                e.Handled = True

                item = e.NewValue
            Else
                item = sender
            End If

            If TypeOf item Is Animation Then
                Dim anim As Animation = TryCast(item, Animation)

                WIZARD_Details.SelectedItem = TAB_AnimDetails
                TAB_AnimDetails.DataContext = Nothing
                TAB_AnimDetails.DataContext = anim
                GROUP_AnimDetails.DataContext = Nothing
                GROUP_AnimDetails.DataContext = anim
                GROUP_AnimDetails.Header = "Details For Animation """ & anim.Name & """:"
                GROUP_AnimDetails.Visibility = System.Windows.Visibility.Visible
                ROW_AnimFrameView.Height = New GridLength(GROUP_AnimDetails.Height)

                ' also update animation set if changed
                If anim.SetOrNull IsNot TCurrentSet Then
                    TAB_AnimSetDetails.DataContext = Nothing
                    TAB_AnimSetDetails.DataContext = anim.SetOrNull

                    If anim.SetOrNull.Character IsNot TCurrentClass Then
                        TAB_ClassDetails.DataContext = Nothing
                        TAB_ClassDetails.DataContext = anim.SetOrNull.Character
                    End If
                End If
            End If

            If TypeOf item Is AnimationSet Then
                Dim animSet As AnimationSet = TryCast(item, AnimationSet)

                WIZARD_Details.SelectedItem = TAB_AnimSetDetails
                TAB_AnimSetDetails.DataContext = Nothing
                TAB_AnimSetDetails.DataContext = animSet
                GROUP_AnimDetails.Visibility = System.Windows.Visibility.Hidden
                ROW_AnimFrameView.Height = New GridLength(0)

                ' also update animation class if changed
                If animSet.Character IsNot TCurrentClass Then
                    TAB_ClassDetails.DataContext = Nothing
                    TAB_ClassDetails.DataContext = animSet.Character
                End If
            End If

            If TypeOf item Is Character Then
                If TCurrentClass IsNot Nothing Then
                    TCurrentClass.AmbientSet = CType(COMBO_ClassAmbientSet.SelectedItem, AnimationSet)
                End If

                Dim Character As Character = TryCast(item, Character)

                WIZARD_Details.SelectedItem = TAB_ClassDetails
                TAB_AnimSetDetails.DataContext = Nothing
                TAB_AnimDetails.DataContext = Nothing
                TAB_ClassDetails.DataContext = Nothing
                TAB_ClassDetails.DataContext = Character
                COMBO_ClassAmbientSet.SelectedItem = Character.AmbientSet
                CHECK_UseAmbientSet.IsChecked = Character.AmbientSet IsNot Nothing
                ROW_AnimFrameView.Height = New GridLength(0)
                GROUP_AnimDetails.Visibility = System.Windows.Visibility.Hidden
            End If

            TAB_StacksAndPlane.DataContext = Nothing
            TAB_StacksAndPlane.DataContext = Me

            UpdateAnimationPlayer()
        End Sub
    End Class
End Namespace
