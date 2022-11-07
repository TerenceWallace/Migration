Imports Migration.Buildings

Namespace Migration.Common

	Public Delegate Sub DDimensionChangedHandler()

	Public Delegate Sub DOnAddBuilding(Of TSender)(ByVal inSender As TSender, ByVal inBuilding As BaseBuilding)
	Public Delegate Sub DOnRemoveBuilding(Of TSender)(ByVal inSender As TSender, ByVal inBuilding As BaseBuilding)

	Public Delegate Sub DOnAddBuildTask(Of TSender)(ByVal inSender As TSender, ByVal inTask As BuildTask)
	Public Delegate Sub DOnRemoveBuildTask(Of TSender)(ByVal inSender As TSender, ByVal inTask As BuildTask)

	Public Delegate Sub DOnGradingStep(Of TSender)(ByVal inSender As TSender, ByVal inGrader As Movable, ByVal onCompletion As Procedure)

	Public Delegate Sub DOnAddResourceStack(Of TSender)(ByVal inSender As TSender, ByVal inStack As GenericResourceStack)
	Public Delegate Sub DOnRemoveResourceStack(Of TSender)(ByVal inSender As TSender, ByVal inStack As GenericResourceStack)

	Public Delegate Sub Procedure()
	Public Delegate Sub Procedure(Of T)(ByVal inParam As T)
	Public Delegate Sub Procedure(Of T1, T2)(ByVal inParam1 As T1, ByVal inParam2 As T2)

	Public Delegate Sub DOnAddMovable(Of TSender)(ByVal inSender As TSender, ByVal inMovable As Movable)
	Public Delegate Sub DOnRemoveMovable(Of TSender)(ByVal inSender As TSender, ByVal inMovable As Movable)

	Public Delegate Sub DOnAddFoilage(Of TSender)(ByVal inSender As TSender, ByVal inFoilage As Foilage)
	Public Delegate Sub DOnRemoveFoilage(Of TSender)(ByVal inSender As TSender, ByVal inFoilage As Foilage)

	Public Delegate Sub DOnAddStone(Of TSender)(ByVal inSender As TSender, ByVal inStone As Stone)
	Public Delegate Sub DOnRemoveStone(Of TSender)(ByVal inSender As TSender, ByVal inStone As Stone)

	Public Delegate Sub DChangeHandler(Of In TObject, In TValue)(ByVal inSender As TObject, ByVal inOldValue As TValue, ByVal inNewValue As TValue)

	Public Delegate Sub DNotifyHandler(Of In TObject)(ByVal inSender As TObject)
	Public Delegate Sub DNotifyHandler(Of In TObject, In TArg1)(ByVal inSender As TObject, ByVal inArg1 As TArg1)
	Public Delegate Sub DNotifyHandler(Of In TObject, In TArg1, TArg2)(ByVal inSender As TObject, ByVal inArg1 As TArg1, ByVal inArg2 As TArg2)
	Public Delegate Sub DNotifyHandler(Of In TObject, In TArg1, TArg2, TArg3)(ByVal inSender As TObject, ByVal inArg1 As TArg1, ByVal inArg2 As TArg2, ByVal inArg3 As TArg3)

	Public Delegate Sub DOnSelectionChanged()

	Public Delegate Function DPrecisionTimerMillis() As Long

End Namespace