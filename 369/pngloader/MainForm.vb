Public Class MainForm
    Inherits Form
    
    Private pictureBox As PictureBox
    Private btnLoad As Button
    Private lblInfo As Label
    Private pngLoader As PNGLoader
    Private pngParser As AdvancedPNGParser
    
    Public Sub New()
        InitializeComponent()
        pngLoader = New PNGLoader()
        pngParser = New AdvancedPNGParser()
    End Sub
    
    Private Sub InitializeComponent()
        Me.Text = "VB.NET PNG Loader"
        Me.Size = New Size(800, 600)
        
        ' Create controls
        pictureBox = New PictureBox With {
            .Size = New Size(400, 300),
            .Location = New Point(20, 60),
            .SizeMode = PictureBoxSizeMode.Zoom,
            .BorderStyle = BorderStyle.FixedSingle
        }
        
        btnLoad = New Button With {
            .Text = "Load PNG File",
            .Location = New Point(20, 20),
            .Size = New Size(100, 30)
        }
        
        lblInfo = New Label With {
            .Location = New Point(450, 60),
            .Size = New Size(300, 200),
            .BorderStyle = BorderStyle.FixedSingle
        }
        
        ' Add event handlers
        AddHandler btnLoad.Click, AddressOf BtnLoad_Click
        
        ' Add controls to form
        Me.Controls.Add(pictureBox)
        Me.Controls.Add(btnLoad)
        Me.Controls.Add(lblInfo)
    End Sub
    
    Private Sub BtnLoad_Click(sender As Object, e As EventArgs)
        Using openDialog As New OpenFileDialog()
            openDialog.Filter = "PNG Files (*.png)|*.png|All Files (*.*)|*.*"
            openDialog.Title = "Select a PNG File"
            
            If openDialog.ShowDialog() = DialogResult.OK Then
                LoadAndDisplayPNG(openDialog.FileName)
            End If
        End Using
    End Sub
    
    Private Sub LoadAndDisplayPNG(filePath As String)
        Try
            ' Load and display image
            Dim pngImage = pngLoader.LoadPNG(filePath)
            If pngImage IsNot Nothing Then
                pictureBox.Image = pngImage
                
                ' Parse PNG structure
                pngParser.ParsePNG(filePath)
                
                ' Display PNG information
                Dim infoText As New StringBuilder()
                infoText.AppendLine($"File: {Path.GetFileName(filePath)}")
                infoText.AppendLine($"Size: {pngImage.Width} x {pngImage.Height}")
                infoText.AppendLine($"Pixel Format: {pngImage.PixelFormat}")
                infoText.AppendLine($"Horizontal Resolution: {pngImage.HorizontalResolution}")
                infoText.AppendLine($"Vertical Resolution: {pngImage.VerticalResolution}")
                
                ' Add advanced parser info
                infoText.AppendLine($"Bit Depth: {pngParser.BitDepth}")
                infoText.AppendLine($"Color Type: {GetColorTypeName(pngParser.ColorType)}")
                infoText.AppendLine($"Interlace: {GetInterlaceName(pngParser.Interlace)}")
                infoText.AppendLine($"Chunks Found: {pngParser.Chunks.Count}")
                
                lblInfo.Text = infoText.ToString()
            End If
            
        Catch ex As Exception
            MessageBox.Show($"Error: {ex.Message}", "PNG Load Error", 
                          MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    
    Private Function GetColorTypeName(colorType As Byte) As String
        Select Case colorType
            Case 0 : Return "Grayscale"
            Case 2 : Return "Truecolor (RGB)"
            Case 3 : Return "Indexed Color"
            Case 4 : Return "Grayscale with Alpha"
            Case 6 : Return "Truecolor with Alpha (RGBA)"
            Case Else : Return $"Unknown ({colorType})"
        End Select
    End Function
    
    Private Function GetInterlaceName(interlace As Byte) As String
        Return If(interlace = 0, "None", "Adam7")
    End Function
    
    ' Clean up resources
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If pictureBox.Image IsNot Nothing Then
            pictureBox.Image.Dispose()
        End If
        MyBase.OnFormClosing(e)
    End Sub
End Class