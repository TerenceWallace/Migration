Imports Migration.Buildings
Imports Migration.Common

Namespace Migration.Configuration
	Public Class StockDistributionArray
		Private ReadOnly m_Dists() As StockDistribution
		Private m_Map As Migration.Game.Map

		Default Public ReadOnly Property Item(ByVal index As Integer) As StockDistribution
			Get
				Return m_Dists(index)
			End Get
		End Property

		Public ReadOnly Property Length() As Integer
			Get
				Return m_Dists.Length
			End Get
		End Property

		Friend Sub New(ByVal inMap As Migration.Game.Map)
			Dim distList As New List(Of StockDistribution)()

			If inMap Is Nothing Then
				Throw New ArgumentNullException()
			End If

			m_Map = inMap

			' currently only MineBuildings are eligable for distribution settings (just makes no sense to include other buildings, if
			' you are not trying to workaround flaws within your economy)
			For Each mine As BuildingConfiguration In m_Map.Race.Buildables.Where(Function(e) GetType(Mine).IsAssignableFrom(e.ClassType))
				Dim dist As New StockDistribution(mine)
				distList.Add(dist)

				' select the resource query with highest quality food, we will enable it by default...
				Dim bestFoodStack As ResourceStack = mine.ResourceStacks.Where(Function(e) e.Direction = StackType.Query).OrderBy(Function(e) e.QualityIndex).First()

				dist.Queries(bestFoodStack.Type) = True
			Next mine

			m_Dists = distList.ToArray()
		End Sub

	End Class
End Namespace