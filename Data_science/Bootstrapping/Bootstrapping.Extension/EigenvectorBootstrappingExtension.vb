﻿#Region "Microsoft.VisualBasic::e36dd099f8241c4e9cbe275c84b08275, ..\sciBASIC#\Data_science\Bootstrapping\Bootstrapping.Extension\EigenvectorBootstrappingExtension.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.DataMining.KMeans.Tree
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module EigenvectorBootstrappingExtension

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"><see cref="LoadData"/>的输出数据</param>
    ''' <returns></returns>
    <Extension>
    Public Function BinaryKMeans(data As IEnumerable(Of VectorTagged(Of Dictionary(Of String, Double))), partitionDepth As Integer, Optional [stop] As Integer = -1) As Dictionary(Of NamedValue(Of Double()), Dictionary(Of String, Double)())
        Dim strTags As NamedValue(Of VectorTagged(Of Dictionary(Of String, Double)))() =
            LinqAPI.Exec(Of NamedValue(Of VectorTagged(Of Dictionary(Of String, Double)))) <=
 _
            From x As VectorTagged(Of Dictionary(Of String, Double))
            In data.AsParallel
            Select New NamedValue(Of VectorTagged(Of Dictionary(Of String, Double))) With {
                .Name = x.Tag.GetJson,
                .Value = x
            }

        Call "Load data complete!".__DEBUG_ECHO

        Dim uid As New Uid
        Dim datasets As EntityClusterModel() = strTags.Select(
            Function(x) New EntityClusterModel With {
                .ID = "boot" & uid.Plus,
                .Properties = x.Value.Tag _
                    .SeqIterator _
                    .ToDictionary(Function(o) CStr(o.i),
                                  Function(o) o.value)   ' 在这里使用特征向量作为属性来进行聚类操作
        }).ToArray

        Call "Creates dataset complete!".__DEBUG_ECHO

        Dim clusters As EntityClusterModel() = datasets.TreeCluster(parallel:=True, [stop]:=[stop])
        Dim out As New Dictionary(Of NamedValue(Of Double()), Dictionary(Of String, Double)())
        Dim raw = (From x As NamedValue(Of VectorTagged(Of Dictionary(Of String, Double)))
                   In strTags
                   Select x
                   Group x By x.Name Into Group) _
                        .ToDictionary(Function(x) x.Name,
                                      Function(x) x.Group.ToArray)
        Dim treeParts = clusters.Partitioning(partitionDepth)

        For Each cluster As Partition In treeParts
            Dim key As New NamedValue(Of Double()) With {
                .Name = cluster.Tag,
                .Value = cluster.PropertyMeans
            } ' out之中的key
            Dim tmp As New List(Of Dictionary(Of String, Double))   ' out之中的value

            For Each x As EntityClusterModel In cluster.members
                Dim rawKey As String = x.Properties.Values.ToArray.GetJson
                Dim rawParams = raw(rawKey).Select(Function(o) o.Value.value)

                tmp += rawParams
            Next

            out(key) = tmp.ToArray
        Next

        Return out
    End Function
End Module
