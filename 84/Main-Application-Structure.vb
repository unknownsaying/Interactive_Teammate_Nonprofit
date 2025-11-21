Public Class VirtualTeammateApp
    Private WithEvents gameMonitorTimer As New Timer()
    Private aiEngine As AIDecisionEngine
    Private actionExecutor As ActionExecutor
    Private gameMonitor As GameStateMonitor
    Private commManager As CommunicationManager
    Private learningSystem As LearningSystem
    
    Public Sub New()
        InitializeComponent()
        SetupApplication()
    End Sub
    
    Private Sub SetupApplication()
        ' Initialize all components
        InitializeSteam()
        
        aiEngine = New AIDecisionEngine()
        actionExecutor = New ActionExecutor()
        gameMonitor = New GameStateMonitor()
        commManager = New CommunicationManager()
        learningSystem = New LearningSystem()
        
        ' Set up monitoring timer (10 times per second)
        gameMonitorTimer.Interval = 100
        gameMonitorTimer.Start()
    End Sub
    
    Private Sub GameMonitorTimer_Tick(sender As Object, e As EventArgs) Handles gameMonitorTimer.Tick
        Dim gameState = gameMonitor.GetCurrentGameState()
        
        If gameState IsNot Nothing Then
            Dim decision = aiEngine.MakeDecision(gameState)
            actionExecutor.ExecuteDecision(decision)
        End If
    End Sub
    
    Public Sub JoinGameWithFriend(friendSteamID As ULong)
        If steamFriends.InviteUserToGame(friendSteamID, "") Then
            commManager.Speak("Joining your game now!")
        End If
    End Sub
End Class