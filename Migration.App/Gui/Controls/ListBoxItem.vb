Imports System.Reflection

Namespace Migration
	<ObfuscationAttribute(Feature := "renaming", ApplyToMembers := True)> _
	Public Class ListBoxItem

		Private privateParams() As Object
		Public Property Params() As Object()
			Get
				Return privateParams
			End Get
			Private Set(ByVal value As Object())
				privateParams = value
			End Set
		End Property

		Private privateCallback As Object
		Public Property Callback() As Object
			Get
				Return privateCallback
			End Get
			Set(ByVal value As Object)
				privateCallback = value
			End Set
		End Property

		Public Sub New(ParamArray ByVal inParams() As String)
			Params = New Object(inParams.Length - 1){}

			For i As Integer = 0 To Params.Length - 1
				If inParams Is Nothing Then
					Throw New ArgumentNullException()
				End If

				Params(i) = inParams(i)
			Next i
		End Sub
	End Class

End Namespace
