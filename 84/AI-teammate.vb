Imports System
Imports System.ComponentModel
Imports System.Drawing
Imports System.Windows.Forms
Imports System.IO
Imports System.Speech.Synthesis
Imports System.Speech.Recognition
Imports System.Collections.Generic
Imports System.Threading
Imports System.Linq

Public Class FnOSTeammate
    Inherits Form

#Region "Global Variables and Constants"
    Private WithEvents updateTimer As New Timer()
    Private speechSynthesizer As SpeechSynthesizer
    Private speechRecognizer As SpeechRecognitionEngine
    Private aiState As AIState = AIState.Idle
    Private gameState As New GameState()
    Private personality As New PersonalityProfile()
    Private knowledgeBase As New KnowledgeBase()
    Private behaviorTree As BehaviorTree
    Private actionQueue As New Queue(Of AIAction)()
    Private performanceMetrics As New PerformanceMetrics()
    
    Private btnStart As Button
    Private btnStop As Button
    Private lblStatus As Label
    Private txtLog As TextBox
    Private picRadar As PictureBox
    Private pnlHealth As Panel
    Private lblHealth As Label
    
    Private Const UPDATE_INTERVAL As Integer = 100
    Private Const MAX_ACTION_QUEUE As Integer = 10
#End Region

#Region "Enums and Structures"
    Public Enum AIState
        Idle
        Patrol
        Combat
        Support
        Retreat
        Dead
        Reviving
    End Enum

    Public Enum TacticalRole
        Assault
        Support
        Sniper
        Medic
        Engineer
    End Enum

    Public Structure GameState
        Public PlayerHealth As Integer
        Public TeammateHealth As Integer
        Public EnemyCount As Integer
        Public AmmoCount As Integer
        Public CurrentObjective As String
        Public PlayerPosition As Point
        Public TeammatePosition As Point
        Public EnemyPositions As List(Of Point)
        Public IsInCombat As Boolean
        Public MissionTime As TimeSpan
        Public Difficulty As String
    End Structure

    Public Class PersonalityProfile
        Public AggressionLevel As Integer
        Public CautionLevel As Integer
        Public TeamworkLevel As Integer
        Public CommunicationStyle As String
        Public PreferredWeapons As List(Of String)
        Public TacticalRole As TacticalRole
        
        Public Sub New()
            ' Default personality - balanced teammate
            AggressionLevel = 5
            CautionLevel = 5
            TeamworkLevel = 8
            CommunicationStyle = "Professional"
            PreferredWeapons = New List(Of String) From {"Assault Rifle", "SMG"}
            TacticalRole = TacticalRole.Support
        End Sub
    End Class

    Public Class AIAction
        Public Property ActionType As String
        Public Property Target As Object
        Public Property Priority As Integer
        Public Property Duration As Integer
        
        Public Sub New(type As String, target As Object, priority As Integer, duration As Integer)
            ActionType = type
            Me.Target = target
            Me.Priority = priority
            Me.Duration = duration
        End Sub
    End Class

    Public Class PerformanceMetrics
        Public Kills As Integer
        Public Deaths As Integer
        Public Assists As Integer
        Public DamageDealt As Integer
        Public HealingProvided As Integer
        Public ObjectivesCompleted As Integer
        Public Accuracy As Double
        Public SurvivalTime As TimeSpan
    End Class
#End Region

#Region "Main Form and Initialization"
    Public Sub New()
        InitializeComponent()
        InitializeFnOSTeammate()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "FnOS Virtual Teammate"
        Me.Size = New Size(600, 700)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(30, 30, 40)
        Me.ForeColor = Color.White

        ' Status Label
        lblStatus = New Label With {
            .Text = "FnOS Teammate: OFFLINE",
            .Location = New Point(20, 20),
            .Size = New Size(400, 25),
            .ForeColor = Color.Lime,
            .Font = New Font("Consolas", 12, FontStyle.Bold)
        }

        ' Start Button
        btnStart = New Button With {
            .Text = "ACTIVATE TEAMMATE",
            .Location = New Point(20, 60),
            .Size = New Size(150, 40),
            .BackColor = Color.FromArgb(0, 120, 0),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat
        }
        AddHandler btnStart.Click, AddressOf BtnStart_Click

        ' Stop Button
        btnStop = New Button With {
            .Text = "DEACTIVATE",
            .Location = New Point(180, 60),
            .Size = New Size(150, 40),
            .BackColor = Color.FromArgb(120, 0, 0),
            .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat,
            .Enabled = False
        }
        AddHandler btnStop.Click, AddressOf BtnStop_Click

        ' Health Display
        pnlHealth = New Panel With {
            .Location = New Point(350, 60),
            .Size = New Size(200, 30),
            .BackColor = Color.FromArgb(60, 60, 70),
            .BorderStyle = BorderStyle.FixedSingle
        }

        lblHealth = New Label With {
            .Text = "HEALTH: 100%",
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleCenter,
            .ForeColor = Color.Lime,
            .Font = New Font("Consolas", 10, FontStyle.Bold)
        }
        pnlHealth.Controls.Add(lblHealth)

        ' Radar Display
        picRadar = New PictureBox With {
            .Location = New Point(20, 120),
            .Size = New Size(250, 250),
            .BackColor = Color.Black,
            .BorderStyle = BorderStyle.FixedSingle
        }
        AddHandler picRadar.Paint, AddressOf PicRadar_Paint

        ' Log Textbox
        txtLog = New TextBox With {
            .Location = New Point(20, 390),
            .Size = New Size(540, 250),
            .Multiline = True,
            .ScrollBars = ScrollBars.Vertical,
            .BackColor = Color.FromArgb(20, 20, 30),
            .ForeColor = Color.Lime,
            .Font = New Font("Consolas", 9),
            .ReadOnly = True
        }

        Me.Controls.AddRange({lblStatus, btnStart, btnStop, pnlHealth, picRadar, txtLog})
        
        ' Update Timer
        updateTimer.Interval = UPDATE_INTERVAL
    End Sub

    Private Sub InitializeFnOSTeammate()
        ' Initialize AI Systems
        InitializeBehaviorTree()
        InitializeSpeechSystems()
        InitializeKnowledgeBase()
        
        ' Set up initial game state
        gameState.PlayerHealth = 100
        gameState.TeammateHealth = 100
        gameState.EnemyCount = 0
        gameState.AmmoCount = 100
        gameState.CurrentObjective = "Patrol Area"
        gameState.IsInCombat = False
        gameState.MissionTime = TimeSpan.Zero
        
        LogMessage("FnOS Teammate Initialized - Ready for Activation")
    End Sub
#End Region

#Region "Core AI Systems"
    Private Sub InitializeBehaviorTree()
        behaviorTree = New BehaviorTree()
        
        ' Root selector - chooses which behavior to execute
        Dim rootSelector As New BehaviorSelector()
        
        ' Emergency behaviors (high priority)
        Dim emergencySequence As New BehaviorSequence()
        emergencySequence.AddBehavior(New CheckHealthNode(AddressOf CheckLowHealth))
        emergencySequence.AddBehavior(New ExecuteActionNode(AddressOf ExecuteRetreat))
        rootSelector.AddBehavior(emergencySequence)
        
        ' Combat behaviors
        Dim combatSequence As New BehaviorSequence()
        combatSequence.AddBehavior(New CheckConditionNode(AddressOf CheckCombatState))
        combatSequence.AddBehavior(New ExecuteActionNode(AddressOf ExecuteCombatActions))
        rootSelector.AddBehavior(combatSequence)
        
        ' Support behaviors
        Dim supportSequence As New BehaviorSequence()
        supportSequence.AddBehavior(New CheckConditionNode(AddressOf CheckSupportNeeded))
        supportSequence.AddBehavior(New ExecuteActionNode(AddressOf ExecuteSupportActions))
        rootSelector.AddBehavior(supportSequence)
        
        ' Patrol behaviors (lowest priority)
        Dim patrolSequence As New BehaviorSequence()
        patrolSequence.AddBehavior(New CheckConditionNode(AddressOf ShouldPatrol))
        patrolSequence.AddBehavior(New ExecuteActionNode(AddressOf ExecutePatrol))
        rootSelector.AddBehavior(patrolSequence)
        
        behaviorTree.Root = rootSelector
    End Sub

    Private Sub InitializeSpeechSystems()
        Try
            ' Text-to-Speech
            speechSynthesizer = New SpeechSynthesizer()
            speechSynthesizer.SetOutputToDefaultAudioDevice()
            speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult)
            
            ' Speech Recognition
            speechRecognizer = New SpeechRecognitionEngine()
            Dim grammar = CreateVoiceCommands()
            speechRecognizer.LoadGrammar(grammar)
            speechRecognizer.SetInputToDefaultAudioDevice()
            AddHandler speechRecognizer.SpeechRecognized, AddressOf SpeechRecognized
            
            LogMessage("Speech systems initialized successfully")
        Catch ex As Exception
            LogMessage($"Speech system error: {ex.Message}")
        End Try
    End Sub

    Private Sub InitializeKnowledgeBase()
        knowledgeBase.GameTactics.Add("Urban Combat", "Use cover, flank enemies, watch for ambushes")
        knowledgeBase.GameTactics.Add("Open Field", "Maintain distance, use scopes, move strategically")
        knowledgeBase.GameTactics.Add("Close Quarters", "Use shotguns/SMGs, quick reflexes, clear corners")
        
        knowledgeBase.DialogueLines.Add("Combat", New List(Of String) From {
            "Engaging hostiles!", "Enemy spotted!", "Suppressing fire!", "Reloading!"
        })
        
        knowledgeBase.DialogueLines.Add("Support", New List(Of String) From {
            "I've got your back!", "Moving to support!", "Covering you!", "On my way!"
        })
        
        knowledgeBase.DialogueLines.Add("Warning", New List(Of String) From {
            "Watch your six!", "Enemy behind you!", "Grenade!", "Sniper!"
        })
    End Sub
#End Region

#Region "Behavior Tree Implementation"
    Public Class BehaviorTree
        Public Property Root As BehaviorNode
        
        Public Function Evaluate() As Boolean
            Return Root?.Execute() ?? False
        End Function
    End Class

    Public MustInherit Class BehaviorNode
        Public MustOverride Function Execute() As Boolean
    End Class

    Public Class BehaviorSelector
        Inherits BehaviorNode
        Private children As New List(Of BehaviorNode)()
        
        Public Sub AddBehavior(node As BehaviorNode)
            children.Add(node)
        End Sub
        
        Public Overrides Function Execute() As Boolean
            For Each child In children
                If child.Execute() Then Return True
            Next
            Return False
        End Function
    End Class

    Public Class BehaviorSequence
        Inherits BehaviorNode
        Private children As New List(Of BehaviorNode)()
        
        Public Sub AddBehavior(node As BehaviorNode)
            children.Add(node)
        End Sub
        
        Public Overrides Function Execute() As Boolean
            For Each child In children
                If Not child.Execute() Then Return False
            Next
            Return True
        End Function
    End Class

    Public Class CheckConditionNode
        Inherits BehaviorNode
        Private condition As Func(Of Boolean)
        
        Public Sub New(cond As Func(Of Boolean))
            condition = cond
        End Sub
        
        Public Overrides Function Execute() As Boolean
            Return condition()
        End Function
    End Class

    Public Class ExecuteActionNode
        Inherits BehaviorNode
        Private action As Action
        
        Public Sub New(act As Action)
            action = act
        End Sub
        
        Public Overrides Function Execute() As Boolean
            action()
            Return True
        End Function
    End Class

    Public Class CheckHealthNode
        Inherits BehaviorNode
        Private condition As Func(Of Boolean)
        
        Public Sub New(cond As Func(Of Boolean))
            condition = cond
        End Sub
        
        Public Overrides Function Execute() As Boolean
            Return condition()
        End Function
    End Class
#End Region

#Region "AI Behavior Conditions and Actions"
    ' Condition Checks
    Private Function CheckLowHealth() As Boolean
        Return gameState.TeammateHealth < 25 Or gameState.PlayerHealth < 30
    End Function

    Private Function CheckCombatState() As Boolean
        Return gameState.IsInCombat Or gameState.EnemyCount > 0
    End Function

    Private Function CheckSupportNeeded() As Boolean
        Return gameState.PlayerHealth < 70 Or gameState.AmmoCount < 30
    End Function

    Private Function ShouldPatrol() As Boolean
        Return Not gameState.IsInCombat And gameState.EnemyCount = 0
    End Function

    ' Action Executors
    Private Sub ExecuteRetreat()
        If aiState <> AIState.Retreat Then
            aiState = AIState.Retreat
            Speak("Taking cover! Health critical!")
            LogMessage("TEAMMATE: Retreating for safety")
            QueueAction(New AIAction("FindCover", "NearestCover", 10, 5000))
        End If
    End Sub

    Private Sub ExecuteCombatActions()
        If aiState <> AIState.Combat Then
            aiState = AIState.Combat
            Speak("Engaging enemies!")
        End If
        
        Select Case personality.TacticalRole
            Case TacticalRole.Assault
                QueueAction(New AIAction("AggressiveAttack", "NearestEnemy", 8, 3000))
            Case TacticalRole.Support
                QueueAction(New AIAction("SuppressingFire", "EnemyGroup", 7, 4000))
            Case TacticalRole.Sniper
                QueueAction(New AIAction("SnipeTarget", "MostDangerousEnemy", 9, 6000))
            Case TacticalRole.Medic
                QueueAction(New AIAction("CoverPlayer", "PlayerPosition", 6, 2000))
        End Select
    End Sub

    Private Sub ExecuteSupportActions()
        If gameState.PlayerHealth < 50 Then
            Speak("I'll cover you while you heal!")
            QueueAction(New AIAction("DefendPlayer", "PlayerPosition", 9, 5000))
        ElseIf gameState.AmmoCount < 20 Then
            Speak("Watch my back while I reload!")
            QueueAction(New AIAction("ReloadWeapon", "CurrentWeapon", 7, 3000))
        Else
            Speak("Moving to support position!")
            QueueAction(New AIAction("TakePosition", "SupportPosition", 5, 4000))
        End If
    End Sub

    Private Sub ExecutePatrol()
        If aiState <> AIState.Patrol Then
            aiState = AIState.Patrol
            LogMessage("TEAMMATE: Patrolling area")
        End If
        
        QueueAction(New AIAction("MoveToPoint", "NextPatrolPoint", 3, 6000))
        QueueAction(New AIAction("ScanArea", "360DegreeScan", 2, 2000))
    End Sub
#End Region

#Region "Voice Command System"
    Private Function CreateVoiceCommands() As Grammar
        Dim commands As New Choices()
        commands.Add("follow me")
        commands.Add("attack")
        commands.Add("defend")
        commands.Add("cover me")
        commands.Add("fall back")
        commands.Add("regroup")
        commands.Add("hold position")
        commands.Add("move forward")
        commands.Add("need ammo")
        commands.Add("need medic")
        commands.Add("status report")
        commands.Add("activate stealth")
        commands.Add("go loud")
        
        Dim grammarBuilder As New GrammarBuilder()
        grammarBuilder.Append(commands)
        
        Return New Grammar(grammarBuilder)
    End Function

    Private Sub SpeechRecognized(sender As Object, e As SpeechRecognizedEventArgs)
        Dim command = e.Result.Text.ToLower()
        LogMessage($"PLAYER COMMAND: {command}")
        
        Select Case command
            Case "follow me"
                Speak("Following you!")
                QueueAction(New AIAction("FollowPlayer", "Player", 8, 0))
                
            Case "attack"
                Speak("Attacking!")
                aiState = AIState.Combat
                QueueAction(New AIAction("AttackEnemies", "AllVisibleEnemies", 9, 0))
                
            Case "defend"
                Speak("Setting up defense!")
                QueueAction(New AIAction("DefendPosition", "CurrentLocation", 7, 0))
                
            Case "cover me"
                Speak("Covering you!")
                QueueAction(New AIAction("CoverPlayer", "Player", 8, 0))
                
            Case "fall back"
                Speak("Falling back!")
                aiState = AIState.Retreat
                QueueAction(New AIAction("Retreat", "SafePosition", 9, 0))
                
            Case "status report"
                GiveStatusReport()
                
            Case "need medic"
                Speak("I've got meds! Coming to you!")
                QueueAction(New AIAction("HealPlayer", "Player", 10, 0))
                
            Case Else
                Speak("Command acknowledged!")
        End Select
    End Sub

    Private Sub GiveStatusReport()
        Dim report = $"Status: {aiState.ToString()}. Health: {gameState.TeammateHealth}%. " &
                    $"Ammo: {gameState.AmmoCount}%. Enemies: {gameState.EnemyCount}"
        Speak(report)
        LogMessage($"STATUS: {report}")
    End Sub
#End Region

#Region "Core Game Loop and Action Processing"
    Private Sub UpdateTimer_Tick(sender As Object, e As EventArgs) Handles updateTimer.Tick
        UpdateGameState()
        ProcessAI()
        UpdateDisplay()
        ProcessNextAction()
    End Sub

    Private Sub UpdateGameState()
        ' Simulate dynamic game state changes
        gameState.MissionTime = gameState.MissionTime.Add(TimeSpan.FromMilliseconds(UPDATE_INTERVAL))
        
        ' Random events for simulation
        Dim rnd As New Random()
        If rnd.Next(0, 100) < 5 Then ' 5% chance per tick to change state
            gameState.EnemyCount = If(gameState.IsInCombat, rnd.Next(1, 5), 0)
            gameState.IsInCombat = gameState.EnemyCount > 0
        End If
        
        ' Simulate health changes
        If gameState.IsInCombat And rnd.Next(0, 100) < 10 Then
            gameState.TeammateHealth = Math.Max(0, gameState.TeammateHealth - rnd.Next(1, 10))
            gameState.PlayerHealth = Math.Max(0, gameState.PlayerHealth - rnd.Next(1, 5))
        End If
        
        ' Simulate ammo usage
        If gameState.IsInCombat Then
            gameState.AmmoCount = Math.Max(0, gameState.AmmoCount - rnd.Next(1, 3))
        End If
    End Sub

    Private Sub ProcessAI()
        ' Execute behavior tree
        behaviorTree.Evaluate()
        
        ' Update AI state based on conditions
        If gameState.TeammateHealth <= 0 Then
            aiState = AIState.Dead
            Speak("I'm down! Need assistance!")
        ElseIf gameState.TeammateHealth < 25 Then
            aiState = AIState.Retreat
        End If
    End Sub

    Private Sub ProcessNextAction()
        If actionQueue.Count > 0 Then
            Dim action = actionQueue.Dequeue()
            ExecuteImmediateAction(action)
        End If
    End Sub

    Private Sub QueueAction(action As AIAction)
        If actionQueue.Count < MAX_ACTION_QUEUE Then
            actionQueue.Enqueue(action)
            LogMessage($"ACTION QUEUED: {action.ActionType}")
        End If
    End Sub

    Private Sub ExecuteImmediateAction(action As AIAction)
        LogMessage($"EXECUTING: {action.ActionType}")
        
        ' Simulate action execution
        Select Case action.ActionType
            Case "AttackEnemies"
                If gameState.EnemyCount > 0 Then
                    Dim rnd As New Random()
                    performanceMetrics.Kills += rnd.Next(0, 2)
                    Speak("Target eliminated!")
                End If
                
            Case "HealPlayer"
                gameState.PlayerHealth = Math.Min(100, gameState.PlayerHealth + 25)
                performanceMetrics.HealingProvided += 25
                Speak("Player healed!")
                
            Case "ReloadWeapon"
                gameState.AmmoCount = 100
                Speak("Reloaded and ready!")
                
            Case "FollowPlayer"
                Speak("Sticking with you!")
        End Select
    End Sub
#End Region

#Region "UI Controls and Display"
    Private Sub BtnStart_Click(sender As Object, e As EventArgs)
        Try
            updateTimer.Start()
            If speechRecognizer IsNot Nothing Then
                speechRecognizer.RecognizeAsync(RecognizeMode.Multiple)
            End If
            
            btnStart.Enabled = False
            btnStop.Enabled = True
            lblStatus.Text = "FnOS Teammate: ACTIVE"
            lblStatus.ForeColor = Color.Cyan
            
            Speak("FnOS teammate activated! Ready for mission!")
            LogMessage("=== TEAMMATE ACTIVATED ===")
            
        Catch ex As Exception
            LogMessage($"Activation error: {ex.Message}")
        End Try
    End Sub

    Private Sub BtnStop_Click(sender As Object, e As EventArgs)
        updateTimer.Stop()
        If speechRecognizer IsNot Nothing Then
            speechRecognizer.RecognizeAsyncStop()
        End If
        
        btnStart.Enabled = True
        btnStop.Enabled = False
        lblStatus.Text = "FnOS Teammate: OFFLINE"
        lblStatus.ForeColor = Color.Lime
        
        Speak("Teammate deactivated. Good luck!")
        LogMessage("=== TEAMMATE DEACTIVATED ===")
    End Sub

    Private Sub UpdateDisplay()
        ' Update health display
        lblHealth.Text = $"HEALTH: {gameState.TeammateHealth}%"
        pnlHealth.BackColor = GetHealthColor(gameState.TeammateHealth)
        
        ' Update status with current state
        lblStatus.Text = $"FnOS Teammate: {aiState.ToString().ToUpper()}"
        
        ' Refresh radar
        picRadar.Invalidate()
    End Sub

    Private Sub PicRadar_Paint(sender As Object, e As PaintEventArgs)
        Dim g = e.Graphics
        g.Clear(Color.Black)
        
        ' Draw radar background
        Dim center As New Point(picRadar.Width \ 2, picRadar.Height \ 2)
        Dim radius As Integer = Math.Min(center.X, center.Y) - 10
        
        ' Draw radar circles
        g.DrawEllipse(Pens.Green, center.X - radius, center.Y - radius, radius * 2, radius * 2)
        g.DrawEllipse(Pens.Green, center.X - radius \ 2, center.Y - radius \ 2, radius, radius)
        
        ' Draw player (center)
        g.FillEllipse(Brushes.Blue, center.X - 3, center.Y - 3, 6, 6)
        
        ' Draw teammate (slightly offset)
        g.FillEllipse(Brushes.Cyan, center.X - 8, center.Y - 2, 4, 4)
        
        ' Draw enemies (random positions for simulation)
        Dim rnd As New Random()
        For i As Integer = 1 To gameState.EnemyCount
            Dim angle = rnd.Next(0, 360)
            Dim distance = rnd.Next(10, radius - 10)
            Dim enemyX = center.X + CInt(distance * Math.Cos(angle * Math.PI / 180))
            Dim enemyY = center.Y + CInt(distance * Math.Sin(angle * Math.PI / 180))
            g.FillEllipse(Brushes.Red, enemyX - 2, enemyY - 2, 4, 4)
        Next
        
        ' Draw radar sweep
        Static sweepAngle As Integer = 0
        sweepAngle = (sweepAngle + 5) Mod 360
        Dim endX = center.X + CInt(radius * Math.Cos(sweepAngle * Math.PI / 180))
        Dim endY = center.Y + CInt(radius * Math.Sin(sweepAngle * Math.PI / 180))
        g.DrawLine(Pens.Lime, center, New Point(endX, endY))
    End Sub

    Private Function GetHealthColor(health As Integer) As Color
        Return If(health > 70, Color.Green,
               If(health > 30, Color.Orange, Color.Red))
    End Function

    Private Sub LogMessage(message As String)
        If txtLog.InvokeRequired Then
            txtLog.Invoke(Sub() LogMessage(message))
        Else
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}")
            txtLog.SelectionStart = txtLog.Text.Length
            txtLog.ScrollToCaret()
        End If
    End Sub

    Private Sub Speak(text As String)
        If speechSynthesizer IsNot Nothing AndAlso speechSynthesizer.State = SynthesizerState.Ready Then
            Try
                speechSynthesizer.SpeakAsync(text)
            Catch ex As Exception
                ' Silent fail for speech errors
            End Try
        End If
    End Sub
#End Region

#Region "Knowledge Base and Data Storage"
    Public Class KnowledgeBase
        Public GameTactics As New Dictionary(Of String, String)()
        Public DialogueLines As New Dictionary(Of String, List(Of String))()
        Public MapKnowledge As New Dictionary(Of String, List(Of String))()
        Public WeaponPreferences As New Dictionary(Of String, Integer)()
        
        Public Function GetRandomDialogue(category As String) As String
            If DialogueLines.ContainsKey(category) AndAlso DialogueLines(category).Count > 0 Then
                Dim rnd As New Random()
                Return DialogueLines(category)(rnd.Next(DialogueLines(category).Count))
            End If
            Return "Roger that!"
        End Function
    End Class
#End Region

#Region "Form Events and Cleanup"
    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        ' Clean up resources
        updateTimer.Stop()
        
        If speechRecognizer IsNot Nothing Then
            speechRecognizer.RecognizeAsyncStop()
            speechRecognizer.Dispose()
        End If
        
        If speechSynthesizer IsNot Nothing Then
            speechSynthesizer.Dispose()
        End If
        
        MyBase.OnFormClosing(e)
    End Sub
#End Region
End Class

' Application Entry Point
Module Program
    <STAThread>
    Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New FnOSTeammate())
    End Sub
End Module