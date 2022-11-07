Imports Migration.Buildings
Imports Migration.Common
Imports Migration.Core

Namespace Migration
	Public MustInherit Class VisualUtilities

		Private Shared privateInstance As VisualUtilities
		Public Shared ReadOnly Property Instance() As VisualUtilities
			Get
				If privateInstance Is Nothing Then
					privateInstance = New AnimationUtilities()
				End If
				Return privateInstance
			End Get
		End Property

		''' <summary>
		''' Schedules the given task for execution at the next rendering cycle. 
		''' </summary>
		Public Shared Sub SynchronizeTask(ByVal inTask As Procedure)
			Instance.SynchronizeTaskImpl(inTask)
		End Sub

		Protected MustOverride Sub SynchronizeTaskImpl(ByVal inTask As Procedure)

		Public Shared Sub SynchronizeTask(ByVal inDueTimeMillis As Long, ByVal inTask As Procedure)
			Instance.SynchronizeTaskImpl(inDueTimeMillis, inTask)
		End Sub
		Protected MustOverride Sub SynchronizeTaskImpl(ByVal inDueTimeMillis As Long, ByVal inTask As Procedure)

		Public Shared Sub Hide(ByVal inMovable As Movable)
			Instance.HideImpl(inMovable)
		End Sub
		Protected MustOverride Sub HideImpl(ByVal inMovable As Movable)

		Public Shared Sub DirectedAnimation(ByVal inMovable As Movable, ByVal inClass As String, ByVal inDirectionOverride? As Direction, ByVal inRestart As Boolean, ByVal inRepeat As Boolean)
			Instance.DirectedAnimationImpl(inMovable, inClass, inDirectionOverride, inRestart, inRepeat)
		End Sub
		Protected MustOverride Sub DirectedAnimationImpl(ByVal inMovable As Movable, ByVal inClass As String, ByVal inDirectionOverride? As Direction, ByVal inRestart As Boolean, ByVal inRepeat As Boolean)

		Public Shared Sub Animate(ByVal inMovable As Movable, ByVal inClass As String, ByVal inRestart As Boolean, ByVal inRepeat As Boolean)
			Instance.AnimateImpl(inMovable, inClass, inRestart, inRepeat)
		End Sub
		Protected MustOverride Sub AnimateImpl(ByVal inMovable As Movable, ByVal inClass As String, ByVal inRestart As Boolean, ByVal inRepeat As Boolean)

		Public Shared Sub Animate(ByVal inBuilding As BaseBuilding, ByVal inClass As String)
			Instance.AnimateImpl(inBuilding, inClass)
		End Sub
		Protected MustOverride Sub AnimateImpl(ByVal inBuilding As BaseBuilding, ByVal inClass As String)

		Public Shared Sub AnimateAroundCenter(ByVal inCenter As Point, ByVal inCharacter As String, ByVal inAnimationSet As String)
			Instance.AnimateAroundCenterImpl(inCenter, inCharacter, inAnimationSet)
		End Sub
		Protected MustOverride Sub AnimateAroundCenterImpl(ByVal inCenter As Point, ByVal inCharacter As String, ByVal inAnimationSet As String)

		Public Shared Function GetDurationMillis(ByVal inMovable As Movable, ByVal inCharacter As String) As Integer
			Return Instance.GetDurationMillisImpl(inMovable, inCharacter)
        End Function

		Protected MustOverride Function GetDurationMillisImpl(ByVal inMovable As Movable, ByVal inCharacter As String) As Integer

		Public Shared Function GetDurationMillis(ByVal inBuilding As BaseBuilding, ByVal inCharacter As String) As Integer
			Return Instance.GetDurationMillisImpl(inBuilding, inCharacter)
		End Function
		Protected MustOverride Function GetDurationMillisImpl(ByVal inBuilding As BaseBuilding, ByVal inCharacter As String) As Integer

		Public Shared Function HasAnimation(ByVal inMovable As Movable, ByVal inCharacter As String) As Boolean
			Return Instance.HasAnimationImpl(inMovable, inCharacter)
		End Function
		Protected MustOverride Function HasAnimationImpl(ByVal inMovable As Movable, ByVal inCharacter As String) As Boolean

		Public Shared Function HasAnimation(ByVal inFoilage As Foilage, ByVal inCharacter As String) As Boolean
			Return Instance.HasAnimationImpl(inFoilage, inCharacter)
		End Function
		Protected MustOverride Function HasAnimationImpl(ByVal inFoilage As Foilage, ByVal inCharacter As String) As Boolean

		Public Shared Function HasAnimation(ByVal inBuilding As BaseBuilding, ByVal inCharacter As String) As Boolean
			Return Instance.HasAnimationImpl(inBuilding, inCharacter)
		End Function
		Protected MustOverride Function HasAnimationImpl(ByVal inBuilding As BaseBuilding, ByVal inCharacter As String) As Boolean

		Public Shared Function GetDurationMillis(ByVal inFoilage As Foilage, ByVal inState As String) As Integer
			Return Instance.GetDurationMillisImpl(inFoilage, inState)
		End Function
		Protected MustOverride Function GetDurationMillisImpl(ByVal inFoilage As Foilage, ByVal inState As String) As Integer

		Public Shared Sub Animate(ByVal inFoilage As Foilage, ByVal inState As String, ByVal inRestart As Boolean, ByVal inRepeat As Boolean)
			Instance.AnimateImpl(inFoilage, inState, inRestart, inRepeat)
		End Sub
		Protected MustOverride Sub AnimateImpl(ByVal inFoilage As Foilage, ByVal inState As String, ByVal inRestart As Boolean, ByVal inRepeat As Boolean)

		Public Shared Sub Animate(ByVal inStone As Stone)
			Instance.AnimateImpl(inStone)
		End Sub
		Protected MustOverride Sub AnimateImpl(ByVal inStone As Stone)

		Public Shared Sub ShowMessage(ByVal inMessage As String)
			Instance.ShowMessageImpl(inMessage)
		End Sub
		Protected MustOverride Sub ShowMessageImpl(ByVal inMessage As String)

		Public Shared Sub ShowError(ByVal inMessage As String)
			Instance.ShowErrorImpl(inMessage)
		End Sub
		Protected MustOverride Sub ShowErrorImpl(ByVal inMessage As String)
	End Class
End Namespace
