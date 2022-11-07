Imports Migration.Interfaces

Namespace Migration
    Friend Structure PathNodeKey
        Implements IQueueItem

        Friend X As Integer
        Friend Y As Integer
        Friend Time As Long
        Friend G As Double
        Friend H As Double
        Friend Length As Integer
        Private m_F As Double
        Private m_Index As Integer

        Friend Shared Comparer As New KeyComparer
        Friend Shared ComparerIgnoreTime As New KeyComparerIgnoreTime
        Private Shared ReadOnly m_AllocationCache As New Stack(Of PathNodeKey)

        Public Property F() As Double Implements IQueueItem.F
            Get
                Return m_F
            End Get
            Set(ByVal value As Double)
                m_F = value
            End Set
        End Property
        Public Property Index() As Integer Implements IQueueItem.Index
            Get
                Return m_Index
            End Get
            Set(ByVal value As Integer)
                m_Index = value
            End Set
        End Property



        Friend Sub New(ByVal inX As Integer, ByVal inY As Integer, ByVal inTime As Long, ByVal inLength As Integer)
            Length = inLength
            H = 0
            G = 0
            X = inX
            Y = inY
            Time = inTime
            m_Index = -1
            m_F = 0
        End Sub

        Friend Sub New(ByVal inX As Integer, ByVal inY As Integer, ByVal inTime As Long)
            Me.New(inX, inY, inTime, 0)
        End Sub

        Public Overrides Function ToString() As String
            Return "X: " & X.ToString().Padding(3) & "; Y: " & Y.ToString().Padding(3) & "; Time: " & Time.ToString().Padding(4) & "; Length: " & Length.ToString().Padding(2)
        End Function

    End Structure
End Namespace
