Module Program
    <STAThread>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New MainForm())
        
        ' Or use directly:
        ' Dim loader As New PNGLoader()
        ' Dim image = loader.LoadPNG("test.png")
        ' 
        ' Dim parser As New AdvancedPNGParser()
        ' parser.ParsePNG("test.png")
        ' parser.DisplayPNGInfo()
    End Sub
End Module