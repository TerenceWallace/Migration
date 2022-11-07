Imports Migration.Common
Imports Migration.Core

Namespace Migration

	Public Class Stone
		Inherits PositionTracker

		Private privateRemainingStones As Integer
		Public Property RemainingStones() As Integer
			Get
				Return privateRemainingStones
			End Get
			Private Set(ByVal value As Integer)
				privateRemainingStones = value
			End Set
		End Property
		Private privateActualStones As Integer
		Public Property ActualStones() As Integer
			Get
				Return privateActualStones
			End Get
			Private Set(ByVal value As Integer)
				privateActualStones = value
			End Set
		End Property
		Public Event OnStateChanged As Procedure(Of Stone)

		Friend Sub New(ByVal inPosition As Point, ByVal inInitialStones As Integer)
			If inInitialStones < 0 Then
				Throw New ArgumentOutOfRangeException()
			End If

			RemainingStones = inInitialStones
			ActualStones = RemainingStones
			Position = CyclePoint.FromGrid(inPosition)
		End Sub

		Private Sub RaiseStateChange()
			RaiseEvent OnStateChanged(Me)
		End Sub

		Friend Sub MarkAsBeingCut()
			If RemainingStones <= 0 Then
				Throw New InvalidOperationException()
			End If

			RemainingStones -= 1
		End Sub

		Friend Sub Cut()
			If ActualStones <= 0 Then
				Throw New InvalidOperationException()
			End If

			ActualStones -= 1

			RaiseStateChange()
		End Sub
	End Class

End Namespace
