Imports Migration.Common

Namespace Migration
	Public Module CollectionExtensions

		<System.Runtime.CompilerServices.Extension> _
		Public Function IsSet(ByVal value As TerrainCellFlags, ByVal flag As TerrainCellFlags) As Boolean
			Return ((value And flag) <> 0)
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Function BitCount(Of TEnum As Structure)(ByVal value As TEnum) As Integer
			If GetType(TEnum).IsEnum Then
				Return Convert.ToInt64(value).BitCount()
			ElseIf GetType(TEnum).IsPrimitive Then
				Dim intVal As Long = Convert.ToInt64(value)
				Dim result As Integer = 0

				For i As Integer = 0 To 63
					If (intVal And ((Convert.ToInt64((1))) << i)) <> 0 Then
						result += 1
					End If
				Next i

				Return result
			Else
				Throw New ArgumentException("TEnum must be an enumerative or integer type")
			End If
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Function IsSet(Of TEnum As Structure)(ByVal value As TEnum, ByVal flag As TEnum) As Boolean
			If Not(GetType(TEnum).IsEnum) Then
				Throw New ArgumentException("TEnum must be an enumerated type")
			End If

			Dim valueBase As Long = Convert.ToInt64(value)
			Dim flagBase As Long = Convert.ToInt64(flag)
			Return (valueBase And flagBase) = flagBase
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Function Padding(ByVal This As String, ByVal inLength As Integer) As String
			If This.Length >= inLength Then
				Return This
			End If

			Return This & New String(" "c, inLength - This.Length)
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Function BinarySearch(Of T)(ByVal list As IList(Of T), ByVal value As T) As Integer
			Dim Comparer1 As IComparer(Of T) = Comparer(Of T).Default
			Return BinarySearch(list, value, Comparer(Of T).Default)
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Function BinarySearch(Of T)(ByVal list As IList(Of T), ByVal value As T, ByVal comparer As IComparer(Of T)) As Integer
			'			#Region "Parameter Validation"

			If Object.ReferenceEquals(Nothing, list) Then
				Throw New ArgumentNullException("list")
			End If
			If Object.ReferenceEquals(Nothing, comparer) Then
				Throw New ArgumentNullException("comparer")
			End If

			'			#End Region

			Return BinarySearch(list, value, 0, list.Count - 1, comparer)
		End Function

		Private Function BinarySearch(Of T)(ByVal list As IList(Of T), ByVal value As T, ByVal low As Integer, ByVal high As Integer, ByVal comparer As IComparer(Of T)) As Integer

			If high < low Then
				Return -1 - low ' not found
			End If

			Dim mid As Integer = low + ((high - low) \ 2)
			Dim comp As Integer = comparer.Compare(list(mid), value)

			If comp > 0 Then
				Return BinarySearch(list, value, low, mid - 1, comparer)
			ElseIf comp < 0 Then
				Return BinarySearch(list, value, mid + 1, high, comparer)
			Else
				Return mid ' found
			End If
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Function IndexOf(Of T)(ByVal inThis As IEnumerable(Of T), ByVal inElement As T) As Integer
			Dim i As Integer = 0

			For Each e As T In inThis
				If e.Equals(inElement) Then
					Return i
				End If

				i += 1
			Next e

			Return -1
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Function CreatePermutations(ByVal inThis() As Integer) As List(Of Integer())
			Dim result As New List(Of Integer())()
			Dim array(inThis.Length - 1) As Integer

			For i As Integer = 0 To array.Length - 1
				array(i) = i
			Next i

			GenSubPermutation(array, 0, result)

			Return result
		End Function

		Private Sub GenSubPermutation(ByVal inBase() As Integer, ByVal inIndex As Integer, ByVal inResult As List(Of Integer()))
			If inIndex + 1 >= inBase.Length Then
				inResult.Add(inBase)

				Return
			End If

			For i As Integer = inIndex + 1 To inBase.Length - 1
				' swap "inIndex" with "i"
				Dim [sub]() As Integer = inBase.ToArray()

				[sub](inIndex) = [sub](i)
				[sub](i) = inBase(inIndex)

				GenSubPermutation([sub], inIndex + 1, inResult)
			Next i

			GenSubPermutation(inBase.ToArray(), inIndex + 1, inResult)
		End Sub

		<System.Runtime.CompilerServices.Extension> _
		Public Function Permutate(Of TValue)(ByVal inThis As List(Of TValue), ByVal inPermutation As IEnumerable(Of Integer)) As List(Of TValue)
			Dim result As New List(Of TValue)()

			For Each i As Integer In inPermutation
				result.Add(inThis(i))
			Next i

			Return result
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Sub Fill(ByVal inThis() As Integer, ByVal inStartValue As Integer, ByVal inStride As Integer)
			Dim i As Integer = 0
			Dim current As Integer = inStartValue
			Do While i < inThis.Length
				inThis(i) = current
				i += 1
				current += inStride
			Loop
		End Sub

		<System.Runtime.CompilerServices.Extension> _
		Public Sub Fill(ByVal inThis() As Int64, ByVal inStartValue As Integer, ByVal inStride As Integer)
			Dim i As Integer = 0
			Dim current As Integer = inStartValue
			Do While i < inThis.Length
				inThis(i) = current
				i += 1
				current += inStride
			Loop
		End Sub

		<System.Runtime.CompilerServices.Extension> _
		Public Sub Fill(ByVal inThis() As Double, ByVal inStartValue As Integer, ByVal inStride As Integer)
			Dim i As Integer = 0
			Dim current As Integer = inStartValue
			Do While i < inThis.Length
				inThis(i) = current
				i += 1
				current += inStride
			Loop
		End Sub

		<System.Runtime.CompilerServices.Extension> _
		Public Function ToObjects(ByVal inCollection As System.Collections.ICollection) As Object()
			Dim result As New List(Of Object)()
			Dim enumerator As System.Collections.IEnumerator = inCollection.GetEnumerator()

			Do While enumerator.MoveNext()
				result.Add(enumerator.Current)
			Loop

			Return result.ToArray()
		End Function
	End Module
End Namespace
