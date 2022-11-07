Imports System
Imports System.Windows.Controls

Namespace Migration.Editor
    ''' <summary>
    ''' Interaction logic for AnimLibraryTab.xaml
    ''' </summary>
    Partial Public Class AnimLibraryTab
        Inherits UserControl

        Private Sub BTN_RemoveResource_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            If LIST_ResourceStacks.SelectedIndex >= 0 Then
                ResourceStacks.RemoveAt(LIST_ResourceStacks.SelectedIndex)
            End If
        End Sub

        Private Sub BTN_ClearResources_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            ResourceStacks.Clear()
        End Sub

        Private Sub BTN_RemoveBound_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            If LIST_SolidBoundaries.SelectedIndex >= 0 Then
                SolidBoundaries.RemoveAt(LIST_SolidBoundaries.SelectedIndex)
            End If
        End Sub

        Private Sub BTN_ClearBoundaries_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            SolidBoundaries.Clear()
        End Sub
    End Class
End Namespace
