using Workio.Models;
using Workio.Models.Chat;

namespace Workio.Services.Chat
{
    /// <summary>
    /// Interface do serviço do chat.
    /// Este serviço faz ligação a uma base de dados de forma a persistir os dados.
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// Obtem uma sala de conversa pelo Id da sala
        /// </summary>
        /// <param name="chatRoomId">Id da sala a procurar</param>
        /// <returns>Objeto da sala encontrada. Null se não foi encontrada nenhuma sala</returns>
        public Task<ChatRoom> GetChatRoomById(Guid chatRoomId);

        /// <summary>
        /// Obtem uma sala de conversa pelo Id da equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <returns>Objeto da sala encontrada. Null se não foi encontrada nenhuma sala</returns>
        public Task<ChatRoom> GetTeamChatRoomById(Guid teamId);

        /// <summary>
        /// Obtem uma sala de conversa entre 2 utilizadores que não seja uma equipa
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <param name="otherUserId">Id do outro utilizador</param>
        /// <returns>Objeto da sala encontrada. Null se não foi encontrada nenhuma sala</returns>
        public Task<ChatRoom> GetChatRoomBetweenUsers(string userId, string otherUserId);

        /// <summary>
        /// Obtem todas as salas de conversas que estão na lista de ativas de um utilizador
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>Coleção de conversas</returns>
        public Task<ICollection<ChatRoom>> GetUserChats(string userId);

        /// <summary>
        /// Obtem todas as salas de conversas que estão na lista de ativas de um utilizador com mensagens
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>Coleção de conversas</returns>
        public Task<ICollection<ChatRoom>> GetUserActiveChats(string userId);
        /// <summary>
        /// Obtem todas as salas de conversas que estão na lista de ativas de um utilizador
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <param name="name">ChatRoom Name</param>
        /// <returns>Coleção de conversas</returns>
        public Task<ICollection<ChatRoom>> GetUserActiveChats(string userId, string name);

        /// <summary>
        /// Cria e guarda uma sala de conversas na base de dados
        /// </summary>
        /// <param name="chatRoom">Sala de conversas</param>
        /// <returns>true se foi salva com sucesso, false caso contrário</returns>
        public Task<bool> CreateChatRoom(ChatRoom chatRoom);
        /// <summary>
        /// Adiciona um utilizador a uma conversa
        /// </summary>
        /// <param name="chatRoomId">Id da conversa que vai receber o utilizador</param>
        /// <param name="user">Utilizador a adicionar a conversa</param>
        /// <returns>true caso tenha adicionado um utilizador com sucesso, false caso contrário</returns>
        public Task<bool> AddUserToChatRoom(Guid chatRoomId, User user);

        /// <summary>
        /// Remove um utilizador de uma conversa
        /// </summary>
        /// <param name="chatRoomId">Id da conversa de onde o utilizador vai ser removido</param>
        /// <param name="user">Utilizador a ser removido da conversa</param>
        /// <returns>true se o utilizador foi removido com sucesso, false caso contrário</returns>
        public Task<bool> RemoveUserFromChatRoom(Guid chatRoomId, User user);
        /// <summary>
        /// Adiciona um utilizador a uma conversa de equipa
        /// </summary>
        /// <param name="teamId">Id da equipa da conversa que vai receber o utilizador</param>
        /// <param name="user">Utilizador a adicionar a conversa</param>
        /// <returns>true caso tenha adicionado um utilizador com sucesso, false caso contrário</returns>
        public Task<bool> AddUserToTeamChatRoom(Guid teamId, User user);

        /// <summary>
        /// Remove um utilizador de uma conversa de equipa
        /// </summary>
        /// <param name="teamId">Id da equipa da conversa de onde o utilizador vai ser removido</param>
        /// <param name="user">Utilizador a ser removido da conversa</param>
        /// <returns>true se o utilizador foi removido com sucesso, false caso contrário</returns>
        public Task<bool> RemoveUserFromTeamChatRoom(Guid teamId, User user);

        /// <summary>
        /// Envia uma mensagem para uma conversa
        /// </summary>
        /// <param name="message">Objeto da mensagem a enviar</param>
        /// <param name="chatRoomId">Id da conversa que vai receber a mensagem</param>
        /// <returns>true se foi guardada com sucesso, falso caso contrário</returns>
        public Task<bool> SendMessageToChat(ChatMessage message, Guid chatRoomId);

        /// <summary>
        /// Envia uma mensagem para uma conversa
        /// </summary>
        /// <param name="message">Objeto da mensagem a enviar</param>
        /// <param name="chatRoomId">Id da conversa que vai receber a mensagem</param>
        /// <returns>Mensagem guardada</returns>
        public Task<ChatMessage> SaveMessage(ChatMessage message, Guid chatRoomId);

        /// <summary>
        /// Verifica se existe algum chat entre 2 utilizadores, que não seja um chat de equipa.
        /// </summary>
        /// <param name="userId">Id de um dos utilizadores</param>
        /// <param name="otherUserId">Id do outro utilizador</param>
        /// <returns>true se já existe, false se não existe</returns>
        public Task<bool> ExistChatBetweenTwoUsers(string userId, string otherUserId);

        /// <summary>
        /// Verifica se existe algum chat para 1 equipa.
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <returns>true se já existe, false se não existe</returns>
        public Task<bool> ExistChatForTeam(Guid teamId);

        /// <summary>
        /// Verifica se existe algum chat para 1 equipa e o user está lá.
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>true se já existe, false se não existe</returns>
        public Task<bool> UserInTeamChat(Guid teamId, string userId);

        /// <summary>
        /// Verifica se um user está num chat.
        /// </summary>
        /// <param name="chatRoomId">Id da conversa</param>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>true se já existe, false se não existe</returns>
        public Task<bool> UserInChat(Guid chatRoomId, string userId);

        /// <summary>
        /// Verifica se um user pode enviar mensagens para um chat.
        /// 1º Verifica se é uma equipa se for verifica se o user está nos membros.
        /// 2º Se não for uma equipa verifica se o user tem as dms abertas, senão tiver verifica se tem uma conexão.
        /// 3º Se nada existir não é possivel enviar mensagem
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <param name="chatroomId">Id da conversa</param>
        /// <returns></returns>
        public Task<bool> CanSendMessageToChatRoom(string userId, Guid chatroomId);

        /// <summary>
        /// Marca todas as mensagens de um chatroom como lidas
        /// </summary>
        /// <param name="chatroomId">Id da sala de conversas</param>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>true se foi lida, false se nao guardou</returns>
        public Task<bool> ReadAllChatroomMessages(Guid chatroomId, string userId);

        /// <summary>
        /// Marca uma mensagem como lida
        /// </summary>
        /// <param name="messageId">Id da mensagem</param>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>true se foi lida, false se nao falhou algo ou já estava lida</returns>
        public Task<bool> ReadMessage(Guid messageId, string userId);

        /// <summary>
        /// Obtem o número total de mensagens por ler
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>Número de mensagens por ler</returns>
        public Task<int> GetMessagesToReadCount(string userId);
    }
}
