using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using System.Drawing;
using ChatClientCS.Services;
using ChatClientCS.Enums;
using ChatClientCS.Models;
using ChatClientCS.Commands;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Reactive.Linq;

namespace ChatClientCS.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IChatService chatService;
        private IDialogService dialogService;
        private TaskFactory ctxTaskFactory;
        private const int MAX_IMAGE_WIDTH = 150;
        private const int MAX_IMAGE_HEIGHT = 150;

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        private string _profilePic;
        public string ProfilePic
        {
            get { return _profilePic; }
            set
            {
                _profilePic = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Participant> _participants = new ObservableCollection<Participant>();
        public ObservableCollection<Participant> Participants
        {
            get { return _participants; }
            set
            {
                _participants = value;
                OnPropertyChanged();
            }
        }

        private Participant _selectedParticipant;
        public Participant SelectedParticipant
        {
            get { return _selectedParticipant; }
            set
            {
                _selectedParticipant = value;
                if (SelectedParticipant.HasSentNewMessage) SelectedParticipant.HasSentNewMessage = false;
                OnPropertyChanged();
            }
        }

        private UserModes _userMode;
        public UserModes UserMode
        {
            get { return _userMode; }
            set
            {
                _userMode = value;
                OnPropertyChanged();
            }
        }

        private string _textMessage;
        public string TextMessage
        {
            get { return _textMessage; }
            set
            {
                _textMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                _isLoggedIn = value;
                OnPropertyChanged();
            }
        }

        #region Connect Command
        private ICommand _connectCommand;
        public ICommand ConnectCommand
        {
            get
            {
                return _connectCommand ?? (_connectCommand = new RelayCommandAsync(() => Connect()));
            }
        }

        private async Task<bool> Connect()
        {
            try
            {
                await chatService.ConnectAsync();
                IsConnected = true;
                return true;
            }
            catch (Exception) { return false; }
        }
        #endregion

        #region Login Command
        private ICommand _loginCommand;
        public ICommand LoginCommand
        {
            get
            {
                return _loginCommand ?? (_loginCommand =
                    new RelayCommandAsync(() => Login(), (o) => CanLogin()));
            }
        }

        private async Task<bool> Login()
        {
            try
            {
                List<User> users = new List<User>();
                users = await chatService.LoginAsync(_userName, Avatar());
                if (users != null)
                {
                    users.ForEach(u => Participants.Add(new Participant { Name = u.Name, Photo = u.Photo }));
                    UserMode = UserModes.Chat;
                    IsLoggedIn = true;
                    return true;
                }
                else
                {
                    dialogService.ShowNotification("Username is already in use");
                    return false;
                }

            }
            catch (Exception) { return false; }
        }

        private bool CanLogin()
        {
            return !string.IsNullOrEmpty(UserName) && UserName.Length >= 2 && IsConnected;
        }
        #endregion

        #region Logout Command
        private ICommand _logoutCommand;
        public ICommand LogoutCommand
        {
            get
            {
                return _logoutCommand ?? (_logoutCommand =
                    new RelayCommandAsync(() => Logout(), (o) => CanLogout()));
            }
        }

        private async Task<bool> Logout()
        {
            try
            {
                await chatService.LogoutAsync();
                UserMode = UserModes.Login;
                return true;
            }
            catch (Exception) { return false; }
        }

        private bool CanLogout()
        {
            return IsConnected && IsLoggedIn;
        }
        #endregion

        #region Typing Command
        private ICommand _typingCommand;
        public ICommand TypingCommand
        {
            get
            {
                return _typingCommand ?? (_typingCommand =
                    new RelayCommandAsync(() => Typing(), (o) => CanUseTypingCommand()));
            }
        }

        private async Task<bool> Typing()
        {
            try
            {
                await chatService.TypingAsync(SelectedParticipant.Name);
                return true;
            }
            catch (Exception) { return false; }
        }

        private bool CanUseTypingCommand()
        {
            return (SelectedParticipant != null && SelectedParticipant.IsLoggedIn);
        }
        #endregion

        #region Send Text Message Command
        private ICommand _sendTextMessageCommand;
        public ICommand SendTextMessageCommand
        {
            get
            {
                return _sendTextMessageCommand ?? (_sendTextMessageCommand =
                    new RelayCommandAsync(() => SendTextMessage(), (o) => CanSendTextMessage()));
            }
        }

        private async Task<bool> SendTextMessage()
        {
            try
            {
                var recepient = _selectedParticipant.Name;
                await chatService.SendUnicastMessageAsync(recepient, _textMessage);
                return true;
            }
            catch (Exception) { return false; }
            finally
            {
                ChatMessage msg = new ChatMessage
                {
                    Author = UserName,
                    Message = _textMessage,
                    Time = DateTime.Now,
                    IsOriginNative = true
                };
                SelectedParticipant.Chatter.Add(msg);
                TextMessage = string.Empty;
            }
        }

        private bool CanSendTextMessage()
        {
            return (!string.IsNullOrEmpty(TextMessage) && IsConnected &&
                _selectedParticipant != null && _selectedParticipant.IsLoggedIn);
        }
        #endregion

        #region Send Picture Message Command
        private ICommand _sendImageMessageCommand;
        public ICommand SendImageMessageCommand
        {
            get
            {
                return _sendImageMessageCommand ?? (_sendImageMessageCommand =
                    new RelayCommandAsync(() => SendImageMessage(), (o) => CanSendImageMessage()));
            }
        }

        private async Task<bool> SendImageMessage()
        {
            var pic = dialogService.OpenFile("Select image file", "Images (*.jpg;*.png)|*.jpg;*.png");
            if (string.IsNullOrEmpty(pic)) return false;

            var img = await Task.Run(() => File.ReadAllBytes(pic));

            try
            {
                var recepient = _selectedParticipant.Name;
                await chatService.SendUnicastMessageAsync(recepient, img);
                return true;
            }
            catch (Exception) { return false; }
            finally
            {
                ChatMessage msg = new ChatMessage { Author = UserName, Picture = pic, Time = DateTime.Now, IsOriginNative = true };
                SelectedParticipant.Chatter.Add(msg);
            }           
        }

        private bool CanSendImageMessage()
        {
            return (IsConnected && _selectedParticipant != null && _selectedParticipant.IsLoggedIn);
        }
        #endregion

        #region Select Profile Picture Command
        private ICommand _selectProfilePicCommand;
        public ICommand SelectProfilePicCommand
        {
            get
            {
                return _selectProfilePicCommand ?? (_selectProfilePicCommand =
                    new RelayCommand((o) => SelectProfilePic()));
            }
        }

        private void SelectProfilePic()
        {
            var pic = dialogService.OpenFile("Select image file", "Images (*.jpg;*.png)|*.jpg;*.png");
            if (!string.IsNullOrEmpty(pic))
            {
                var img = Image.FromFile(pic);
                if (img.Width > MAX_IMAGE_WIDTH || img.Height > MAX_IMAGE_HEIGHT)
                {
                    dialogService.ShowNotification($"Image size should be {MAX_IMAGE_WIDTH} x {MAX_IMAGE_HEIGHT} or less.");
                    return;
                }
                ProfilePic = pic;
            }
        }
        #endregion

        #region Open Image Command
        private ICommand _openImageCommand;
        public ICommand OpenImageCommand
        {
            get
            {
                return _openImageCommand ?? (_openImageCommand =
                    new RelayCommand<ChatMessage>((m) => OpenImage(m)));
            }
        }

        private void OpenImage(ChatMessage msg)
        {
            var img = msg.Picture;
            if (string.IsNullOrEmpty(img) || !File.Exists(img)) return;
            Process.Start(img);
        }
        #endregion

        #region Event Handlers
        private void NewTextMessage(string name, string msg, MessageType mt)
        {
            if (mt == MessageType.Unicast)
            {
                ChatMessage cm = new ChatMessage { Author = name, Message = msg, Time = DateTime.Now };
                var sender = _participants.Where((u) => string.Equals(u.Name, name)).FirstOrDefault();
                ctxTaskFactory.StartNew(() => sender.Chatter.Add(cm)).Wait();

                if (!(SelectedParticipant != null && sender.Name.Equals(SelectedParticipant.Name)))
                {
                    ctxTaskFactory.StartNew(() => sender.HasSentNewMessage = true).Wait();
                }
            }
        }

        private void NewImageMessage(string name, byte[] pic, MessageType mt)
        {
            if (mt == MessageType.Unicast)
            {
                var imgsDirectory = Path.Combine(Environment.CurrentDirectory, "Image Messages");
                if (!Directory.Exists(imgsDirectory)) Directory.CreateDirectory(imgsDirectory);

                var imgsCount = Directory.EnumerateFiles(imgsDirectory).Count() + 1;
                var imgPath = Path.Combine(imgsDirectory, $"IMG_{imgsCount}.jpg");

                ImageConverter converter = new ImageConverter();
                using (Image img = (Image)converter.ConvertFrom(pic))
                {
                    img.Save(imgPath);
                }

                ChatMessage cm = new ChatMessage { Author = name, Picture = imgPath, Time = DateTime.Now };
                var sender = _participants.Where(u => string.Equals(u.Name, name)).FirstOrDefault();
                ctxTaskFactory.StartNew(() => sender.Chatter.Add(cm)).Wait();

                if (!(SelectedParticipant != null && sender.Name.Equals(SelectedParticipant.Name)))
                {
                    ctxTaskFactory.StartNew(() => sender.HasSentNewMessage = true).Wait();
                }
            }
        }

        private void ParticipantLogin(User u)
        {
            var ptp = Participants.FirstOrDefault(p => string.Equals(p.Name, u.Name));
            if (_isLoggedIn && ptp == null)
            {
                ctxTaskFactory.StartNew(() => Participants.Add(new Participant
                {
                    Name = u.Name,
                    Photo = u.Photo
                })).Wait();
            }
        }

        private void ParticipantDisconnection(string name)
        {
            var person = Participants.Where((p) => string.Equals(p.Name, name)).FirstOrDefault();
            if (person != null) person.IsLoggedIn = false;
        }

        private void ParticipantReconnection(string name)
        {
            var person = Participants.Where((p) => string.Equals(p.Name, name)).FirstOrDefault();
            if (person != null) person.IsLoggedIn = true;
        }

        private void Reconnecting()
        {
            IsConnected = false;
            IsLoggedIn = false;
        }

        private async void Reconnected()
        {
            var pic = Avatar();
            if (!string.IsNullOrEmpty(_userName)) await chatService.LoginAsync(_userName, pic);
            IsConnected = true;
            IsLoggedIn = true;
        }

        private async void Disconnected()
        {
            var connectionTask = chatService.ConnectAsync();
            await connectionTask.ContinueWith(t => {
                if (!t.IsFaulted)
                {
                    IsConnected = true;
                    chatService.LoginAsync(_userName, Avatar()).Wait();
                    IsLoggedIn = true;
                }
            });
        }

        private void ParticipantTyping(string name)
        {
            var person = Participants.Where((p) => string.Equals(p.Name, name)).FirstOrDefault();
            if (person != null && !person.IsTyping)
            {
                person.IsTyping = true;
                Observable.Timer(TimeSpan.FromMilliseconds(1500)).Subscribe(t => person.IsTyping = false);
            }
        }
        #endregion

        private byte[] Avatar()
        {
            byte[] pic = null;
            if (!string.IsNullOrEmpty(_profilePic)) pic = File.ReadAllBytes(_profilePic);
            return pic;
        }

        public MainWindowViewModel(IChatService chatSvc, IDialogService diagSvc)
        {
            dialogService = diagSvc;
            chatService = chatSvc;

            chatSvc.NewTextMessage += NewTextMessage;
            chatSvc.NewImageMessage += NewImageMessage;
            chatSvc.ParticipantLoggedIn += ParticipantLogin;
            chatSvc.ParticipantLoggedOut += ParticipantDisconnection;
            chatSvc.ParticipantDisconnected += ParticipantDisconnection;
            chatSvc.ParticipantReconnected += ParticipantReconnection;
            chatSvc.ParticipantTyping += ParticipantTyping;
            chatSvc.ConnectionReconnecting += Reconnecting;
            chatSvc.ConnectionReconnected += Reconnected;
            chatSvc.ConnectionClosed += Disconnected;

            ctxTaskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }

    }
}