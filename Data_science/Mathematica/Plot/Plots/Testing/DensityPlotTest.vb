﻿Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Statistics.Heatmap
Imports Microsoft.VisualBasic.Data.csv.IO

Module DensityPlotTest

    Sub Main()
        Dim data = DataSet.LoadDataSet("D:\OneDrive\2017-8-31\3. DEPs\Time_series\T4vsT3.csv")
        Dim points = data.Select(Function(x)
                                     Return New PointF(x!log2FC, -Math.Log10(x("p.value")))
                                 End Function).ToArray

        Call DensityPlot.Plot(points, ptSize:=15, levels:=50).Save("./test.png")
    End Sub
End Module
