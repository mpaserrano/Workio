using Microsoft.EntityFrameworkCore;
using Workio.Data;
using Workio.Models.Events;
using Workio.Models;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Workio.Services.RequestEntityStatusServices
{
    public class RequestEntityStatusService : IRequestEntityStatusService
    {
        /// <summary>
        /// Contexto da Base de Dados
        /// </summary>
        private ApplicationDbContext _context;

        public RequestEntityStatusService(ApplicationDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Cria um pedido e guarda na base de dados
        /// </summary>
        /// <param name="request">Objeto com os dados do pedido</param>
        /// <returns>True se foi guardada com sucesso, false caso contrário</returns>
        public async Task<bool> CreateRequest(RequestEntityStatus request)
        {

            var success = 0;

            try
            {
                _context.Add(request);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // use an optimistic concurrency strategy from:
                // https://learn.microsoft.com/en-us/ef/core/saving/concurrency#resolving-concurrency-conflicts
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }
        /// <summary>
        /// Permite validar se um utilizador pode atualizar um pedido
        /// </summary>
        /// <param name="request">Objeto com os dados do pedido</param>
        /// <returns>True se foi atualizado com sucesso, false caso contrário</returns>

        public async Task<bool> UpdateRequest(RequestEntityStatus request)
        {
            var success = 0;

            try
            {
                _context.Update(request);
                success = await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // use an optimistic concurrency strategy from:
                // https://learn.microsoft.com/en-us/ef/core/saving/concurrency#resolving-concurrency-conflicts
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error_ " + ex.Message);
                success = 0;
            }

            return success > 0;
        }
        /// <summary>
        /// Permite validar se um utilizador já realizou um pedido
        /// </summary>
        /// <param name="id">Id do utilizador atual</param>
        /// <returns>True se ja realizou pedido, false caso contrário</returns>
        public bool AlreadyRequested(Guid id)
        {
            var alreadyRequested = _context.RequestEntityStatus.Any(r => r.UserId == id);
            if (alreadyRequested)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Permite obter um pedido pelo seu id
        /// </summary>
        /// <param name="id">Id do pedido</param>
        /// <returns>Pedido com o id correspondente</returns>
        public async Task<RequestEntityStatus> GetRequestById(Guid id)
        {
            return await  _context.RequestEntityStatus.Where(r => r.Id == id).FirstOrDefaultAsync();
        }
        /// <summary>
        /// Permite o estado de um pedido por id do utilizador
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Pedido com o id correspondente</returns>

        public async Task<RequestState> GetRequestStateByUserId (Guid id)
        {
            return await _context.RequestEntityStatus.Where(r => r.UserId == id).Select(r => r.RequestState).FirstOrDefaultAsync();
        }
        /// <summary>
        /// Permite obter uma lista com os dados do pedido efetuado
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Pedido com o id correspondente</returns>

        public async Task<List<RequestEntityStatus>> GetUserInfo(Guid id)
        {
            return await _context.RequestEntityStatus.Where(r => r.UserId == id).ToListAsync();
        }
        /// <summary>
        /// Permite obter um pedido pelo id do utilizador
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Pedido com o id correspondente</returns>

        public async Task<Guid> GetRequestId(Guid id)
        {
            return await _context.RequestEntityStatus.Where(r => r.UserId == id).Select(r => r.Id).FirstOrDefaultAsync();
        }
    }
}
