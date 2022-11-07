Imports System.IO
Imports System.Threading
Imports Migration.Common
Imports Migration.Configuration

Namespace Migration.Gui
	Public NotInheritable Class Loader

		Private Shared privateGUIConfig As XMLGUIConfig
		Public Shared Property GUIConfig() As XMLGUIConfig
			Get
				Return privateGUIConfig
			End Get
			Private Set(ByVal value As XMLGUIConfig)
				privateGUIConfig = value
			End Set
		End Property

		Private Shared privateGUILayout As XMLGUILayout
		Public Shared Property GUILayout() As XMLGUILayout
			Get
				Return privateGUILayout
			End Get
			Private Set(ByVal value As XMLGUILayout)
				privateGUILayout = value
			End Set
		End Property

		Private Shared privateGUI As GUIBindings
		Public Shared Property GUI() As GUIBindings
			Get
				Return privateGUI
			End Get
			Private Set(ByVal value As GUIBindings)
				privateGUI = value
			End Set
		End Property

		Private Sub New()
		End Sub

		Public Shared Sub Update()
			UpdateGUIConfig()
		End Sub

		Private Shared Sub UpdateGUIConfig()
			Dim backupConfig As XMLGUIConfig = GUIConfig
			Dim backupLayout As XMLGUILayout = GUILayout

			Try
				Game.Setup.Timer.Change(Timeout.Infinite, Timeout.Infinite)

				SyncLock [Global].GUILoadLock
#If DEBUG Then
					' in debug mode, the layout can be updated at runtime (resulting in some kind of GUI designer)
					If GUILayout IsNot Nothing Then
						Dim mDate As Date = (New FileInfo([Global].GetResourcePath("GUI\Default\Layout.xml"))).LastWriteTime
						If mDate > GUILayout.LastWriteTime Then
							GUILayout = Nothing
						End If
					End If

					If GUIConfig IsNot Nothing Then
						If (New FileInfo([Global].GetResourcePath("GUI\Default\Config.xml"))).LastWriteTime > GUIConfig.LastWriteTime Then
							GUIConfig = Nothing
							GUILayout = Nothing
						End If
					End If
#End If

					If GUIConfig Is Nothing Then
						GUIConfig = Nothing
						GUIConfig = XMLGUIConfig.Load(Game.Setup.Renderer, [Global].GetResourcePath("GUI\Default\Config.xml"))
					End If

					If GUILayout Is Nothing Then
						Dim fullPath As String = [Global].GetResourcePath("GUI\Default\Layout.xml")
						Dim source As String = File.ReadAllText(fullPath)

						source = source.Replace("{Race}", Game.Setup.Map.Race.Name)

						GUILayout = XMLGUILayout.Load(source, New FileInfo(fullPath))
						BindGUI()
					Else
						GUI.Update()
					End If
				End SyncLock

				Game.Setup.Timer.Change(1000, 1000)
			Catch e As Exception
				'                
				'                 * Restore previous layout and give the designer a chance to fix errors.
				'                 * When he closes the modal box, the design will try to update again...
				'                 
				GUIConfig = backupConfig
				GUILayout = backupLayout

				Log.LogExceptionModal(e)
				Game.Setup.Timer.Change(1000, 1000)
			End Try
		End Sub

		Private Shared Sub BindGUI()
			Dim buildingLists() As ListBox = { CType(GUILayout.RootElement.FindControl("#list:EconomyBuildings"), ListBox), CType(GUILayout.RootElement.FindControl("#list:FoodBuildings"), ListBox), CType(GUILayout.RootElement.FindControl("#list:WarBuildings"), ListBox), (CType(GUILayout.RootElement.FindControl("#list:OtherBuildings"), ListBox)) }

			If buildingLists(0) Is Nothing Then
				Throw New ArgumentException("Config does not contain a list named ""#list:EconomyBuildings"".")
			End If
			If buildingLists(1) Is Nothing Then
				Throw New ArgumentException("Config does not contain a list named ""#list:FoodBuildings"".")
			End If
			If buildingLists(2) Is Nothing Then
				Throw New ArgumentException("Config does not contain a list named ""#list:WarBuildings"".")
			End If
			If buildingLists(3) Is Nothing Then
				Throw New ArgumentException("Config does not contain a list named ""#list:OtherBuildings"".")
			End If

			For Each building As BuildingConfiguration In Game.Setup.Map.Race.Buildables
				buildingLists(building.TabIndex).AddItem(New ListBoxItem("Race\Romans\Buildings\" & building.Character & ".png") With {.Callback = building})
			Next building

			For Each list As ListBox In buildingLists
				Dim current As ListBox = list

				AddHandler current.OnSelectionChanged, Sub(sender, oldItem, newItem)
					If newItem IsNot Nothing Then
						Game.Setup.ShowGrid(CType(newItem.Callback, BuildingConfiguration))

						For Each toUncheck As ListBox In buildingLists
							If toUncheck Is current Then
								Continue For
							End If

							toUncheck.SelectionIndex = -1
						Next toUncheck
					End If
				End Sub
			Next list

			GUI = New GUIBindings(GUILayout)
		End Sub

	End Class
End Namespace
