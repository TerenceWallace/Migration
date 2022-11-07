Namespace Migration.Interfaces
	Public Interface ISoundPlayer
		Sub Dispose()
		Sub Play(ByVal inDelayMillis As Int64)
		Sub [Stop]()
		Property Animation() As Animation
	End Interface
End Namespace