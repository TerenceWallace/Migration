Namespace Migration
	<Serializable()> _
	Friend Class RoutingNodeLink
		Public ReadOnly Node As RoutingNode
		Private privateCosts As Double
		Public Property Costs() As Double
			Get
				Return privateCosts
			End Get
			Private Set(ByVal value As Double)
				privateCosts = value
			End Set
		End Property

		Public Sub New(ByVal inNode As RoutingNode, ByVal inCosts As Double)
			If inNode Is Nothing Then
				Throw New ArgumentNullException()
			End If

			Node = inNode
			Costs = inCosts
		End Sub
	End Class
End Namespace
