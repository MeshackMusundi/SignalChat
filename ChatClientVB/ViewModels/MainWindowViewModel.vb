Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Drawing

Public Class MainWindowViewModel
    Inherits ViewModelBase

    Private chatService As IChatService
    Private dialogService As IDialogService
    Private ctxTaskFactory As TaskFactory
    Private Const MAX_IMAGE_WIDTH As Integer = 150
    Private Const MAX_IMAGE_HEIGHT As Integer = 150

    Private _userName As String
    Public Property UserName As String
        Get
            Return _userName
        End Get
        Set(value As String)
            _userName = value
            OnPropertyChanged()
        End Set
    End Property

    Private _photo As String
    Public Property Photo As String
        Get
            Return _photo
        End Get
        Set(value As String)
            _photo = value
            OnPropertyChanged()
        End Set
    End Property

    Private _participants As New ObservableCollection(Of Participant)
    Public Property Participants As ObservableCollection(Of Participant)
        Get
            Return _participants
        End Get
        Set(value As ObservableCollection(Of Participant))
            _participants = value
            OnPropertyChanged()
        End Set
    End Property

    Private _selectedParticipant As Participant
    Public Property SelectedParticipant As Participant
        Get
            Return _selectedParticipant
        End Get
        Set(value As Participant)
            _selectedParticipant = value
            If SelectedParticipant.HasSentNewMessage Then SelectedParticipant.HasSentNewMessage = False
            OnPropertyChanged()
        End Set
    End Property

    Private _userMode As UserModes
    Public Property UserMode As UserModes
        Get
            Return _userMode
        End Get
        Set(value As UserModes)
            _userMode = value
            OnPropertyChanged()
        End Set
    End Property

    Private _message As String
    Public Property Message As String
        Get
            Return _message
        End Get
        Set(value As String)
            _message = value
            OnPropertyChanged()
        End Set
    End Property

    Private _isConnected As Boolean
    Public Property IsConnected As Boolean
        Get
            Return _isConnected
        End Get
        Set(value As Boolean)
            _isConnected = value
            OnPropertyChanged()
        End Set
    End Property

    Private _isLoggedIn As Boolean
    Public Property IsLoggedIn As Boolean
        Get
            Return _isLoggedIn
        End Get
        Set(value As Boolean)
            _isLoggedIn = value
            OnPropertyChanged()
        End Set
    End Property

    Private _connectCommand As ICommand
    Public ReadOnly Property ConnectCommand As ICommand
        Get
            If _connectCommand Is Nothing Then _connectCommand = New RelayCommandAsync(Function() Connect())
            Return _connectCommand
        End Get
    End Property

    Private Async Function Connect() As Task(Of Boolean)
        Try
            Await chatService.ConnectAsync()
            IsConnected = True
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private _loginCommand As ICommand
    Public ReadOnly Property LoginCommand As ICommand
        Get
            If _loginCommand Is Nothing Then _loginCommand =
                New RelayCommandAsync(Function() Login(), AddressOf CanLogin)
            Return _loginCommand
        End Get
    End Property

    Private Async Function Login() As Task(Of Boolean)
        Try
            Dim users As List(Of User)
            users = Await chatService.LoginAsync(_userName, Avatar())
            If users IsNot Nothing Then
                users.ForEach(Sub(u) Participants.Add(New Participant With {.Name = u.Name, .Photo = u.Photo}))
                UserMode = UserModes.Chat
                IsLoggedIn = True
                Return True
            Else
                dialogService.ShowNotification("Username is already in use")
                Return False
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Function CanLogin() As Boolean
        Return Not String.IsNullOrEmpty(UserName) AndAlso UserName.Length >= 2 AndAlso IsConnected
    End Function

    Private _logoutCommand As ICommand
    Public ReadOnly Property LogoutCommand As ICommand
        Get
            If _logoutCommand Is Nothing Then _logoutCommand =
                New RelayCommandAsync(Function() Logout(), AddressOf CanLogout)
            Return _logoutCommand
        End Get
    End Property

    Private Async Function Logout() As Task(Of Boolean)
        Try
            Await chatService.LogoutAsync()
            UserMode = UserModes.Login
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Function CanLogout() As Boolean
        Return IsConnected AndAlso IsLoggedIn
    End Function

    Private _sendMessageCommand As ICommand
    Public ReadOnly Property SendMessageCommand As ICommand
        Get
            If _sendMessageCommand Is Nothing Then _sendMessageCommand =
                New RelayCommandAsync(Function() SendMessage(), AddressOf CanSendMessage)
            Return _sendMessageCommand
        End Get
    End Property

    Private Async Function SendMessage() As Task(Of Boolean)
        Try
            Dim recepient = _selectedParticipant.Name
            Await chatService.SendUnicastMessageAsync(recepient, _message)
            Return True
        Catch ex As Exception
            Return False
        Finally
            Dim msg As New ChatMessage With {.Author = UserName, .Message = _message,
                .Time = DateTime.Now, .IsOriginNative = True}
            SelectedParticipant.Chatter.Add(msg)
            Message = String.Empty
        End Try
    End Function

    Private Function CanSendMessage() As Boolean
        Return Not String.IsNullOrEmpty(Message) AndAlso IsConnected AndAlso
            _selectedParticipant IsNot Nothing AndAlso _selectedParticipant.IsLoggedIn
    End Function

    Private _selectPhotoCommand As ICommand
    Public ReadOnly Property SelectPhotoCommand As ICommand
        Get
            If _selectPhotoCommand Is Nothing Then _selectPhotoCommand = New RelayCommand(AddressOf SelectPhoto)
            Return _selectPhotoCommand
        End Get
    End Property

    Private Sub SelectPhoto()
        Dim pic = dialogService.OpenFile("Select image file", "Images (*.jpg;*.png)|*.jpg;*.png")
        If Not String.IsNullOrEmpty(pic) Then
            Dim img = Image.FromFile(pic)
            If img.Width > MAX_IMAGE_WIDTH OrElse img.Height > MAX_IMAGE_HEIGHT Then
                dialogService.ShowNotification($"Image size should be {MAX_IMAGE_WIDTH} x {MAX_IMAGE_HEIGHT} or less.")
                Exit Sub
            End If
            Photo = pic
        End If
    End Sub

#Region "Event handlers"
    Private Sub NewMessage(name As String, msg As String, mt As MessageType)
        If mt = MessageType.Unicast Then
            Dim cm As New ChatMessage With {.Author = name, .Message = msg, .Time = DateTime.Now}
            Dim sender = _participants.Where(Function(u) String.Equals(u.Name, name)).FirstOrDefault
            ctxTaskFactory.StartNew(Sub() sender.Chatter.Add(cm)).Wait()

            If Not (SelectedParticipant IsNot Nothing AndAlso sender.Name.Equals(SelectedParticipant.Name)) Then
                ctxTaskFactory.StartNew(Sub() sender.HasSentNewMessage = True).Wait()
            End If
        End If
    End Sub

    Private Sub ParticipantLogin(ByVal u As User)
        Dim ptp = Participants.FirstOrDefault(Function(p) String.Equals(p.Name, u.Name))
        If _isLoggedIn AndAlso ptp Is Nothing Then
            ctxTaskFactory.StartNew(Sub() Participants.Add(New Participant With {.Name = u.Name, .Photo = u.Photo})).Wait()
        End If
    End Sub

    Private Sub ParticipantDisconnection(ByVal name As String)
        Dim person = Participants.Where(Function(p) String.Equals(p.Name, name)).FirstOrDefault
        If person IsNot Nothing Then person.IsLoggedIn = False
    End Sub

    Private Sub ParticipantReconnection(ByVal name As String)
        Dim person = Participants.Where(Function(p) String.Equals(p.Name, name)).FirstOrDefault
        If person IsNot Nothing Then person.IsLoggedIn = True
    End Sub

    Private Sub Reconnecting()
        IsConnected = False
        IsLoggedIn = False
    End Sub

    Private Async Sub Reconnected()
        Dim pic = Avatar()
        If Not String.IsNullOrEmpty(_userName) Then Await chatService.LoginAsync(_userName, pic)
        IsConnected = True
        IsLoggedIn = True
    End Sub

    Private Async Sub Disconnected()
        Dim connectionTask = chatService.ConnectAsync()
        Await connectionTask.ContinueWith(Sub(t)
                                              If Not t.IsFaulted Then
                                                  IsConnected = True
                                                  chatService.LoginAsync(_userName, Avatar()).Wait()
                                                  IsLoggedIn = True
                                              End If
                                          End Sub)
    End Sub
#End Region

    Private Function Avatar() As Byte()
        Dim pic As Byte() = Nothing
        If Not String.IsNullOrEmpty(_photo) Then pic = File.ReadAllBytes(_photo)
        Return pic
    End Function

    Public Sub New(chatSvc As IChatService, diagSvc As IDialogService)
        dialogService = diagSvc
        chatService = chatSvc
        AddHandler chatSvc.NewMessage, AddressOf NewMessage
        AddHandler chatSvc.ParticipantLoggedIn, AddressOf ParticipantLogin
        AddHandler chatSvc.ParticipantLoggedOut, AddressOf ParticipantDisconnection
        AddHandler chatSvc.ParticipantDisconnected, AddressOf ParticipantDisconnection
        AddHandler chatSvc.ParticipantReconnected, AddressOf ParticipantReconnection
        AddHandler chatSvc.ConnectionReconnecting, AddressOf Reconnecting
        AddHandler chatSvc.ConnectionReconnected, AddressOf Reconnected
        AddHandler chatSvc.ConnectionClosed, AddressOf Disconnected

        ctxTaskFactory = New TaskFactory(TaskScheduler.FromCurrentSynchronizationContext)
    End Sub
End Class