﻿#Region "Microsoft.VisualBasic::eb74c599568e445c0125e186129dac48, Data_science\Mathematica\SignalProcessing\Signal.IO\cdfSignalsWriter.vb"

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

    ' Module cdfSignalsWriter
    ' 
    '     Function: createAttributes, ReadCDF, WriteCDF
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.IO.netCDF
Imports Microsoft.VisualBasic.Data.IO.netCDF.Components
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' cdf writer of the signals
''' </summary>
Public Module cdfSignalsWriter

    <Extension>
    Public Function ReadCDF(file As netCDFReader) As IEnumerable(Of GeneralSignal)
        Dim signals As Integer = file.getAttribute("signals")
        Dim metaNames = file.getAttribute("metadata").ToString.LoadJSON(Of String())
        Dim description = file.getAttribute("description").ToString
        Dim x As New Vector(Of Double)(file.getDataVariable("measure_buffer").numerics)
        Dim y As New Vector(Of Double)(file.getDataVariable("signal_buffer").numerics)
        Dim chunk_size = file.getDataVariable("chunk_size")
        Dim signal_guid = file.getDataVariable("signal_guid").chars.LoadJSON(Of String())
        Dim measure_unit = file.getDataVariable("measure_unit").chars.LoadJSON(Of String())
        Dim index As New List(Of (start%, ends%))
        Dim buffer_size = 0
        Dim meta As Array() = metaNames _
            .Select(Function(str) "meta:" & str) _
            .Select(Function(name, idx)
                        Dim data = file.getDataVariable(name)
                        Dim info = file.getDataVariableEntry(name)
                        Dim retriveVal As Array

                        If info.FindAttribute("type").value = "json" Then
                            retriveVal = data.chars.LoadJSON(Of String())
                        Else
                            retriveVal = DirectCast(data.genericValue, IEnumerable).ToArray(Of Object)
                        End If

                        Return retriveVal
                    End Function) _
            .ToArray

        For Each size In chunk_size.integers
            index.Add((buffer_size, buffer_size + size - 1))
            buffer_size = buffer_size + size
        Next

        Return index _
            .Select(Function(range, idx)
                        Dim measure = x(range)
                        Dim signal = y(range)
                        Dim metadata As New Dictionary(Of String, String)

                        For i As Integer = 0 To metaNames.Length - 1
                            Call metadata.Add(metaNames(i), meta(i).GetValue(idx).ToString)
                        Next

                        Return New GeneralSignal With {
                            .description = description,
                            .Measures = measure,
                            .Strength = signal,
                            .meta = metadata,
                            .measureUnit = measure_unit(idx),
                            .reference = signal_guid(idx)
                        }
                    End Function)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="signals"></param>
    ''' <param name="file"></param>
    ''' <param name="description"></param>
    ''' <param name="enableCDFExtension">enable netCDF extension type?</param>
    ''' <returns></returns>
    <Extension>
    Public Function WriteCDF(signals As IEnumerable(Of GeneralSignal), file As String,
                             Optional description$ = Nothing,
                             Optional enableCDFExtension As Boolean = False) As Boolean

        ' 20200704 redesign of the general signal cdf file storage layout:
        ' the previous version of the signal data file is too slow that reading 
        ' in R script code when the signal data count is large

        Using cdffile As New CDFWriter(file)
            Call cdffile.Dimensions(Dimension.Double, Dimension.Float, Dimension.Integer, Dimension.Long, Dimension.Text(fixedChars:=1024))
            Call cdffile.GlobalAttributes(New attribute With {.name = "time", .type = CDFDataTypes.CHAR, .value = Now.ToString})
            Call cdffile.GlobalAttributes(New attribute With {.name = "filename", .type = CDFDataTypes.CHAR, .value = file.FileName})
            Call cdffile.GlobalAttributes(New attribute With {.name = "github", .type = CDFDataTypes.CHAR, .value = LICENSE.githubURL})

            If Not description.StringEmpty Then
                Call cdffile.GlobalAttributes(New attribute With {.name = NameOf(description), .type = CDFDataTypes.CHAR, .value = description})
            End If

            Dim package As GeneralSignal() = signals.ToArray
            Dim chunksize As New List(Of Integer)
            Dim offsets As New List(Of Long)
            Dim dataDimension As Dimension
            Dim attrNames As New List(Of String)
            Dim measures As New List(Of Double)
            Dim signalDatas As New List(Of Double)
            Dim annotation As attribute

            Call cdffile.GlobalAttributes(New attribute With {.name = "signals", .type = CDFDataTypes.INT, .value = package.Length})

            For Each attr As NamedValue(Of CDFData) In package.createAttributes(enableCDFExtension)
                dataDimension = New Dimension With {
                    .name = "attribute_data: " & attr.Name,
                    .size = attr.Value.Length
                }
                annotation = New attribute With {
                    .name = "type",
                    .type = CDFDataTypes.CHAR,
                    .value = attr.Description
                }

                Call cdffile.AddVariable("meta:" & attr.Name, attr.Value, dataDimension, {annotation})
                Call attrNames.Add(attr.Name)
            Next

            Call cdffile.GlobalAttributes(New attribute With {.name = "metadata", .type = CDFDataTypes.CHAR, .value = attrNames.AsEnumerable.GetJson})

            Dim bufferOffset As Long = Scan0
            Dim signal_guid As New CDFData With {.chars = signals.Select(Function(sig) sig.reference).GetJson}
            Dim units As New CDFData With {.chars = signals.Select(Function(sig) sig.measureUnit).GetJson}

            For Each signal As GeneralSignal In signals
                measures.AddRange(signal.Measures)
                signalDatas.AddRange(signal.Strength)
                offsets.Add(bufferOffset)
                chunksize.Add(signal.Measures.Length)

                bufferOffset += signal.Measures.Length
            Next

            dataDimension = New Dimension With {.name = "data_chunks", .size = measures.Count}

            Call cdffile.AddVariable("measure_buffer", New CDFData With {.numerics = measures.ToArray}, dataDimension)
            Call cdffile.AddVariable("signal_buffer", New CDFData With {.numerics = signalDatas.ToArray}, dataDimension)

            dataDimension = New Dimension With {.name = "signals", .size = chunksize.Count}
            cdffile.AddVariable("chunk_size", New CDFData With {.integers = chunksize.ToArray}, dataDimension)

            If enableCDFExtension Then
                Call cdffile.AddVariable("buffer_offset", New CDFData With {.longs = offsets.ToArray}, dataDimension)
            End If

            dataDimension = New Dimension With {.name = "guid_json_chars", .size = signal_guid.chars.Length}
            cdffile.AddVariable("signal_guid", signal_guid, dataDimension)

            dataDimension = New Dimension With {.name = "measure_unit_json_chars", .size = units.chars.Length}
            cdffile.AddVariable("measure_unit", units, dataDimension)
        End Using

        Return True
    End Function

    <Extension>
    Private Iterator Function createAttributes(package As GeneralSignal(), enableCDFExtension As Boolean) As IEnumerable(Of NamedValue(Of CDFData))
        Dim allNames As String() = package.Select(Function(sig) sig.meta.Keys).IteratesALL.Distinct.ToArray

        For Each name As String In allNames
            Dim values As New List(Of String)
            Dim data As CDFData
            Dim type As String

            For Each signal As GeneralSignal In package
                values.Add(signal.meta.TryGetValue(name, [default]:=""))
            Next

            If values.AsParallel.Select(Function(s) s Is Nothing OrElse s = "" OrElse s.IsPattern("\d+")).All(Function(t) t = True) Then
                Dim longs = values.Select(AddressOf Long.Parse).ToArray

                If longs.All(Function(b) b <= 255 AndAlso b >= -255) Then
                    data = New CDFData With {.byteStream = longs.Select(Function(l) CByte(l)).ToBase64String}
                    type = "base64"
                ElseIf longs.All(Function(s) s <= Short.MaxValue AndAlso s >= Short.MinValue) Then
                    data = New CDFData With {.tiny_int = longs.Select(Function(l) CShort(l)).ToArray}
                    type = "int16"
                ElseIf longs.All(Function(i) i <= Integer.MaxValue AndAlso i >= Integer.MinValue) Then
                    data = New CDFData With {.integers = longs.Select(Function(l) CInt(l)).ToArray}
                    type = "i32"
                ElseIf enableCDFExtension Then
                    data = New CDFData With {.longs = longs}
                    type = "i64"
                Else
                    Throw New InvalidProgramException($"{GetType(Long).FullName} is not supports when option '{NameOf(enableCDFExtension)}' is not enable!")
                End If

            ElseIf values.AsParallel.Select(Function(s) s Is Nothing OrElse s = "" OrElse s.IsNumeric).All(Function(t) t = True) Then
                data = New CDFData With {.numerics = values.Select(AddressOf ParseDouble).ToArray}
                type = "f64"
            ElseIf values.AsParallel.Select(Function(s) s Is Nothing OrElse s = "" OrElse s.IsPattern("((true)|(false))", RegexICSng)).All(Function(t) t = True) Then
                If enableCDFExtension Then
                    data = New CDFData With {.flags = values.Select(AddressOf ParseBoolean).ToArray}
                    type = "flags"
                Else
                    data = New CDFData With {.tiny_int = values.Select(AddressOf ParseBoolean).Select(Function(f) CShort(If(f, 1, 0))).ToArray}
                    type = "i16"
                End If
            Else
                data = New CDFData With {.chars = values.AsEnumerable.GetJson}
                type = "json"
            End If

            Yield New NamedValue(Of CDFData) With {
                .Name = name,
                .Value = data,
                .Description = type
            }
        Next
    End Function
End Module
