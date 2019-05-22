﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.IO.HDF5.IO
Imports Microsoft.VisualBasic.Data.IO.HDF5.Structure
Imports BinaryReader = Microsoft.VisualBasic.Data.IO.HDF5.IO.BinaryReader

Namespace HDF5

    Public Class HDF5File : Implements IDisposable

        ReadOnly reader As BinaryReader
        ReadOnly fileName As String

        Public ReadOnly Property Superblock As Superblock
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New Superblock(reader, Scan0)
            End Get
        End Property

        Dim rootObjects As Dictionary(Of String, DataObjectFacade)

        Default Public ReadOnly Property GetObject(symbolName As String) As HDF5Reader
            Get
                Dim path As String() = symbolName _
                    .Replace("\", "/") _
                    .Split("/"c) _
                    .Where(Function(t) Not t.StringEmpty) _
                    .ToArray
                Dim rootName$ = path(Scan0)
                Dim obj As DataObjectFacade = rootObjects _
                    .Keys _
                    .First(Function(name) name.TextEquals(rootName)) _
                    .GetValueOrDefault(rootObjects)
                Dim reader As New HDF5Reader(fileName, obj)

                For Each token As String In path.Skip(1)
                    reader = reader.ParseDataObject(dataSetName:=token)
                Next

                Return reader
            End Get
        End Property

        Sub New(fileName As String)
            Me.reader = New BinaryFileReader(fileName)
            Me.fileName = fileName

            Call parseHeader()
        End Sub

        Private Sub parseHeader()
            Dim sb As Superblock = Me.Superblock
            Dim rootSymbolTableEntry As SymbolTableEntry = sb.rootGroupSymbolTableEntry
            Dim objectFacade As New DataObjectFacade(Me.reader, sb, "root", rootSymbolTableEntry.objectHeaderAddress)
            Dim rootGroup As New Group(Me.reader, sb, objectFacade)
            Dim objects As List(Of DataObjectFacade) = rootGroup.objects

            rootObjects = objects.ToDictionary(Function(o) o.symbolName)
        End Sub

        Public Overrides Function ToString() As String
            Return fileName
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' 要检测冗余调用

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: 释放托管状态(托管对象)。
                    Call reader.Dispose()
                End If

                ' TODO: 释放未托管资源(未托管对象)并在以下内容中替代 Finalize()。
                ' TODO: 将大型字段设置为 null。
            End If
            disposedValue = True
        End Sub

        ' TODO: 仅当以上 Dispose(disposing As Boolean)拥有用于释放未托管资源的代码时才替代 Finalize()。
        'Protected Overrides Sub Finalize()
        '    ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' Visual Basic 添加此代码以正确实现可释放模式。
        Public Sub Dispose() Implements IDisposable.Dispose
            ' 请勿更改此代码。将清理代码放入以上 Dispose(disposing As Boolean)中。
            Dispose(True)
            ' TODO: 如果在以上内容中替代了 Finalize()，则取消注释以下行。
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace