﻿#Region "Microsoft.VisualBasic::27557ea3fb20ccfa42ca228fc5bec2ae, Data_science\Graph\Model\Abstract\Graph.vb"

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

    ' Class Graph
    ' 
    '     Properties: graphEdges, size, vertex
    ' 
    '     Function: (+3 Overloads) AddEdge, AddEdges, (+2 Overloads) AddVertex, CreateEdge, (+3 Overloads) Delete
    '               ExistEdge, ExistVertex, GetConnectedVertex, GetEnumerator, IEnumerable_GetEnumerator
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports TV = Microsoft.VisualBasic.Data.GraphTheory.Vertex

''' <summary>
''' A graph ``G = (V, E)`` consists of a set V of vertices and a set E edges, that is, unordered
''' pairs Of vertices. Unless explicitly stated otherwise, we assume that the graph Is simple,
''' that Is, it has no multiple edges And no self-loops.
''' (使用迭代器来访问这个图之中的边连接的集合)
''' </summary>
Public MustInherit Class Graph(Of V As {New, TV}, Edge As {New, Edge(Of V)}, G As Graph(Of V, Edge, G))
    Implements IEnumerable(Of Edge)

#Region "Let G=(V, E) be a simple graph"
    Protected Friend edges As New Dictionary(Of Edge)

    ''' <summary>
    ''' <see cref="vertices"/>和<see cref="buffer"/>哈希表分别使用了两种属性来对节点进行索引的建立：
    ''' 
    ''' + <see cref="vertices"/>使用<see cref="TV.Label"/>来建立字符串索引
    ''' + <see cref="buffer"/>使用<see cref="TV.ID"/>来建立指针的索引
    ''' </summary>
    Protected vertices As New Dictionary(Of V)

    ''' <summary>
    ''' Visit nodes directly by index number
    ''' </summary>
    Protected Friend buffer As New HashList(Of V)
#End Region

    ''' <summary>
    ''' ``[numof(vertex), numof(edges)]``
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property size As (vertex%, edges%)
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return (vertices.Count, edges.Count)
        End Get
    End Property

    ''' <summary>
    ''' 这个图之中的所有的节点的集合. 请注意，这个只读属性是一个枚举集合，
    ''' 所以为了减少性能上的损失，不可以过多的使用下标来访问集合元素
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property vertex As IEnumerable(Of V)
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return vertices.Values
        End Get
    End Property

    ''' <summary>
    ''' 获取得到这个图中的所有的节点的边的集合，请注意，这个只读属性是一个枚举集合，
    ''' 所以为了减少性能上的损失，不可以过多的使用下标来访问集合元素
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property graphEdges As IEnumerable(Of Edge)
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return edges.Values
        End Get
    End Property

    ''' <summary>
    ''' 返回所有至少具有一条边连接的节点的集合
    ''' </summary>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetConnectedVertex() As V()
        Return edges.Values _
            .Select(Function(e) {e.U, e.V}) _
            .IteratesALL _
            .Distinct _
            .ToArray
    End Function

    ''' <summary>
    ''' <see cref="TV.Label"/> should contains its index value before this method was called.
    ''' (如果已经存在目标ID的节点，则无操作)
    ''' </summary>
    ''' <param name="u"></param>
    ''' <returns></returns>
    Public Function AddVertex(u As V) As G
        If Not vertices.Have(u) Then
            vertices += u
            buffer.Add(u)
        End If

        Return Me
    End Function

    ''' <summary>
    ''' 通过<see cref="TV.label"/>作为主键进行查询目标节点是否存在于当前的图对象之中
    ''' </summary>
    ''' <param name="name"><see cref="TV.label"/></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ExistVertex(name$) As Boolean
        Return vertices.ContainsKey(name)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ExistEdge(edge As Edge) As Boolean
        Return edges.ContainsKey(edge.ID)
    End Function

    ''' <summary>
    ''' 假若目标<paramref name="label"/>已经存在于顶点列表<see cref="vertices"/>之中，
    ''' 那么将不会添加新的节点而是直接返回原来已经存在的节点
    ''' </summary>
    ''' <param name="label$"></param>
    ''' <returns></returns>
    Public Function AddVertex(label As String) As V
        If vertices.ContainsKey(label) Then
            Return vertices(label)
        Else
            With New V With {
                .ID = buffer.GetAvailablePos,
                .Label = label
            }
                Call AddVertex(.ByRef)
                Return .ByRef
            End With
        End If
    End Function

    Public Function AddEdge(u As V, v As V, Optional weight# = 0) As G
        edges += New Edge With {
            .U = u,
            .V = v,
            .weight = weight
        }
        If Not vertices.ContainsKey(u.Label) Then
            vertices += u
            buffer.Add(u)
        End If
        If Not vertices.ContainsKey(v.Label) Then
            vertices += v
            buffer.Add(v)
        End If

        Return Me
    End Function

    ''' <summary>
    ''' 这个函数使用起来比较方便，但是要求节点都必须要存在于列表之中
    ''' </summary>
    ''' <param name="src$"></param>
    ''' <param name="targets$"></param>
    ''' <returns></returns>
    Public Function AddEdges(src$, targets$()) As G
        Dim U As V = vertices(src)

        If U Is Nothing Then
            Throw New EntryPointNotFoundException($"Source vertex {src} is not found!")
        ElseIf targets.Any(Function(v) Not vertices.ContainsKey(v)) Then
            Throw New EntryPointNotFoundException($"At least one of the target vertex in {targets.GetJson} is not found!")
        End If

        For Each V As String In targets
            Call AddEdge(U, vertices(V))
        Next

        Return Me
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overridable Function AddEdge(i%, j%, Optional weight# = 0) As G
        edges += New Edge With {
            .U = buffer(i),
            .V = buffer(j),
            .weight = weight
        }

        Return Me
    End Function

    ''' <summary>
    ''' <paramref name="u"/> and <paramref name="v"/> is the property ``<see cref="Data.GraphTheory.Vertex.label"/>``
    ''' </summary>
    ''' <param name="u$"></param>
    ''' <param name="v$"></param>
    ''' <param name="weight#"></param>
    ''' <returns></returns>
    Public Overridable Function AddEdge(u$, v$, Optional weight# = 0) As G
        edges += CreateEdge(u, v, weight)
        Return Me
    End Function

    ''' <summary>
    ''' 这个函数仅仅是使用图对象中的node数据来创建edge对象，并不会添加edge到图中的edge列表中
    ''' </summary>
    ''' <param name="u$"></param>
    ''' <param name="v$"></param>
    ''' <param name="weight#"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function CreateEdge(u$, v$, Optional weight# = 0) As Edge
        Return New Edge With {
            .U = vertices(u),
            .V = vertices(v),
            .weight = weight
        }
    End Function

    ''' <summary>
    ''' 只会删除边，并不会删除节点<paramref name="U"/>和<paramref name="V"/>
    ''' </summary>
    ''' <param name="U"></param>
    ''' <param name="V"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Delete(U As V, V As V) As G
        Return Delete(U.ID, V.ID)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Delete(u$, v$) As G
        Return Delete(vertices(u).ID, vertices(v).ID)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Delete(u%, v%) As G
        Dim key$ = $"{u}-{v}"

        If edges.ContainsKey(key) Then
            Call edges.Remove(key)
        End If

        Return Me
    End Function

    ''' <summary>
    ''' 因为图的主要关注点是放在节点对象的相互关系之上，所以在这里图对象是表现为一个边连接的集合
    ''' </summary>
    ''' <returns></returns>
    Public Iterator Function GetEnumerator() As IEnumerator(Of Edge) Implements IEnumerable(Of Edge).GetEnumerator
        For Each edge As Edge In edges.Values
            Yield edge
        Next
    End Function

    Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function
End Class
