Imports Migration.Common
Imports Migration.Core


Namespace Migration.Jobs
	Friend Class JobCarrying
		Inherits JobOnce

		Private privateProvider As GenericResourceStack
		Friend Property Provider() As GenericResourceStack
			Get
				Return privateProvider
			End Get
			Private Set(ByVal value As GenericResourceStack)
				privateProvider = value
			End Set
		End Property
		Private privateQuery As GenericResourceStack
		Friend Property Query() As GenericResourceStack
			Get
				Return privateQuery
			End Get
			Private Set(ByVal value As GenericResourceStack)
				privateQuery = value
			End Set
		End Property
		Private privateCarryTo As Point
		Friend Property CarryTo() As Point
			Get
				Return privateCarryTo
			End Get
			Private Set(ByVal value As Point)
				privateCarryTo = value
			End Set
		End Property
		Friend Event OnPickedUp As Procedure(Of JobCarrying)

		Friend Sub New(ByVal inMovable As Movable, ByVal inProvider As GenericResourceStack, ByVal inQuery As GenericResourceStack)
			MyBase.New(inMovable)
			If (inProvider Is Nothing) OrElse (inQuery Is Nothing) Then
				Throw New ArgumentNullException()
			End If

			Provider = inProvider
			Query = inQuery
			CarryTo = Query.Position.ToPoint()

			Provider.AddJob(Movable)
			Query.AddJob(Movable)
		End Sub

		Friend Sub New(ByVal inMovable As Movable, ByVal inProvider As GenericResourceStack, ByVal inCarryTo As Point)
			MyBase.New(inMovable)
			If inProvider Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Provider = inProvider
			CarryTo = inCarryTo

			Provider.AddJob(Movable)
		End Sub

		Friend Overrides Function Prepare() As Boolean
			If Not(MyBase.Prepare()) Then
				Return False
			End If

			' pick up resource
			' walk to target
			AddAnimationStep(Provider.Position.ToPoint(), Nothing, AddressOf AnonymousMethod1)

			' add resource to query
			' finish job
			AddAnimationStep(CarryTo, Nothing, AddressOf AnonymousMethod2)

			Return True
		End Function

		Private Function AnonymousMethod1(ByVal successProv As Boolean) As Boolean
			Provider.RemoveJob(Movable)
			If Query IsNot Nothing Then
				Query.RemoveJob(Movable)
			End If
			If Not successProv Then
				Movable.Stop()
				RaiseCompletion(False)
				Return False
			End If
			Provider.RemoveResource()
			Movable.Carrying = Provider.Resource
			If Query IsNot Nothing Then
				Query.AddJob(Movable)
			End If
			RaiseEvent OnPickedUp(Me)
			Return True
		End Function

		Private Function AnonymousMethod2(ByVal successQuery As Boolean) As Boolean
			If Query IsNot Nothing Then
				Query.RemoveJob(Movable)
			End If
			If ((Not successQuery)) OrElse ((Query IsNot Nothing) AndAlso Query.IsRemoved) Then
				Movable.Stop()
				RaiseCompletion(False)
				Return False
			End If
			If Query IsNot Nothing Then
				Query.AddResource()
			End If
			Movable.Carrying = Nothing
			Movable.Stop()
			RaiseCompletion(True)
			Return True
		End Function
	End Class
End Namespace
