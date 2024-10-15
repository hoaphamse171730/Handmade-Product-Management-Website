namespace HandmadeProductManagement.Services.SignalRHub
{
    public interface IChatClient
    {
        Task ReceiveMessage(string message);
    }
}
