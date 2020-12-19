﻿Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Scripting

Public Structure ZScores

    Dim serials As DataSet()

    Dim groups As Dictionary(Of String, String())
    ''' <summary>
    ''' Colors for the <see cref="groups"/>
    ''' </summary>
    Dim colors As Dictionary(Of String, Color)
    Dim shapes As Dictionary(Of String, LegendStyles)

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Range() As DoubleRange
        Return serials _
            .Select(Function(d) d.Properties.Values) _
            .IteratesALL _
            .Where(Function(x) Not x.IsNaNImaginary) _
            .Range
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <param name="groups"></param>
    ''' <param name="colors"></param>
    ''' <param name="transpose">是否需要将<paramref name="path"/>所加载上来的矩阵进行转置处理</param>
    ''' <param name="labelTranslate"></param>
    ''' <returns></returns>
    Public Shared Function Load(path$,
                                groups As Dictionary(Of String, String()),
                                colors As Color(),
                                Optional transpose As Boolean = False,
                                Optional labelTranslate As Dictionary(Of String, String) = Nothing) As ZScores

        Dim datalist As DataSet() = DataSet _
            .LoadDataSet(path) _
            .ToArray

        If transpose Then
            datalist = datalist.Transpose
        End If

        Return ZScores.Load(datalist, groups, colors, labelTranslate)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="groups"></param>
    ''' <param name="colors"></param>
    ''' <returns></returns>
    Public Shared Function Load(datalist As DataSet(),
                                groups As Dictionary(Of String, String()),
                                colors As Color(),
                                Optional labelTranslate As Dictionary(Of String, String) = Nothing) As ZScores

        Dim colorlist As LoopArray(Of Color) = colors

        If labelTranslate Is Nothing Then
            labelTranslate = New Dictionary(Of String, String)
        End If

        Dim names As New NamedVectorFactory(datalist.PropertyNames)
        Dim zscores = datalist _
            .Select(Function(serial)
                        Dim z As Vector = names _
                            .AsVector(serial.Properties) _
                            .Z()
                        Dim label$ = labelTranslate.TryGetValue(
                            index:=serial.ID,
                            [default]:=serial.ID
                        )

                        Return New DataSet With {
                            .ID = label,
                            .Properties = names.Translate(z)
                        }
                    End Function) _
            .ToArray
        Dim colorProfile = groups _
            .ToDictionary(Function(x) x.Key,
                          Function(x) colorlist.Next)
        Dim shapeProfile = groups _
            .ToDictionary(Function(x) x.Key,
                          Function(x) LegendStyles.Circle)

        Return New ZScores With {
            .serials = zscores,
            .groups = groups,
            .colors = colorProfile,
            .shapes = shapeProfile
        }
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <param name="groups"></param>
    ''' <param name="colors$"></param>
    ''' <param name="transpose">是否需要将<paramref name="path"/>所加载上来的矩阵进行转置处理</param>
    ''' <param name="labelTranslate">
    ''' 对<paramref name="path"/>所加载的矩阵之中的rownames进行显示标题的转换
    ''' </param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function Load(path$, groups As Dictionary(Of String, String()),
                                Optional colors$ = ColorBrewer.QualitativeSchemes.Set2_8,
                                Optional transpose As Boolean = False,
                                Optional labelTranslate As Dictionary(Of String, String) = Nothing) As ZScores
        Return ZScores.Load(
            path, groups,
            Designer.GetColors(colors),
            transpose:=transpose,
            labelTranslate:=labelTranslate
        )
    End Function
End Structure
