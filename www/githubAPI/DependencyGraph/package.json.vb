﻿#Region "Microsoft.VisualBasic::58d88ab90ff4d2470f0de67536b7ce5d, ..\sciBASIC#\www\githubAPI\DependencyGraph\package.json.vb"

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

Public Class package
    Public Property name As String
    Public Property description As String
    Public Property version As String
    Public Property author As String
    Public Property main As String
    Public Property scripts As scripts
    Public Property repository As repository
    Public Property keywords As String()
    Public Property dependencies As dependencies
    Public Property devDependencies As dependencies
    Public Property license As String

    Public Overrides Function ToString() As String
        Return $"[{name}] {description}"
    End Function
End Class

Public Class scripts
    Public Property test As String
End Class

Public Class repository
    Public Property type As String
    Public Property url As String
End Class

Public Class dependencies
    Public Property summary As String
    Public Property distributions As String
    Public Property eslint As String
    Public Property tap As String
End Class
