Imports Migration.Common
Imports Migration.Core

Namespace Migration

	Public Class Foilage
		Inherits PositionTracker

		Private privateState As FoilageState
		Public Property State() As FoilageState
			Get
				Return privateState
			End Get
			Private Set(ByVal value As FoilageState)
				privateState = value
			End Set
		End Property
		Private privateType As FoilageType
		Public Property Type() As FoilageType
			Get
				Return privateType
			End Get
			Private Set(ByVal value As FoilageType)
				privateType = value
			End Set
		End Property
		Public Event OnStateChanged As Procedure(Of Foilage)

		Friend Sub New(ByVal inPosition As Point, ByVal inType As FoilageType, ByVal inState As FoilageState)
			Type = inType
			State = inState
			Position = CyclePoint.FromGrid(inPosition)
		End Sub

		Private Sub RaiseStateChange()
			RaiseEvent OnStateChanged(Me)
		End Sub

		Friend Sub MarkAsGrown()
			If State <> FoilageState.Growing Then
				Throw New InvalidOperationException()
			End If

			State = FoilageState.Grown

			RaiseStateChange()
		End Sub

		Friend Sub MarkAsBeingCut()
			If State <> FoilageState.Grown Then
				Throw New InvalidOperationException()
			End If

			State = FoilageState.BeingCut

			RaiseStateChange()
		End Sub
	End Class

End Namespace
