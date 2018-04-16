Public Interface IClient
    Sub ParticipantDisconnection(name As String)
    Sub ParticipantReconnection(name As String)
    Sub ParticipantLogin(client As User)
    Sub ParticipantLogout(name As String)
    Sub BroadcastTextMessage(sender As String, message As String)
    Sub BroadcastPictureMessage(sender As String, ByVal img As Byte())
    Sub UnicastTextMessage(sender As String, message As String)
    Sub UnicastPictureMessage(sender As String, ByVal img As Byte())
    Sub ParticipantTyping(sender As String)
End Interface
