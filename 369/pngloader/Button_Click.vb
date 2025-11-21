Imports System.Drawing
Imports System.IO

Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Using openFileDialog As New OpenFileDialog()
            openFileDialog.Filter = "PNG Files (*.png)|*.png|All files (*.*)|*.*"
            openFileDialog.RestoreDirectory = True

            If openFileDialog.ShowDialog() = DialogResult.OK Then
                Try
                    ' Load the image and display in PictureBox
                    PictureBox1.Image = Image.FromFile(openFileDialog.FileName)
                Catch ex As Exception
                    MessageBox.Show("Error loading image: " & ex.Message)
                End Try
            End If
        End Using
    End Sub
End Class