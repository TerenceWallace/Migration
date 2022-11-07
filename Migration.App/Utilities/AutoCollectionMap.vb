Imports Migration.Common


Namespace Migration

	<Serializable> _
	Public Class AutoCollectionMap(Of TKey, TValue)
		Inherits Map(Of TKey, TValue)

		Public Sub New()
			MyBase.New(MapPolicy.AllowDuplicates Or MapPolicy.DefaultForNonExisting Or MapPolicy.CreateNonExisting)
		End Sub
	End Class

End Namespace
