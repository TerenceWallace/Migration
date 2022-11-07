Imports Migration.Common


Namespace Migration

	<Serializable> _
	Public Class CollectionMap(Of TKey, TValue)
		Inherits Map(Of TKey, TValue)

		Public Sub New()
			MyBase.New(MapPolicy.AllowDuplicates)
		End Sub
	End Class

End Namespace
