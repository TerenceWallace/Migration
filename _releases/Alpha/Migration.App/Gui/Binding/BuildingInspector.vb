Imports Migration.Buildings
Imports Migration.Common
Imports Migration.Core
Imports Migration.Interfaces
Imports Migration.Rendering
Imports Migration.Visuals

Namespace Migration
	Public Class BuildingInspector

		Private Shared privateInstance As BuildingInspector
		Public Shared Property Instance() As BuildingInspector
			Get
				Return privateInstance
			End Get
			Private Set(ByVal value As BuildingInspector)
				privateInstance = value
			End Set
		End Property

		Private privateTask As BuildTask
		Public Property Task() As BuildTask
			Get
				Return privateTask
			End Get
			Private Set(ByVal value As BuildTask)
				privateTask = value
			End Set
		End Property
		Private privateBuilding As BaseBuilding
		Public Property Building() As BaseBuilding
			Get
				Return privateBuilding
			End Get
			Private Set(ByVal value As BaseBuilding)
				privateBuilding = value
			End Set
		End Property
		Private privateBinding As GUIBindings
		Public Property Binding() As GUIBindings
			Get
				Return privateBinding
			End Get
			Private Set(ByVal value As GUIBindings)
				privateBinding = value
			End Set
		End Property

		Private Btn_BoostPrio As Button
		Private Btn_Produce As Button
		Private Btn_SetProdRange As Button
		Private Btn_SetTarget As Button
		Private Btn_Destroy As Button
		Private OI_BuildingImage As Image
		Private OI_BuildingName As Label
		Private OI_BuildProgress As Label
		Private OI_ResourceQueries As ListBox
		Private OI_ResourceProviders As ListBox
		Private OI_StackInfo As Control
		Private OI_MarketInfo As Control
		Private OI_MarketResources As ListBox
		Private OI_MarketTransports As ListBox
		Private QueryInfo As Control
		Private ProviderInfo As Control
		Private m_WorkingArea As VisualWorkingArea = Nothing

		Public Shared Sub Show(ByVal inBindings As GUIBindings, ByVal inBuilding As BaseBuilding, ByVal inTask As BuildTask)
			Abort()

			Instance = New BuildingInspector(inBindings, inBuilding, inTask)
		End Sub

		Public Shared Sub Abort()
			If Instance Is Nothing Then
				Return
			End If

			Instance.Dispose()

			Instance = Nothing
		End Sub

		Public Shared Sub Close()
			If Instance IsNot Nothing Then
				Instance.Save()
			End If

			Abort()
		End Sub

		Private Sub Save()
			If m_WorkingArea IsNot Nothing Then
				Game.Setup.Map.SetWorkingArea(Building, m_WorkingArea.Position.ToPoint())
			End If
		End Sub

		Private Sub Dispose()
			Binding.MainMenu.IsVisible = True
			Binding.ObjectInspector.IsVisible = False

			If m_WorkingArea IsNot Nothing Then
				RemoveHandler Game.Setup.Terrain.OnMouseGridMove, AddressOf TerrainRenderer_OnMouseGridMove

				m_WorkingArea.Dispose()
				m_WorkingArea = Nothing
			End If

			' unregister events
			RemoveHandler Btn_BoostPrio.OnClick, AddressOf Btn_BoostPrio_OnClick
			RemoveHandler Btn_Produce.OnClick, AddressOf Btn_Produce_OnClick
			RemoveHandler Btn_SetProdRange.OnClick, AddressOf Btn_SetProdRange_OnClick
			RemoveHandler Btn_SetTarget.OnClick, AddressOf Btn_SetTarget_OnClick
			RemoveHandler Btn_Destroy.OnClick, AddressOf Btn_Destroy_OnClick

			RemoveHandler Game.Setup.Map.OnRemoveBuildTask, AddressOf Map_OnRemoveBuildTask
			RemoveHandler Game.Setup.Map.OnRemoveBuilding, AddressOf Map_OnRemoveBuilding
		End Sub

		Private Sub New(ByVal inBindings As GUIBindings, ByVal inBuilding As BaseBuilding, ByVal inTask As BuildTask)
			Binding = inBindings
			Building = inBuilding
			Task = inTask

			OI_ResourceQueries = Binding.FindControl(Of ListBox)("#list:OI_ResourceQueries")
			OI_ResourceProviders = Binding.FindControl(Of ListBox)("#list:OI_ResourceProviders")
			OI_BuildingName = Binding.FindControl(Of Label)("#label:OI_BuildingName")
			OI_BuildingImage = Binding.FindControl(Of Image)("#image:OI_BuildingImage")
			OI_StackInfo = Binding.FindControl(Of Control)("#ctrl:OI_StackInfo")
			OI_MarketInfo = Binding.FindControl(Of Control)("#ctrl:OI_MarketInfo")
			OI_MarketResources = Binding.FindControl(Of ListBox)("#list:OI_MarketResources")
			OI_MarketTransports = Binding.FindControl(Of ListBox)("#list:OI_MarketTransports")
			OI_BuildProgress = Binding.FindControl(Of Label)("#label:OI_BuildProgress")

			Btn_BoostPrio = TryCast(Binding.ObjectInspector.FindControl("Btn_BoostPrio"), Button)
			Btn_Produce = TryCast(Binding.ObjectInspector.FindControl("Btn_Produce"), Button)
			Btn_SetProdRange = TryCast(Binding.ObjectInspector.FindControl("Btn_SetProdRange"), Button)
			Btn_SetTarget = TryCast(Binding.ObjectInspector.FindControl("Btn_SetTarget"), Button)
			Btn_Destroy = TryCast(Binding.ObjectInspector.FindControl("Btn_Destroy"), Button)
			QueryInfo = Binding.ObjectInspector.FindControl("QueryInfo")
			ProviderInfo = Binding.ObjectInspector.FindControl("ProviderInfo")

			AddHandler Game.Setup.Map.OnRemoveBuildTask, AddressOf Map_OnRemoveBuildTask
			AddHandler Game.Setup.Map.OnRemoveBuilding, AddressOf Map_OnRemoveBuilding

			AddHandler Btn_BoostPrio.OnClick, AddressOf Btn_BoostPrio_OnClick
			AddHandler Btn_Produce.OnClick, AddressOf Btn_Produce_OnClick
			AddHandler Btn_SetProdRange.OnClick, AddressOf Btn_SetProdRange_OnClick
			AddHandler Btn_SetTarget.OnClick, AddressOf Btn_SetTarget_OnClick
			AddHandler Btn_Destroy.OnClick, AddressOf Btn_Destroy_OnClick

			Update()

			Binding.MainMenu.IsVisible = False
			Binding.ObjectInspector.IsVisible = True
		End Sub

		Private Sub Map_OnRemoveBuilding(ByVal inSender As Migration.Game.Map, ByVal inBuilding As BaseBuilding)
			If inBuilding Is Building Then
				Abort()
			End If
		End Sub

		Private Sub Map_OnRemoveBuildTask(ByVal inSender As Migration.Game.Map, ByVal inTask As BuildTask)
			If inTask Is Task Then
				Abort()
			End If
		End Sub

		Private Sub Btn_Destroy_OnClick(ByVal inSender As Button)
			If Task IsNot Nothing Then
				Game.Setup.Map.RemoveBuildTask(Task)
			Else
				Game.Setup.Map.RemoveBuilding(Building)
			End If
		End Sub

		Private Sub Btn_BoostPrio_OnClick(ByVal inSender As Button)
			If Task IsNot Nothing Then
				inSender.IsDown = False
				Game.Setup.Map.RaiseTaskPriority(Task)
			Else
				If inSender.IsDown Then
					Game.Setup.Map.RaiseBuildingPriority(Building)
				Else
					Game.Setup.Map.LowerBuildingPriority(Building)
				End If
			End If
		End Sub

		Private Sub Btn_Produce_OnClick(ByVal inSender As Button)
			Dim isSuspended As Boolean = inSender.IsDown
			If Task IsNot Nothing Then
				Game.Setup.Map.SetTaskSuspended(Task, isSuspended)
			Else
				Game.Setup.Map.SetBuildingSuspended(Building, isSuspended)
			End If
		End Sub

		Private Sub Btn_SetProdRange_OnClick(ByVal inSender As Button)
			' show current working area
			m_WorkingArea = New VisualWorkingArea()
			m_WorkingArea.Databind(Game.Setup.Terrain, Building.WorkingArea, (TryCast(Building, IBuildingWithWorkingArea)).WorkingRadius)

			AddHandler Game.Setup.Terrain.OnMouseGridMove, AddressOf TerrainRenderer_OnMouseGridMove
		End Sub

		Private Sub TerrainRenderer_OnMouseGridMove(ByVal inSender As TerrainRenderer, ByVal inNewGridXY As Point)
			If m_WorkingArea IsNot Nothing Then
				m_WorkingArea.Position = CyclePoint.FromGrid(inNewGridXY)
			End If
		End Sub

		Private Sub Btn_SetTarget_OnClick(ByVal inSender As Button)
		End Sub


		Public Sub Update()
			If Task IsNot Nothing Then
				UpdateBuildTask()
			ElseIf TypeOf Building Is Market Then
				UpdateMarketBuilding()
			ElseIf (TypeOf Building Is Workshop) OrElse (TypeOf Building Is Factory) OrElse (TypeOf Building Is Home) Then
				UpdateProductionBuilding()
			End If
		End Sub

		Private Sub DefaultBuildingUpdate()
			OI_ResourceQueries.Clear()
			OI_ResourceProviders.Clear()

			OI_BuildingName.Text = Building.Config.Name
			OI_BuildingImage.SourceString = "Race/Romans/Buildings/" & Building.Config.Character & ".png"

			OI_StackInfo.IsVisible = True
			OI_MarketInfo.IsVisible = False
			OI_BuildProgress.IsVisible = False
			Btn_BoostPrio.IsToggleable = True
			Btn_BoostPrio.IsDown = False

			QueryInfo.IsVisible = Building.QueriesReadonly.Count() > 0
			ProviderInfo.IsVisible = Building.ProvidersReadonly.Count() > 0
		End Sub

		Public Sub UpdateBuildTask()
			DefaultBuildingUpdate()

			Btn_BoostPrio.IsVisible = True
			Btn_Produce.IsVisible = True
			Btn_SetProdRange.IsVisible = False
			Btn_SetTarget.IsVisible = False
			OI_BuildProgress.IsVisible = True

			' compute progress
			Dim progress As Integer = Convert.ToInt32(CInt(Fix(Task.Progress * 50)))

			If Task.IsGraded Then
				progress += 50
			End If
			If Task.IsBuilt Then
				progress = 100
			End If

			OI_BuildProgress.Text = progress.ToString() & "%"

			' visualize queries
			For Each query As GenericResourceStack In Task.Queries
				If OI_ResourceQueries.Count >= 3 Then
					Exit For
				End If

				If query Is Nothing Then
					Continue For
				End If

				OI_ResourceQueries.AddItem(New ListBoxItem(query.Resource.ToString(), query.Available.ToString(), query.VirtualCount.ToString()))
			Next query

			QueryInfo.IsVisible = True
			ProviderInfo.IsVisible = False
		End Sub

		Private Sub UpdateMarketBuilding()
			Dim market As Market = TryCast(Building, Market)
			Dim btnID As Integer = 0

			DefaultBuildingUpdate()

			Btn_BoostPrio.IsVisible = True
			Btn_Produce.IsVisible = True
			Btn_SetProdRange.IsVisible = False
			Btn_SetTarget.IsVisible = True

			OI_StackInfo.IsVisible = False
			OI_MarketInfo.IsVisible = True

			OI_MarketResources.Clear()
			OI_MarketTransports.Clear()

			For Each resVal As Resource In System.Enum.GetValues(GetType(Resource))
				Dim res As Resource = resVal
				If res <> Resource.Max Then
					Me.OI_MarketResources.AddItem(New ListBoxItem(New String() { res.ToString(), btnID.ToString() }))
					AddHandler TryCast(Me.OI_MarketResources.FindControl(("Btn_" & btnID)), Button).OnClick, Sub(sender As Button) Game.Setup.Map.AddMarketTransport(market, res)
					btnID += 1
				End If
			Next resVal


			btnID = 0

			For Each resVal As Resource In market.Transports
				Dim res As Resource = resVal
				Me.OI_MarketTransports.AddItem(New ListBoxItem(New String() { res.ToString(), btnID.ToString() }))
				AddHandler TryCast(Me.OI_MarketTransports.FindControl(("Btn_" & btnID)), Button).OnClick, Sub(sender As Button) Game.Setup.Map.RemoveMarketTransport(market, res)
				btnID += 1

			Next resVal
		End Sub

		Private Sub UpdateProductionBuilding()
			DefaultBuildingUpdate()

			Btn_BoostPrio.IsVisible = Building.QueriesReadonly.Count() > 0
			Btn_Produce.IsVisible = TypeOf Building Is ISuspendableBuilding
			Btn_SetProdRange.IsVisible = TypeOf Building Is IBuildingWithWorkingArea
			Btn_SetTarget.IsVisible = False

			For Each query As GenericResourceStack In Building.QueriesReadonly
				If OI_ResourceQueries.Count >= 3 Then
					Exit For
				End If

				If query Is Nothing Then
					Continue For
				End If

				OI_ResourceQueries.AddItem(New ListBoxItem(query.Resource.ToString(), query.Available.ToString(), query.VirtualCount.ToString()))
			Next query

			For Each prov As GenericResourceStack In Building.ProvidersReadonly
				If OI_ResourceProviders.Count >= 6 Then
					Exit For
				End If

				If (prov Is Nothing) OrElse (prov.Resource = Resource.Max) Then
					Continue For
				End If

				OI_ResourceProviders.AddItem(New ListBoxItem(prov.Resource.ToString(), prov.Available.ToString(), prov.VirtualCount.ToString()))
			Next prov
		End Sub
	End Class
End Namespace
