Imports System.IO
Imports System.Text

Public Class PngLoader
    Public Structure PngChunk
        Public Length As Integer
        Public Type As String
        Public Data As Byte()
        Public Crc As UInteger
    End Structure

    Public Shared Function ReadPngChunks(filePath As String) As List(Of PngChunk)
        Dim chunks As New List(Of PngChunk)()

        Using stream = New FileStream(filePath, FileMode.Open, FileAccess.Read)
            ' Read and verify the signature
            Dim signature(7) As Byte
            stream.Read(signature, 0, 8)

            If Not CheckSignature(signature) Then
                Throw New Exception("Not a valid PNG file.")
            End If

            ' Read chunks until end of file
            While stream.Position < stream.Length
                Dim chunk = ReadChunk(stream)
                chunks.Add(chunk)

                ' If we hit the IEND chunk, we break
                If chunk.Type = "IEND" Then
                    Exit While
                End If
            End While
        End Using

        Return chunks
    End Function

    Private Shared Function CheckSignature(signature As Byte()) As Boolean
        Dim expected As Byte() = {137, 80, 78, 71, 13, 10, 26, 10}
        For i As Integer = 0 To 7
            If signature(i) <> expected(i) Then
                Return False
            End If
        Next
        Return True
    End Function

    Private Shared Function ReadChunk(stream As Stream) As PngChunk
        Dim chunk As New PngChunk()

        ' Read length (4 bytes, big-endian)
        Dim lengthBytes(3) As Byte
        stream.Read(lengthBytes, 0, 4)
        Array.Reverse(lengthBytes) ' Convert big-endian to little-endian for BitConverter
        chunk.Length = BitConverter.ToInt32(lengthBytes, 0)

        ' Read type (4 bytes)
        Dim typeBytes(3) As Byte
        stream.Read(typeBytes, 0, 4)
        chunk.Type = Encoding.ASCII.GetString(typeBytes)

        ' Read data
        chunk.Data = New Byte(chunk.Length - 1) {}
        stream.Read(chunk.Data, 0, chunk.Length)

        ' Read CRC (4 bytes, big-endian)
        Dim crcBytes(3) As Byte
        stream.Read(crcBytes, 0, 4)
        Array.Reverse(crcBytes)
        chunk.Crc = BitConverter.ToUInt32(crcBytes, 0)

        Return chunk
    End Function
End Class