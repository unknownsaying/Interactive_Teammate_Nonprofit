Public Class CommunicationManager
    Private speechSynthesizer As SpeechSynthesizer
    Private voiceRecognizer As VoiceRecognizer
    
    Public Sub InitializeCommunication()
        ' Text-to-speech for AI responses
        speechSynthesizer = New SpeechSynthesizer()
        speechSynthesizer.SetOutputToDefaultAudioDevice()
        
        ' Voice recognition for player commands
        voiceRecognizer = New VoiceRecognizer()
        AddHandler voiceRecognizer.SpeechRecognized, AddressOf OnVoiceCommand
    End Sub
    
    Private Sub OnVoiceCommand(sender As Object, e As SpeechRecognizedEventArgs)
        Dim command = e.Result.Text.ToLower()
        
        Select Case True
            Case command.Contains("follow me")
                ExecuteCommand(New FollowPlayerCommand())
            Case command.Contains("attack")
                ExecuteCommand(New AttackCommand())
            Case command.Contains("defend")
                ExecuteCommand(New DefendCommand())
            Case command.Contains("heal")
                ExecuteCommand(New HealCommand())
        End Select
    End Sub
    
    Public Sub Speak(message As String)
        speechSynthesizer.SpeakAsync(message)
    End Sub
End Class