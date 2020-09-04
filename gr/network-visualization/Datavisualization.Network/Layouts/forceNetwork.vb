﻿#Region "Microsoft.VisualBasic::1bf005724b66dab1f77de93dfa2faffb, gr\network-visualization\Datavisualization.Network\Layouts\forceNetwork.vb"

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

    '     Module forceNetwork
    ' 
    '         Function: CheckZero, (+2 Overloads) doForceLayout, doRandomLayout
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Serialization.JSON
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Layouts

    Public Module forceNetwork

        ''' <summary>
        ''' <see cref="Parameters.Load"/>
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="parameters"></param>
        ''' <param name="showProgress"></param>
        <ExportAPI("Layout.ForceDirected")>
        <Extension>
        Public Function doForceLayout(ByRef net As NetworkGraph, parameters As ForceDirectedArgs, Optional showProgress As Boolean = False) As NetworkGraph
            With parameters
                Return net.doForceLayout(.Stiffness, .Repulsion, .Damping, .Iterations, showProgress:=showProgress)
            End With
        End Function

        ''' <summary>
        ''' 这个函数用来检查所有的节点是否都是处于零位置
        ''' 
        ''' #### 20200616
        ''' 
        ''' **假若所有的节点都是处于零位置，则<see cref="doForceLayout"/>函数无法正常工作**
        ''' 
        ''' 因为布局引擎会自动使用随机位置初始化位置为空值的节点
        ''' 所以在这里只需要检查非空位置的节点即可
        ''' </summary>
        ''' <param name="g"></param>
        ''' <returns></returns>
        <Extension>
        Public Function CheckZero(g As NetworkGraph) As Boolean
            For Each v As Node In g.vertex
                If Not v.data?.initialPostion Is Nothing Then
                    Dim p As AbstractVector = v.data.initialPostion

                    If p.x <> 0 OrElse p.y <> 0 OrElse p.z <> 0 Then
                        Return False
                    End If
                End If
            Next

            Return True
        End Function

        ''' <summary>
        ''' Applies the force directed layout. Parameter can be read from a ``*.ini`` profile file by using <see cref="Parameters.Load"/>
        ''' (如果有些时候这个函数不起作用的话，考虑一下在调用这个函数之前，先使用<see cref="doRandomLayout"/>初始化随机位置)
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="iterations">网络的布局layout的计算迭代次数</param>
        ''' <param name="Stiffness">
        ''' 密度：影响网络的节点的距离，这个参数值越小，则网络节点的相互之间的距离越大，即这个密度参数值越小，则单位面积内的节点数量越少
        ''' </param>
        ''' <param name="Damping">
        ''' 阻尼：这个参数值越小，则在计算layout的时候，某一个节点所能够影响到的节点数量也越少。即某一个节点的位置调整之后，被影响的其他节点的数量也越少。
        ''' 这个参数值介于0-1之间，超过1的时候网络永远也不会处于稳定的状态
        ''' </param>
        ''' <param name="Repulsion">
        ''' 排斥力：节点之间的排斥力的大小，当这个参数值越大的时候，节点之间的排斥力也会越大，则节点之间的距离也越远。反之节点之间的距离也越近
        ''' </param>
        <ExportAPI("Layout.ForceDirected")>
        <Extension>
        Public Function doForceLayout(ByRef net As NetworkGraph,
                                      Optional Stiffness# = 80,
                                      Optional Repulsion# = 4000,
                                      Optional Damping# = 0.83,
                                      Optional iterations% = 1000,
                                      Optional showProgress As Boolean = False,
                                      Optional clearScreen As Boolean = False,
                                      Optional progressCallback As Action(Of String) = Nothing) As NetworkGraph

            Dim physicsEngine As New ForceDirected2D(net, Stiffness, Repulsion, Damping)
            Dim tick As Action(Of Integer)
            Dim progress As ProgressBar = Nothing
            Dim args$ = New ForceDirectedArgs With {
                    .Damping = Damping,
                    .Iterations = iterations,
                    .Repulsion = Repulsion,
                    .Stiffness = Stiffness
                }.GetJson

            If progressCallback Is Nothing Then
                progressCallback = Sub()

                                   End Sub
            End If

            If showProgress Then
                Dim ticking As ProgressProvider
                Dim ETA$
                Dim details$

                progress = New ProgressBar("Do Force Directed Layout...", 1, CLS:=clearScreen)
                ticking = New ProgressProvider(progress, iterations)
                tick = Sub(i%)
                           ETA = "ETA=" & ticking.ETA().FormatTime
                           details = args & $" ({i}/{iterations}) " & ETA
                           progress.SetProgress(ticking.StepProgress, details)
                           progressCallback(details)
                       End Sub
            Else
                tick = Sub(i%)
                           Dim details = args & $" [{i}/{iterations}]"
                           progressCallback(details)
                       End Sub
            End If

            For i As Integer = 0 To iterations
                Call physicsEngine.Calculate(0.05F)
                Call tick(i)
            Next

            Call physicsEngine.EachNode(
                Sub(node As Node, point As LayoutPoint)
                    node.data.initialPostion = point.position
                End Sub)

            If Not progress Is Nothing Then
                Call progress.Dispose()
            End If

            Return net
        End Function

        <Extension>
        Public Function doRandomLayout(ByRef network As NetworkGraph) As NetworkGraph
            SyncLock randf.seeds
                For Each x As Node In network.vertex
                    x.data.initialPostion = New FDGVector2 With {
                        .x = randf.seeds.NextDouble * 1000,
                        .y = randf.seeds.NextDouble * 1000
                    }
                Next
            End SyncLock

            Return network
        End Function
    End Module
End Namespace
