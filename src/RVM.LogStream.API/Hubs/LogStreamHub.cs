using Microsoft.AspNetCore.SignalR;

namespace RVM.LogStream.API.Hubs;

public class LogStreamHub : Hub
{
    public async Task JoinSourceGroup(string source)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"source:{source}");

    public async Task LeaveSourceGroup(string source)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"source:{source}");
}
