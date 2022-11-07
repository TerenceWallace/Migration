Imports Migration.Common


Namespace Migration.Jobs
	Friend Class JobOnce
		Inherits JobBase

		Private m_WasRun As Boolean = False
		Friend Event OnCompleted As Procedure(Of JobOnce, Boolean)

		Friend Sub New(ByVal inMovable As Movable)
			MyBase.New(inMovable)
		End Sub

		Protected Sub RaiseCompletion(ByVal succeeded As Boolean)
			Movable.Job = Nothing

			RaiseEvent OnCompleted(Me, succeeded)
		End Sub

		Friend Overrides Function Prepare() As Boolean
			' assignment jobs are only run once per instance
			If m_WasRun Then
				Return False
			End If

			m_WasRun = True

			Return True
		End Function
	End Class
End Namespace
