Imports Migration.Common

Namespace Migration

	<Serializable> _
	Public Class UniqueMap(Of TKey, TValue)
		Inherits Map(Of TKey, TValue)

		Public Sub New()
			MyBase.New(MapPolicy.None)
		End Sub
	End Class
End Namespace
