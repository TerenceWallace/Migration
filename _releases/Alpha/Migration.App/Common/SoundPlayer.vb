Imports System.IO
Imports Migration.Interfaces

Namespace Migration.Common
	Friend Class SoundPlayer
		Inherits System.Media.SoundPlayer
		Implements ISoundPlayer

		Private privateAnimation As Animation
		Public Property Animation() As Animation Implements ISoundPlayer.Animation
			Get
				Return privateAnimation
			End Get
			Set(ByVal value As Animation)
				privateAnimation = value
			End Set
		End Property

		Public Sub New(ByVal inAudioBytes() As Byte)
			MyBase.New(New MemoryStream(inAudioBytes))
		End Sub

		Public Overloads Sub Play(ByVal inDelayMillis As Int64) Implements ISoundPlayer.Play
			If inDelayMillis <= 0 Then
				Play()
			Else
				AnimationUtilities.SynchronizeTask(inDelayMillis, Sub() Play())
			End If
		End Sub

		Private Sub ISoundPlayer_Dispose() Implements ISoundPlayer.Dispose
			Me.Dispose1()
		End Sub
		Public Sub Dispose1()
			MyBase.Dispose()
		End Sub

		Private Sub ISoundPlayer_Stop() Implements ISoundPlayer.Stop
			Me.Stop1()
		End Sub
		Public Sub Stop1()
			MyBase.Stop()
		End Sub
	End Class
End Namespace
