using Workio.Data;
using Workio.Models.Admin.Logs;

namespace Workio.Services.Admin.Log
{
    /// <summary>
    /// Interface para interagir e guardar dados na base de dados relativamente aos logs do frontend.
    /// </summary>
    public interface ILogsService
    {
        /// <summary>
        /// Insere um log relativamente a uma ação aplicada a um utilizador na base de dados.
        /// </summary>
        /// <param name="description">Descrição/Motivo</param>
        /// <param name="changedUserId">Id do utilizador que foi afetado pela alteração ao seu estado por parte do moderador.</param>
        /// <param name="actionType">Tipo de ação aplicada.</param>
        /// <returns>True: Se bem sucedido False: Se Falhou</returns>
        public Task<bool> CreateUserActionLog(string description, string changedUserId, UserActionLogType? actionType);

        /// <summary>
        /// Insere um log relativamente a uma ação feita por um utilizador que não influencia nenhuma 2ª entidade.
        /// </summary>
        /// <param name="description">Descrição/Motivo</param>
        /// <param name="actionType">Tipo de ação aplicada.</param>
        /// <returns>True: Se bem sucedido False: Se Falhou</returns>
        public Task<bool> CreateAdminActionLog(string description, AdministrationActionLogType? actionType);

        /// <summary>
        /// Insere um log relativamente a uma ação aplicada a uma equipa na base de dados.
        /// </summary>
        /// <param name="description">Descrição/Motivo</param>
        /// <param name="changedTeamId">Id da equipa que foi afetado pela alteração ao seu estado por parte do moderador.</param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        public Task<bool> CreateTeamActionLog(string description, string changedTeamId, TeamActionLogType? actionType);

        /// <summary>
        /// Insere um log relativamente a uma ação aplicada a um evento na base de dados.
        /// </summary>
        /// <param name="description">Descrição/Motivo</param>
        /// <param name="changedEventId">Id do evento que foi afetado pela alteração ao seu estado por parte do moderador.</param>
        /// <param name="actionType">Tipo de ação aplicada.</param>
        /// <returns>True: Se bem sucedido False: Se Falhou</returns>
        public Task<bool> CreateEventActionLog(string description, string changedEventId, EventActionLogType? actionType);

        /// <summary>
        /// Insere um log relativamente a algum acontecimento que não foi necessáriamente desencadeado por um utilizador ou relativamente a um utilizador.
        /// </summary>
        /// <param name="description">Descrição do log</param>
        /// <returns></returns>
        public Task<bool> CreateSystemLog(string description);

        /// <summary>
        /// Retorna todos os dados relativamente a logs de UserAction
        /// </summary>
        /// <returns>Lista com logs de UserAction</returns>
        public Task<List<UserActionLog>> GetUserActionLogData();

        /// <summary>
        /// Retorna todos os logs relativamente ao UserActionLog aplicado um filtro de pesquisa que se for null
        /// apenas é ignorado e retorna todos os dados.
        /// </summary>
        /// <param name="search">Filtro que faz com que os dados sejam filtrados
        /// pelos cujo o texto contenha o que foi recebido.</param>
        /// <returns>Retorna os dados filtrados.</returns>
        public Task<IEnumerable<Object>> GetUserActionDataFiltered(string search);

        /// <summary>
        /// Retorna todos os logs relativamente à TeamActionLog aplicando um filtro de pesquisa que se for null
        /// apenas é ignorado e retorna todos os dados.
        /// </summary>
        /// <param name="search">Filtro que faz com que os dados sejam filtrados cujo
        /// o texto contenha o mesmo texto recebido.</param>
        /// <returns>Retorna os dados de logs de equipas filtrados.</returns>
        public Task<IEnumerable<Object>> GetTeamActionLogDataFiltered(string search);

        /// <summary>
        /// Retorna todos os logs relativamente aos Eventos aplicando um filtro de pesquisa que se for null
        /// apenas é ignorado e retorna todos os dados.
        /// </summary>
        /// <param name="search">Filtro que faz com que os dados sejam filtrados cujo
        /// o texto contenha o mesmo texto recebido.</param>
        /// <returns>Retorna os dados de logs de eventos filtrados.</returns>
        public Task<IEnumerable<object>> GetEventActionLogDataFiltered(string search);

        /// <summary>
        /// Retorna todos os logs relativamente às alterações de configurações na administração aplicando um filtro de pesquisa que se for null
        /// apenas é ignorado e retorna todos os dados.
        /// </summary>
        /// <param name="search">Filtro que faz com que os dados sejam filtrados cujo
        /// o texto contenha o mesmo texto recebido.</param>
        /// <returns>Retorna os dados de logs de eventos filtrados.</returns>
        public Task<IEnumerable<object>> GetAdminActionLogDataFiltered(string search);

        /// <summary>
        /// Obtem e retorna um log especifico referente ao UserLogAction através do Id recebido.
        /// </summary>
        /// <param name="userActionLogId">Id para a procura.</param>
        /// <returns>Um objeto com as informações do log do utilizador</returns>
        public Task<UserActionLog> GetUserActionLogByLogId(string userActionLogId);

        /// <summary>
        /// Obtem e retorna um log especifico referente ao TeamActionLog através do Id recebido.
        /// </summary>
        /// <param name="userActionLogId">Id para a procura.</param>
        /// <returns>Um objeto com as informações do log de uma equipa</returns>
        public Task<TeamActionLog> GetTeamActionLogByLogId(string teamActionLogId);

        /// <summary>
        /// Obtem e retorna um log especifico referente ao EventActionLog através do Id recebido.
        /// </summary>
        /// <param name="userActionLogId">Id para a procura.</param>
        /// <returns>Um objeto com as informações do log de um evento</returns>
        public Task<EventActionLog> GetEventActionLogByLogId(string eventActionLogId);

        /// <summary>
        /// Obtem e retorna um log especifico referente ao AdministrationActionLog através do Id recebido.
        /// </summary>
        /// <param name="userActionLogId">Id para a procura.</param>
        /// <returns>Um objeto com as informações do log de uma modificação de configurações da plataforma
        /// pelos administradores.</returns>
        public Task<AdministrationActionLog> GetAdministrationActionLogByLogId(string administrationActionLogId);
    }
}
