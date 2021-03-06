﻿#Region "Microsoft.VisualBasic::79e35472b64eb1b4a17ef3f226248879, gr\Microsoft.VisualBasic.Imaging\test\gifTest.vb"

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

    ' Module gifTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Imaging

Module gifTest

    Sub Main()
        Using file As FileStream = "./test.gif".Open, gif As New GifEncoder(file)
            Dim frame1 As Graphics2D = New Size(200, 200).CreateGDIDevice
            Dim frame2 As Graphics2D = New Size(200, 200).CreateGDIDevice

            Call frame1.DrawString("Hello", New Font(FontFace.BookmanOldStyle, 20, FontStyle.Bold), Brushes.Red, 10, 10)
            Call frame2.DrawString("World", New Font(FontFace.BookmanOldStyle, 20, FontStyle.Bold), Brushes.Red, 10, 10)

            Call gif.AddFrame(frame1,,, New TimeSpan(0, 0, 3))
            Call gif.AddFrame(frame2,,, New TimeSpan(0, 0, 3))
        End Using
    End Sub
End Module
