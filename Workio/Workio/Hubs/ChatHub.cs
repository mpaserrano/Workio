using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Workio.Models.Chat;
using Workio.Services.Chat;

namespace Workio.Hubs
{
    /// <summary>
    /// Hub para comunicação do chat em tempo-real
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("wow");
            if (Context.UserIdentifier != null)
            {
                // Retrieve the user's group memberships from database
                var chats = await _chatService.GetUserChats(Context.UserIdentifier);

                // Rejoin the user to their previous groups
                foreach (ChatRoom chat in chats)
                {
                    Console.WriteLine("chat added");
                    await Groups.AddToGroupAsync(Context.ConnectionId, chat.ChatRoomId.ToString());
                }
            }

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(string chatRoomId, ChatMessage message)
        {
            if (Context.UserIdentifier != null)
            {
                // Retrieve the user's group memberships from database
                var inChat = await _chatService.UserInChat(Guid.Parse(chatRoomId), Context.UserIdentifier);

                if (inChat)
                {
                    // Send message
                    await Clients.Group(chatRoomId).SendAsync("ReceiveMessage", message);
                } 
            }    
        }

        public async Task JoinConversation(string chatRoomId)
        {
            if (Context.UserIdentifier != null)
            {
                // Retrieve the user's group memberships from database
                var inChat = await _chatService.UserInChat(Guid.Parse(chatRoomId), Context.UserIdentifier);

                if (inChat)
                {
                    // Add the user to the team group
                    await Groups.AddToGroupAsync(Context.ConnectionId, chatRoomId);
                }
            }          
        }

        public async Task LeaveTeam(string chatRoomId)
        {
            // Remove the user from the team group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoomId);
        }
    }
}
