using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NToastNotify;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Security.Claims;
using System.Web;
using System.Xml.Linq;
using Workio.Hubs;
using Workio.Migrations;
using Workio.Models;
using Workio.Models.Chat;
using Workio.Models.Chat.ViewModels;
using Workio.Services;
using Workio.Services.Chat;
using Workio.Services.Connections;
using Workio.Services.Interfaces;
using Workio.Services.Teams;
using Workio.ViewModels;
using static System.Net.Mime.MediaTypeNames;

namespace Workio.Controllers.Chat
{
    /// <summary>
    /// Controlador para o chat. O utilizador tem de estar logado para poder utilizador as suas funções
    /// </summary>
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly ITeamsService _teamsService;
        private readonly IToastNotification _toastNotification;
        private readonly CommonLocalizationService _stringLocalizer;
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly IConnectionService _connectionService;

        public ChatController(IChatService chatService,
                IUserService userService,
                ITeamsService teamsService,
                IToastNotification toastNotification,
                CommonLocalizationService commonLocalizationService,
                IHubContext<ChatHub> chatHubContext,
                IConnectionService connectionService)
        {
            _chatService = chatService;
            _userService = userService;
            _teamsService = teamsService;
            _toastNotification = toastNotification;
            _stringLocalizer = commonLocalizationService;
            _chatHubContext = chatHubContext;
            _connectionService = connectionService;
        }

        public IActionResult Index(Guid? roomId)
        {
            if(roomId.HasValue)
                ViewBag.InitialConversationId = roomId;
            return View();
        }

        /// <summary>
        /// Carrega os dados básicos iniciais como a foto de perfil
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> InitialLoad()
        {
            var userId = GetCurrentUserId();
            var user = await _userService.GetUser(Guid.Parse(userId));

            var profilePicture = user.ProfilePicture ?? "default.png";

            var data = new
            {
                profilePicture = profilePicture,
                userId = userId,
            };

            return Json(data);
        }

        public async Task<IActionResult> LeaveChat(Guid chatRoomId)
        {
            if(chatRoomId == Guid.Empty)
            {
                return NotFound();
            }

            var chat = await _chatService.GetChatRoomById(chatRoomId);

            if(chat == null || chat.Team == null)
            {
                _toastNotification.AddErrorToastMessage(_stringLocalizer.Get("Error getting chatroom"));
                return RedirectToAction("Index");
            }

            var userId = GetCurrentUserId();

            if(chat.Team.OwnerId.ToString() == userId)
            {
                _toastNotification.AddErrorToastMessage(_stringLocalizer.Get("Can't leave chats where you are the owner"));
                return RedirectToAction("Index", new { roomId = chatRoomId});
            }

            return RedirectToAction("Leave", "Teams", new { id = chat.TeamId, returnUrl = "/Chat" });
        }
        /// <summary>
        /// Obtem as conexões do user e equipas em formato de chats
        /// </summary>
        /// <returns>ChatViewModel com as conexões e equipas</returns>
        private async Task<List<ChatViewModel>> GetNewPossibleChats()
        {
            var userId = GetCurrentUserId();

            if (userId == null || userId == Guid.Empty.ToString()) return new List<ChatViewModel>();

            var connections = await _connectionService.GetUserConnectionsAsync(Guid.Parse(userId));

            var myTeams = await _teamsService.GetAllUserTeamsByUserId(Guid.Parse(userId));

            List<ChatViewModel> chats = new List<ChatViewModel>();

            // Iterate through user connections
            foreach (var connection in connections)
            {
                if(connection.UserId == userId)
                {
                    chats.Add(new ChatViewModel
                    {
                        Id = connection.Id,
                        Name = connection.RequestedUser.Name,
                        ProfilePicture = connection.RequestedUser.ProfilePicture,
                        Email = connection.RequestedUser.Email,
                        Type = ChatViewModelType.User
                    });
                }
                else
                {
                    chats.Add(new ChatViewModel
                    {
                        Id = connection.Id,
                        Name = connection.RequestOwner.Name,
                        ProfilePicture = connection.RequestOwner.ProfilePicture,
                        Email = connection.RequestOwner.Email,
                        Type = ChatViewModelType.User
                    });
                }
            }

            // Iterate through user teams
            foreach (var team in myTeams)
            {
                chats.Add(new ChatViewModel
                {
                    Id = team.TeamId,
                    Name = team.TeamName,
                    Type = ChatViewModelType.Team
                });
            }

            return chats;
        }

        /// <summary>
        /// Retorna um componente modal para criar/abrir um chat.
        /// </summary>
        /// <returns>Componente modal.</returns>
        public async Task<IActionResult> NewChatModal()
        {
            var chats = new List<ChatViewModel>();
            chats = await GetNewPossibleChats();
            return ViewComponent("NewChatModal", new { chats = chats.Take(5).ToList() });
        }

        /// <summary>
        /// Procura por uma conexão ou equipa.
        /// </summary>
        /// <param name="query">Dados para a pesquisa.</param>
        /// <returns>ViewComponent com o resultado da procura.</returns>
        public async Task<IActionResult> NewChatSearch(string query)
        {
            // Query your database or any other data source based on the search query
            var chats = new List<ChatViewModel>();
            chats = await GetNewPossibleChats();

            if (query != null)
            {
                chats = chats.Where(c => c.Name == query || c.Email == query).ToList();
            }

            if (chats == null) chats = new List<ChatViewModel>();
            return ViewComponent("NewChatModal", new { chats = chats });
        }

        /// <summary>
        /// Marca todas as mensagens como lidas pelo utilizador atual
        /// </summary>
        /// <param name="chatroomId">Id da conversa</param>
        /// <returns>200 se tudo correu bem, 400 se os ids sao invalidos, 500 se algo ocorreu</returns>
        public async Task<IActionResult> ReadMessages(Guid chatroomId)
        {
            var userId = GetCurrentUserId();

            if (chatroomId == Guid.Empty || userId == Guid.Empty.ToString()) return BadRequest();

            var success = await _chatService.ReadAllChatroomMessages(chatroomId, userId);

            if (success)
            {
                return Ok();
            }

            return Problem();
        }

        /// <summary>
        /// Marca uma mensagem como lida pelo utilizador atual
        /// </summary>
        /// <param name="messageId">Id da mensagem</param>
        /// <returns>200 se tudo correu bem, 400 se os ids sao invalidos, 500 se algo ocorreu</returns>
        public async Task<IActionResult> ReadMessage(Guid messageId)
        {
            var userId = GetCurrentUserId();

            if (messageId == Guid.Empty || userId == Guid.Empty.ToString()) return BadRequest();

            var success = await _chatService.ReadMessage(messageId, userId);

            if (success)
            {
                return Ok();
            }

            return Problem();
        }

        /// <summary>
        /// Envia uma mensagem para uma sala de conversa
        /// </summary>
        /// <param name="messageRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageViewModel messageRequest)
        {
            if(messageRequest.Text.Length > 250)
            {
                return BadRequest();
            }

            var chatRoomId = messageRequest.ChatRoomId;
            var text = messageRequest.Text;

            if (chatRoomId == Guid.Empty) return Json(new { });

            var userId = GetCurrentUserId();

            var canSendMessage = await _chatService.CanSendMessageToChatRoom(userId.ToString(), chatRoomId);

            if (!canSendMessage) {
                //_toastNotification.AddErrorToastMessage(_stringLocalizer.Get("Error Sending Message. This user has closed DMs"));
                return Unauthorized();
            }

            ChatMessage message = new ChatMessage()
            {
                MessageId = Guid.NewGuid(),
                SenderId = userId,
                Text = HttpUtility.HtmlEncode(text)
            };

            var savedMessage = await _chatService.SaveMessage(message, chatRoomId);
            if(savedMessage != null)
            {
                var data = new
                {
                    id = savedMessage.MessageId,
                    messageTime = savedMessage.SendAt.ToString("yyyy-MM-dd HH:mm"),
                    text = savedMessage.Text,
                    isMine = true,
                    profilePicture = savedMessage.Sender.ProfilePicture ?? "default.png",
                    sender_id = savedMessage.SenderId,
                    name = savedMessage.Sender.Name,
                    chatRoomId = chatRoomId,
                };
                await _chatHubContext.Clients.GroupExcept(chatRoomId.ToString(), userId).SendAsync("ReceiveMessage", data);
                return Json(data);
            }
            else
            {
                //_toastNotification.AddErrorToastMessage(_stringLocalizer.Get("Error Sending Message"));
                return BadRequest();
            }
        }

        /// <summary>
        /// Obtem as informações de um url
        /// </summary>
        /// <param name="url">URL de um website</param>
        /// <returns>Metatags</returns>
        public async Task<IActionResult> GetMetaTags(string url)
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var title = document.DocumentNode.SelectSingleNode("//meta[@property='og:title']")?.Attributes["content"]?.Value
                        ?? document.DocumentNode.SelectSingleNode("//meta[@name='twitter:title']")?.Attributes["content"]?.Value;

            var description = document.DocumentNode.SelectSingleNode("//meta[@property='og:description']")?.Attributes["content"]?.Value
                        ?? document.DocumentNode.SelectSingleNode("//meta[@name='twitter:description']")?.Attributes["content"]?.Value;

            var image = document.DocumentNode.SelectSingleNode("//meta[@property='og:image']")?.Attributes["content"]?.Value
                        ?? document.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']")?.Attributes["content"]?.Value;

            var urlLink = document.DocumentNode.SelectSingleNode("//meta[@property='og:url']")?.Attributes["content"]?.Value
                        ?? document.DocumentNode.SelectSingleNode("//meta[@name='twitter:url']")?.Attributes["content"]?.Value;


            var data = new
            {
                title = title,
                description = description,
                image = image,
                url = urlLink
            };

            return Json(data);
        }

        /// <summary>
        /// Obtem as informações de uma conversa, como as mensagens
        /// </summary>
        /// <param name="chatRoomId">Id da conversa</param>
        /// <returns>JSON com as mensagens da conversa</returns>
        [HttpGet("Chat/GetConversation/{chatRoomId}")]
        public async Task<IActionResult> GetConversation(Guid chatRoomId)
        {
            if (chatRoomId == Guid.Empty) return Json(new { });  

            var userId = GetCurrentUserId();

            var chat = await _chatService.GetChatRoomById(chatRoomId);

            if (chat == null) return NotFound();

            if(!chat.Members.Any(m => m.UserId == userId)) return BadRequest();

            var filteredMessages = chat.Messages.OrderBy(x => x.SendAt).Select(x => {
                return new
                {
                    id = x.MessageId,
                    name = x.Sender.Name,
                    sender_id = x.SenderId,
                    profilePicture = x.Sender.ProfilePicture ?? "default.png",
                    messageTime = x.SendAt.ToString("yyyy-MM-dd HH:mm"),
                    text = x.Text,
                    isMine = x.SenderId == userId ? true : false
                };
            });

            var chatInfo = new
            {
                name = GetChatRoomName(chat, userId),
                image = GetChatRoomImage(chat, userId),
                targetId = GetTargetId(chat, userId),
                isTeam = chat.TeamId != null ? true : false
            };


            var data = new
            {
                messages = filteredMessages,
                info = chatInfo
            };

            return Json(data);
        }

        /// <summary>
        /// Obtem todos os chats ativos do utilizador
        /// </summary>
        /// <returns>JSON com id, nome do chat, última mensagem enviada e a hora da última mensagem</returns>
        public async Task<IActionResult> GetActiveChats(string? chatroomName)
        {
            var userId = GetCurrentUserId();

            var chats = await _chatService.GetUserActiveChats(userId);
               
            var filteredChats = chats.Select(x => {
                var lastMessage = GetLastMessage(x, userId);
                return new
                {
                    id = x.ChatRoomId,
                    name = GetChatRoomName(x, userId),
                    image = GetChatRoomImage(x, userId),
                    lastMessageText = lastMessage.Item1,
                    lastMessageTime = lastMessage.Item2?.ToString("yyyy-MM-dd HH:mm"),
                    isRead = lastMessage.Item3
                };
            });

            if (!string.IsNullOrEmpty(chatroomName))
            {
                Func<string, string, bool> containsIgnoringCaseAndAccentuation = (s1, s2) =>
                CultureInfo.InvariantCulture.CompareInfo.IndexOf(s1, s2, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

                filteredChats = filteredChats.Where(c => containsIgnoringCaseAndAccentuation(c.name, chatroomName));
            }

            return Json(filteredChats.OrderByDescending(f => f.lastMessageTime));
        }

        /// <summary>
        /// Cria e adiciona os users a uma conversa
        /// </summary>
        /// <param name="otherUserId"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task<IActionResult> CreateUserChatRoom(string otherUserId, string returnUrl)
        {
            if (otherUserId == Guid.Empty.ToString()) {
                _toastNotification.AddErrorToastMessage(_stringLocalizer.Get("Invalid Operation"));
                return LocalRedirect(returnUrl);
            }

            var userId = GetCurrentUserId();

            var otherUser = await _userService.GetUser(Guid.Parse(otherUserId));
            var user = await _userService.GetUser(Guid.Parse(userId));

            if (otherUser == null || userId == Guid.Empty.ToString() || user == null || otherUserId == userId)
            {
                _toastNotification.AddErrorToastMessage(_stringLocalizer.Get("Invalid Operation"));
                return LocalRedirect(returnUrl);
            }
            
            if(otherUser.BlockedUsers.Any(u => u.BlockedUserId == userId))
            {
                _toastNotification.AddErrorToastMessage(_stringLocalizer.Get("You're Blocked"));
                return LocalRedirect(returnUrl);
            }

            ChatRoom newChat = new ChatRoom()
            {
                ChatRoomId = Guid.NewGuid()
            };

            UserChatRoom userChat = new UserChatRoom()
            {
                ChatRoom = newChat,
                User = user,
                Status = UserChatRoomStatus.Active
            };

            UserChatRoom otherChat = new UserChatRoom()
            {
                ChatRoom = newChat,
                User = otherUser,
                Status = UserChatRoomStatus.Active
            };

            newChat.Members.Add(userChat);
            newChat.Members.Add(otherChat);

            var success = await _chatService.CreateChatRoom(newChat);

            if(!success)
            {
                _toastNotification.AddErrorToastMessage(_stringLocalizer.Get("Couldn't create a chat at the moment"));
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", new { roomId = newChat.ChatRoomId });
        }

        public async Task<IActionResult> CreateTeamChatRoom(Guid teamId, string returnUrl)
        {
            if (teamId == Guid.Empty)
            {
                _toastNotification.AddErrorToastMessage(_stringLocalizer.Get("Invalid Operation"));
                return LocalRedirect(returnUrl);
            }

            var team = await _teamsService.GetTeamById(teamId);

            if (team == null || team.OwnerId == null)
            {
                _toastNotification.AddErrorToastMessage(_stringLocalizer.Get("Invalid Operation"));
                return LocalRedirect(returnUrl);
            }

            var chat = await _chatService.GetTeamChatRoomById(teamId);

            if(chat != null)
            {
                return RedirectToAction("Index", new { roomId = chat.ChatRoomId });
            }

            ChatRoom newChat = new ChatRoom()
            {
                ChatRoomId = Guid.NewGuid(),
                TeamId = teamId,
                ChatRoomName = team.TeamName
            };

            UserChatRoom ownerChat = new UserChatRoom()
            {
                ChatRoom = newChat,
                UserId = team.OwnerId.Value.ToString(),
                Status = UserChatRoomStatus.Active
            };

            newChat.Members.Add(ownerChat);

            foreach (User member in team.Members){
                UserChatRoom userChat = new UserChatRoom()
                {
                    ChatRoom = newChat,
                    User = member,
                    Status = UserChatRoomStatus.Active
                };

                newChat.Members.Add(userChat);
            }

            var success = await _chatService.CreateChatRoom(newChat);

            if (!success)
            {
                _toastNotification.AddErrorToastMessage(_stringLocalizer.Get("Couldn't create a chat at the moment"));
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", new { roomId = newChat.ChatRoomId });
        }

        public async Task<IActionResult> AddToTeamChatRoom(Guid teamId, Guid userId, string returnUrl)
        {
            if (teamId == Guid.Empty || userId == Guid.Empty)
            {
                _toastNotification.AddErrorToastMessage(_stringLocalizer.Get("Invalid Operation"));
                return LocalRedirect(returnUrl);
            }

            var team = await _teamsService.GetTeamById(teamId);

            if (team == null || !(team.OwnerId == userId || team.Members.Any(m => m.Id == userId.ToString())))
            {
                _toastNotification.AddErrorToastMessage(_stringLocalizer.Get("Invalid Operation"));
                return LocalRedirect(returnUrl);
            }

            ChatRoom newChat = new ChatRoom()
            {
                ChatRoomId = Guid.NewGuid(),
                TeamId = teamId,
                ChatRoomName = team.TeamName
            };

            UserChatRoom userChat = new UserChatRoom()
            {
                ChatRoom = newChat,
                UserId = userId.ToString(),
                Status = UserChatRoomStatus.Active
            };

            newChat.Members.Add(userChat);


            var success = await _chatService.CreateChatRoom(newChat);

            if (!success)
            {
                _toastNotification.AddErrorToastMessage(_stringLocalizer.Get("Couldn't create a chat at the moment"));
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Retorna um componente modal de diálog de decisão.
        /// </summary>
        /// <param name="teamId">Id de equipa</param>
        /// <returns>Componente modal.</returns>
        public IActionResult OpenModal(Guid teamId)
        {
            return ViewComponent("ShareModal", new { users = new List<User>() });
        }

        /// <summary>
        /// Obtem o número total de notificações por ler
        /// </summary>
        /// <returns>Número de notificações por ler</returns>
        public async Task<int> GetChatCount()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty.ToString())
            {
                return 0;
            }
            else
            {
                var counter = await _chatService.GetMessagesToReadCount(userId);
                return counter;
            }

            return 0;
        }

        /// <summary>
        /// Obtem o nome de um chatroom. Leva em consideração vários factores como se o chat tem nome definido no objeto por padrão,
        /// senão verifica se é uma equipa e dá o seu nome, por último utiliza o membro contrário a si para ser o nome do chat
        /// </summary>
        /// <param name="chatRoom">Chat room</param>
        /// <param name="currentUserId">Id do utilizador atual</param>
        /// <returns>Nome do chatroom</returns>
        private string GetChatRoomName(ChatRoom chatRoom, string currentUserId)
        {
            if (!string.IsNullOrEmpty(chatRoom.ChatRoomName)) return chatRoom.ChatRoomName;

            if (chatRoom.TeamId != null && chatRoom.Team != null) return chatRoom.Team.TeamName;

            var user = chatRoom.Members.Where(m => m.UserId != currentUserId).Select(x => x.User).FirstOrDefault();

            if (user == null) return "Default Name";

            return user.Name;
        }

        /// <summary>
        /// Obtem a imagem de um chatroom. Leva em consideração vários factores como se o chat é uma equipa e dá uma imagem padrão, 
        /// por último utiliza o membro contrário a usa a sua profile picture para imagem do chat
        /// </summary>
        /// <param name="chatRoom">Chat room</param>
        /// <param name="currentUserId">Id do utilizador atual</param>
        /// <returns>Nome do chatroom</returns>
        private string GetChatRoomImage(ChatRoom chatRoom, string currentUserId)
        {
            //if (!string.IsNullOrEmpty(chatRoom.ChatRoomName)) return chatRoom.ChatRoomName;

            if (chatRoom.TeamId != null) return "/pfp/default.png";

            var user = chatRoom.Members.Where(m => m.UserId != currentUserId).Select(x => x.User).FirstOrDefault();

            if (user == null || user.ProfilePicture == null) return "/pfp/default.png";

            return "/pfp/" + user.ProfilePicture;
        }

        /// <summary>
        /// Obtem o id do target da conversa - se for user-to-user dá o id do user contrário, se for team chat dá o id da equipa
        /// </summary>
        /// <param name="chatRoom">Chat room</param>
        /// <param name="currentUserId">Id do utilizador atual</param>
        /// <returns>Nome do chatroom</returns>
        private Guid GetTargetId(ChatRoom chatRoom, string currentUserId)
        {
            if (chatRoom.TeamId != null) return chatRoom.TeamId.Value;

            var user = chatRoom.Members.Where(m => m.UserId != currentUserId).Select(x => x.User).FirstOrDefault();

            if (user == null) return Guid.Empty;

            return Guid.Parse(user.Id);
        }

        /// <summary>
        /// Obtem a última mensagem enviada para o chat
        /// </summary>
        /// <param name="chatRoom">Chatroom</param>
        /// /// <param name="userId">Id do utilizador atual</param>
        /// <returns>Retorna um objeto anónimo com o texto da mensagem e a hora</returns>
        private (string, DateTime?, bool) GetLastMessage(ChatRoom chatRoom, string userId)
        {
            var message = chatRoom.Messages.OrderByDescending(m => m.SendAt).FirstOrDefault();

            if (message == null) return ("", null, false);

            var textMessage = message.Text;

            if(chatRoom.TeamId != null)
            {
                textMessage = message.SenderId == userId ? message.Text : message.Sender.Name + ": " + textMessage;
            }

            var isReaded = message.Readers.Any(r => r.ReaderId == userId);

            return (textMessage, message.SendAt, isReaded);
        }

        /// <summary>
        /// Obtem o id do utilizador logado
        /// </summary>
        /// <returns>Id do utilizador</returns>
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private async Task<string> GetCurrentProfilePicture(User user)
        {
            if (user == null || user.ProfilePicture == null) return "/pfp/default.png";

            var profilePicture = "/pfp/" + user.ProfilePicture;

            return "/pfp/" + user.ProfilePicture;
        }
    }
}
