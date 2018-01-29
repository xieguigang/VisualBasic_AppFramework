﻿#Region "Microsoft.VisualBasic::b57dcc1227426316a2f91bd0ccfb6c0d, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Test\LPPTest.vb"

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

Imports Microsoft.VisualBasic.Math.Algebra.LinearProgramming
''' <summary>
''' https://github.com/gthole/lpp
''' </summary>
Public Module LPPTest

    ' TODO: Fill in meta information beyond ObjectiveFunctionValue (constraint sensitivity, etc.)

    Sub main()
        Call testSolveMaximizeExample()
        Call testSolveMinimizeExample()
        Call testSolveSmallMinimizeExample()
        Call testSolveStrictEqualityExample()
        Call testSolveTransshipmentExample()

        Call Main2(Nothing)

        Console.ReadLine()
    End Sub

    Public Sub Main2(args() As String)
        Dim lpp As LPP = LPPExamples.transshipment()
        Console.WriteLine(lpp.ToString())

        Dim solved As LPPSolution = lpp.solve()
        Console.WriteLine(solved.ToString())
        Console.WriteLine(solved.constraintSensitivityString())
        Console.WriteLine(solved.SolutionLog)


    End Sub

    Public Sub testSolveTransshipmentExample()

        Dim lpp As LPP = LPPExamples.transshipment()
        Dim solved As LPPSolution = lpp.solve()
        assertEquals("ObjectiveFunctionValue", 10043, solved.ObjectiveFunctionValue, 0.0001)

    End Sub

    Public Sub testSolveMinimizeExample()

        Dim lpp As LPP = LPPExamples.minimizeExample()
        Dim solved As LPPSolution = lpp.solve()
        assertEquals("ObjectiveFunctionValue", 2.625, solved.ObjectiveFunctionValue, 0.0001)
    End Sub

    Public Sub testSolveSmallMinimizeExample()
        Dim lpp As LPP = LPPExamples.smallMinimizeExample()
        Dim solved As LPPSolution = lpp.solve()
        assertEquals("ObjectiveFunctionValue", 57, solved.ObjectiveFunctionValue, 0.0001)

    End Sub

    Public Sub testSolveMaximizeExample()
        Dim lpp As LPP = LPPExamples.maximizeExample()
        Dim solved As LPPSolution = lpp.solve()
        assertEquals("ObjectiveFunctionValue", 70, solved.ObjectiveFunctionValue, 0.0001)
    End Sub


    Public Sub testSolveStrictEqualityExample()

        Dim lpp As LPP = LPPExamples.strictEquality()
        Dim solved As LPPSolution = lpp.solve()
        assertEquals("ObjectiveFunctionValue", 55, solved.ObjectiveFunctionValue, 0.0001)

    End Sub

    Sub assertEquals(text$, x#, y#, esp#)
        If Math.Abs(x - y) <= esp Then
            Call Console.WriteLine($"{text} test success")
        Else
            Throw New Exception(text)
        End If
    End Sub
End Module

