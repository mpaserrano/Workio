using Workio.Models;

namespace Workio.Services.Admin
{
    /// <summary>
    /// Interface para interagir e guardar dados na base de dados relativamente aos pedidos de entidade registada
    /// </summary>
    public interface IAdminService
    {
        /// <summary>
        /// Obtem uma lista de pedidos de entidade registadas
        /// </summary>
        /// <returns>Lista de pedidos de entidades registadas</returns>
        public Task<List<RequestEntityStatus>> GetRequestsAsync();
        /// <summary>
        /// Verifica se é possível aprovar um pedido para ser entidade registada
        /// </summary>
        /// <param name="id">Id do pedido</param>
        /// <returns>True: Se bem sucedido False: Se Falhou</returns>
        public Task<bool> ApproveRequest(Guid id);
        /// <summary>
        /// Verifica se é possível rejeitar um pedido para ser entidade registada
        /// </summary>
        /// <param name="id">Id do pedido</param>
        /// <returns>True: Se bem sucedido False: Se Falhou</returns>
        public Task<bool> RejectRequest(Guid id);
        /// <summary>
        /// Obtem um pedido de entidade registada pelo seu id
        /// </summary>
        /// <param name="id">Id do pedido</param>
        /// <returns>RequestEntityStatus - pedido com o id correspondente</returns>
        public Task<RequestEntityStatus> GetRequestById(Guid id);
        /// <summary>
        /// Obtem uma lista de de todos os pedidos de entidade registada
        /// </summary>
        /// <returns>Lista com os pedidos</returns>
        public Task<List<RequestEntityStatus>> GetRequests();

        /// <summary>
        /// Suspende um utilizador durante x dias
        /// </summary>
        /// <param name="userId">Id do utilizador a suspender</param>
        /// <param name="duration">Duração da suspensão em dias</param>
        /// <returns>True se o utilizador foi suspenso com sucesso, false caso contrário</returns>
        public Task<bool> SuspendUser(Guid userId, int duration = 5);

        /// <summary>
        /// Remove a suspensão a um utilizador 
        /// </summary>
        /// <param name="userId">Id do utilizador a suspender</param>
        /// <returns>True se o utilizador foi reintegrado com sucesso, false caso contrário</returns>
        public Task<bool> UnsuspendUser(Guid userId);

        /// <summary>
        /// Obtem uma lista de reports que foram feitos contra o utilizador
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Lista de denúncias</returns>
        public Task<List<ReportUser>> GetUserReports(Guid id);
        /// <summary>
        /// Obtem uma lista de reports que foram feitos contra o equipas que o utilizador participa
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Lista de denúncias</returns>
        public Task<List<ReportTeam>> GetUserTeamsReports(Guid id);
        /// <summary>
        /// Obtem uma lista de reports que foram feitos contra o eventos que o utilizador participa
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Lista de denúncias</returns>
        public Task<List<ReportEvent>> GetUserEventsReports(Guid id);
    }
}
