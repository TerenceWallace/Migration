Imports Migration.Common


Namespace Migration.Configuration

	Public Class GameConfiguration

		Private ReadOnly m_Lock As New Object()

		Private privateStockDistributions As StockDistributionArray
		Public Property StockDistributions() As StockDistributionArray
			Get
				Return privateStockDistributions
			End Get
			Private Set(ByVal value As StockDistributionArray)
				privateStockDistributions = value
			End Set
		End Property
		Private privateStockQuantities As StockQuantityArray
		Public Property StockQuantities() As StockQuantityArray
			Get
				Return privateStockQuantities
			End Get
			Private Set(ByVal value As StockQuantityArray)
				privateStockQuantities = value
			End Set
		End Property
		Private privateMigrantTypeCounts As MigrantTypeCountArray
		Public Property MigrantTypeCounts() As MigrantTypeCountArray
			Get
				Return privateMigrantTypeCounts
			End Get
			Private Set(ByVal value As MigrantTypeCountArray)
				privateMigrantTypeCounts = value
			End Set
		End Property
		Private privateTools As ToolsArray
		Public Property Tools() As ToolsArray
			Get
				Return privateTools
			End Get
			Private Set(ByVal value As ToolsArray)
				privateTools = value
			End Set
		End Property
		Private privateHouseSpaceCount As Integer
		Public Property HouseSpaceCount() As Integer
			Get
				Return privateHouseSpaceCount
			End Get
			Private Set(ByVal value As Integer)
				privateHouseSpaceCount = value
			End Set
		End Property
		Private privateMigrantCount As Integer
		Public Property MigrantCount() As Integer
			Get
				Return privateMigrantCount
			End Get
			Private Set(ByVal value As Integer)
				privateMigrantCount = value
			End Set
		End Property
		Private privateSoldierCount As Integer
		Public Property SoldierCount() As Integer
			Get
				Return privateSoldierCount
			End Get
			Private Set(ByVal value As Integer)
				privateSoldierCount = value
			End Set
		End Property
		Private privateMap As Migration.Game.Map
		Public Property Map() As Migration.Game.Map
			Get
				Return privateMap
			End Get
			Private Set(ByVal value As Migration.Game.Map)
				privateMap = value
			End Set
		End Property

		Friend Sub New(ByVal inParent As Migration.Game.Map)
			If inParent Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Map = inParent
			StockDistributions = New StockDistributionArray(inParent)
			StockQuantities = New StockQuantityArray()
			MigrantTypeCounts = New MigrantTypeCountArray()
			Tools = New ToolsArray(Me)

			Update()
		End Sub

		Friend Sub Update()
			MigrantTypeCounts.Update(Me)
			StockQuantities.Update(Map)

			HouseSpaceCount = Map.BuildingManager.ComputeHouseSpaceCount()
			SoldierCount = MigrantTypeCounts(MigrantStatisticTypes.Speer) + MigrantTypeCounts(MigrantStatisticTypes.Swordsman) + MigrantTypeCounts(MigrantStatisticTypes.Archer)
			MigrantCount = MigrantTypeCounts(MigrantStatisticTypes.Migrant) + MigrantTypeCounts(MigrantStatisticTypes.Worker) + MigrantTypeCounts(MigrantStatisticTypes.Grader) + MigrantTypeCounts(MigrantStatisticTypes.Constructor) + MigrantTypeCounts(MigrantStatisticTypes.Agent) + MigrantTypeCounts(MigrantStatisticTypes.Geologist) + MigrantTypeCounts(MigrantStatisticTypes.Pioneer)
		End Sub

		Friend Sub ChangeToolSetting(ByVal inTool As Resource, ByVal inNewTodo As Integer, ByVal inNewPercentage As Double)
			Tools(inTool).Todo = inNewTodo
			Tools(inTool).Percentage = inNewPercentage
		End Sub

		Friend Sub ChangeDistributionSetting(ByVal inBuildingClass As String, ByVal inResource As Resource, ByVal inIsEnabled As Boolean)
			For i As Integer = 0 To StockDistributions.Length - 1
				If StockDistributions(i).Building.Character = inBuildingClass Then
					StockDistributions(i).Queries(inResource) = inIsEnabled

					Return
				End If
			Next i
		End Sub

	End Class
End Namespace
