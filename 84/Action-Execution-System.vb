Public Class ActionExecutor
    Private inputSimulator As InputSimulator
    Private chatHandler As ChatHandler
    
    Public Sub ExecuteDecision(decision As AIDecision)
        Select Case decision.ActionType
            Case ActionType.MoveToPosition
                MoveToPosition(decision.TargetPosition)
                
            Case ActionType.AttackTarget
                AttackEnemy(decision.TargetEnemy)
                
            Case ActionType.UseItem
                UseItem(decision.ItemType)
                
            Case ActionType.Communicate
                SendChatMessage(decision.Message)
                
            Case ActionType.ReviveTeammate
                RevivePlayer(decision.TargetPlayer)
        End Select
    End Sub
    
    Private Sub SendChatMessage(message As String)
        ' Send in-game chat messages
        steamFriends.SendChatMessage(GetCurrentLobby(), EChatEntryType.ChatMsg, message)
    End Sub
    
    Private Sub MoveToPosition(position As Vector3)
        ' Simulate mouse/keyboard input for movement
        inputSimulator.MoveMouseToPosition(CalculateScreenPosition(position))
        inputSimulator.KeyPress(Keys.W) ' Move forward
    End Sub
End Class