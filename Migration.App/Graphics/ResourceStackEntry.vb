Imports Migration.Common
Imports Migration.Core

Namespace Migration

	Public Class ResourceStackEntry

		Private privatePosition As Point
		Public Property Position() As Point
			Get
				Return privatePosition
			End Get
			Set(ByVal value As Point)
				privatePosition = value
			End Set
		End Property

		Private privateResource As Resource
		Public Property Resource() As Resource
			Get
				Return privateResource
			End Get
			Set(ByVal value As Resource)
				privateResource = value
			End Set
		End Property
	End Class

End Namespace
