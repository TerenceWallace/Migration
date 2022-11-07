Imports Migration.Common
Imports Migration.Core
Imports Migration.Interfaces

Namespace Migration

	''' <summary>
	''' Intended to provide some default behavior for all essentially non-movable objects,
	''' which still are renderable, like resources or buildings.
	''' </summary>
	Public Class PositionTracker
		Implements IPositionTracker

		Private m_Position As CyclePoint

		Public Property Position() As CyclePoint Implements IPositionTracker.Position
			Get
				Return m_Position
			End Get
			Set(ByVal value As CyclePoint)
				Dim hasListener As Boolean = (OnPositionChangedEvent IsNot Nothing)
				Dim old As New Point()

				If hasListener Then
					old = m_Position.ToPoint()
				End If

				m_Position = value

				If hasListener AndAlso ((old.X <> value.XGrid) OrElse (old.Y <> value.YGrid)) Then
					RaiseEvent OnPositionChanged(Me, CyclePoint.FromGrid(old), value)
				End If
			End Set
		End Property

		Private privateUserContext As Object
		Public Property UserContext() As Object
			Get
				Return privateUserContext
			End Get
			Set(ByVal value As Object)
				privateUserContext = value
			End Set
		End Property

		Public Event OnPositionChanged As DChangeHandler(Of IPositionTracker, CyclePoint) Implements IPositionTracker.OnPositionChanged
	End Class

End Namespace
