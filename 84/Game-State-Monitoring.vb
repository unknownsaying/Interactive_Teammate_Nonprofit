Public Class GameStateMonitor
    Private gameMemory As GameMemoryReader
    Private gameData As GameSpecificData
    
    Public Sub MonitorGameState()
        ' Monitor active game window
        Dim activeProcess As Process = GetForegroundProcess()
        
        If IsSteamGame(activeProcess) Then
            Dim gameState = ReadGameState(activeProcess)
            UpdateAIDecision(gameState)
        End If
    End Sub
    
    Private Function ReadGameState(process As Process) As GameState
        ' Read game memory for player position, health, objectives, etc.
        Return New GameState With {
            .PlayerHealth = memoryReader.ReadHealth(),
            .PlayerPosition = memoryReader.ReadPosition(),
            .EnemyPositions = memoryReader.ReadEnemies(),
            .Objectives = memoryReader.ReadObjectives(),
            .TeamStatus = memoryReader.ReadTeamStatus()
        }
    End Function
End Class