Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Core
Imports Migration.Interfaces
Imports Migration.Jobs

Namespace Migration.Buildings
	Public Class Market
		Inherits BaseBuilding
		Implements IBuildingWithWorkingArea

		Private ReadOnly m_Transports As New List(Of Resource)()
		Private ReadOnly m_QueryJobCounts() As Integer

		Private privateWorkingRadius As Integer
		Public Property WorkingRadius() As Integer Implements IBuildingWithWorkingArea.WorkingRadius
			Get
				Return privateWorkingRadius
			End Get
			Set(ByVal value As Integer)
				privateWorkingRadius = value
			End Set
		End Property
		Public ReadOnly Property Transports() As IEnumerable(Of Resource)
			Get
				Return m_Transports.AsReadOnly()
			End Get
		End Property

		Friend Sub New(ByVal inParent As BuildingManager, ByVal inConfig As BuildingConfiguration, ByVal inPosition As Point)
			MyBase.New(inParent, inConfig, inPosition)
			Dim queryCount As Integer = 0

			WorkingRadius = 5
			WorkingArea = inPosition

			For Each stack As ResourceStack In inConfig.ResourceStacks
				If stack.Direction <> StackType.Query Then
					Continue For
				End If

				If stack.Type <> Resource.Max Then
					Throw New ArgumentException("A market building shall only contain queries of ""Resource.Max"" type.")
				End If

				queryCount += 1
			Next stack

			If queryCount = 0 Then
				Throw New ArgumentException("A market shall contain at least one resource query.")
			End If

			m_QueryJobCounts = New Integer(queryCount - 1){}
		End Sub

		Friend Sub AddResourceTransport(ByVal inResource As Resource)
			Do While m_Transports.Count >= Queries.Count
				RemoveResourceTransport(m_Transports(0))
			Loop

			' find empty query
			Dim i As Integer = 0

			Do While i < Queries.Count
				If Queries(i) Is Nothing Then
					Exit Do
				End If
				i += 1
			Loop

			If i >= Queries.Count Then
				Throw New ApplicationException()
			End If

			' add resource query
			Dim stackCfg As ResourceStack = QueriesConfig(i)
			Dim stackPos As CyclePoint = CyclePoint.FromGrid(stackCfg.Position.X + Position.X, stackCfg.Position.Y + Position.Y)

			Queries(i) = Parent.ResourceManager.AddResourceStack(Me, stackPos, StackType.Query, inResource, stackCfg.MaxStackSize)

			m_Transports.Add(inResource)
		End Sub

		Friend Sub RemoveResourceTransport(ByVal inResource As Resource)
			If Not(m_Transports.Remove(inResource)) Then
				Return
			End If

			' find resource query of given type
			Dim query As GenericResourceStack = Nothing
			Dim i As Integer = 0

			Do While i < Queries.Count
				If (Queries(i) IsNot Nothing) AndAlso (Queries(i).Resource = inResource) Then
					query = Queries(i)

					Exit Do
				End If
				i += 1
			Loop

			If query Is Nothing Then
				Throw New ApplicationException()
			End If

			' remove resource query and drop resources around market
			Queries(i) = Nothing

			Parent.ResourceManager.DropResource(Position.ToPoint(), query.Resource, query.Available)
			Parent.ResourceManager.RemoveResource(query)
		End Sub

		Friend Overrides Function Update() As Boolean

			Dim hasTransport As Boolean = False

			' schedule one Migrant for each queue per update, at maximum...
			For fori As Integer = 0 To Queries.Count - 1
				Dim i As Integer = fori ' localization for lambda expression
				Dim query As GenericResourceStack = Queries(i)

				If (query Is Nothing) OrElse ((query.Available - m_QueryJobCounts(i)) <= 0) Then
					Continue For
				End If

				' find Migrant around query
				Dim Migrant As Movable = Parent.MoveManager.FindFreeMovableAround(query.Position.ToPoint(), MovableType.Migrant)

				If Migrant Is Nothing Then
					Exit For
				End If

				' carry resource...
				Dim job As New JobMarketCarrying(Me, Migrant, query, WorkingArea)

				m_QueryJobCounts(i) += 1

				Migrant.Job = job

				AddHandler job.OnCompleted, Sub(unused, succeeded) m_QueryJobCounts(i) -= 1

			Next fori

			Return hasTransport
		End Function
	End Class
End Namespace
