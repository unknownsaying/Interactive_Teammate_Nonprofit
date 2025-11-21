Imports Steamworks

Public Class VirtualTeammate
    Private steamClient As SteamClient
    Private steamUser As SteamUser
    Private steamFriends As SteamFriends
    Private steamUtils As SteamUtils
    Private isInitialized As Boolean = False
    
    Public Sub InitializeSteam()
        Try
            If SteamAPI.Init() Then
                steamClient = SteamClient.Instance
                steamUser = SteamUser.Instance
                steamFriends = SteamFriends.Instance
                steamUtils = SteamUtils.Instance
                isInitialized = True
                
                AddHandler steamFriends.OnGameLobbyJoinRequested, AddressOnGameLobbyJoinRequested
                AddHandler steamFriends.OnPersonaStateChange, AddressOf OnFriendStatusChanged
                
                Console.WriteLine("Steam integration initialized successfully")
            Else
                Throw New Exception("Failed to initialize Steam API")
            End If
        Catch ex As Exception
            Console.WriteLine($"Steam init error: {ex.Message}")
        End Try
    End Sub
End Class