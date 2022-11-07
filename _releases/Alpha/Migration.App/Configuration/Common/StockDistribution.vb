Namespace Migration.Configuration
	Public Class StockDistribution

		Private privateBuilding As BuildingConfiguration
		Public Property Building() As BuildingConfiguration
			Get
				Return privateBuilding
			End Get
			Private Set(ByVal value As BuildingConfiguration)
				privateBuilding = value
			End Set
		End Property
		Private privateQueries As QueryArray
		Public Property Queries() As QueryArray
			Get
				Return privateQueries
			End Get
			Private Set(ByVal value As QueryArray)
				privateQueries = value
			End Set
		End Property

		Friend Sub New(ByVal inBuilding As BuildingConfiguration)
			If inBuilding Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Building = inBuilding
			Queries = New QueryArray()
		End Sub
	End Class
End Namespace
