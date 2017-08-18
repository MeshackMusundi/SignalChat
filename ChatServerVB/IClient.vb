Public Interface IClient
    Sub ParticipantDisconnection(name As String)
    Sub ParticipantReconnection(name As String)
    Sub ParticipantLogin(client As User)
    Sub ParticipantLogout(name As String)
    Sub BroadcastMessage(sender As String, message As String)
    Sub UnicastMessage(sender As String, message As String)
End Interface
