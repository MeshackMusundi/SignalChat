using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatClientCS.Models;
using ChatClientCS.Enums;

namespace ChatClientCS.Services
{
    public interface IChatService
    {
        event Action<User> ParticipantLoggedIn;
        event Action<string> ParticipantLoggedOut;
        event Action<string> ParticipantDisconnected;
        event Action<string> ParticipantReconnected;
        event Action ConnectionReconnecting;
        event Action ConnectionReconnected;
        event Action ConnectionClosed;
        event Action<string, string, MessageType> NewTextMessage;
        event Action<string, byte[], MessageType> NewImageMessage;
        event Action<string> ParticipantTyping;

        Task ConnectAsync();
        Task<List<User>> LoginAsync(string name, byte[] photo);
        Task LogoutAsync();

        Task SendBroadcastMessageAsync(string msg);
        Task SendBroadcastMessageAsync(byte[] img);
        Task SendUnicastMessageAsync(string recepient, string msg);
        Task SendUnicastMessageAsync(string recepient, byte[] img);
        Task TypingAsync(string recepient);
    }
}