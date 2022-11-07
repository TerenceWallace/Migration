Imports System.IO
Imports System.Windows.Controls

Namespace Migration.Editor
    ''' <summary>
    ''' Interaction logic for AnimLibraryTab.xaml
    ''' </summary>
    Partial Public Class AnimLibraryTab
        Inherits UserControl

        Private Sub BTN_CreateClass_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            TLibrary.AddClass(EDIT_NewClassName.Text)
        End Sub

        Private Sub BTN_RemoveClass_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            TLibrary.RemoveClass(TCurrentClass)
        End Sub

        Private Sub BTN_CreateSet_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            TCurrentClass.AddAnimationSet(EDIT_NewSetName.Text)
        End Sub

        Private Sub BTN_ShowLibraryDetails_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            WIZARD_Details.SelectedItem = TAB_LibDetails
            ROW_AnimFrameView.Height = New GridLength(0)
            GROUP_AnimDetails.Visibility = System.Windows.Visibility.Hidden
        End Sub

        Private Sub BTN_CreateAnimation_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            TCurrentSet.AddAnimation(EDIT_NewAnimName.Text)
        End Sub

        Private Sub BTN_RemoveFrame_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            If LIST_AnimFrames.SelectedItem IsNot Nothing Then
                TCurrentAnim.RemoveFrame(CType(LIST_AnimFrames.SelectedItem, AnimationFrame))
            End If
        End Sub

        Private Sub BTN_RemoveAnim_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            If TCurrentAnim.SetOrNull IsNot Nothing Then
                TCurrentAnim.SetOrNull.RemoveAnimation(TCurrentAnim)
            End If
        End Sub

        Private Sub BTN_RenameAnim_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            TCurrentAnim.SetOrNull.Rename(TCurrentAnim, EDIT_RenameAnim.Text)
        End Sub

        Private Sub BTN_RemoveSet_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            TCurrentSet.Character.RemoveAnimationSet(TCurrentSet)
        End Sub

        Private Sub BTN_RenameSet_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            TCurrentSet.Character.Rename(TCurrentSet, EDIT_SetRename.Text)
        End Sub

        Private Sub BTN_ClassRename_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            TCurrentClass.Library.Rename(TCurrentClass, EDIT_ClassRename.Text)
        End Sub

        Private Sub BTN_AddAudio_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            TLibrary.AddAudio(EDIT_AudioName.Text, File.ReadAllBytes(EDIT_AudioPath.Text))
        End Sub

        Private Sub BTN_RemoveAudio_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            Dim audio As AudioObject = TryCast(LIST_AudioObjects.SelectedItem, AudioObject)

            If audio IsNot Nothing Then
                TLibrary.RemoveAudio(audio)
            End If
        End Sub

        Private Sub BTN_Play_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            e.Handled = True

            Dim audio As AudioObject = TryCast(LIST_AudioObjects.SelectedItem, AudioObject)

            If audio IsNot Nothing Then
                audio.CreatePlayer(Nothing).Play(0)
            End If
        End Sub
    End Class
End Namespace
