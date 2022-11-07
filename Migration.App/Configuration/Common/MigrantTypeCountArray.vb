Imports Migration.Common

Namespace Migration.Configuration
    Public Class MigrantTypeCountArray
        Private ReadOnly m_Counts(Convert.ToInt32(CInt(MigrantStatisticTypes.Max)) - 1) As Integer

        Default Public ReadOnly Property Item(ByVal index As MigrantStatisticTypes) As Integer
            Get
                Return m_Counts(Convert.ToInt32(CInt(index)))
            End Get
        End Property

        Friend Sub Update(ByVal inConfig As GameConfiguration)
            inConfig.Map.MovableManager.CountMovables(m_Counts)
        End Sub
    End Class
End Namespace
