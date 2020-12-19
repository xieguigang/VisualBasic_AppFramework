﻿#Region "Microsoft.VisualBasic::17cd8bc99ae6ca58d7bc9f1d62bdb31d, Data_science\Visualization\Plots\Scatter\Data.vb"

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

    ' Class SerialData
    ' 
    '     Properties: DataAnnotations, title
    ' 
    '     Function: GetEnumerator, GetPen, GetPointByX, IEnumerable_GetEnumerator, ToString
    ' 
    '     Sub: AddMarker
    ' 
    ' Structure PointData
    ' 
    '     Constructor: (+3 Overloads) Sub New
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

''' <summary>
''' 绘图的点的数据
''' </summary>
Public Structure PointData

    ''' <summary>
    ''' 坐标数据不需要进行额外的转换，绘图函数内部会自动进行mapping转换的
    ''' </summary>
    Public pt As PointF
    ''' <summary>
    ''' 正负误差
    ''' </summary>
    Public errPlus#, errMinus#, tag$, value#
    ''' <summary>
    ''' 可能会有数据点在<see cref="errPlus"/>或者<see cref="errMinus"/>范围内，或者范围外
    ''' </summary>
    Public Statics#()
    Public color$
    Public stroke$

    ''' <summary>
    ''' 坐标轴的值模式为字符串模式的时候
    ''' </summary>
    Public axisLabel As String

    Sub New(x!, y!)
        pt = New PointF(x, y)
    End Sub

    Sub New(pt As PointF)
        Me.pt = pt
    End Sub

    Sub New(pt As Point)
        Me.pt = New PointF(pt.X, pt.Y)
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{pt.ToString}] {value} {color} {tag}"
    End Function
End Structure
