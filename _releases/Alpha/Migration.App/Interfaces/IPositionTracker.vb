Imports Migration.Common
Imports Migration.Core

Namespace Migration.Interfaces
	Public Interface IPositionTracker
		Property Position() As CyclePoint
		Event OnPositionChanged As DChangeHandler(Of IPositionTracker, CyclePoint)
	End Interface
End Namespace
