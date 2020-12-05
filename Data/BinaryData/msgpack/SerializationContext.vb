﻿Imports System
Imports System.Collections.Generic
Imports MsgPack.Serialization

Namespace scopely.msgpacksharp
    Public Class SerializationContext
        Private _Serializers As System.Collections.Generic.Dictionary(Of System.Type, scopely.msgpacksharp.MsgPackSerializer)

        Friend Property Serializers As Dictionary(Of Type, MsgPackSerializer)
            Get
                Return _Serializers
            End Get
            Private Set(ByVal value As Dictionary(Of Type, MsgPackSerializer))
                _Serializers = value
            End Set
        End Property

        Private _serializationMethod As SerializationMethod

        Public Property SerializationMethod As SerializationMethod
            Get
                Return _serializationMethod
            End Get
            Set(ByVal value As SerializationMethod)

                If _serializationMethod <> value Then
                    Select Case value
                        Case SerializationMethod.Array, SerializationMethod.Map
                            _serializationMethod = value
                        Case Else
                            Throw New ArgumentOutOfRangeException("value")
                    End Select

                    Serializers = New Dictionary(Of Type, MsgPackSerializer)()
                End If
            End Set
        End Property

        Public Sub New()
            Serializers = New Dictionary(Of Type, MsgPackSerializer)()
            _serializationMethod = SerializationMethod.Array
        End Sub

        Public Sub RegisterSerializer(Of T)(ByVal propertyDefinitions As IList(Of MessagePackMemberDefinition))
            Serializers(GetType(T)) = New MsgPackSerializer(GetType(T), propertyDefinitions)
        End Sub

        Public Sub RegisterSerializer(Of T)(ParamArray propertyNames As String())
            Dim defs = New List(Of MessagePackMemberDefinition)()

            For Each propertyName In propertyNames
                defs.Add(New MessagePackMemberDefinition() With {
                    .PropertyName = propertyName,
                    .NilImplication = NilImplication.MemberDefault
                })
            Next

            Serializers(GetType(T)) = New MsgPackSerializer(GetType(T), defs)
        End Sub
    End Class
End Namespace
