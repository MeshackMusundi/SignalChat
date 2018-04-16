Imports System.Net
Imports ChatClientVB
Imports Microsoft.AspNet.SignalR.Client

Public Class ChatService
    Implements IChatService

    Public Event ParticipantDisconnected(name As String) Implements IChatService.ParticipantDisconnected
    Public Event ParticipantLoggedIn(participant As User) Implements IChatService.ParticipantLoggedIn
    Public Event ParticipantLoggedOut(name As String) Implements IChatService.ParticipantLoggedOut
    Public Event ParticipantReconnected(name As String) Implements IChatService.ParticipantReconnected
    Public Event ConnectionReconnecting() Implements IChatService.ConnectionReconnecting
    Public Event ConnectionReconnected() Implements IChatService.ConnectionReconnected
    Public Event ConnectionClosed() Implements IChatService.ConnectionClosed
    Public Event NewTextMessage(sender As String, msg As String, mt As MessageType) Implements IChatService.NewTextMessage
    Public Event NewImageMessage(sender As String, img As Byte(), mt As MessageType) Implements IChatService.NewImageMessage
    Public Event ParticipantTyping(name As String) Implements IChatService.ParticipantTyping

    Private hubProxy As IHubProxy
    Private connection As HubConnection
    Private url As String = "http://localhost:8080/signalchat"

    Public Async Function ConnectAsync() As Task Implements IChatService.ConnectAsync
        connection = New HubConnection(url)
        hubProxy = connection.CreateHubProxy("ChatHub")
        hubProxy.On(Of User)("ParticipantLogin", Sub(u) RaiseEvent ParticipantLoggedIn(u))
        hubProxy.On(Of String)("ParticipantLogout", Sub(n) RaiseEvent ParticipantLoggedOut(n))
        hubProxy.On(Of String)("ParticipantDisconnection", Sub(n) RaiseEvent ParticipantDisconnected(n))
        hubProxy.On(Of String)("ParticipantReconnection", Sub(n) RaiseEvent ParticipantReconnected(n))
        hubProxy.On(Of String, String)("BroadcastTextMessage",
                                       Sub(n, m) RaiseEvent NewTextMessage(n, m, MessageType.Broadcast))
        hubProxy.On(Of String, Byte())("BroadcastPictureMessage",
                                       Sub(n, m) RaiseEvent NewImageMessage(n, m, MessageType.Broadcast))
        hubProxy.On(Of String, String)("UnicastTextMessage",
                                       Sub(n, m) RaiseEvent NewTextMessage(n, m, MessageType.Unicast))
        hubProxy.On(Of String, Byte())("UnicastPictureMessage",
                                       Sub(n, m) RaiseEvent NewImageMessage(n, m, MessageType.Unicast))
        hubProxy.On(Of String)("ParticipantTyping", Sub(p) RaiseEvent ParticipantTyping(p))

        AddHandler connection.Reconnecting, AddressOf Reconnecting
        AddHandler connection.Reconnected, AddressOf Reconnected
        AddHandler connection.Closed, AddressOf Disconnected

        ServicePointManager.DefaultConnectionLimit = 10
        Await connection.Start()
    End Function

    Private Sub Disconnected()
        RaiseEvent ConnectionClosed()
    End Sub

    Private Sub Reconnecting()
        RaiseEvent ConnectionReconnecting()
    End Sub

    Private Sub Reconnected()
        RaiseEvent ConnectionReconnected()
    End Sub

    Public Async Function LoginAsync(name As String, photo As Byte()) As Task(Of List(Of User)) Implements IChatService.LoginAsync
        Dim users = Await hubProxy.Invoke(Of List(Of User))("Login", New Object() {name, photo})
        Return users
    End Function

    Public Async Function LogoutAsync() As Task Implements IChatService.LogoutAsync
        Await hubProxy.Invoke("Logout")
    End Function

    Public Async Function SendBroadcastMessageAsync(msg As String) As Task Implements IChatService.SendBroadcastMessageAsync
        Await hubProxy.Invoke("BroadcastTextMessage", msg)
    End Function

    Public Async Function SendBroadcastMessageAsync(img As Byte()) As Task Implements IChatService.SendBroadcastMessageAsync
        Await hubProxy.Invoke("BroadcastImageMessage", img)
    End Function

    Public Async Function SendUnicastMessageAsync(recepient As String, msg As String) As Task Implements IChatService.SendUnicastMessageAsync
        Await hubProxy.Invoke("UnicastTextMessage", New Object() {recepient, msg})
    End Function

    Public Async Function SendUnicastMessageAsync(recepient As String, img As Byte()) As Task Implements IChatService.SendUnicastMessageAsync
        Await hubProxy.Invoke("UnicastImageMessage", New Object() {recepient, img})
    End Function

    Public Async Function TypingAsync(recepient As String) As Task Implements IChatService.TypingAsync
        Await hubProxy.Invoke("Typing", recepient)
    End Function
End Class
