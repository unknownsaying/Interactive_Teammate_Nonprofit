Imports System.Drawing.Imaging

Public Class PNGUtility
    Public Shared Function ConvertToPNG(sourceImage As Image) As Bitmap
        Dim pngBitmap As New Bitmap(sourceImage.Width, sourceImage.Height, 
                                  PixelFormat.Format32bppArgb)
        
        Using g As Graphics = Graphics.FromImage(pngBitmap)
            g.DrawImage(sourceImage, New Rectangle(0, 0, sourceImage.Width, sourceImage.Height))
        End Using
        
        Return pngBitmap
    End Function
    
    Public Shared Sub SaveAsPNG(image As Image, filePath As String, Optional quality As Long = 90L)
        ' Create encoder parameters for quality
        Dim encoderParams As New EncoderParameters(1)
        encoderParams.Param(0) = New EncoderParameter(Encoder.Quality, quality)
        
        ' Get PNG codec info
        Dim pngCodec = GetEncoderInfo("image/png")
        
        image.Save(filePath, pngCodec, encoderParams)
    End Sub
    
    Private Shared Function GetEncoderInfo(mimeType As String) As ImageCodecInfo
        Dim codecs As ImageCodecInfo() = ImageCodecInfo.GetImageEncoders()
        
        For Each codec In codecs
            If codec.MimeType = mimeType Then
                Return codec
            End If
        Next
        
        Return Nothing
    End Function
    
    Public Shared Function ExtractPNGMetadata(filePath As String) As Dictionary(Of String, String)
        Dim metadata As New Dictionary(Of String, String)()
        
        Try
            Using img As Image = Image.FromFile(filePath)
                For Each propItem In img.PropertyItems
                    Dim value As String = ""
                    
                    Select Case propItem.Type
                        Case 2 ' ASCII
                            value = Encoding.ASCII.GetString(propItem.Value).Trim(Chr(0))
                        Case Else
                            value = BitConverter.ToString(propItem.Value)
                    End Select
                    
                    metadata.Add($"{propItem.Id:X}", value)
                Next
            End Using
        Catch ex As Exception
            Console.WriteLine($"Error reading metadata: {ex.Message}")
        End Try
        
        Return metadata
    End Function
End Class