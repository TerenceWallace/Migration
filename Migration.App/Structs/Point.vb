Imports System.Runtime.Serialization
Imports System.Xml.Serialization

Namespace Migration

	''' <summary>
    ''' To prepent System.Drawing dependency just for this structure.
    ''' </summary>
    <Serializable(), DataContract()> _
    Public Structure Point
        <XmlAttribute(), DataMember()> _
        Public X As Integer
        <XmlAttribute(), DataMember()> _
        Public Y As Integer

        Public Sub New(inXY As Integer)
            Me.New(inXY, inXY)
        End Sub

        Public Sub New(ByVal inX As Integer, ByVal inY As Integer)
            X = inX
            Y = inY
        End Sub

        Public Shared Operator <>(ByVal inA As Point, ByVal inB As Point) As Boolean
            Return (inA.X <> inB.X) OrElse (inA.Y <> inB.Y)
        End Operator

        Public Shared Operator =(ByVal inA As Point, ByVal inB As Point) As Boolean
            Return (inA.X = inB.X) AndAlso (inA.Y = inB.Y)
        End Operator

        Public Shadows Function Equals(ByVal obj As Point) As Boolean
            If Convert.ToBoolean(obj.X = Me.X AndAlso obj.Y = Me.Y) Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Function DistanceTo(ByVal inPoint As Point) As Double
            Return Math.Sqrt((inPoint.X - X) * (inPoint.X - X) + (inPoint.Y - Y) * (inPoint.Y - Y))
        End Function

        Public Overrides Function ToString() As String
            Return "X: " & X.ToString() & "; Y: " & Y.ToString()
        End Function

        Public Shared Function Normalize(ByVal pnt As Point) As Point
            Dim mLength As Double = Point.Length(pnt)
            If mLength > 0 Then
                Return New Point(CInt(Fix((pnt.X / Point.Length(pnt)))), CInt(Fix((pnt.Y / Point.Length(pnt)))))
            Else
                Return New Point(CInt(Fix(CSng(pnt.X / 1))), CInt(Fix((CSng(pnt.Y / 1)))))
            End If
        End Function

        Public Function Length() As Double
            Return Math.Sqrt(X * X + Y * Y)
        End Function

        Public Shared Function Length(ByVal pnt As Point) As Double
            Return Math.Sqrt(pnt.X * pnt.X + pnt.Y * pnt.Y)
        End Function

        Public Shared Function Add(ByVal p1 As Point, ByVal p2 As Point) As Point
            Return New Point(p1.X + p2.X, p1.Y + p2.Y)
        End Function

        Public Shared Function Subtract(ByVal p1 As Point, ByVal p2 As Point) As Point
            Return New Point(p1.X - p2.X, p1.Y - p2.Y)
        End Function

        Public Shared Function Dot(ByVal p1 As Point, ByVal p2 As Point) As Single
            Return (p1.X * p2.X + p1.Y * p2.Y)
        End Function

        Public Shared Function Multiply(ByVal p1 As Point, ByVal p2 As Single) As Point
            Return New Point(CInt(Fix(p1.X * p2)), CInt(Fix(p1.Y * p2)))
        End Function

        Public Shared Operator +(ByVal p1 As Point, ByVal a As Integer) As Point
            Return New Point(p1.X + a, p1.Y + a)
        End Operator

        Public Shared Operator +(ByVal p1 As Point, ByVal a As Point) As Point
            Return New Point(p1.X + a.X, p1.Y + a.Y)
        End Operator

        Public Shared Operator *(ByVal p1 As Point, ByVal a As Integer) As Point
            Return New Point(CInt(Fix(p1.X * a)), CInt(Fix(p1.Y * a)))
        End Operator

        Public Shared Operator *(ByVal p1 As Point, ByVal a As Single) As Point
            Return New Point(CInt(Fix(p1.X * a)), CInt(Fix(p1.Y * a)))
        End Operator

        Public Shared Operator /(ByVal p1 As Point, ByVal a As Integer) As Point
            If a <> 0 Then
                Return New Point(CInt(Fix(p1.X / a)), CInt(Fix(p1.Y / a)))
            Else
                Return p1
            End If
        End Operator

        Public Shared Operator /(ByVal p1 As Point, ByVal a As Single) As Point
            If a <> 0 Then
                Return New Point(CInt(Fix(p1.X / a)), CInt(Fix(p1.Y / a)))
            Else
                Return p1
            End If
        End Operator

        Public Shared Operator /(ByVal p1 As Point, ByVal a As Double) As Point
            If a <> 0 Then
                Return New Point(CInt(Fix(p1.X / a)), CInt(Fix(p1.Y / a)))
            Else
                Return p1
            End If
        End Operator

        Friend Shared Function Truncate(ByVal inPoint As Point, ByVal max_value As Single) As Point
            If inPoint.Length() > max_value Then
                Return Point.Multiply(Point.Normalize(inPoint), max_value)
            End If
            Return inPoint
        End Function

    End Structure

End Namespace
