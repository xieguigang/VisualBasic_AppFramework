﻿' ***
'  Encoding usage:
'  
'  new BDictionary()
'  {
'   {"Some Key", "Some Value"},
'   {"Another Key", 42}
'  }.ToBencodedString();
'  
'  Decoding usage:
'  
'  BencodeDecoder.Decode("d8:Some Key10:Some Value13:Another Valuei42ee");
'  
'  Feel free to use it.
'  More info about Bencoding at http://wiki.theory.org/BitTorrentSpecification#bencoding
'  
'  Originally posted at http://snipplr.com/view/37790/ by SuprDewd.
'  

Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Serialization.Bencoding

    ''' <summary>
    ''' A class used for decoding Bencoding.
    ''' </summary>
    Public Module BencodeDecoder
        ''' <summary>
        ''' Decodes the string.
        ''' </summary>
        ''' <paramname="bencodedString">The bencoded string.</param>
        ''' <returns>An array of root elements.</returns>
        Public Function Decode(ByVal bencodedString As String) As BElement()
            Dim index = 0

            Try
                If Equals(bencodedString, Nothing) Then Return Nothing
                Dim rootElements As List(Of BElement) = New List(Of BElement)()

                While bencodedString.Length > index
                    rootElements.Add(ReadElement(bencodedString, index))
                End While

                Return rootElements.ToArray()
            Catch __unusedBencodingException1__ As BencodingException
                Throw
            Catch e As Exception
                Throw [Error](e)
            End Try
        End Function

        Private Function ReadElement(ByRef bencodedString As String, ByRef index As Integer) As BElement
            Select Case bencodedString(index)
                Case "0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c
                    Return ReadString(bencodedString, index)
                Case "i"c
                    Return ReadInteger(bencodedString, index)
                Case "l"c
                    Return ReadList(bencodedString, index)
                Case "d"c
                    Return ReadDictionary(bencodedString, index)
                Case Else
                    Throw [Error]()
            End Select
        End Function

        Private Function ReadDictionary(ByRef bencodedString As String, ByRef index As Integer) As BDictionary
            index += 1
            Dim dict As BDictionary = New BDictionary()

            Try

                While bencodedString(index) <> "e"c
                    Dim K = ReadString(bencodedString, index)
                    Dim V = ReadElement(bencodedString, index)
                    dict.Add(K, V)
                End While

            Catch __unusedBencodingException1__ As BencodingException
                Throw
            Catch e As Exception
                Throw [Error](e)
            End Try

            index += 1
            Return dict
        End Function

        Private Function ReadList(ByRef bencodedString As String, ByRef index As Integer) As BList
            index += 1
            Dim lst As BList = New BList()

            Try

                While bencodedString(index) <> "e"c
                    lst.Add(ReadElement(bencodedString, index))
                End While

            Catch __unusedBencodingException1__ As BencodingException
                Throw
            Catch e As Exception
                Throw [Error](e)
            End Try

            index += 1
            Return lst
        End Function

        Private Function ReadInteger(ByRef bencodedString As String, ByRef index As Integer) As BInteger
            index += 1
            Dim [end] = bencodedString.IndexOf("e"c, index)
            If [end] = -1 Then Throw [Error]()
            Dim [integer] As Long

            Try
                [integer] = Convert.ToInt64(bencodedString.Substring(index, [end] - index))
                index = [end] + 1
            Catch e As Exception
                Throw [Error](e)
            End Try

            Return New BInteger([integer])
        End Function

        Private Function ReadString(ByRef bencodedString As String, ByRef index As Integer) As BString
            Dim length, colon As Integer

            Try
                colon = bencodedString.IndexOf(":"c, index)
                If colon = -1 Then Throw [Error]()
                length = Convert.ToInt32(bencodedString.Substring(index, colon - index))
            Catch e As Exception
                Throw [Error](e)
            End Try

            index = colon + 1
            Dim tmpIndex = index
            index += length

            Try
                Return New BString(bencodedString.Substring(tmpIndex, length))
            Catch e As Exception
                Throw [Error](e)
            End Try
        End Function

        Private Function [Error](ByVal e As Exception) As Exception
            Return New BencodingException("Bencoded string invalid.", e)
        End Function

        Private Function [Error]() As Exception
            Return New BencodingException("Bencoded string invalid.")
        End Function
    End Module




End Namespace
