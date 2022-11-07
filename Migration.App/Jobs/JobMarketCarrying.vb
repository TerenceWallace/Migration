Imports Migration.Buildings
Imports Migration.Core

Namespace Migration.Jobs
	Friend Class JobMarketCarrying
		Inherits JobOnce

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
		Private privateMarket As Market
		Friend Property Market() As Market
			Get
				Return privateMarket
			End Get
			Private Set(ByVal value As Market)
				privateMarket = value
			End Set
		End Property

		Friend Sub New(ByVal inMarket As Market, ByVal inMovable As Movable, ByVal inQuery As GenericResourceStack, ByVal inCarryTo As Point)
			MyBase.New(inMovable)
			If (inQuery Is Nothing) OrElse (inMarket Is Nothing) Then
				Throw New ArgumentNullException()
			End If

			Query = inQuery
			CarryTo = inCarryTo
			Market = inMarket
		End Sub

		Friend Overrides Function Prepare() As Boolean
			If Not(MyBase.Prepare()) Then
				Return False
			End If

			' pick up resource
			AddAnimationStep(Query.Position.ToPoint(), Nothing, AddressOf AnonymousMethod1)

			' drop resource 
			' special marked drop, to prevent re-carrying 
			AddAnimationStep(CarryTo, Nothing, AddressOf AnonymousMethod2)

			Return True
		End Function

		Private Function AnonymousMethod1(ByVal successProv As Boolean) As Boolean
			If Not successProv Then
				Movable.Stop()
				RaiseCompletion(False)
				Return False
			End If
			Query.RemoveResource()
			Movable.Carrying = Query.Resource
			Return True
		End Function

		Private Function AnonymousMethod2(ByVal successQuery As Boolean) As Boolean
			successQuery = successQuery AndAlso Not Query.IsRemoved
			If successQuery Then
				Movable.Carrying = Nothing
				Market.Parent.ResourceManager.DropMarketResource(Market, CarryTo, Query.Resource, 1)
			End If
			Movable.Stop()
			RaiseCompletion(successQuery)
			Movable.Job = Nothing
			Return successQuery
		End Function
	End Class
End Namespace
