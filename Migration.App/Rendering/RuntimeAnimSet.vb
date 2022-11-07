Namespace Migration.Rendering

	Friend Class RuntimeAnimSet
		Private privateDuration As Long
		Public Property Duration() As Long
			Get
				Return privateDuration
			End Get
			Private Set(ByVal value As Long)
				privateDuration = value
			End Set
		End Property
		Private privateSet As AnimationSet
		Public Property [Set]() As AnimationSet
			Get
				Return privateSet
			End Get
			Private Set(ByVal value As AnimationSet)
				privateSet = value
			End Set
		End Property
		Private privateAnimations() As Animation
		Public Property Animations() As Animation()
			Get
				Return privateAnimations
			End Get
			Private Set(ByVal value As Animation())
				privateAnimations = value
			End Set
		End Property

		Public Sub New(ByVal inSet As AnimationSet)
			[Set] = inSet
			Duration = inSet.DurationMillisBounded
			Animations = inSet.Animations.OrderBy(Function(e) e.RenderIndex).ToArray()
		End Sub
	End Class
End Namespace
