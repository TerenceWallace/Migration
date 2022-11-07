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

Namespace Migration.Editor
    Public Class Wizard
        Inherits System.Windows.Controls.TabControl

        Private m_Style As Style

        Public Sub New()
            m_Style = New Style(GetType(TabItem))
            Dim setter As New Setter(TabItem.TemplateProperty, New ControlTemplate(GetType(TabItem)))

            m_Style.Setters.Add(setter)

            AddHandler Loaded, AddressOf Wizard_Loaded

            Background = Brushes.Transparent
            BorderThickness = New Thickness(0, 0, 0, 0)
        End Sub

        Private Sub Wizard_Loaded(ByVal sender As Object, ByVal eargs As RoutedEventArgs)
            For Each e In Items
                TryCast(e, TabItem).Style = m_Style
            Next e
        End Sub
    End Class
End Namespace
