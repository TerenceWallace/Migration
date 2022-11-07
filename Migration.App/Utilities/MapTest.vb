Namespace Migration

	Friend Class MapTest
		Friend Shared Sub Run()
			Dim test As New UniqueMap(Of Int32, Int32)()
			Dim rnd As New CrossRandom()

			For i As Integer = 0 To 29
				test.Add(rnd.Next(), i)
			Next i

			For i As Integer = 1 To 29
				If test.Keys.ElementAt(i - 1) > test.Keys.ElementAt(i) Then
					Throw New ApplicationException("MapTest failed!")
				End If
			Next i

			For i As Integer = 0 To 29
                If test(test.Keys.ElementAt(i)) <> test.Values(i) Then
                    Throw New ApplicationException("MapTest failed!")
                End If
			Next i
		End Sub
	End Class
End Namespace
