﻿#Region "Microsoft.VisualBasic::5478b77a683b8264cd34c8c4b7307353, Data_science\MachineLearning\MachineLearning\NeuralNetwork\Trainings\DarwinismHybrid\Fitness.vb"

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

    '     Class Fitness
    ' 
    '         Properties: Cacheable
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Calculate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure

Namespace NeuralNetwork.DarwinismHybrid

    Public Class Fitness : Implements Fitness(Of NetworkIndividual)

        Dim dataSets As TrainingSample()

        Sub New(trainingSet As Sample())
            Me.dataSets = trainingSet.Select(Function(a) New TrainingSample(a)).ToArray
        End Sub

        Public ReadOnly Property Cacheable As Boolean Implements Fitness(Of NetworkIndividual).Cacheable
            Get
                Return False
            End Get
        End Property

        Public Function Calculate(chromosome As NetworkIndividual, parallel As Boolean) As Double Implements Fitness(Of NetworkIndividual).Calculate
            Dim errors As Double() = ANNTrainer.trainingImpl(
                network:=chromosome.target,
                dataSets:=dataSets,
                parallel:=parallel,
                selective:=True,
                dropoutRate:=0,
                backPropagate:=False
            )

            Return errors.Average
        End Function
    End Class
End Namespace
