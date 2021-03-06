﻿#Region "Microsoft.VisualBasic::ed25eb80ba636e8a5edbdac2f788d227, Data_science\Visualization\Plots\Scatter\Plot\Scatter2D.vb"

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

    '     Class Scatter2D
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: PlotInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConvexHull
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Statistics.Linq

Namespace Plots

    Public Class Scatter2D : Inherits Plot

        ReadOnly array As SerialData()
        ReadOnly scatterReorder As Boolean = False
        ReadOnly fillPie As Boolean = True
        ReadOnly ablines As Line()
        ReadOnly hullPolygonIndex As Index(Of String)

        Public Sub New(data As IEnumerable(Of SerialData), theme As Theme,
                       Optional scatterReorder As Boolean = False,
                       Optional fillPie As Boolean = True,
                       Optional ablines As Line() = Nothing,
                       Optional hullConvexList As IEnumerable(Of String) = Nothing)

            Call MyBase.New(theme)

            Me.hullPolygonIndex = hullConvexList.SafeQuery.ToArray
            Me.array = data.ToArray
            Me.scatterReorder = scatterReorder
            Me.fillPie = fillPie
            Me.ablines = ablines
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, rect As GraphicsRegion)
            Dim XTicks#(), YTicks#()

            '    With array.CreateAxisTicks(
            '    preferPositive:=preferPositive,
            '    scaleX:=If(XaxisAbsoluteScalling, 1, 1.25),
            '    scaleY:=If(YaxisAbsoluteScalling, 1, 1.25)
            ')

            '        XTicks = .x
            '        YTicks = .y
            '    End With
            XTicks = array.Select(Function(s) s.pts).IteratesALL.Select(Function(p) CDbl(p.pt.X)).Range.CreateAxisTicks
            YTicks = array.Select(Function(s) s.pts).IteratesALL.Select(Function(p) CDbl(p.pt.Y)).Range.CreateAxisTicks

            'If ticksY > 0 Then
            '    YTicks = AxisScalling.GetAxisByTick(YTicks, tick:=ticksY)
            'End If

            Dim canvas As IGraphics = g
            Dim region As Rectangle = rect.PlotRegion
            Dim X As d3js.scale.Scaler
            Dim Y As d3js.scale.LinearScale

            ' 使用手动指定的范围
            ' 手动指定坐标轴值的范围的时候，X坐标轴无法使用term离散映射
            'If Not xaxis.StringEmpty AndAlso Not yaxis.StringEmpty Then
            '    XTicks = AxisProvider.TryParse(xaxis).AxisTicks
            '    YTicks = AxisProvider.TryParse(yaxis).AxisTicks
            '    X = XTicks.LinearScale.range(integers:={region.Left, region.Right})
            '    Y = YTicks.LinearScale.range(integers:={region.Bottom, region.Top})
            'Else
            ' 如果所有数据点都有单词，则X轴使用离散映射
            If array.All(Function(line) line.pts.All(Function(a) Not a.axisLabel.StringEmpty)) Then
                    Dim allTermLabels As String() = array _
                    .Select(Function(line)
                                Return line.pts.Select(Function(a) a.axisLabel)
                            End Function) _
                    .IteratesALL _
                    .Distinct _
                    .ToArray

                    X = d3js.scale _
                    .ordinal _
                    .domain(allTermLabels) _
                    .range(integers:={region.Left, region.Right})
                Else
                    X = d3js.scale _
                    .linear _
                    .domain(XTicks) _
                    .range(integers:={region.Left, region.Right})
                End If

                Y = d3js.scale.linear.domain(YTicks).range(integers:={region.Bottom, region.Top})
            ' End If

            Dim scaler As New DataScaler With {
            .X = X,
            .Y = Y,
            .region = region,
            .AxisTicks = (XTicks, YTicks)
        }
            Dim gSize As Size = rect.Size

            If theme.drawAxis Then
                Call g.DrawAxis(
                    rect, scaler, theme.drawGrid,
                    xlabel:=xlabel, ylabel:=ylabel,
                    htmlLabel:=theme.htmlLabel,
                    tickFontStyle:=theme.axisTickCSS,
                    labelFont:=theme.axisLabelCSS,
                    xlayout:=theme.xAxisLayout,
                    ylayout:=theme.yAxisLayout,
                    gridX:=theme.gridStrokeX,
                    gridY:=theme.gridStrokeY,
                    gridFill:=theme.gridFill,
                    XtickFormat:=theme.axisTickFormat,
                    YtickFormat:=theme.axisTickFormat,
                    axisStroke:=theme.axisStroke
                )
            End If

            Dim width As Double = rect.PlotRegion.Width / 200
            Dim annotations As New Dictionary(Of String, (raw As SerialData, line As SerialData))

            For Each line As SerialData In array
                Dim pen As Pen = line.GetPen
                Dim br As New SolidBrush(line.color)
                Dim fillBrush As New SolidBrush(Color.FromArgb(100, baseColor:=line.color))
                Dim d! = line.pointSize
                Dim r As Single = line.pointSize / 2
                Dim bottom! = gSize.Height - rect.PlotRegion.Bottom
                Dim getPointBrush = Function(pt As PointData)
                                        If pt.color.StringEmpty Then
                                            Return br
                                        Else
                                            Return pt.color.GetBrush
                                        End If
                                    End Function
                Dim polygon As New List(Of PointF)

                Dim scatter As IEnumerable(Of PointData)

                If scatterReorder Then
                    scatter = line.pts.OrderBy(Function(a) a.value)
                Else
                    scatter = line.pts
                End If

                For Each pt As PointData In scatter
                    Dim pt1 = scaler.Translate(pt)

                    polygon.Add(pt1)

                    If fillPie Then
                        Call g.FillPie(getPointBrush(pt), pt1.X - r, pt1.Y - r, d, d, 0, 360)
                    End If

                    Call Parallel.DoEvents()
                Next

                If line.title Like hullPolygonIndex Then
                    Call polygon _
                        .DoCall(AddressOf ConvexHull.JarvisMatch) _
                        .DoCall(Sub(hull)
                                    If hull.Length >= 3 Then
                                        Call HullPolygonDraw.DrawHullPolygon(canvas, hull, line.color)
                                    End If
                                End Sub)
                End If

                If Not line.DataAnnotations.IsNullOrEmpty Then
                    Dim raw = array.Where(Function(s) s.title = line.title).First

                    Call annotations.Add(line.title, (raw, line))
                End If
            Next

            For Each part In annotations.Values
                For Each annotation As Annotation In part.line.DataAnnotations
                    Call annotation.Draw(g, scaler, part.raw, rect)
                Next
            Next

            If theme.drawLegend Then
                Dim legends As LegendObject() = LinqAPI.Exec(Of LegendObject) _
 _
                    () <= From s As SerialData
                          In array
                          Let sColor As String = s.color.RGBExpression
                          Select New LegendObject With {
                                .color = sColor,
                                .fontstyle = theme.legendLabelCSS,
                                .style = s.shape,
                                .title = s.title
                            }

                Call DrawLegends(g, legends, rect)
            End If

            Call DrawMainTitle(g, region)

            ' draw ablines
            For Each line As Line In ablines.SafeQuery
                Dim a As PointF = scaler.Translate(line.A)
                Dim b As PointF = scaler.Translate(line.B)

                Call g.DrawLine(line.Stroke, a, b)
            Next
        End Sub
    End Class
End Namespace
