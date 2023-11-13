using Microsoft.AspNetCore.SignalR;
using NuGet.Protocol.Plugins;
using Org.BouncyCastle.Asn1.Mozilla;
using Workio.Managers.Connections;
using Workio.Models;

namespace Workio.Hubs
{
    /// <summary>
    /// Hub para acomunicação server-client das notificações
    /// </summary>
    public class NotificationHub : Hub
    {
        /*public async Task SendNotification(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message);
        }*/
    }
}
