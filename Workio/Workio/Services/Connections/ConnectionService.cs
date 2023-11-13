using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Workio.Data;
using Workio.Models;
using Workio.Services.Interfaces;

namespace Workio.Services.Connections
{
    /// <summary>
    ///Serviço para intereações com a base de dados relativas às conexões
    /// </summary>
    public class ConnectionService : IConnectionService
    {
        /// <summary>
        ///Contexto da base de dados
        /// </summary>
        private readonly ApplicationDbContext _context;
        /// <summary>
        ///Manager de utilizadores
        /// </summary>
        private UserManager<User> _userManager;
        /// <summary>
        ///Contexto http
        /// </summary>
        private IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        ///Construtor de classe
        /// </summary>
        /// <param name="httpContextAccessor">Variavel para aceder ao IHttpContextAcessor</param>
        /// <param name="context">Contexto da base de dados</param>
        /// <param name="userManager">Manager de utilizadores</param>
        public ConnectionService(ApplicationDbContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        ///Metodo responsavel por obter todas as conexões
        /// </summary>
        /// <returns>Uma lista com as conexões</returns>
        public async Task<List<Connection>> GetConnectionsAsync()
        {
            return await _context.Connections.ToListAsync();
        }
        /// <summary>
        ///Metodo responsavel por adicionar uma conexão à base de dados
        /// </summary>
        ///<param name="connection">Conexão a ser adicionada à base de dados</param>
        /// <returns>True se teve sucesso, False se não tiver</returns>
        public async Task<bool> AddConnection(Connection connection)
        {
            if (connection == null) return false;

            var areFriends = await GetConnectionBetweenUsers(new Guid(connection.UserId), new Guid(connection.RequestedUserId));

            if (areFriends != null) return false;

            var isBlocked = await IsUserBlocked(connection.RequestedUserId, connection.UserId);

            if(isBlocked) return false;

            var success = 0;
            try
            {
                _context.Connections.Add(connection);
                success = await _context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }
            
            return success == 1;
        }
        /// <summary>
        ///Metodo responsavel por remover uma conexão da base de dados
        /// </summary>
        ///<param name="connection">Conexão a ser removida à base de dados</param>
        /// <returns>True se teve sucesso, False se não tiver</returns>
        public async Task<bool> RemoveConnection(Connection connection)
        {
            if (connection == null) return false;

            var existConnection = await GetConnectionBetweenUsers(new Guid(connection.UserId), new Guid(connection.RequestedUserId));

            if (existConnection == null) return false;

            var success = 0;
            try
            {
                _context.Connections.Remove(existConnection);
                success = await _context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }
            
            return success == 1;
        }

        /// <summary>
        ///Metodo responsavel por atualizar uma conexão na base de dados
        /// </summary>
        ///<param name="connection">Conexão a ser atualizada à base de dados</param>
        /// <returns>True se teve sucesso, False se não tiver</returns>
        public async Task<bool> UpdateConnection(Connection connection)
        {
            if (connection == null) return false;

            var existConnection = await GetConnectionBetweenUsers(new Guid(connection.UserId), new Guid(connection.RequestedUserId));

            if (existConnection == null) return false;

            var success = 0;

            try
            {
                existConnection.State = connection.State;
                existConnection.ConnectionDate = connection.ConnectionDate;
                
                _context.Connections.Update(existConnection);
                success = await _context.SaveChangesAsync();
            }
            catch
            {
                return false;
            }
            
            return success == 1;
        }

        /// <summary>
        /// Obtem todas as conexões aceites de um utilizador
        /// </summary>
        /// <param name="id">Id do utilizador a procurar as conexões</param>
        /// <returns>Conexões do utilizador com determinado id</returns>
        public async Task<List<Connection>> GetUserConnectionsAsync(Guid id)
        {
            if (id == Guid.Empty) return new List<Connection>();

            return await _context.Connections
                .Include(c => c.RequestedUser)
                .Include(c => c.RequestOwner)
                .Where(c => (c.UserId == id.ToString() || c.RequestedUserId == id.ToString()) && c.State == ConnectionState.Accepted).ToListAsync();
        }

        /// <summary>
        /// Obtem todos os users bloqueados por um utilizador
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>Lista de utilizadores bloqueados, null se o id nao for valido</returns>
        public async Task<List<User>> GetBlockedUsersAsync(string userId)
        {
            if (userId == Guid.Empty.ToString()) return null;

            return await _context.BlockedUsersModel.Where(b => b.SourceUserId == userId).Select(b => b.BlockedUser).ToListAsync();
        }

        /// <summary>
        /// Obtem as conexões pedentes do utilizador
        /// </summary>
        /// <returns>Conexões que o utilizador ainda não aceitou</returns>
        public async Task<List<Connection>> GetUserPendingConnectionsAsync()
        {
            User user = await GetCurrentUser();

            if(user == null) return new List<Connection>();

            return await _context.Connections
                .Include(c => c.RequestedUser)
                .Include(c => c.RequestOwner)
                .Where(c => (c.UserId == user.Id || c.RequestedUserId == user.Id) && c.State == ConnectionState.Pending).ToListAsync();
        }

        /// <summary>
        /// Verifica se existe uma conexão entre 2 utilizadores
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="otherUserId"></param>
        /// <returns></returns>
        public async Task<bool> AreFriends(Guid userId, Guid otherUserId)
        {
            if (userId == Guid.Empty || otherUserId == Guid.Empty) return false;

            var haveConnection = await _context.Connections.AnyAsync(c => (c.UserId == userId.ToString() || c.RequestedUserId == userId.ToString()) && (c.UserId == otherUserId.ToString() || c.RequestedUserId == otherUserId.ToString()) && c.State == ConnectionState.Accepted);

            return haveConnection;
        }

        /// <summary>
        /// Obtem a conexão entre 2 utilizadores
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <param name="otherUserId">Id do outro utilizador</param>
        /// <returns>Retorna uma conexão entre utilizadores. Null se não existir</returns>
        public async Task<Connection> GetConnectionBetweenUsers(Guid userId, Guid otherUserId)
        {
            if (userId == Guid.Empty || otherUserId == Guid.Empty) return null;

            return await _context.Connections.Where(c => (c.UserId == userId.ToString() || c.RequestedUserId == userId.ToString()) && (c.UserId == otherUserId.ToString() || c.RequestedUserId == otherUserId.ToString())).FirstOrDefaultAsync();
        }

        private Task<User> GetCurrentUser()
        {
            if (_httpContextAccessor.HttpContext == null) throw new Exception();
            return _userManager.FindByIdAsync(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        /// <summary>
        /// Verifica se um utilizador já bloqueou outro
        /// </summary>
        /// <param name="sourceId">Id do utilizador que bloqueou</param>
        /// <param name="blockedId">Id do utilizador bloqueado</param>
        /// <returns>true se foi encontrado esse registo, false caso contrário</returns>
        private async Task<bool> IsUserBlocked(string sourceId, string blockedId)
        {
            return await _context.BlockedUsersModel.Where(b => b.SourceUserId == sourceId && b.BlockedUserId == blockedId).AnyAsync();
        }

        private async Task<bool> ConnectionExist(Guid connectionId)
        {
            return await _context.Connections.AnyAsync(c => c.Id == connectionId);
        }
    }
}
