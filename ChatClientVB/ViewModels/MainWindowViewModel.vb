Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Drawing
Imports System.Reactive.Linq

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

    Private _profilePic As String
    Public Property ProfilePic As String
        Get
            Return _profilePic
        End Get
        Set(value As String)
            _profilePic = value
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

    Private _textMessage As String
    Public Property TextMessage As String
        Get
            Return _textMessage
        End Get
        Set(value As String)
            _textMessage = value
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

#Region "Connect Command"
    Private _connectCommand As ICommand
    Public ReadOnly Property ConnectCommand As ICommand
        Get
            Return If(_connectCommand, New RelayCommandAsync(Function() Connect()))
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
#End Region

#Region "Login Command"
    Private _loginCommand As ICommand
    Public ReadOnly Property LoginCommand As ICommand
        Get
            Return If(_loginCommand, New RelayCommandAsync(Function() Login(), AddressOf CanLogin))
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
#End Region

#Region "Logout Command"
    Private _logoutCommand As ICommand
    Public ReadOnly Property LogoutCommand As ICommand
        Get
            Return If(_logoutCommand, New RelayCommandAsync(Function() Logout(), AddressOf CanLogout))
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
#End Region

#Region "Typing Command"
    Private _typingCommand As ICommand
    Public ReadOnly Property TypingCommand As ICommand
        Get
            Return If(_typingCommand, New RelayCommandAsync(Function() Typing(), AddressOf CanUseTypingCommand))
        End Get
    End Property

    Private Async Function Typing() As Task(Of Boolean)
        Try
            Await chatService.TypingAsync(SelectedParticipant.Name)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Function CanUseTypingCommand() As Boolean
        Return SelectedParticipant IsNot Nothing AndAlso SelectedParticipant.IsLoggedIn
    End Function
#End Region

#Region "Send Text Message Command"
    Private _sendTextMessageCommand As ICommand
    Public ReadOnly Property SendTextMessageCommand As ICommand
        Get
            Return If(_sendTextMessageCommand, New RelayCommandAsync(Function() SendTextMessage(),
                                                                     AddressOf CanSendTextMessage))
        End Get
    End Property

    Private Async Function SendTextMessage() As Task(Of Boolean)
        Try
            Dim recepient = _selectedParticipant.Name
            Await chatService.SendUnicastMessageAsync(recepient, _textMessage)
            Return True
        Catch ex As Exception
            Return False
        Finally
            Dim msg As New ChatMessage With {.Author = UserName, .Message = _textMessage,
                .Time = DateTime.Now, .IsOriginNative = True}
            SelectedParticipant.Chatter.Add(msg)
            TextMessage = String.Empty
        End Try
    End Function

    Private Function CanSendTextMessage() As Boolean
        Return Not String.IsNullOrEmpty(TextMessage) AndAlso IsConnected AndAlso
            _selectedParticipant IsNot Nothing AndAlso _selectedParticipant.IsLoggedIn
    End Function
#End Region

#Region "Send Picture Message Command"
    Private _sendImageMessageCommand As ICommand
    Public ReadOnly Property SendImageMessageCommand As ICommand
        Get
            Return If(_sendImageMessageCommand, New RelayCommandAsync(Function() SendImageMessage(),
                                                                     AddressOf CanSendImageMessage))
        End Get
    End Property

    Private Async Function SendImageMessage() As Task(Of Boolean)
        Dim pic = dialogService.OpenFile("Select image file", "Images (*.jpg;*.png)|*.jpg;*.png")
        If String.IsNullOrEmpty(pic) Then Return False

        Dim img = Await Task.Run(Function() File.ReadAllBytes(pic))

        Try
            Dim recepient = _selectedParticipant.Name
            Await chatService.SendUnicastMessageAsync(recepient, img)
            Return True
        Catch ex As Exception
            Return False
        Finally
            Dim msg As New ChatMessage With {.Author = UserName, .Picture = pic, .Time = DateTime.Now, .IsOriginNative = True}
            SelectedParticipant.Chatter.Add(msg)
        End Try
    End Function

    Private Function CanSendImageMessage() As Boolean
        Return IsConnected AndAlso _selectedParticipant IsNot Nothing AndAlso _selectedParticipant.IsLoggedIn
    End Function
#End Region

#Region "Select Profile Picture Command"
    Private _selectProfilePicCommand As ICommand
    Public ReadOnly Property SelectPhotoCommand As ICommand
        Get
            Return If(_selectProfilePicCommand, New RelayCommand(AddressOf SelectProfilePic))
        End Get
    End Property

    Private Sub SelectProfilePic()
        Dim pic = dialogService.OpenFile("Select image file", "Images (*.jpg;*.png)|*.jpg;*.png")
        If Not String.IsNullOrEmpty(pic) Then
            Dim img = Image.FromFile(pic)
            If img.Width > MAX_IMAGE_WIDTH OrElse img.Height > MAX_IMAGE_HEIGHT Then
                dialogService.ShowNotification($"Image size should be {MAX_IMAGE_WIDTH} x {MAX_IMAGE_HEIGHT} or less.")
                Exit Sub
            End If
            ProfilePic = pic
        End If
    End Sub
#End Region

#Region "Open Image Command"
    Private _openImageCommand As ICommand
    Public ReadOnly Property OpenImageCommand As ICommand
        Get
            Return If(_openImageCommand, New RelayCommand(Of ChatMessage)(Sub(m) OpenImage(m)))
        End Get
    End Property

    Private Sub OpenImage(ByVal msg As ChatMessage)
        Dim img = msg.Picture
        If (String.IsNullOrEmpty(img) OrElse Not File.Exists(img)) Then Exit Sub
        Process.Start(img)
    End Sub
#End Region

#Region "Event handlers"
    Private Sub NewTextMessage(name As String, msg As String, mt As MessageType)
        If mt = MessageType.Unicast Then
            Dim cm As New ChatMessage With {.Author = name, .Message = msg, .Time = DateTime.Now}
            Dim sender = _participants.Where(Function(u) String.Equals(u.Name, name)).FirstOrDefault
            ctxTaskFactory.StartNew(Sub() sender.Chatter.Add(cm)).Wait()

            If Not (SelectedParticipant IsNot Nothing AndAlso sender.Name.Equals(SelectedParticipant.Name)) Then
                ctxTaskFactory.StartNew(Sub() sender.HasSentNewMessage = True).Wait()
            End If
        End If
    End Sub

    Private Sub NewImageMessage(name As String, pic As Byte(), mt As MessageType)
        If mt = MessageType.Unicast Then
            Dim imgsDirectory = Path.Combine(Environment.CurrentDirectory, "Image Messages")
            If Not Directory.Exists(imgsDirectory) Then Directory.CreateDirectory(imgsDirectory)

            Dim imgsCount = Directory.EnumerateFiles(imgsDirectory).Count() + 1
            Dim imgPath = Path.Combine(imgsDirectory, $"IMG_{imgsCount}.jpg")

            Dim converter As New ImageConverter
            Using img As Image = CType(converter.ConvertFrom(pic), Image)
                img.Save(imgPath)
            End Using

            Dim cm As New ChatMessage With {.Author = name, .Picture = imgPath, .Time = DateTime.Now}
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

    Private Sub ParticipantTyping(ByVal name As String)
        Dim person = Participants.Where(Function(p) String.Equals(p.Name, name)).FirstOrDefault()
        If person IsNot Nothing AndAlso Not person.IsTyping Then
            person.IsTyping = True
            Observable.Timer(TimeSpan.FromMilliseconds(1500)).Subscribe(Sub(t) person.IsTyping = False)
        End If
    End Sub
#End Region

    Private Function Avatar() As Byte()
        Dim pic As Byte() = Nothing
        If Not String.IsNullOrEmpty(_profilePic) Then pic = File.ReadAllBytes(_profilePic)
        Return pic
    End Function

    Public Sub New(chatSvc As IChatService, diagSvc As IDialogService)
        dialogService = diagSvc
        chatService = chatSvc
        AddHandler chatSvc.NewTextMessage, AddressOf NewTextMessage
        AddHandler chatSvc.NewImageMessage, AddressOf NewImageMessage
        AddHandler chatSvc.ParticipantLoggedIn, AddressOf ParticipantLogin
        AddHandler chatSvc.ParticipantLoggedOut, AddressOf ParticipantDisconnection
        AddHandler chatSvc.ParticipantDisconnected, AddressOf ParticipantDisconnection
        AddHandler chatSvc.ParticipantReconnected, AddressOf ParticipantReconnection
        AddHandler chatSvc.ParticipantTyping, AddressOf ParticipantTyping
        AddHandler chatSvc.ConnectionReconnecting, AddressOf Reconnecting
        AddHandler chatSvc.ConnectionReconnected, AddressOf Reconnected
        AddHandler chatSvc.ConnectionClosed, AddressOf Disconnected

        ctxTaskFactory = New TaskFactory(TaskScheduler.FromCurrentSynchronizationContext)
    End Sub
End Class