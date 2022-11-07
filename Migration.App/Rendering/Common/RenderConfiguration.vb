Imports System.Reflection


#If EMBEDDED Then
Imports OpenTK.Graphics.ES20
#Else

#End If

Namespace Migration.Rendering
	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class RenderConfiguration

		Private privateViewportWidth As Integer
		Public Property ViewportWidth() As Integer
			Get
				Return privateViewportWidth
			End Get
			Set(ByVal value As Integer)
				privateViewportWidth = value
			End Set
		End Property

		Private privateViewportHeight As Integer
		Public Property ViewportHeight() As Integer
			Get
				Return privateViewportHeight
			End Get
			Set(ByVal value As Integer)
				privateViewportHeight = value
			End Set
		End Property

		Private privateIsFullScreen As Boolean
		Public Property IsFullScreen() As Boolean
			Get
				Return privateIsFullScreen
			End Get
			Set(ByVal value As Boolean)
				privateIsFullScreen = value
			End Set
		End Property

		Public Sub New()
			ViewportWidth = 800
			ViewportHeight = 600
			IsFullScreen = False
		End Sub
	End Class

End Namespace
