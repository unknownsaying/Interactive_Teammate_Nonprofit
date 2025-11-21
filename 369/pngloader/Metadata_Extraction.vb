Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms

Public Class PNGLoader
    Public Function LoadPNG(filePath As String) As Bitmap
        Try
            If Not File.Exists(filePath) Then
                Throw New FileNotFoundException("PNG file not found: " & filePath)
            End If
            
            ' Verify file signature (PNG magic number)
            Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read)
                Dim header(7) As Byte
                fs.Read(header, 0, 8)
                
                If Not IsValidPNGHeader(header) Then
                    Throw New ArgumentException("Invalid PNG file format")
                End If
            End Using
            
            ' Load the PNG using Bitmap class
            Dim pngImage As New Bitmap(filePath)
            Return pngImage
            
        Catch ex As Exception
            MessageBox.Show($"Error loading PNG: {ex.Message}", "PNG Loader Error")
            Return Nothing
        End Try
    End Function
    
    Private Function IsValidPNGHeader(header As Byte()) As Boolean
        ' PNG file signature: 89 50 4E 47 0D 0A 1A 0A
        Dim pngSignature() As Byte = {&H89, &H50, &H4E, &H47, &HD, &HA, &H1A, &HA}
        
        For i As Integer = 0 To 7
            If header(i) <> pngSignature(i) Then
                Return False
            End If
        Next
        
        Return True
    End Function
End Class