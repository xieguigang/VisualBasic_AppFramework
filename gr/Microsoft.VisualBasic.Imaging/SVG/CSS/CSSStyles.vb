﻿#Region "Microsoft.VisualBasic::5804ddfdf67d5e406973a89df2ab782b, gr\Microsoft.VisualBasic.Imaging\SVG\CSS\CSSStyles.vb"

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

    '     Class CSSStyles
    ' 
    '         Properties: filters, linearGradients, radialGradients, styles
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.XmlMeta

Namespace SVG.CSS

    ''' <summary>
    ''' 在这个SVG对象之中所定义的CSS样式数据
    ''' </summary>
    Public Class CSSStyles : Inherits Node

        <XmlElement("linearGradient")>
        Public Property linearGradients As linearGradient()
        <XmlElement("radialGradient")>
        Public Property radialGradients As radialGradient()
        <XmlElement("style")>
        Public Property styles As XmlMeta.CSS()
        <XmlElement("filter")>
        Public Property filters As Filter()

    End Class
End Namespace
