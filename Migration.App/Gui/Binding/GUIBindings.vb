Imports Migration.Common
Imports Migration.Configuration
Imports Migration.Rendering
Imports Migration.Visuals

Namespace Migration
	Public Class GUIBindings

		Private privateLayout As XMLGUILayout
		Public Property Layout() As XMLGUILayout
			Get
				Return privateLayout
			End Get
			Private Set(ByVal value As XMLGUILayout)
				privateLayout = value
			End Set
		End Property

		Private privateRootElement As RootControl
		Public Property RootElement() As RootControl
			Get
				Return privateRootElement
			End Get
			Private Set(ByVal value As RootControl)
				privateRootElement = value
			End Set
		End Property

		Private privateMainMenu As Control
		Public Property MainMenu() As Control
			Get
				Return privateMainMenu
			End Get
			Private Set(ByVal value As Control)
				privateMainMenu = value
			End Set
		End Property

		Private privateObjectInspector As Control
		Public Property ObjectInspector() As Control
			Get
				Return privateObjectInspector
			End Get
			Private Set(ByVal value As Control)
				privateObjectInspector = value
			End Set
		End Property

		Private privateToolConfigList As ListBox
		Public Property ToolConfigList() As ListBox
			Get
				Return privateToolConfigList
			End Get
			Private Set(ByVal value As ListBox)
				privateToolConfigList = value
			End Set
		End Property

		Private privateStockQuantityList As ListBox
		Public Property StockQuantityList() As ListBox
			Get
				Return privateStockQuantityList
			End Get
			Private Set(ByVal value As ListBox)
				privateStockQuantityList = value
			End Set
		End Property

		Private privateStockDistList As ListBox
		Public Property StockDistList() As ListBox
			Get
				Return privateStockDistList
			End Get
			Private Set(ByVal value As ListBox)
				privateStockDistList = value
			End Set
		End Property

		Private privateMigrantProfessionList As ListBox
		Public Property MigrantProfessionList() As ListBox
			Get
				Return privateMigrantProfessionList
			End Get
			Private Set(ByVal value As ListBox)
				privateMigrantProfessionList = value
			End Set
		End Property

		Private privateMigrantStatList As ListBox
		Public Property MigrantStatList() As ListBox
			Get
				Return privateMigrantStatList
			End Get
			Private Set(ByVal value As ListBox)
				privateMigrantStatList = value
			End Set
		End Property

		Private privateGameTime As Label
		Public Property GameTime() As Label
			Get
				Return privateGameTime
			End Get
			Private Set(ByVal value As Label)
				privateGameTime = value
			End Set
		End Property

		Public Sub New(ByVal inLayout As XMLGUILayout)
			Layout = inLayout
			RootElement = Layout.RootElement

			MainMenu = FindControl(Of Control)("#wizard:MainMenu")
			ObjectInspector = FindControl(Of Control)("#wizard:ObjectInspector")
			ToolConfigList = FindControl(Of ListBox)("#list:ToolConfig")
			StockQuantityList = FindControl(Of ListBox)("#list:StockQuantities")
			StockDistList = FindControl(Of ListBox)("#list:StockDist")
			MigrantProfessionList = FindControl(Of ListBox)("#list:MigrantProfessions")
			MigrantStatList = FindControl(Of ListBox)("#list:MigrantStats")
			GameTime = FindControl(Of Label)("#label:Time")

			InitializeMigrantConfig()

			Update()
		End Sub

		Public Sub Update()
			Game.Setup.Map.UpdateConfig()

			' compute game time
			GameTime.Text = TimeSpan.FromMilliseconds(Game.Setup.Map.CurrentCycle * CyclePoint.CYCLE_MILLIS).ToString().Split("."c)(0)

			UpdateStockQuantities()
			UpdateMigrantStatistics()
			UpdateObjectInspector()
			UpdateToolConfig()
			UpdateStockDist()
		End Sub

		Public Sub UpdateStockQuantities()
			StockQuantityList.Clear()

			For Each resVal As Resource In System.Enum.GetValues(GetType(Resource))
				Dim res As Resource = CType(resVal, Resource)

				If res = Resource.Max Then
					Continue For
				End If

				StockQuantityList.AddItem(New ListBoxItem(res.ToString(), Game.Setup.Map.Configuration.StockQuantities(res).ToString()))
			Next resVal
		End Sub

		Private Sub UpdateMigrantStatistics()
			MigrantStatList.Clear()

			For Each val As MigrantStatisticTypes In System.Enum.GetValues(GetType(MigrantStatisticTypes))

				If val = MigrantStatisticTypes.Max Then
					Continue For
				End If

				MigrantStatList.AddItem(New ListBoxItem(val.ToString()))

				Dim lblText As Label = TryCast(MigrantStatList.FindControl("Text_" & val), Label)
				If lblText IsNot Nothing Then
					lblText.Text = Game.Setup.Map.Configuration.MigrantTypeCounts(val).ToString()
				End If
			Next val

			FindControl(Of Label)("#label:HouseSpaceCount").Text = Game.Setup.Map.Configuration.HouseSpaceCount.ToString()
			FindControl(Of Label)("#label:MigrantCount").Text = Game.Setup.Map.Configuration.MigrantCount.ToString()
			FindControl(Of Label)("#label:SoldierMigrantCount").Text = (Game.Setup.Map.Configuration.MigrantCount + Game.Setup.Map.Configuration.SoldierCount).ToString()
			FindControl(Of Label)("#label:SoldierCount").Text = Game.Setup.Map.Configuration.SoldierCount.ToString()
		End Sub

		Private Sub InitializeMigrantConfig()
			For Each mProfession As MigrantProfessions In System.Enum.GetValues(GetType(MigrantProfessions))
				Dim value As MigrantProfessions = mProfession

				If mProfession <> MigrantProfessions.Max Then
					Dim Title As String = mProfession.ToString()
					MigrantProfessionList.AddItem(New ListBoxItem(Title))

					Dim btnUp As Button = TryCast(MigrantProfessionList.FindControl("Up_" & Title), Button)
					Dim btnPageUp As Button = TryCast(MigrantProfessionList.FindControl("PageUp_" & Title), Button)
					Dim btnDown As Button = TryCast(MigrantProfessionList.FindControl("Down_" & Title), Button)
					Dim btnPageDown As Button = TryCast(MigrantProfessionList.FindControl("PageDown_" & Title), Button)

					If btnUp IsNot Nothing Then
						AddHandler btnUp.OnClick, Sub(sender As Button) Game.Setup.Map.ChangeProfession(value, 1)
					End If
					If btnPageUp IsNot Nothing Then
						AddHandler btnPageUp.OnClick, Sub(sender As Button) Game.Setup.Map.ChangeProfession(value, 5)
					End If
					If btnDown IsNot Nothing Then
						AddHandler btnDown.OnClick, Sub(sender As Button) Game.Setup.Map.ChangeProfession(value, -1)
					End If
					If btnPageDown IsNot Nothing Then
						AddHandler btnPageDown.OnClick, Sub(sender As Button) Game.Setup.Map.ChangeProfession(value, -5)
					End If
				End If
			Next mProfession
		End Sub

		Private Sub UpdateStockDist()
			StockDistList.Clear()

			For i As Integer = 0 To Game.Setup.Map.Configuration.StockDistributions.Length - 1
				Dim dist As StockDistribution = Game.Setup.Map.Configuration.StockDistributions(i)
				Dim queries() As ResourceStack = dist.Building.ResourceStacks.Where(Function(e) e.Direction = StackType.Query).ToArray()
				Dim suffix As String = dist.Building.Character

				StockDistList.AddItem(New ListBoxItem(suffix, queries(0).Type.ToString(), queries(1).Type.ToString(), queries(2).Type.ToString()))

				For Each forQuery As ResourceStack In queries
					Dim query As ResourceStack = forQuery ' localization for lambda expression
					Dim btn As Button = (TryCast(StockDistList.FindControl("Btn_" & query.Type.ToString() & "_" & suffix), Button))

					btn.IsDown = Not(dist.Queries(query.Type))
					AddHandler btn.OnClick, Sub(sender As Button) Game.Setup.Map.ChangeDistributionSetting(dist.Building.Character, query.Type, ((Not sender.IsDown)))
				Next forQuery
			Next i
		End Sub

		Private Sub UpdateToolConfig()
			Dim tools() As Resource = { Resource.Axe, Resource.Hammer, Resource.Hook, Resource.PickAxe, Resource.Saw, Resource.Scythe, Resource.Shovel, Resource.Bow, Resource.Sword, Resource.Spear }

			ToolConfigList.Clear()

			For Each forRes As Resource In tools
				Dim res As Resource = forRes ' localize for lambda expressions...
				Dim Title As String = res.ToString()

				ToolConfigList.AddItem(New ListBoxItem(res.ToString(), Game.Setup.Map.Configuration.Tools(res).Todo.ToString()))

				Dim ctrl As Control = ToolConfigList.FindControl("Overlay_" & Title)
				Dim bar As Image = TryCast(ctrl.Children.First(), Image)
				Dim label As Label = TryCast(ToolConfigList.FindControl("Count_" & Title), Label)
				Dim item As ToolConfiguration = Game.Setup.Map.Configuration.Tools(res)

				AddHandler ctrl.OnMouseButtonDown, Sub(sender, x, y, btn)
					Game.Setup.Map.ChangeToolSetting(res, item.Todo, x / Convert.ToDouble(ctrl.Width))

					label.Text = item.Todo.ToString()
					bar.Width = Convert.ToInt32(CInt(Fix(ctrl.Width * item.Percentage)))
					bar.ProcessScaling()
				End Sub

				AddHandler TryCast(ToolConfigList.FindControl("Up_" & Title), Button).OnClick, Sub(sender As Button) Game.Setup.Map.ChangeToolSetting(res, item.Todo + 1, item.Percentage)
				AddHandler TryCast(ToolConfigList.FindControl("Down_" & Title), Button).OnClick, Sub(sender As Button) Game.Setup.Map.ChangeToolSetting(res, item.Todo - 1, item.Percentage)
			Next forRes
		End Sub

		Public Function FindControl(Of T As Control)(ByVal inName As String) As T
			Dim result As Control = Nothing

			result = RootElement.FindControl(inName)
			If result Is Nothing Then
				Throw New ArgumentException("Config does not contain a control named """ & inName & """.")
			End If

			If Not(TypeOf result Is T) Then
				Throw New ArgumentException("Control named """ & inName & """ is not of expected type """ & GetType(T).FullName & """.")
			End If

			Return CType(result, T)
		End Function

		Public Sub ShowObjectInspector(ByVal inObjectOrNull As RenderableVisual)
			Dim hasInspector As Boolean = False

			If inObjectOrNull IsNot Nothing Then
				If TypeOf inObjectOrNull.UserContext Is VisualBuilding Then
					BuildingInspector.Show(Me, CType(inObjectOrNull.UserContext, VisualBuilding).Building, Nothing)
					hasInspector = True
				End If
			End If

			If Not hasInspector Then
				' check for build task
				Dim task As VisualBuildTask = Nothing
				task = VisualBuildTask.GetBuildTaskAt(Game.Setup.Terrain.GridXY)

				If task IsNot Nothing Then
					BuildingInspector.Show(Me, task.Task.Building, task.Task)
					hasInspector = True
				End If
			End If

			If Not hasInspector Then
				MainMenu.IsVisible = True
				ObjectInspector.IsVisible = False

				BuildingInspector.Close()
			Else
				MainMenu.IsVisible = False
				ObjectInspector.IsVisible = True
			End If
		End Sub

		Public Sub UpdateObjectInspector()
			If BuildingInspector.Instance IsNot Nothing Then
				BuildingInspector.Instance.Update()
			End If
		End Sub

	End Class
End Namespace
