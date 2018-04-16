Public Interface IChatService
    Event ParticipantLoggedIn(ByVal participant As User)
    Event ParticipantLoggedOut(ByVal name As String)
    Event ParticipantDisconnected(ByVal name As String)
    Event ParticipantReconnected(ByVal name As String)
    Event ConnectionReconnecting()
    Event ConnectionReconnected()
    Event ConnectionClosed()
    Event NewTextMessage(ByVal sender As String, ByVal msg As String, ByVal mt As MessageType)
    Event NewImageMessage(ByVal sender As String, ByVal img As Byte(), ByVal mt As MessageType)
    Event ParticipantTyping(ByVal name As String)

    Function ConnectAsync() As Task
    Function LoginAsync(name As String, photo As Byte()) As Task(Of List(Of User))
    Function LogoutAsync() As Task

    Function SendBroadcastMessageAsync(msg As String) As Task
    Function SendBroadcastMessageAsync(img As Byte()) As Task
    Function SendUnicastMessageAsync(ByVal recepient As String, ByVal msg As String) As Task
    Function SendUnicastMessageAsync(ByVal recepient As String, ByVal img As Byte()) As Task
    Function TypingAsync(recepient As String) As Task
End Interface