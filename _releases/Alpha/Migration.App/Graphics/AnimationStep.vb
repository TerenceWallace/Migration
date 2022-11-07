Imports Migration.Common
Imports Migration.Core

Namespace Migration
	Friend Class AnimationStep
		Implements IDisposable

		Private privateTarget As Point
		Friend Property Target() As Point
			Get
				Return privateTarget
			End Get
            Private Set(ByVal value As Point)
                privateTarget = value
            End Set
		End Property

		Private privateMillis As Integer
		Friend Property Millis() As Integer
			Get
				Return privateMillis
			End Get
			Set(ByVal value As Integer)
				privateMillis = value
			End Set
		End Property

		Private privateIsCompleted As Boolean
		Friend Property IsCompleted() As Boolean
			Get
				Return privateIsCompleted
			End Get
			Private Set(ByVal value As Boolean)
				privateIsCompleted = value
			End Set
		End Property

		Private privateStartCycles As Long
		Friend Property StartCycles() As Long
			Get
				Return privateStartCycles
			End Get
			Set(ByVal value As Long)
				privateStartCycles = value
			End Set
		End Property

		Private privateOnCompleted As Func(Of Boolean, Boolean)
		Friend Property OnCompleted() As Func(Of Boolean, Boolean)
			Get
				Return privateOnCompleted
			End Get
			Private Set(ByVal value As Func(Of Boolean, Boolean))
				privateOnCompleted = value
			End Set
		End Property

		Private privateOnStarted As Func(Of Boolean)
		Friend Property OnStarted() As Func(Of Boolean)
			Get
				Return privateOnStarted
			End Get
			Private Set(ByVal value As Func(Of Boolean))
				privateOnStarted = value
			End Set
		End Property

		Private privateCustomAnimPlayback As Procedure(Of Procedure)
		Friend Property CustomAnimPlayback() As Procedure(Of Procedure)
			Get
				Return privateCustomAnimPlayback
			End Get
			Private Set(ByVal value As Procedure(Of Procedure))
				privateCustomAnimPlayback = value
			End Set
		End Property

		Friend Sub MarkAsCompleted()
			IsCompleted = True
			OnCompleted = Nothing
		End Sub

		Public Sub Dispose() Implements IDisposable.Dispose
			OnCompleted = Nothing
		End Sub

		Friend Sub New(ByVal inCustomAnimPlayback As Procedure(Of Procedure))
			If inCustomAnimPlayback Is Nothing Then
				Throw New ArgumentNullException()
			End If

			CustomAnimPlayback = inCustomAnimPlayback
		End Sub

		Friend Sub New(ByVal inTarget As Point, ByVal onStepStarted As Func(Of Boolean), ByVal onStepCompleted As Func(Of Boolean, Boolean))
            privateTarget = inTarget
			OnCompleted = onStepCompleted
			OnStarted = onStepStarted
		End Sub

		Friend Sub New(ByVal inMillis As Integer, ByVal onStepStarted As Func(Of Boolean), ByVal onStepCompleted As Func(Of Boolean, Boolean))
			If inMillis <= 0 Then
				Throw New ArgumentOutOfRangeException()
			End If

			Millis = inMillis
			OnCompleted = onStepCompleted
			OnStarted = onStepStarted
		End Sub
	End Class
End Namespace
