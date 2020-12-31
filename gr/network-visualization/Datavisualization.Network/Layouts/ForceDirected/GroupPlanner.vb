﻿#Region "Microsoft.VisualBasic::9e4f822e6e92fabf943fe81bad3f67f3, gr\network-visualization\Datavisualization.Network\Layouts\ForceDirected\GroupPlanner.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Class GroupPlanner
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: runAttraction, runRepulsive
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports stdNum = System.Math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Layouts.ForceDirected

    ''' <summary>
    ''' 这个模块之中的方法会尽量让相同类型分组的节点聚集在一块
    ''' </summary>
    Public Class GroupPlanner : Inherits Planner

        ReadOnly groupBy As Dictionary(Of String, String)

        ReadOnly groupAttraction As Double = 5
        ReadOnly groupRepulsive As Double = 5
        ReadOnly mass As New MassHandler

        Public Sub New(g As NetworkGraph,
                       Optional ejectFactor As Integer = 6,
                       Optional condenseFactor As Integer = 3,
                       Optional groupAttraction As Double = 5,
                       Optional groupRepulsive As Double = 5,
                       Optional maxtx As Integer = 4,
                       Optional maxty As Integer = 3,
                       Optional dist_threshold As String = "30,250",
                       Optional size As String = "1000,1000")

            Call MyBase.New(g, ejectFactor, condenseFactor, maxtx, maxty, dist_threshold, size)

            Me.groupAttraction = groupAttraction
            Me.groupRepulsive = groupRepulsive
            Me.groupBy = g.vertex _
                .ToDictionary(Function(n) n.label,
                              Function(n)
                                  Dim classLabel As String = n.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE)

                                  If classLabel.StringEmpty Then
                                      Return "n/a"
                                  Else
                                      Return classLabel
                                  End If
                              End Function)

            For Each group As IGrouping(Of String, KeyValuePair(Of String, String)) In groupBy _
                .GroupBy(Function(t) t.Value) _
                .Where(Function(gi)
                           Return gi.Key <> "n/a"
                       End Function)

                Dim idlist As String() = group.Select(Function(t) t.Key).ToArray
                Dim x As Double = randf.NextInteger(CANVAS_WIDTH)
                Dim y As Double = randf.NextInteger(CANVAS_HEIGHT)

                For Each id As String In idlist
                    With g.GetElementByID(id).data
                        .initialPostion = New FDGVector2(x, y)
                    End With
                Next
            Next
        End Sub

        Protected Overrides Sub runAttraction()
            Dim u, v As Node
            Dim distX, distY, dist As Double
            Dim dx, dy As Double

            For Each edge As Edge In g.graphEdges
                u = edge.U
                v = edge.V
                distX = u.data.initialPostion.x - v.data.initialPostion.x
                distY = u.data.initialPostion.y - v.data.initialPostion.y
                dist = stdNum.Sqrt(distX * distX + distY * distY)
                dx = distX * dist / k * condenseFactor
                dy = distY * dist / k * condenseFactor

                If groupBy(v.label) = groupBy(u.label) AndAlso groupBy(v.label) <> "n/a" Then
                    dx *= groupAttraction
                    dy *= groupAttraction
                Else
                    dx /= groupAttraction
                    dy /= groupAttraction
                End If

                With mass.DeltaMass(u, v, dx, dy)
                    mDxMap(u.label) = mDxMap(u.label) - .X
                    mDyMap(u.label) = mDyMap(u.label) - .Y
                End With

                With mass.DeltaMass(v, u, dx, dy)
                    mDxMap(v.label) = mDxMap(v.label) + .X
                    mDyMap(v.label) = mDyMap(v.label) + .Y
                End With
            Next
        End Sub

        Protected Overrides Sub runRepulsive()
            Dim distX, distY, dist As Double
            Dim ejectFactor = Me.ejectFactor
            Dim dx, dy As Double

            For Each u As Node In g.vertex
                For Each v As Node In g.vertex.Where(Function(ui) Not ui Is u)
                    distX = u.data.initialPostion.x - v.data.initialPostion.x
                    distY = u.data.initialPostion.y - v.data.initialPostion.y
                    dist = stdNum.Sqrt(distX * distX + distY * distY)

                    If (dist < dist_thresh.Min) Then
                        ejectFactor = 5
                    End If

                    If dist > 0 AndAlso dist < dist_thresh.Max Then
                        dx = (distX / dist * k * k / dist) * ejectFactor
                        dy = (distY / dist * k * k / dist) * ejectFactor

                        If groupBy(u.label) = groupBy(v.label) AndAlso groupBy(u.label) <> "n/a" Then
                            dx /= groupRepulsive
                            dy /= groupRepulsive
                        Else
                            dx *= groupRepulsive
                            dy *= groupRepulsive
                        End If

                        With mass.DeltaMass(u, v, dx, dy)
                            mDxMap(u.label) = mDxMap(u.label) + .X
                            mDyMap(u.label) = mDyMap(u.label) + .Y
                        End With
                    End If
                Next
            Next
        End Sub
    End Class
End Namespace
