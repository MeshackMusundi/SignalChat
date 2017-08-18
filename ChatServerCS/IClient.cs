namespace ChatServerCS
{
    public interface IClient
    {
        void ParticipantDisconnection(string name);
        void ParticipantReconnection(string name);
        void ParticipantLogin(User client);
        void ParticipantLogout(string name);
        void BroadcastMessage(string sender, string message);
        void UnicastMessage(string sender, string message);
    }
}
