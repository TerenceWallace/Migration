Imports Migration.Common

Namespace Migration.Configuration
	Public Class QueryArray
		Private ReadOnly m_Queries(Convert.ToInt32(Resource.Max) - 1) As Boolean

		Default Public Property Item(ByVal index As Resource) As Boolean
			Get
				Return m_Queries(Convert.ToInt32(CInt(index)))
			End Get
			Friend Set(ByVal value As Boolean)
				m_Queries(Convert.ToInt32(CInt(index))) = value
			End Set
		End Property
	End Class
End Namespace
