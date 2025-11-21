Imports System.Text

Public Class AdvancedPNGParser
    Public Structure PNGChunk
        Public Length As UInteger
        Public ChunkType As String
        Public Data As Byte()
        Public CRC As UInteger
    End Structure
    
    Public Property Chunks As List(Of PNGChunk)
    Public Property Width As Integer
    Public Property Height As Integer
    Public Property BitDepth As Byte
    Public Property ColorType As Byte
    Public Property Compression As Byte
    Public Property Filter As Byte
    Public Property Interlace As Byte
    
    Public Sub ParsePNG(filePath As String)
        Chunks = New List(Of PNGChunk)()
        
        Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read)
            ' Skip PNG signature (already verified)
            fs.Seek(8, SeekOrigin.Begin)
            
            While fs.Position < fs.Length
                Dim chunk = ReadChunk(fs)
                Chunks.Add(chunk)
                
                ' Process critical chunks
                ProcessChunk(chunk)
                
                ' Stop after IEND chunk
                If chunk.ChunkType = "IEND" Then
                    Exit While
                End If
            End While
        End Using
    End Sub
    
    Private Function ReadChunk(stream As FileStream) As PNGChunk
        Dim chunk As New PNGChunk()
        
        ' Read length (4 bytes, big-endian)
        Dim lengthBytes(3) As Byte
        stream.Read(lengthBytes, 0, 4)
        Array.Reverse(lengthBytes)
        chunk.Length = BitConverter.ToUInt32(lengthBytes, 0)
        
        ' Read chunk type (4 bytes)
        Dim typeBytes(3) As Byte
        stream.Read(typeBytes, 0, 4)
        chunk.ChunkType = Encoding.ASCII.GetString(typeBytes)
        
        ' Read chunk data
        If chunk.Length > 0 Then
            chunk.Data = New Byte(chunk.Length - 1) {}
            stream.Read(chunk.Data, 0, CInt(chunk.Length))
        End If
        
        ' Read CRC (4 bytes)
        Dim crcBytes(3) As Byte
        stream.Read(crcBytes, 0, 4)
        Array.Reverse(crcBytes)
        chunk.CRC = BitConverter.ToUInt32(crcBytes, 0)
        
        Return chunk
    End Function
    
    Private Sub ProcessChunk(chunk As PNGChunk)
        Select Case chunk.ChunkType
            Case "IHDR"
                ParseIHDRChunk(chunk)
            Case "PLTE"
                ParsePLTEChunk(chunk)
            Case "IDAT"
                ' Image data chunks
            Case "IEND"
                ' End of image
            Case "tEXt"
                ParseTextChunk(chunk)
            Case "tIME"
                ParseTimeChunk(chunk)
        End Select
    End Sub
    
    Private Sub ParseIHDRChunk(chunk As PNGChunk)
        If chunk.Data.Length >= 13 Then
            ' Width (4 bytes)
            Dim widthBytes(3) As Byte
            Array.Copy(chunk.Data, 0, widthBytes, 0, 4)
            Array.Reverse(widthBytes)
            Width = BitConverter.ToInt32(widthBytes, 0)
            
            ' Height (4 bytes)
            Dim heightBytes(3) As Byte
            Array.Copy(chunk.Data, 4, heightBytes, 0, 4)
            Array.Reverse(heightBytes)
            Height = BitConverter.ToInt32(heightBytes, 0)
            
            ' Other IHDR fields
            BitDepth = chunk.Data(8)
            ColorType = chunk.Data(9)
            Compression = chunk.Data(10)
            Filter = chunk.Data(11)
            Interlace = chunk.Data(12)
        End If
    End Sub
    
    Private Sub ParsePLTEChunk(chunk As PNGChunk)
        ' Palette chunk - contains RGB triplets
        Dim paletteEntries = chunk.Data.Length \ 3
        Console.WriteLine($"Palette entries: {paletteEntries}")
    End Sub
    
    Private Sub ParseTextChunk(chunk As PNGChunk)
        ' Textual information chunk
        Dim textData = Encoding.ASCII.GetString(chunk.Data)
        Dim nullIndex = textData.IndexOf(Chr(0))
        
        If nullIndex >= 0 Then
            Dim keyword = textData.Substring(0, nullIndex)
            Dim text = textData.Substring(nullIndex + 1)
            Console.WriteLine($"Text chunk - {keyword}: {text}")
        End If
    End Sub
    
    Private Sub ParseTimeChunk(chunk As PNGChunk)
        ' Time chunk - last modification time
        If chunk.Data.Length = 7 Then
            Dim year = chunk.Data(0) << 8 Or chunk.Data(1)
            Dim month = chunk.Data(2)
            Dim day = chunk.Data(3)
            Dim hour = chunk.Data(4)
            Dim minute = chunk.Data(5)
            Dim second = chunk.Data(6)
            
            Console.WriteLine($"Last modified: {year}-{month}-{day} {hour}:{minute}:{second}")
        End If
    End Sub
    
    Public Sub DisplayPNGInfo()
        Console.WriteLine($"PNG Dimensions: {Width}x{Height}")
        Console.WriteLine($"Bit Depth: {BitDepth}")
        Console.WriteLine($"Color Type: {ColorType}")
        Console.WriteLine($"Compression: {Compression}")
        Console.WriteLine($"Filter: {Filter}")
        Console.WriteLine($"Interlace: {Interlace}")
        Console.WriteLine($"Total Chunks: {Chunks.Count}")
        
        For Each chunk In Chunks
            Console.WriteLine($"Chunk: {chunk.ChunkType}, Length: {chunk.Length}")
        Next
    End Sub
End Class