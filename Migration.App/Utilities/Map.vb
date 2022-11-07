Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports Migration.Common

Namespace Migration

    ''' <summary>
    ''' A more flexible solution than the original SortedList provided by Microsoft.
    ''' </summary>
    ''' <typeparam name="TKey"></typeparam>
    ''' <typeparam name="TValue"></typeparam>
    <Serializable()> _
    Public Class Map(Of TKey, TValue)
        Implements IDictionary(Of TKey, TValue), ICollection(Of KeyValuePair(Of TKey, TValue)), IEnumerable(Of KeyValuePair(Of TKey, TValue))

        Shared Sub New()
            MapTest.Run()
        End Sub

        Private m_Keys As New BindingList(Of TKey)()
        Private m_Values As New BindingList(Of TValue)()

        <NonSerialized()> _
        Private modID As Integer = 0
        Private m_Policy As MapPolicy
        Private m_Comparator As IComparer(Of TKey)

        Public ReadOnly Property Policy() As MapPolicy
            Get
                Return m_Policy
            End Get
        End Property

        Public Sub New(ByVal inPolicy As MapPolicy)
            m_Policy = inPolicy
            m_Comparator = Comparer(Of TKey).Default
        End Sub

        Public Sub New(ByVal inPolicy As MapPolicy, ByVal inComparator As Func(Of TKey, TKey, Integer))
            m_Policy = inPolicy
            m_Comparator = New FuncComparer(inComparator)
        End Sub

        <Serializable()> _
        Private Class FuncComparer
            Implements IComparer(Of TKey)

            Private privateComparator As Func(Of TKey, TKey, Integer)
            Friend Property Comparator() As Func(Of TKey, TKey, Integer)
                Get
                    Return privateComparator
                End Get
                Set(ByVal value As Func(Of TKey, TKey, Integer))
                    privateComparator = value
                End Set
            End Property

            Friend Sub New(ByVal inComparator As Func(Of TKey, TKey, Integer))
                Comparator = inComparator
            End Sub

            Public Function Compare(ByVal x As TKey, ByVal y As TKey) As Integer Implements IComparer(Of TKey).Compare
                Return Comparator(x, y)
            End Function
        End Class

        Public Sub New(ByVal inPolicy As MapPolicy, ByVal inComparator As IComparer(Of TKey))
            m_Policy = inPolicy
            m_Comparator = inComparator
        End Sub

        Public Sub InitializeUnsorted(ByVal inKeys As IEnumerable(Of TKey), ByVal inValues As IEnumerable(Of TValue))
            InitializeUnsorted(inKeys.AsQueryable(), inValues.AsQueryable())
        End Sub

        Public Sub InitializeUnsorted(ByVal inPairs As IEnumerable(Of KeyValuePair(Of TKey, TValue)))
            InitializeSortedUnsafe(inPairs.OrderBy(Function(e) e.Key, m_Comparator))
        End Sub

        ''' <summary>
        ''' Only use this method for high-performance cases, since initial sorting is not enforced. This may lead
        ''' to a corrupted map if you don't know exactly that the given pairs are already sorted!
        ''' </summary>
        ''' <param name="inPairs"></param>
        Public Sub InitializeSortedUnsafe(ByVal inPairs As IEnumerable(Of KeyValuePair(Of TKey, TValue)))
            m_Keys = New BindingList(Of TKey)(inPairs.Select(Function(e) e.Key).ToList())
            m_Values = New BindingList(Of TValue)(inPairs.Select(Function(e) e.Value).ToList())
        End Sub

        Public Sub Add(ByVal key As TKey, ByVal value As TValue) Implements IDictionary(Of TKey, TValue).Add
            Add(New KeyValuePair(Of TKey, TValue)(key, value))
        End Sub

        Public Function ContainsKey(ByVal key As TKey) As Boolean Implements IDictionary(Of TKey, TValue).ContainsKey
            Return m_Keys.BinarySearch(key, m_Comparator) >= 0
        End Function

        Public Function GetKeyBinding() As BindingList(Of TKey)
            Return m_Keys
        End Function

        Public Function GetValueBinding() As BindingList(Of TValue)
            Return m_Values
        End Function

        Public ReadOnly Property Keys() As ICollection(Of TKey) Implements IDictionary(Of TKey, TValue).Keys
            Get
                Return New ReadOnlyCollection(Of TKey)(m_Keys)
            End Get
        End Property

        Public Function Remove(ByVal key As TKey) As Boolean Implements IDictionary(Of TKey, TValue).Remove
            Dim pos As Integer = m_Keys.BinarySearch(key, m_Comparator)

            If pos < 0 Then
                Return False
            End If

            m_Keys.RemoveAt(pos)
            m_Values.RemoveAt(pos)

            modID += 1

            Return True
        End Function

        Public Function TryGetValue(ByVal key As TKey, <System.Runtime.InteropServices.Out()> ByRef value As TValue) As Boolean Implements IDictionary(Of TKey, TValue).TryGetValue
            Dim pos As Integer = m_Keys.BinarySearch(key, m_Comparator)

            value = Nothing

            If pos < 0 Then
                Return False
            End If

            value = m_Values(pos)

            Return True
        End Function

        Public ReadOnly Property Values() As IList(Of TValue)
            Get
                Return New ReadonlySettableList(Of TValue)(m_Values)
            End Get
        End Property

        Private ReadOnly Property IDictionaryGeneric_Values() As ICollection(Of TValue) Implements System.Collections.Generic.IDictionary(Of TKey, TValue).Values
            Get
                Return Me.IDictionary_Values
            End Get
        End Property
        Private ReadOnly Property IDictionary_Values() As ICollection(Of TValue)
            Get
                Return New ReadOnlyCollection(Of TValue)(m_Values)
            End Get
        End Property

        Default Public Property Item(ByVal key As TKey) As TValue Implements IDictionary(Of TKey, TValue).Item
            Get
                Dim pos As Integer = m_Keys.BinarySearch(key, m_Comparator)

                If pos < 0 Then
                    If (Policy And MapPolicy.DefaultForNonExisting) = 0 Then
                        Throw New KeyNotFoundException()
                    End If

                    Return Nothing
                End If

                Return m_Values(pos)
            End Get
            Set(ByVal value As TValue)
                Dim pos As Integer = m_Keys.BinarySearch(key, m_Comparator)

                If pos < 0 Then
                    If (Policy And MapPolicy.CreateNonExisting) = 0 Then
                        Throw New KeyNotFoundException()
                    End If

                    Add(key, value)

                    Return
                End If

                m_Values(pos) = value
            End Set
        End Property

        Public Sub Add(ByVal item As KeyValuePair(Of TKey, TValue)) Implements IDictionary(Of TKey, TValue).Add
            Dim pos As Integer = m_Keys.BinarySearch(item.Key, m_Comparator)

            If pos < 0 Then
                ' unique insert
                pos = -pos - 1
            Else
                ' duplicate insert
                If (Policy And MapPolicy.AllowDuplicates) = 0 Then
                    Throw New NotSupportedException("The map already contains a key with this value and duplicates are not permitted.")
                End If
            End If

            m_Keys.Insert(pos, item.Key)
            m_Values.Insert(pos, item.Value)
            modID += 1
        End Sub

        Public Sub Clear() Implements IDictionary(Of TKey, TValue).Clear
            m_Keys.Clear()
            m_Values.Clear()
            modID += 1
        End Sub

        Public Function Contains(ByVal item As KeyValuePair(Of TKey, TValue)) As Boolean Implements IDictionary(Of TKey, TValue).Contains
            Dim value As TValue = Nothing

            If Not (TryGetValue(item.Key, value)) Then
                Return False
            End If

            Return item.Value.Equals(value)
        End Function

        Public Sub CopyTo(ByVal array() As KeyValuePair(Of TKey, TValue), ByVal arrayIndex As Integer) Implements IDictionary(Of TKey, TValue).CopyTo
            If Count + arrayIndex > array.Length Then
                Throw New IndexOutOfRangeException()
            End If

            Dim i As Integer = 0
            Dim x As Integer = arrayIndex
            Dim c As Integer = Count
            Do While i < c
                array(x) = New KeyValuePair(Of TKey, TValue)(m_Keys(i), m_Values(i))
                i += 1
                x += 1
            Loop
        End Sub

        Public ReadOnly Property Count() As Integer Implements IDictionary(Of TKey, TValue).Count
            Get
                Return m_Keys.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly() As Boolean Implements IDictionary(Of TKey, TValue).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(ByVal item As KeyValuePair(Of TKey, TValue)) As Boolean Implements IDictionary(Of TKey, TValue).Remove ', ICollection(Of KeyValuePair(Of TKey, TValue)).Remove
            Dim pos As Integer = m_Keys.BinarySearch(item.Key, m_Comparator)

            If pos < 0 Then
                Return False
            End If

            If m_Values(pos).Equals(item.Value) Then
                Return False
            End If

            m_Keys.RemoveAt(pos)
            m_Values.RemoveAt(pos)
            modID += 1

            Return True
        End Function

        Public Function Search(ByVal inPattern As TKey, ByVal inComparer As IComparer(Of TKey)) As IEnumerable(Of KeyValuePair(Of TKey, TValue))
            Dim offset As Integer = m_Keys.BinarySearch(inPattern, inComparer)
            Dim result As New List(Of KeyValuePair(Of TKey, TValue))()

            If offset < 0 Then
                Return result
            End If

            For n As Integer = offset To 0 Step -1
                If inComparer.Compare(inPattern, m_Keys(n)) <> 0 Then
                    Exit For
                End If

                result.Add(New KeyValuePair(Of TKey, TValue)(m_Keys(n), m_Values(n)))
            Next n

            Dim i As Integer = offset + 1

            Dim m_count As Integer = m_Keys.Count
            Do While i < m_count
                If inComparer.Compare(inPattern, m_Keys(i)) <> 0 Then
                    Exit Do
                End If

                result.Add(New KeyValuePair(Of TKey, TValue)(m_Keys(i), m_Values(i)))
                i += 1
            Loop

            Return result
        End Function

        Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of System.Collections.Generic.KeyValuePair(Of TKey, TValue)) Implements System.Collections.Generic.IEnumerable(Of System.Collections.Generic.KeyValuePair(Of TKey, TValue)).GetEnumerator
            Return New KeyValuePairEnum(Me)
        End Function

        Private Class KeyValuePairEnum
            Implements IEnumerator(Of KeyValuePair(Of TKey, TValue))

            Private map As Map(Of TKey, TValue)
            Private modID As Integer
            Private index As Integer = -1

            Friend Sub New(ByVal inMap As Map(Of TKey, TValue))
                map = inMap
                modID = inMap.modID
            End Sub

            Private Sub CheckModification()
                If modID <> map.modID Then
                    Throw New InvalidOperationException("Collection has been modified during enumeration.")
                End If
            End Sub

            Public ReadOnly Property Current() As KeyValuePair(Of TKey, TValue) Implements IEnumerator(Of System.Collections.Generic.KeyValuePair(Of TKey, TValue)).Current
                Get
                    CheckModification()

                    Return New KeyValuePair(Of TKey, TValue)(map.m_Keys(index), map.m_Values(index))
                End Get
            End Property

            Public Sub Dispose() Implements System.IDisposable.Dispose
                map = Nothing
                index = -1
            End Sub

            Private ReadOnly Property IEnumerator_Current1() As Object Implements System.Collections.IEnumerator.Current
                Get
                    Return Me.IEnumerator_Current
                End Get
            End Property
            Private ReadOnly Property IEnumerator_Current() As Object
                Get
                    Return Me.Current
                End Get
            End Property

            Public Function MoveNext() As Boolean Implements System.Collections.IEnumerator.MoveNext
                CheckModification()

                If index + 1 >= map.Count Then
                    Return False
                End If

                index += 1

                Return True
            End Function

            Public Sub Reset() Implements System.Collections.IEnumerator.Reset
                CheckModification()

                index = -1
            End Sub

        End Class

        Private Function IEnumerable_GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
            Return Me.IEnumerable_GetEnumerator()
        End Function
        Private Function IEnumerable_GetEnumerator() As System.Collections.IEnumerator
            Return Me.GetEnumerator()
        End Function

    End Class
End Namespace
