Namespace Migration
	Friend NotInheritable Class Debug
		Private Sub New()
		End Sub
		Friend Shared Sub Assert(ByVal inExpression As Boolean)
			Assert(inExpression, "Debug assertion failed.")
		End Sub

		Friend Shared Sub Assert(ByVal inExpression As Boolean, ByVal inMessage As String, ParamArray ByVal inParams() As Object)
#If DEBUG Then
			If Not inExpression Then
				Throw New ApplicationException(String.Format(inMessage, inParams))
			End If
#End If
		End Sub
	End Class

End Namespace
