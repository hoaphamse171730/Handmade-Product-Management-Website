using HandmadeProductManagement.Services.SignalRHub;
using Microsoft.AspNetCore.SignalR;

public sealed class ChatHub : Hub<IChatClient>
{
    private static readonly Dictionary<string, string> _userNames = new();

    public override async Task OnConnectedAsync()
    {
        _userNames[Context.ConnectionId] = $"User{Context.ConnectionId.Substring(0, 5)}"; // Default username
        await Clients.All.ReceiveMessage($"{_userNames[Context.ConnectionId]} joined the chat");
    }
    
    public async Task SendMessage(string message)
    {
        var userName = _userNames[Context.ConnectionId];
        await Clients.All.ReceiveMessage($"{userName}: {message}");
    }

    public async Task SetUserName(string userName)
    {
        _userNames[Context.ConnectionId] = userName;
        await Clients.All.ReceiveMessage($"{userName} has set their name.");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_userNames.TryGetValue(Context.ConnectionId, out var userName))
        {
            await Clients.All.ReceiveMessage($"{userName} left the chat");
            _userNames.Remove(Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}