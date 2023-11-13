using Workio.Models;

namespace Workio.Services.Connections
{
    /// <summary>
    /// Interface para o serviço de conexões
    /// </summary>
    public interface IConnectionService
    {
        /// <summary>
        /// Obtem todas as conexões aceites de um utilizador
        /// </summary>
        /// <param name="id">Id do utilizador a procurar as conexões</param>
        /// <returns>Conexões do utilizador com determinado id</returns>
        Task<List<Connection>> GetUserConnectionsAsync(Guid id);
        /// <summary>
        /// Obtem as conexões pedentes do utilizador
        /// </summary>
        /// <returns>Conexões que o utilizador ainda não aceitou</returns>
        Task<List<Connection>> GetUserPendingConnectionsAsync();
        /// <summary>
        /// Obtem todos os users bloqueados por um utilizador
        /// </summary>
        /// <returns>Lista de utilizadores bloqueados, null se o id nao for valido</returns>
        Task<List<User>> GetBlockedUsersAsync(string userId);
        /// <summary>
        ///Metodo responsavel por obter todas as conexões
        /// </summary>
        /// <returns>Uma lista com as conexões</returns>
        Task<List<Connection>> GetConnectionsAsync();
        /// <summary>
        ///Metodo responsavel por adicionar uma conexão à base de dados
        /// </summary>
        ///<param name="connection">Conexão a ser adicionada à base de dados</param>
        /// <returns>True se teve sucesso, False se não tiver</returns>
        Task<bool> AddConnection(Connection connection);
        /// <summary>
        ///Metodo responsavel por remover uma conexão da base de dados
        /// </summary>
        ///<param name="connection">Conexão a ser removida à base de dados</param>
        /// <returns>True se teve sucesso, False se não tiver</returns>
        Task<bool> RemoveConnection(Connection connection);
        /// <summary>
        ///Metodo responsavel por atualizar uma conexão na base de dados
        /// </summary>
        ///<param name="connection">Conexão a ser atualizada à base de dados</param>
        /// <returns>True se teve sucesso, False se não tiver</returns>
        Task<bool> UpdateConnection(Connection connection);
        /// <summary>
        /// Verifica se existe uma conexão entre 2 utilizadores
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="otherUserId"></param>
        /// <returns></returns>
        Task<bool> AreFriends(Guid userId, Guid otherUserId);
        /// <summary>
        /// Obtem a conexão entre 2 utilizadores
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <param name="otherUserId">Id do outro utilizador</param>
        /// <returns>Retorna uma conexão entre utilizadores. Null se não existir</returns>
        public Task<Connection> GetConnectionBetweenUsers(Guid userId, Guid otherUserId);
    }
}
