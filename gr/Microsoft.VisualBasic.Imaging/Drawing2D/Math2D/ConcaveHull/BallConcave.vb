﻿Imports System.Drawing
Imports sys = System.Math

Namespace Drawing2D.Math2D.ConcaveHull

    ''' <summary>
    ''' + http://www.tuicool.com/articles/iUvMjm
    ''' + http://www.ian-ko.com/ET_GeoWizards/UserGuide/concaveHull.htm
    ''' </summary>
    Public Class BallConcave

        Private Structure Point2dInfo : Implements IComparable(Of Point2dInfo)

            Public Point As Point
            Public Index As Integer
            Public DistanceTo As Double

            Public Sub New(p As Point, i As Integer, dis As Double)
                Me.Point = p
                Me.Index = i
                Me.DistanceTo = dis
            End Sub

            Public Function CompareTo(other As Point2dInfo) As Integer Implements IComparable(Of Point2dInfo).CompareTo
                Return DistanceTo.CompareTo(other.DistanceTo)
            End Function

            Public Overrides Function ToString() As String
                Return Convert.ToString(Point) & "," & Index & "," & DistanceTo
            End Function
        End Structure

        Public Sub New(list As IEnumerable(Of Point))
            Me.points = list _
                .OrderBy(Function(p) p.X * CDbl(p.Y)) _
                .ToList
            '  points.Sort()
            flags = New Boolean(points.Count - 1) {}
            For i As Integer = 0 To flags.Length - 1
                flags(i) = False
            Next
            InitDistanceMap()
            InitNearestList()
        End Sub

        Private flags As Boolean()
        Private points As List(Of Point)
        Private distanceMap As Double(,)
        Private rNeigbourList As List(Of Integer)()

        Private Sub InitNearestList()
            rNeigbourList = New List(Of Integer)(points.Count - 1) {}
            For i As Integer = 0 To rNeigbourList.Length - 1
                rNeigbourList(i) = GetSortedNeighbours(i)
            Next
        End Sub

        Private Sub InitDistanceMap()
            distanceMap = New Double(points.Count - 1, points.Count - 1) {}
            For i As Integer = 0 To points.Count - 1
                For j As Integer = 0 To points.Count - 1
                    distanceMap(i, j) = GetDistance(points(i), points(j))
                Next
            Next
        End Sub

        Public ReadOnly Property RecomandedRadius() As Double
            Get
                Dim r As Double = Double.MinValue
                For i As Integer = 0 To points.Count - 1
                    If distanceMap(i, rNeigbourList(i)(1)) > r Then
                        r = distanceMap(i, rNeigbourList(i)(1))
                    End If
                Next
                Return r
            End Get
        End Property

        Public Function GetMinEdgeLength() As Double
            Dim min As Double = Double.MaxValue
            For i As Integer = 0 To points.Count - 1
                For j As Integer = 0 To points.Count - 1
                    If i < j Then
                        If distanceMap(i, j) < min Then
                            min = distanceMap(i, j)
                        End If
                    End If
                Next
            Next
            Return min
        End Function

        Public Function GetConcave_Ball(radius As Double) As List(Of Point)
            Dim ret As New List(Of Point)()
            Dim adjs As List(Of Integer)() = GetInRNeighbourList(2 * radius)
            ret.Add(points(0))
            'flags[0] = true;
            Dim i As Integer = 0, j As Integer = -1, prev As Integer = -1
            While True
                j = GetNextPoint_BallPivoting(prev, i, adjs(i), radius)
                If j = -1 Then
                    Exit While
                End If
                Dim p As Point = BallConcave.GetCircleCenter(points(i), points(j), radius)
                ret.Add(points(j))
                flags(j) = True
                prev = i
                i = j
            End While
            Return ret
        End Function

        Public Function GetConcave_Edge(radius As Double) As List(Of Point)
            Dim ret As New List(Of Point)()
            Dim adjs As List(Of Integer)() = GetInRNeighbourList(2 * radius)
            ret.Add(points(0))
            Dim i As Integer = 0, j As Integer = -1, prev As Integer = -1
            While True
                j = GetNextPoint_EdgePivoting(prev, i, adjs(i), radius)
                If j = -1 Then
                    Exit While
                End If
                'Point2d p = BallConcave.GetCircleCenter(points[i], points[j], radius);
                ret.Add(points(j))
                flags(j) = True
                prev = i
                i = j
            End While
            Return ret
        End Function

        Private Function CheckValid(adjs As List(Of Integer)()) As Boolean
            For i As Integer = 0 To adjs.Length - 1
                If adjs(i).Count < 2 Then
                    Return False
                End If
            Next
            Return True
        End Function

        Public Function CompareAngel(a As Point, b As Point, m_origin As Point, m_dreference As Point) As Boolean

            Dim da As New Point(a.X - m_origin.X, a.Y - m_origin.Y)
            Dim db As New Point(b.X - m_origin.X, b.Y - m_origin.Y)
            Dim detb As Double = GetCross(m_dreference, db)

            ' nothing is less than zero degrees
            If detb = 0 AndAlso db.X * m_dreference.X + db.Y * m_dreference.Y >= 0 Then
                Return False
            End If

            Dim deta As Double = GetCross(m_dreference, da)

            ' zero degrees is less than anything else
            If deta = 0 AndAlso da.X * m_dreference.X + da.Y * m_dreference.Y >= 0 Then
                Return True
            End If

            If deta * detb >= 0 Then
                ' both on same side of reference, compare to each other
                Return GetCross(da, db) > 0
            End If

            ' vectors "less than" zero degrees are actually large, near 2 pi
            Return deta > 0
        End Function

        Public Function GetNextPoint_EdgePivoting(prev As Integer, current As Integer, list As List(Of Integer), radius As Double) As Integer
            If list.Count = 2 AndAlso prev <> -1 Then
                Return list(0) + list(1) - prev
            End If
            Dim dp As Point
            If prev = -1 Then
                dp = New Point(1, 0)
            Else
                dp = points(prev) - points(current)
            End If
            Dim min As Integer = -1
            For j As Integer = 0 To list.Count - 1
                If Not flags(list(j)) Then
                    If min = -1 Then
                        min = list(j)
                    Else
                        Dim t As Point = points(list(j))
                        If CompareAngel(points(min), t, points(current), dp) AndAlso GetDistance(t, points(current)) < radius Then
                            min = list(j)
                        End If
                    End If
                End If
            Next
            'main.ShowMessage("seek P" + points[min].Index);
            Return min
        End Function

        Public Function GetNextPoint_BallPivoting(prev As Integer, current As Integer, list As List(Of Integer), radius As Double) As Integer
            SortAdjListByAngel(list, prev, current)
            For j As Integer = 0 To list.Count - 1
                If flags(list(j)) Then
                    Continue For
                End If
                Dim adjIndex As Integer = list(j)
                Dim xianp As Point = points(adjIndex)
                Dim rightCirleCenter As Point = GetCircleCenter(points(current), xianp, radius)
                If Not HasPointsInCircle(list, rightCirleCenter, radius, adjIndex) Then
                    ' main.DrawCircleWithXian(rightCirleCenter, points(current), points(adjIndex), radius)
                    Return list(j)
                End If
            Next
            Return -1
        End Function

        Private Sub SortAdjListByAngel(list As List(Of Integer), prev As Integer, current As Integer)
            Dim origin As Point = points(current)
            Dim df As Point
            If prev <> -1 Then
                df = New Point(points(prev).X - origin.X, points(prev).Y - origin.Y)
            Else
                df = New Point(1, 0)
            End If
            Dim temp As Integer = 0
            For i As Integer = list.Count To 1 Step -1
                For j As Integer = 0 To i - 2
                    If CompareAngel(points(list(j)), points(list(j + 1)), origin, df) Then
                        temp = list(j)
                        list(j) = list(j + 1)
                        list(j + 1) = temp
                    End If
                Next
            Next
        End Sub

        Private Function HasPointsInCircle(adjPoints As List(Of Integer), center As Point, radius As Double, adjIndex As Integer) As Boolean
            For k As Integer = 0 To adjPoints.Count - 1
                If adjPoints(k) <> adjIndex Then
                    Dim index2 As Integer = adjPoints(k)
                    If IsInCircle(points(index2), center, radius) Then
                        Return True
                    End If
                End If
            Next
            Return False
        End Function

        Public Shared Function GetCircleCenter(a As Point, b As Point, r As Double) As Point
            Dim dx As Double = b.X - a.X
            Dim dy As Double = b.Y - a.Y
            Dim cx As Double = 0.5 * (b.X + a.X)
            Dim cy As Double = 0.5 * (b.Y + a.Y)
            If r * r / (dx * dx + dy * dy) - 0.25 < 0 Then
                Return New Point(-1, -1)
            End If
            Dim sqrt As Double = sys.Sqrt(r * r / (dx * dx + dy * dy) - 0.25)
            Return New Point(cx - dy * sqrt, cy + dx * sqrt)
        End Function

        Public Shared Function IsInCircle(p As Point, center As Point, r As Double) As Boolean
            Dim dis2 As Double = (p.X - center.X) * (p.X - center.X) + (p.Y - center.Y) * (p.Y - center.Y)
            Return dis2 < r * r
        End Function

        Public Function GetInRNeighbourList(radius As Double) As List(Of Integer)()
            Dim adjs As List(Of Integer)() = New List(Of Integer)(points.Count - 1) {}
            For i As Integer = 0 To points.Count - 1
                adjs(i) = New List(Of Integer)()
            Next
            For i As Integer = 0 To points.Count - 1

                For j As Integer = 0 To points.Count - 1
                    If i < j AndAlso distanceMap(i, j) < radius Then
                        adjs(i).Add(j)
                        adjs(j).Add(i)
                    End If
                Next
            Next
            Return adjs
        End Function

        Private Function GetSortedNeighbours(index As Integer) As List(Of Integer)
            Dim infos As New List(Of Point2dInfo)(points.Count)
            For i As Integer = 0 To points.Count - 1
                infos.Add(New Point2dInfo(points(i), i, distanceMap(index, i)))
            Next
            infos.Sort()
            Dim adj As New List(Of Integer)()
            For i As Integer = 1 To infos.Count - 1
                adj.Add(infos(i).Index)
            Next
            Return adj
        End Function

        Public Shared Function GetDistance(p1 As Point, p2 As Point) As Double
            Return sys.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y))
        End Function

        Public Shared Function GetCross(a As Point, b As Point) As Double
            Return a.X * b.Y - a.Y * b.X
        End Function
    End Class
End Namespace