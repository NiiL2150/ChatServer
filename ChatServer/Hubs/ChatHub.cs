using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatServer.Hubs;

public class ChatHub : Hub
{
    static List<string> CurrentUsers = new List<string>();
    public async Task Send(string token, string message)
    {
        string id = Context.ConnectionId;
        var token2 = new JwtSecurityToken(token);
        var claim = token2.Claims.First(c => c.Type == ClaimsIdentity.DefaultNameClaimType).Value;

        if (message.StartsWith("/"))
        {
            string receiver = message.Split(" ")[0][1..];
            await Clients.Clients(receiver, id).SendAsync("ReceivePrivate", claim, $"to {receiver}: {message.Substring(receiver.Length+2)}", id);
        }
        else
        {
            await Clients.All.SendAsync("Receive", claim, message, id);
        }
    }

    public override async Task OnConnectedAsync()
    {
        CurrentUsers.Add(Context.ConnectionId);
        await Clients.Client(Context.ConnectionId).SendAsync("UserList", string.Join(";", CurrentUsers));
        await Clients.AllExcept(Context.ConnectionId).SendAsync("Notify", $"{Context.ConnectionId} is connected", DateTime.Now, Context.ConnectionId, true);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        CurrentUsers.Remove(Context.ConnectionId);
        await Clients.AllExcept(Context.ConnectionId).SendAsync("Notify", $"{Context.ConnectionId} is disconnected", DateTime.Now, Context.ConnectionId, false);
    }
}
