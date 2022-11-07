Imports Migration.Common

Namespace Migration.Configuration
	Public Class StockQuantityArray
		Private ReadOnly m_Counts(Convert.ToInt32(CInt(Resource.Max)) - 1) As Integer

		Default Public ReadOnly Property Item(ByVal index As Resource) As Integer
			Get
				Return m_Counts(Convert.ToInt32(CInt(index)))
			End Get
		End Property

		Friend Sub Update(ByVal inMap As Migration.Game.Map)
			inMap.ResourceManager.CountResources(m_Counts)
		End Sub
	End Class
End Namespace
