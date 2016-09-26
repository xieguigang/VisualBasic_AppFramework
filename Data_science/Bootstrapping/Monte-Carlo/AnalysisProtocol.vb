﻿Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.diffEq

Namespace MonteCarlo

    Public Module AnalysisProtocol

        ''' <summary>
        ''' 加载dll文件之中的计算模型
        ''' </summary>
        ''' <param name="dll"></param>
        ''' <returns></returns>
        Public Function DllParser(dll As String) As Type
            Dim assem As Assembly = Assembly.LoadFile(dll.GetFullPath)
            Dim types As Type() = assem.GetTypes

            Return LinqAPI.DefaultFirst(Of Type) <= From type As Type
                                                    In types
                                                    Where type.IsInheritsFrom(GetType(Model)) AndAlso
                                                    Not type.IsAbstract
                                                    Select type
        End Function

        <Extension>
        Public Function Gety0(def As Type) As NamedValue(Of PreciseRandom)()
            Dim obj As Object = Activator.CreateInstance(def)
            Dim model As Model = DirectCast(obj, Model)
            Return model.yinit
        End Function

        <Extension>
        Public Function GetRandomParameters(def As Type) As NamedValue(Of PreciseRandom)()
            Dim obj As Object = Activator.CreateInstance(def)
            Dim model As Model = DirectCast(obj, Model)
            Return model.params
        End Function

        ''' <summary>
        ''' 假若模型定义之中没有定义这个特征向量的构建方法的话，则使用默认的方法：平均数+标准差
        ''' </summary>
        ''' <param name="def"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetEigenvector(def As Type) As Dictionary(Of String, Eigenvector)
            Dim obj As Object = Activator.CreateInstance(def)
            Dim model As Model = DirectCast(obj, Model)
            Dim eigenvectors As Dictionary(Of String, Eigenvector) = model.eigenvector

            If eigenvectors Is Nothing Then
                Dim vars = Model.GetVariables(def)
                Dim ys = Model.GetParameters(def)
                eigenvectors = vars.Join(ys).DefaultEigenvector
            End If

            Return eigenvectors
        End Function

        ''' <summary>
        ''' 加载目标dll之中的计算模型然后提供计算数据
        ''' </summary>
        ''' <param name="dll"></param>
        ''' <param name="k"></param>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Run(dll As String, k As Long, n As Integer, a As Integer, b As Integer) As IEnumerable(Of ODEsOut)
            Dim model As Type = DllParser(dll)
            Dim y0 = model.Gety0
            Dim parms = model.GetRandomParameters
            Return model.Bootstrapping(parms, y0, k, n, a, b,,)
        End Function

        <Extension>
        Public Function Sampling(data As IEnumerable(Of ODEsOut),
                                 eigenvector As Dictionary(Of String, Eigenvector),
                                 Optional partN As Integer = 20) As IEnumerable(Of VectorTagged(Of Dictionary(Of String, Double)))
            Return data.Select(Function(x) x.Sampling(eigenvector, partN))
        End Function

        Public Const Observation As String = NameOf(Observation)

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="eigenvector"></param>
        ''' <param name="partN"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Sampling(x As ODEsOut,
                                 eigenvector As Dictionary(Of String, Eigenvector),
                                 Optional partN As Integer = 20,
                                 Optional tag As String = Nothing) As VectorTagged(Of Dictionary(Of String, Double))

            Dim vector As New List(Of Double)

            For Each var As String In eigenvector.Keys
                Dim y As Double() = x.y(var).x
                Dim n As Integer = y.Length / partN

                For Each block As Double() In Parallel.Linq.SplitIterator(y, n)
                    vector += eigenvector(var)(block)
                Next
            Next

            Return New VectorTagged(Of Dictionary(Of String, Double)) With {
                .Tag = vector.ToArray,   ' 所提取采样出来的特征向量
                .value = x.params,       ' 生成原始数据的参数列表
                .TagStr = tag
            }
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dll"></param>
        ''' <param name="observation">实验观察里面只需要y值列表就足够了，不需要参数信息</param>
        ''' <param name="k"></param>
        ''' <param name="n"></param>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="expected"></param>
        ''' <param name="[stop]"></param>
        ''' <param name="work">工作的临时文件夹工作区间，默认使用dll的文件夹</param>
        Public Sub Iterations(dll As String,
                              observation As ODEsOut,
                              k As Long,
                              n As Integer,
                              a As Integer,
                              b As Integer,
                              expected As Integer,
                              Optional [stop] As Integer = -1,
                              Optional partN As Integer = 20,
                              Optional cut As Double = 0.3,
                              Optional work As String = Nothing)

            Dim model As Type = DllParser(dll)
            Dim y0 = model.Gety0
            Dim parms = model.GetRandomParameters
            Dim eigenvectors As Dictionary(Of String, Eigenvector) = model.GetEigenvector

            If work Is Nothing Then
                work = dll.TrimSuffix & $"-MonteCarlo-{App.PID}/"
            End If

            Do While True
                Dim randSamples = observation _
                    .Join(model.Bootstrapping(parms, y0, k, n, a, b,, )) _
                    .Sampling(eigenvectors, partN)
                Dim kmeansResult As Dictionary(Of Double(), NamedValue(Of Dictionary(Of String, Double)())()) =
                    randSamples.KMeans(expected, [stop])
                Dim required As VectorTagged(Of NamedValue(Of Dictionary(Of String, Double)())()) = Nothing

                For Each cluster In kmeansResult
                    For Each x In cluster.Value
                        If Not String.IsNullOrEmpty(x.Name) Then
                            required = New VectorTagged(Of NamedValue(Of Dictionary(Of String, Double)())()) With {
                                .Tag = cluster.Key,
                                .value = cluster.Value
                            }
                            Exit For
                        End If
                    Next

                    If Not required Is Nothing Then
                        Exit For
                    End If
                Next

                Dim total As Integer = GetEntityNumbers(kmeansResult.Values.ToArray)
                Dim requires As Integer = GetEntityNumbers(required)

                If requires / total >= cut Then  ' 已经满足条件了，准备返回数据

                End If
            Loop
        End Sub

        Public Function GetEntityNumbers(ParamArray data As NamedValue(Of Dictionary(Of String, Double)())()()) As Integer
            Dim array = data.MatrixAsIterator.Select(Function(x) x.x).MatrixAsIterator
            Dim value As Integer = array.Count
            Return value
        End Function
    End Module
End Namespace