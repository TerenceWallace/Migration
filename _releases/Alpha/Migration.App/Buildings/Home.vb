Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core

Namespace Migration.Buildings
    Public Class Home
        Inherits BaseBuilding

        Private m_MigrantCount As Integer = 0
        Private m_SpawnPoint As CyclePoint

        Friend Sub New(ByVal inParent As BuildingManager, ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
            MyBase.New(inParent, inConfig, inPosition)
            Dim pos As Point = inConfig.ResourceStacks(0).Position

            m_MigrantCount = inConfig.MigrantCount
            m_SpawnPoint = CyclePoint.FromGrid(pos.X + Position.X, pos.Y + Position.Y)
        End Sub

        Friend Overrides Function Update() As Boolean
            ' already producing?
            Dim currentTime As Long = Parent.MoveManager.CurrentCycle
            Dim timeOffset As Long = (currentTime - ProductionStart)

            If timeOffset >= ProductionTime Then
                If m_MigrantCount > 0 Then
                    m_MigrantCount -= 1

                    ' spawn Migrant
                    Parent.MoveManager.AddMovable(m_SpawnPoint, MovableType.Migrant)
                End If

                ProductionStart = currentTime
            End If

            Return False
        End Function
    End Class
End Namespace
