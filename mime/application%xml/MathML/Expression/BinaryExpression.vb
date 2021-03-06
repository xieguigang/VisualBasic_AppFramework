﻿#Region "Microsoft.VisualBasic::1cb495362d06e159857805a1fab69bf2, mime\application%xml\MathML\BinaryExpression.vb"

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

    '     Class BinaryExpression
    ' 
    '         Properties: [operator], applyleft, applyright
    ' 
    '         Function: ToString
    ' 
    '     Class SymbolExpression
    ' 
    '         Properties: isNumericLiteral, text
    ' 
    '         Function: ToString
    ' 
    '     Class MathExpression
    ' 
    ' 
    ' 
    '     Class LambdaExpression
    ' 
    '         Properties: lambda, parameters
    ' 
    '         Function: (+2 Overloads) FromMathML, ToString
    ' 
    '     Class MathFunctionExpression
    ' 
    '         Properties: name, parameters
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language

Namespace MathML

    Public Class BinaryExpression : Inherits MathExpression

        Public Property [operator] As String

        Public Property applyleft As MathExpression
        Public Property applyright As MathExpression

        Public Overrides Function ToString() As String
            Return ContentBuilder.ToString(Me)
        End Function

    End Class

    Public MustInherit Class MathExpression
    End Class


End Namespace
