using Workio.Models;

namespace Workio.Services.RequestEntityStatusServices
{
    /// <summary>
    /// Interface de serviços para guardar dados relativamente aos pedidos de entidades registadas na base de dados.
    /// </summary>
    public interface IRequestEntityStatusService
    {
        /// <summary>
        /// Cria um pedido e guarda na base de dados
        /// </summary>
        /// <param name="request">Objeto com os dados do pedido</param>
        /// <returns>True se foi guardada com sucesso, false caso contrário</returns>
        public Task<bool> CreateRequest(RequestEntityStatus request);

        /// <summary>
        /// Permite validar se um utilizador pode atualizar um pedido
        /// </summary>
        /// <param name="request">Objeto com os dados do pedido</param>
        /// <returns>True se foi atualizado com sucesso, false caso contrário</returns>

        public Task<bool> UpdateRequest(RequestEntityStatus request);

        /// <summary>
        /// Permite validar se um utilizador já realizou um pedido
        /// </summary>
        /// <param name="id">Id do utilizador atual</param>
        /// <returns>True se ja realizou pedido, false caso contrário</returns>
        public bool AlreadyRequested(Guid id);

        /// <summary>
        /// Permite obter um pedido pelo seu id
        /// </summary>
        /// <param name="id">Id do pedido</param>
        /// <returns>Pedido com o id correspondente</returns>
        public Task<RequestEntityStatus> GetRequestById(Guid id);
        /// <summary>
        /// Permite o estado de um pedido por id do utilizador
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Pedido com o id correspondente</returns>
        public Task<RequestState> GetRequestStateByUserId(Guid id);
        /// <summary>
        /// Permite obter uma lista com os dados do pedido efetuado
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Pedido com o id correspondente</returns>
        public Task<List<RequestEntityStatus>> GetUserInfo(Guid id);
        /// <summary>
        /// Permite obter um pedido pelo id do utilizador
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Pedido com o id correspondente</returns>
        public Task<Guid> GetRequestId(Guid id);
    }
}
