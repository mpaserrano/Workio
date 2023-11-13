using Org.BouncyCastle.Asn1.Mozilla;
using Workio.Models;

namespace Workio.Services.Teams
{
    /// <summary>
    /// Representa a interação com a base de dados relativamente as equipas.
    /// </summary>
    public interface ITeamsService
    {
        /// <summary>
        /// Obtem todas as equipas que um utilizador faz parte e é o autor para determinado utilizador.
        /// </summary>
        /// <param name="userId">Id do utilizador do qual vai se obter as equipas.</param>
        /// <returns>Coleção de todos as equipas que o utilizador faz parte incluindo as próprias.</returns>
        public Task<ICollection<Team>> GetAllUserTeamsByUserId(Guid userId);

        /// <summary>
        /// Obtem todas as equipas na base de dados
        /// </summary>
        /// <returns>Coleção de equipas</returns>
        public Task<ICollection<Team>> GetTeams();
        /// <summary>
        /// Obtem todas as equipas que o utilizador faz parte na base de dados
        /// </summary>
        /// <returns>Coleção de equipas</returns>
        public Task<ICollection<Team>> GetMyTeams();
        /// <summary>
        /// Obtem todas as equipas que o utilizador faz parte na base de dados
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Coleção de equipas</returns>
        public Task<ICollection<Team>> GetUserTeams(Guid id);
        /// <summary>
        /// Obtem todas as equipas onde o utilizador é o dono
        /// </summary>
        /// <returns>Coleção de equipas</returns>
        public Task<ICollection<Team>> GetOwnTeams();
        /// <summary>
        /// Obtem as equipas de que um utilizador é o dono.
        /// </summary>
        /// <param name="userId">Id do utilizador do qual a obter as equipas.</param>
        /// <returns>Coleção das equipas de que o utilizado referido é dono.</returns>
        public Task<ICollection<Team>> GetOwnAllTeamsByUserId(Guid userId);
        /// <summary>
        /// Obtem uma equipa com um id especifico
        /// </summary>
        /// <param name="id">Id da equipa</param>
        /// <returns>Equipa, caso não seja possivel encontrar é retornado null</returns>
        public Task<Team> GetTeamById(Guid id);

        /// <summary>
        /// Obtem a lista de utilizadores que pediram para participar na equipa.
        /// </summary>
        /// <param name="teamId">Id da equipa da qual obtem a lista de pedidos de participação.</param>
        /// <returns>Lista com os pedidos de participação na equipa.</returns>
        public Task<List<PendingUserTeam>> GetTeamPendingRequestsByTeamId(Guid teamId);

        /// <summary>
        /// Cria uma equipa e guarda na base de dados
        /// </summary>
        /// <param name="team">Objeto com os dados da equipa</param>
        /// <returns>True se foi guardada com sucesso, false caso contrário</returns>
        public Task<bool> CreateTeam(Team team);

        /// <summary>
        /// Atualiza os dados de uma equipa
        /// </summary>
        /// <param name="team">Equipa atualizada</param>
        /// <returns>true caso tenha sido atualizada com sucesso, false caso contrário</returns>
        public Task<bool> UpdateTeam(Team team);

        /// <summary>
        /// Altera o estado da equipa
        /// </summary>
        /// <param name="teamStatus">Novo estado</param>
        /// <param name="teamId">Id da equipa</param>
        /// <returns>true caso seja alterado com sucesso, false caso contrário</returns>
        public Task<bool> ChangeTeamStatus(TeamStatus teamStatus, Guid teamId);

        /// <summary>
        /// Adiciona um user a equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <param name="userId">Id do utilizador a adicionar</param>
        /// <returns>true se conseguiu adicionar o utilizador, false caso contrário</returns>
        public Task<bool> AddUser(Guid teamId, Guid userId);

        /// <summary>
        /// Remove um user da equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <param name="userId">Id do utilizador a remover</param>
        /// <returns>true se conseguiu remover o utilizador, false caso contrário</returns>
        public Task<bool> RemoveUser(Guid teamId, Guid userId);

        /// <summary>
        /// Pedir para fazer parte de uma equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <returns>true se o pedido foi feito com sucesso, false caso contrário</returns>
        public Task<bool> AskAccess(Guid teamId);

        /// <summary>
        /// Aceita o pedido de um user e adiciona-o a equipa
        /// </summary>
        /// <param name="requestId">Id do pedido</param>
        /// <returns>true se conseguiu adicionar o utilizador, false caso contrário</returns>
        public Task<bool> AcceptAccess(Guid requestId);

        /// <summary>
        /// Rejeitar um pedido para fazer parte de uma equipa
        /// </summary>
        /// <param name="requestId">Id do pedido</param>
        /// <returns>true se o pedido foi feito com sucesso, false caso contrário</returns>
        public Task<bool> RejectAccess(Guid requestId);

        /// <summary>
        /// Abandonar equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <returns>true caso tenha abandonado a equipa com sucesso, false caso contrário</returns>
        public Task<bool> LeaveTeam(Guid teamId);

        /// <summary>
        /// Convida um utilizador a participar na equipa
        /// </summary>
        /// <param name="teamId">Id da equipa para a qual está a ser convidado</param>
        /// <param name="userId">Id do utilizador convidado</param>
        /// <returns></returns>
        public Task<bool> InviteUserToTeam(Guid teamId, Guid userId);
        /// <summary>
        /// Aceita o pedido de uma equipa e adiciona-o a equipa
        /// </summary>
        /// <param name="requestId">Id do pedido</param>
        /// <returns>true se conseguiu adicionar o utilizador, false caso contrário</returns>
        public Task<bool> AcceptInvite(Guid requestId);
        /// <summary>
        /// Aceita o pedido de uma equipa e adiciona-o a equipa
        /// </summary>
        /// <param name="team">Id da equipa que fez o pedido</param>
        /// <returns>true se conseguiu adicionar o utilizador, false caso contrário</returns>
        public Task<bool> AcceptInviteByTeam(Guid teamId);
        /// <summary>
        /// Rejeita o pedido de uma equipa
        /// </summary>
        /// <param name="requestId">Id do pedido</param>
        /// <returns>true se conseguiu adicionar o utilizador, false caso contrário</returns>
        public Task<bool> RejectInvite(Guid requestId);
        /// <summary>
        /// Rejeita o pedido de uma equipa
        /// </summary>
        /// <param name="teamId">Id da equipa que fez o pedido</param>
        /// <returns>true se conseguiu rejeitar o utilizador, false caso contrário</returns>
        public Task<bool> RejectInviteByTeam(Guid teamId);
        /// <summary>
        /// Cancela o convite para participar numa equipa
        /// </summary>
        /// <param name="requestId">Id do pedido</param>
        /// <returns>true se conseguiu cancelar o pedido, false caso contrário</returns>
        public Task<bool> CancelInvite(Guid requestId);

        /// <summary>
        /// Obtem todas as tags já criadas
        /// </summary>
        /// <returns>Coleção de tags (skills)</returns>
        public Task<ICollection<Tag>> GetTags();

        /// <summary>
        /// Adiciona uma tag a base de dados
        /// </summary>
        /// <param name="tag">Tag a adicionar</param>
        /// <returns>true caso tenha inserido a tag com sucesso, false caso contrário</returns>
        public Task<bool> CreateTag(Tag tag);
        /// <summary>
        /// Adiciona uma milestone a equipa
        /// </summary>
        /// <param name="milestone">Milestone a adicionar</param>
        /// <param name="teamId">Id da equipa que vai receber a milestone</param>
        /// <returns>true caso tenha sido inserida com sucesso, false caso contrário</returns>
        public Task<bool> AddMilestone(Milestone milestone, Guid teamId);
        /// <summary>
        /// Atualiza uma milestone
        /// </summary>
        /// <param name="milestone">Milestone atualizada</param>
        /// <param name="teamId">Id da equipa</param>
        /// <returns>true caso tenha sido atualizada com sucesso, false caso contrário</returns>
        public Task<bool> UpdateMilestone(Milestone milestone, Guid teamId);
        /// <summary>
        /// Elimina uma milestone
        /// </summary>
        /// <param name="milestoneId">Id da milestone a apagar</param>
        /// <param name="teamId">Id da equipa com a milestone</param>
        /// <returns></returns>
        public Task<bool> DeleteMilestone(Guid milestoneId, Guid teamId);
        /// <summary>
        /// Altera o estado de uma milestone
        /// </summary>
        /// <param name="milestoneId">Id da milestone</param>
        /// <param name="teamId">Id da equipa</param>
        /// <param name="state">Novo Estado</param>
        /// <returns>true se conseguiu alterar o estado, false caso contrário</returns>
        public Task<bool> ChangeMilestoneStatus(Guid milestoneId, Guid teamId, MilestoneState state);
        /// <summary>
        /// Verifica se dois utilizadores pertencem a mesma equipa
        /// </summary>
        /// <param name="tag">Utilizador a ser avaliado</param>
        /// <returns>true caso 2 utilizadores pertençam a mesma equipa false caso contrário</returns>
        public Task<bool> AreTeammates(Guid otherUserId);

        /// <summary>
        /// Dá ownership a um user da equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <param name="userId">Id do utilizador a remover</param>
        /// <returns>true se conseguiu dar ownership</returns>
        public Task<bool> GiveOwnership(Guid teamId, Guid userId);

        /// <summary>
        /// Dá o valor médio das avaliações dos utilizadores da equipa
        /// </summary>
        /// <param name="teamId">Id da equipa</param>
        /// <returns>valor médio das avaliações dos utilizadores da equipa</returns>
        public Task<double> GetAverageRating(Guid teamId);

        /// <summary>
        /// Retorna as equipas abertas das quais o utilizador nao faz parte
        /// </summary>
        /// <returns>Retorna as equipas abertas das quais o utilizador nao faz parte</returns>
        public Task<ICollection<Team>> GetOpenNewTeams();

        /// <summary>
        /// Obtem o pedido de adesão a uma equipa.
        /// </summary>
        /// <param name="id">Id do pedido.</param>
        /// <returns>Objeto do pedido de adesão.</returns>
        public Task<PendingUserTeam> GetRequestById(Guid id);

        /// <summary>
        /// Obtem a equipa de um convite pelo id do convite.
        /// </summary>
        /// <param name="inviteId">Id do convite.</param>
        /// <returns>Objeto da equipa.</returns>
        public Task<TeamInviteUser> GetTeamByInviteId(Guid inviteId);
    }
}
