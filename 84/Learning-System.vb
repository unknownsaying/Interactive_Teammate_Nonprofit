Public Class LearningSystem
    Private trainingData As List(Of TrainingExample)
    Private model As NeuralNetwork
    
    Public Sub LearnFromExperience(gameState As GameState, decision As AIDecision, outcome As GameOutcome)
        Dim example As New TrainingExample With {
            .InputState = gameState,
            .ActionTaken = decision,
            .Reward = CalculateReward(outcome)
        }
        
        trainingData.Add(example)
        
        If trainingData.Count > 1000 Then
            RetrainModel()
        End If
    End Sub
    
    Private Function CalculateReward(outcome As GameOutcome) As Double
        Dim reward As Double = 0
        
        If outcome.MissionSuccess Then reward += 100
        If outcome.TeamWiped Then reward -= 50
        reward += outcome.EnemiesKilled * 10
        reward -= outcome.Deaths * 25
        reward += outcome.ObjectiveProgress * 30
        
        Return reward
    End Function
End Class