using MailKit;
using MessagePack.Formatters;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using Workio.Data;
using Workio.Models;
using Workio.Models.Chat;

namespace Workio.Services.Chat
{
    /// <summary>
    /// Implementação do serviço do chat.
    /// Ligação a uma BD de forma a guardar os dados.
    /// Este serviço dá informação acerca de chat rooms, como mensagens entre 
    /// </summary>
    public class ChatService : IChatService
    {
        /// <summary>
        /// Contexto da base de dados
        /// </summary>
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Construtor do serviço
        /// </summary>
        /// <param name="context">Contexto da base de dados de onde será obtida a informação</param>
        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtem uma sala de conversa pelo Id da sala
        /// </summary>
        /// <param name="chatRoomId">Id da sala a procurar</param>
        /// <returns>Objeto da sala encontrada. Null se não foi encontrada nenhuma sala</returns>
        public async Task<ChatRoom> GetChatRoomById(Guid chatRoomId)
        {
            if (chatRoomId == Guid.Empty) return null;

            var chatRoom = await _context.ChatRooms
                .Include(c => c.Members)
                    .ThenInclude(c => c.User)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .Include(c => c.Team)
                .Where(c => c.ChatRoomId == chatRoomId).FirstOrDefaultAsync();

            return chatRoom;
        }

        /// <summary>
        /// Obtem uma sala de conversa pelo Id da equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <returns>Objeto da sala encontrada. Null se não foi encontrada nenhuma sala</returns>
        public async Task<ChatRoom> GetTeamChatRoomById(Guid teamId)
        {
            if (teamId == Guid.Empty) return null;

            var chatRoom = await _context.ChatRooms
                .Include(c => c.Members)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .Include(c => c.Team)
                .Where(c => c.TeamId == teamId).FirstOrDefaultAsync();

            return chatRoom;
        }

        /// <summary>
        /// Obtem uma sala de conversa entre 2 utilizadores que não seja uma equipa
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <param name="otherUserId">Id do outro utilizador</param>
        /// <returns>Objeto da sala encontrada. Null se não foi encontrada nenhuma sala</returns>
        public async Task<ChatRoom> GetChatRoomBetweenUsers(string userId, string otherUserId)
        {
            if (userId == Guid.Empty.ToString() || otherUserId == Guid.Empty.ToString()) return null;

            var chatRoom = await _context.ChatRooms
                .Include(c => c.Members)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .Where(c => c.TeamId == null && c.Members.Count() == 2 && c.Members.Any(m => m.UserId == userId) && c.Members.Any(m => m.UserId == otherUserId)).FirstOrDefaultAsync();

            return chatRoom;
        }

        /// <summary>
        /// Obtem todas as salas de conversas que estão na lista de ativas de um utilizador
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>Coleção de conversas</returns>
        public async Task<ICollection<ChatRoom>> GetUserChats(string userId)
        {
            if (userId == null || userId == Guid.Empty.ToString()) return new List<ChatRoom>();

            var chats = await _context.UserChatRooms
                .Include(x => x.ChatRoom)
                    .ThenInclude(c => c.Members)
                        .ThenInclude(c => c.User)
                .Include(x => x.ChatRoom)
                    .ThenInclude(c => c.Messages)
                        .ThenInclude(c => c.Sender)
                .Where(c => c.UserId == userId && c.Status == UserChatRoomStatus.Active)
                .Select(x => x.ChatRoom).ToListAsync();

            return chats;
        }

        /// <summary>
        /// Obtem todas as salas de conversas que estão na lista de ativas de um utilizador com mensagens
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>Coleção de conversas</returns>
        public async Task<ICollection<ChatRoom>> GetUserActiveChats(string userId)
        {
            if (userId == null || userId == Guid.Empty.ToString()) return new List<ChatRoom>();

            var chats = await _context.UserChatRooms
                .Include(x => x.ChatRoom)
                    .ThenInclude(c => c.Members)
                        .ThenInclude(c => c.User)
                .Include(x => x.ChatRoom)
                    .ThenInclude(c => c.Messages)
                        .ThenInclude(c => c.Sender)
                 .Include(x => x.ChatRoom)
                    .ThenInclude(c => c.Messages)
                        .ThenInclude(c => c.Readers)
                .Where(c => c.UserId == userId && c.Status == UserChatRoomStatus.Active && c.ChatRoom.Messages.Any())
                .Select(x => x.ChatRoom).ToListAsync();

            return chats;
        }

        /// <summary>
        /// Obtem todas as salas de conversas que estão na lista de ativas de um utilizador
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <param name="name">ChatRoom Name</param>
        /// <returns>Coleção de conversas</returns>
        public async Task<ICollection<ChatRoom>> GetUserActiveChats(string userId, string name)
        {
            if (userId == null || userId == Guid.Empty.ToString()) return new List<ChatRoom>();

            var chats = await _context.UserChatRooms
                .Include(x => x.ChatRoom)
                    .ThenInclude(c => c.Members)
                        .ThenInclude(c => c.User)
                .Include(x => x.ChatRoom)
                    .ThenInclude(c => c.Messages)
                        .ThenInclude(c => c.Sender)
                .Where(c => c.UserId == userId && c.Status == UserChatRoomStatus.Active && (c.ChatRoom.ChatRoomName != null && c.ChatRoom.ChatRoomName.Contains(name)))
                .Select(x => x.ChatRoom).ToListAsync();

            return chats;
        }

        /// <summary>
        /// Cria e guarda uma sala de conversas na base de dados
        /// </summary>
        /// <param name="chatRoom">Sala de conversas</param>
        /// <returns>true se foi salva com sucesso, false caso contrário</returns>
        public async Task<bool> CreateChatRoom(ChatRoom chatRoom)
        {
            if(chatRoom == null) throw new ArgumentNullException(nameof(chatRoom));

            if(chatRoom.TeamId == null)
            {
                if (chatRoom.Members.Count() > 2) return false;

                // Verifica se um chat que não é o de equipa já existe entre 2 utilizadores
                var user1 = chatRoom.Members.First();
                var user2 = chatRoom.Members.Last();
                var existChatAlready = await ExistChatBetweenTwoUsers(user1.UserId, user2.UserId);

                if(existChatAlready) return false;
            }

            var success = 0;
            try
            {
                _context.ChatRooms.Add(chatRoom);

                success = await _context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return success > 0;
        }
        /// <summary>
        /// Adiciona um utilizador a uma conversa
        /// </summary>
        /// <param name="chatRoomId">Id da conversa que vai receber o utilizador</param>
        /// <param name="user">Utilizador a adicionar a conversa</param>
        /// <returns></returns>
        public async Task<bool> AddUserToChatRoom(Guid chatRoomId, User user)
        {
            var chatRoom = await GetChatRoomById(chatRoomId);

            if(chatRoom == null) return false;

            if(chatRoom.TeamId == null) return false;

            var success = 0;
            try
            {
                var userAlreadyInChat = chatRoom.Members.Any(m => m.UserId == user.Id);

                if(userAlreadyInChat) return false;

                UserChatRoom userChat = new UserChatRoom()
                {
                    ChatRoomId = chatRoomId,
                    UserId = user.Id,
                    Status = UserChatRoomStatus.Active
                };

                user.ChatRooms.Add(userChat);

                _context.Users.Update(user);

                success = await _context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return success == 2;
        }

        /// <summary>
        /// Remove um utilizador de uma conversa
        /// </summary>
        /// <param name="chatRoomId">Id da conversa de onde o utilizador vai ser removido</param>
        /// <param name="user">Utilizador a ser removido da conversa</param>
        /// <returns></returns>
        public async Task<bool> RemoveUserFromChatRoom(Guid chatRoomId, User user)
        {
            var chatRoom = await GetChatRoomById(chatRoomId);

            if (chatRoom == null) return false;

            if (chatRoom.TeamId == null) return false;

            var success = 0;
            try
            {
                var userInChat = chatRoom.Members.Any(m => m.UserId == user.Id);

                if (userInChat) return false;

                var userChatRoom = GetUserChatRoom(chatRoom, user.Id);

                if(userChatRoom == null) return false;

                chatRoom.Members.Remove(userChatRoom);

                _context.ChatRooms.Update(chatRoom);

                success = await _context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return success == 2;
        }

        /// <summary>
        /// Adiciona um utilizador a uma conversa de equipa
        /// </summary>
        /// <param name="teamId">Id da equipa da conversa que vai receber o utilizador</param>
        /// <param name="user">Utilizador a adicionar a conversa</param>
        /// <returns>true caso tenha adicionado um utilizador com sucesso, false caso contrário</returns>
        public async Task<bool> AddUserToTeamChatRoom(Guid teamId, User user)
        {
            var chatRoom = await GetTeamChatRoomById(teamId);

            if (chatRoom == null) return false;

            if (chatRoom.TeamId == null) return false;

            if(chatRoom.Team == null || !chatRoom.Team.Members.Any(m => m.Id == user.Id)) return false;

            var success = 0;
            try
            {
                var userAlreadyInChat = chatRoom.Members.Any(m => m.UserId == user.Id);

                if (userAlreadyInChat) return false;

                UserChatRoom userChat = new UserChatRoom()
                {
                    ChatRoomId = chatRoom.ChatRoomId,
                    UserId = user.Id,
                    Status = UserChatRoomStatus.Active
                };

                user.ChatRooms.Add(userChat);

                _context.Users.Update(user);

                success = await _context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return success == 2;
        }

        /// <summary>
        /// Remove um utilizador de uma conversa de equipa
        /// </summary>
        /// <param name="teamId">Id da equipa da conversa de onde o utilizador vai ser removido</param>
        /// <param name="user">Utilizador a ser removido da conversa</param>
        /// <returns>true se o utilizador foi removido com sucesso, false caso contrário</returns>
        public async Task<bool> RemoveUserFromTeamChatRoom(Guid teamId, User user)
        {
            var chatRoom = await GetTeamChatRoomById(teamId);

            if (chatRoom == null) return false;

            if (chatRoom.TeamId == null) return false;

            var success = 0;
            try
            {
                var userInChat = chatRoom.Members.Any(m => m.UserId == user.Id);

                if (!userInChat) return false;

                var userChatRoom = GetUserChatRoom(chatRoom, user.Id);

                if (userChatRoom == null) return false;

                chatRoom.Members.Remove(userChatRoom);

                _context.ChatRooms.Update(chatRoom);

                success = await _context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return success == 2;
        }

        /// <summary>
        /// Envia uma mensagem para uma conversa
        /// </summary>
        /// <param name="message">Objeto da mensagem a enviar</param>
        /// <param name="chatRoomId">Id da conversa que vai receber a mensagem</param>
        /// <returns>true se foi guardada com sucesso, falso caso contrário</returns>
        public async Task<bool> SendMessageToChat(ChatMessage message, Guid chatRoomId)
        {
            var chatRoom = await GetChatRoomById(chatRoomId);

            if (chatRoom == null) return false;

            var userInChat = chatRoom.Members.Any(m => m.UserId == message.SenderId);

            if (!userInChat) return false;

            var success = 0;
            try
            {
                _context.ChatMessages.Add(message);

                chatRoom.Messages.Add(message);

                _context.ChatRooms.Update(chatRoom);

                success = await _context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }

            return success == 2;
        }

        /// <summary>
        /// Envia uma mensagem para uma conversa
        /// </summary>
        /// <param name="message">Objeto da mensagem a enviar</param>
        /// <param name="chatRoomId">Id da conversa que vai receber a mensagem</param>
        /// <returns>Mensagem guardada</returns>
        public async Task<ChatMessage> SaveMessage(ChatMessage message, Guid chatRoomId)
        {
            var chatRoom = await GetChatRoomById(chatRoomId);

            if (chatRoom == null) return null;

            var userInChat = chatRoom.Members.Any(m => m.UserId == message.SenderId);

            if (!userInChat) return null;

            try
            {
                var savedMessage = _context.ChatMessages.Add(message);

                chatRoom.Messages.Add(message);

                _context.ChatRooms.Update(chatRoom);

                var success = await _context.SaveChangesAsync();

                if(success == 2)
                {
                    return savedMessage.Entity;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        /// <summary>
        /// Verifica se existe algum chat entre 2 utilizadores, que não seja um chat de equipa.
        /// </summary>
        /// <param name="userId">Id de um dos utilizadores</param>
        /// <param name="otherUserId">Id do outro utilizador</param>
        /// <returns>true se já existe, false se não existe</returns>
        public async Task<bool> ExistChatBetweenTwoUsers(string userId, string otherUserId)
        {
            var chatRoom = await _context.ChatRooms.Include(c => c.Members).Where(c => c.TeamId == null && c.Members.Any(m => m.UserId == userId) && c.Members.Any(m => m.UserId == otherUserId)).FirstOrDefaultAsync();

            return chatRoom != null;
        }

        /// <summary>
        /// Verifica se existe algum chat para 1 equipa.
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <returns>true se já existe, false se não existe</returns>
        public async Task<bool> ExistChatForTeam(Guid teamId)
        {
            var chatRoom = await _context.ChatRooms.Where(c => c.TeamId == teamId).FirstOrDefaultAsync();

            return chatRoom != null;
        }

        /// <summary>
        /// Verifica se existe algum chat para 1 equipa e o user está lá.
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>true se já existe, false se não existe</returns>
        public async Task<bool> UserInTeamChat(Guid teamId, string userId)
        {
            var chatRoom = await _context.ChatRooms.Include(c => c.Members).Where(c => c.TeamId == teamId && c.Members.Any(m => m.UserId == userId)).FirstOrDefaultAsync();

            return chatRoom != null;
        }

        /// <summary>
        /// Verifica se um user está num chat.
        /// </summary>
        /// <param name="chatRoomId">Id da conversa</param>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>true se já existe, false se não existe</returns>
        public async Task<bool> UserInChat(Guid chatRoomId, string userId)
        {
            var chatRoom = await _context.ChatRooms.Include(c => c.Members).Where(c => c.Members.Any(m => m.UserId == userId)).FirstOrDefaultAsync();

            return chatRoom != null;
        }

        /// <summary>
        /// Verifica se um user pode enviar mensagens para um chat.
        /// 1º Verifica se é uma equipa se for verifica se o user está nos membros.
        /// 2º Se não for uma equipa verifica se o user tem as dms abertas, senão tiver verifica se tem uma conexão.
        /// 3º Se nada existir não é possivel enviar mensagem
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <param name="chatroomId">Id da conversa</param>
        /// <returns></returns>
        public async Task<bool> CanSendMessageToChatRoom(string userId, Guid chatroomId)
        {
            if(userId == Guid.Empty.ToString() && chatroomId == Guid.Empty) return false;

            var chat = await GetChatRoomById(chatroomId);

            if(chat == null) return false;

            //Se for uma equipa mas o user que está a tentar mandar msg não está nela então não pode mandar
            if(chat.TeamId != null)
            {
                if (!chat.Members.Any(m => m.UserId == userId)) return false;
                return true;
            }

            var otherMember = chat.Members.Where(m => m.UserId != userId).FirstOrDefault();

            if(otherMember == null) return false;

            // Se o outro user quer receber mensagem então prosseguir
            if (await WantToReceiveDM(otherMember.UserId)) return true;

            if (await AreFriends(userId, otherMember.UserId)) return true;

            return false;
        }

        /// <summary>
        /// Marca todas as mensagens de um chatroom como lidas
        /// </summary>
        /// <param name="chatroomId">Id da sala de conversas</param>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>true se foi lida, false se nao guardou</returns>
        public async Task<bool> ReadAllChatroomMessages(Guid chatroomId, string userId)
        {
            if (chatroomId == Guid.Empty || userId == Guid.Empty.ToString()) return false;

            var chatRoom = await _context.ChatRooms.Include(c => c.Messages).ThenInclude(c => c.Readers)
                .Include(m => m.Members)
                .Where(c => c.ChatRoomId == chatroomId).FirstOrDefaultAsync();

            if(chatRoom == null) return false;

            if (!chatRoom.Members.Any(u => u.UserId == userId)) return false;

            var messagesToRead = chatRoom.Messages.Where(m => !m.Readers.Any(r => r.ReaderId == userId)).ToList();

            try {
                foreach(var message in messagesToRead)
                {
                    MessageReader messageReader = new MessageReader()
                    {
                        MessageReaderId = new Guid(),
                        ReaderId = userId,

                    };
                    message.Readers.Add(messageReader);
                }
                if (messagesToRead.Any())
                {
                    _context.UpdateRange(messagesToRead);

                    var success = await _context.SaveChangesAsync();
                    return success > 0;
                }

                return true;
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Marca uma mensagem como lida
        /// </summary>
        /// <param name="messageId">Id da mensagem</param>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>true se foi lida, false se nao falhou algo ou já estava lida</returns>
        public async Task<bool> ReadMessage(Guid messageId, string userId)
        {
            if (messageId == Guid.Empty || userId == Guid.Empty.ToString()) return false;

            var message = await _context.ChatMessages.Include(c => c.Readers)
                .Where(m => m.MessageId == messageId).FirstOrDefaultAsync();

            if (message == null) return false;

            if (message.Readers.Any(m => m.ReaderId == userId)) return false;

            try
            {
                MessageReader messageReader = new MessageReader()
                {
                    MessageReaderId = new Guid(),
                    ReaderId = userId,

                };

                message.Readers.Add(messageReader);

                _context.Update(message);

                var success = await _context.SaveChangesAsync();
                return success > 0;
            }
            catch
            {
                return false;
            }

            return false;


        }

        /// <summary>
        /// Obtem o número total de mensagens por ler
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>Número de mensagens por ler</returns>
        public async Task<int> GetMessagesToReadCount(string userId)
        {
            if(userId == Guid.Empty.ToString()) { return 0; }

            var messagesToRead = await _context.ChatRooms
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Readers)
                .Include(m => m.Members)
                .Where(c => c.Members.Any(x => x.UserId == userId) && c.Messages.Any(m => !m.Readers.Any(r => r.ReaderId == userId)))
                .SelectMany(c => c.Messages.Where(m => !m.Readers.Any(r => r.ReaderId == userId)))
                .CountAsync();

            return messagesToRead;
        }

        private async Task<bool> AreFriends(string userId, string userId2)
        {
            if (userId == Guid.Empty.ToString() || userId2 == Guid.Empty.ToString()) return false;
            if(userId == userId2) return false;

            var friends = await _context.Connections.AnyAsync(c => 
                    (c.UserId == userId || c.RequestedUserId == userId) && 
                    (c.UserId == userId2 || c.RequestedUserId == userId2) && 
                     c.State == ConnectionState.Accepted
                );

            return friends;
        }

        private async Task<bool> WantToReceiveDM(string userId)
        {
            return await _context.UserPreferences.Where(p => p.UserId == userId && p.IsDMOpen == true).AnyAsync();
        }

        private UserChatRoom GetUserChatRoom(ChatRoom chatRoom, string userId)
        {
            return chatRoom.Members.Where(m => m.UserId == userId).FirstOrDefault();
        }
    }
}
