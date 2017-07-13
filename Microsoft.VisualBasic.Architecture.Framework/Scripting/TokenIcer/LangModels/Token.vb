﻿#Region "Microsoft.VisualBasic::f46a6d56938ad6c9413401160c725fc7, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Scripting\TokenIcer\LDM\LDM.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
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

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace Scripting.TokenIcer

    ''' <summary>
    ''' a Token object class, This defines the Token object
    ''' </summary>
    ''' <typeparam name="Tokens">应该是枚举类型</typeparam>
    ''' <remarks>
    ''' A Token object holds the token and token value.
    ''' </remarks>
    Public Class Token(Of Tokens As IComparable) : Implements Value(Of String).IValueOf

        ''' <summary>
        ''' Token type
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute("name")> Public Property name As Tokens

        ''' <summary>
        ''' 函数参数列表
        ''' </summary>
        ''' <returns></returns>
        Public Property Arguments As Statement(Of Tokens)()
        ''' <summary>
        ''' 这个token所拥有的闭包下一级代码，这个属性的值可以看作为具体的函数体
        ''' </summary>
        ''' <returns></returns>
        Public Property Closure As Main(Of Tokens)

        ''' <summary>
        ''' The text that makes up the token.
        ''' </summary>
        ''' <returns></returns>
        <XmlText> Public Property Value As String Implements Value(Of String).IValueOf.value

        ''' <summary>
        ''' You must keep the UNDEFINED type equals to ZERO!.
        ''' (务必要保持0为未定义，如果不是使用零来定义未定义类型，则不能够使用这个属性来判断)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property UNDEFINED(Optional int% = 0) As Boolean
            Get
                If TypeOf name Is [Enum] OrElse TypeOf name Is Integer Then
                    Dim o As Object = name
                    Dim i As Integer = CInt(o)
                    If i = int Then
                        Return True
                    End If
                End If

                Return name Is Nothing OrElse
                    String.IsNullOrEmpty(Value)
            End Get
        End Property

        ''' <summary>
        ''' The token type, this property is an alias of <see cref="name"/> property
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Type As Tokens
            Get
                Return name
            End Get
        End Property

        ''' <summary>
        ''' Gets the text value that make up this token value, this readonly property 
        ''' is an alias of the <see cref="Value"/> property.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Text As String
            Get
                Return Value
            End Get
        End Property

        ''' <summary>
        ''' Returns a Boolean value indicating whether an expression can be evaluated as
        ''' a number.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsNumeric As Boolean
            Get
                Return Information.IsNumeric(Text)
            End Get
        End Property

        Public Sub New(name As Tokens, value$)
            Me.name = name
            Me.Value = value
        End Sub

        Sub New(name As Tokens)
            Me.name = name
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            If UNDEFINED Then
                Return "UNDEFINED --> " & Value
            End If
            Return $"[{name}]" & vbTab & Value
        End Function

        Public Function GetValue() As Object
            Return Me.TryCast
        End Function
    End Class

    ''' <summary>
    ''' The statement line that parsing from the script text
    ''' </summary>
    Public Class Statement(Of T As IComparable)

        <XmlElement("t")> Public Property tokens As Token(Of T)()
        ''' <summary>
        ''' The script text raw data?
        ''' </summary>
        ''' <returns></returns>
        Public Property Trace As LineValue

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    ''' <summary>
    ''' inner closure or program main
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Main(Of T As IComparable)
        Public Property program As Statement(Of T)()
    End Class
End Namespace