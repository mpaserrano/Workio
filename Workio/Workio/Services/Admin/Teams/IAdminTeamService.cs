using Workio.Data;
using Workio.Models;

namespace Workio.Services.Admin.Teams
{
    /// <summary>
    /// Interface para o administrador interagir com as equipas a partir da lista de equipas disponivel no dashboard
    /// </summary>
    public interface IAdminTeamService
    {


        /// <summary>
        /// Obtem todas as equipas.
        /// </summary>
        /// <returns>Collection de equipas</returns>
        public Task<ICollection<Team>> GetTeams();


        /// <summary>
        /// Bane um evento.
        /// </summary>
        /// <returns>sucesso se baniu um evento</returns>
        public Task<bool> BanTeam(Guid teamId);

        /// <summary>
        /// Remove o ban um evento.
        /// </summary>
        /// <returns>sucesso se removeu o ban um evento</returns>
        public Task<bool> UnbanTeam(Guid teamId);
    }
}
