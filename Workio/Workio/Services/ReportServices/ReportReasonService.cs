using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Workio.Data;
using Workio.Models;
using Workio.Models.Events;
using Workio.Services.Admin;
using Workio.Services.Admin.Events;
using Workio.Services.Admin.Teams;
using Workio.Services.Events;
using Workio.Services.Interfaces;
using Workio.Services.Teams;

namespace Workio.Services.ReportServices
{
    /// <summary>
    /// Classe com os metodos de gestão de denuncias com a base de dados
    /// </summary>
    public class ReportReasonService : IReportReasonService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly ITeamsService _teamService;
        private readonly IEventsService _eventService;
        private readonly IAdminEventService _adminEventService;
        private readonly IAdminTeamService _adminTeamService;
        private readonly IAdminService _adminService;

        public ReportReasonService(ApplicationDbContext context, IUserService userService, ITeamsService teamsService, IEventsService eventsService, IAdminEventService adminEventService, IAdminTeamService adminTeamService, IAdminService adminService)
        {
            _context = context;
            _userService = userService;
            _teamService = teamsService;
            _eventService = eventsService;
            _adminEventService = adminEventService;
            _adminTeamService = adminTeamService;
            _adminService = adminService;
        }
        /// <summary>
        /// Obtem todas as razoes de denuncias de utilizador
        /// </summary>
        /// <returns>Lista de razões de denuncia</returns>
        public async Task<List<ReportReason>> GetReportReasonsUserAsync()
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            List<ReportReason> reportReasons = await _context.ReportReason.Where(r => r.ReasonType == ReasonType.User).ToListAsync<ReportReason>();
            if (culture == "en")
            {
                // If the culture is English, return the original ReportReason objects
                return reportReasons;
            }
            else
            {
                // Otherwise, get the translations for each ReportReason
                List<ReportReason> translatedReportReasons = new List<ReportReason>();
                foreach (ReportReason reportReason in reportReasons)
                {
                    ReportReasonLocalization localization = await _context.ReportReasonLocalizations.SingleOrDefaultAsync(r => r.ReportId == reportReason.Id && r.LocalizationCode == culture);
                    if (localization != null)
                    {
                        // If a translation exists for the current culture, use it
                        translatedReportReasons.Add(new ReportReason { Id = reportReason.Id, Reason = localization.Description, ReasonType = reportReason.ReasonType });
                    }
                    else
                    {
                        // Otherwise, use the original ReportReason object
                        translatedReportReasons.Add(reportReason);
                    }
                }
                return translatedReportReasons;
            }
        }
        /// <summary>
        /// Obtem todas as razoes de denuncias de equipas
        /// </summary>
        /// <returns>Lista de razões de denuncia</returns>
        public async Task<List<ReportReason>> GetReportReasonsTeamAsync()
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            List<ReportReason> reportReasons = await _context.ReportReason.Where(r => r.ReasonType == ReasonType.Team).ToListAsync<ReportReason>();
            if (culture == "en")
            {
                // If the culture is English, return the original ReportReason objects
                return reportReasons;
            }
            else
            {
                // Otherwise, get the translations for each ReportReason
                List<ReportReason> translatedReportReasons = new List<ReportReason>();
                foreach (ReportReason reportReason in reportReasons)
                {
                    ReportReasonLocalization localization = await _context.ReportReasonLocalizations.SingleOrDefaultAsync(r => r.ReportId == reportReason.Id && r.LocalizationCode == culture);
                    if (localization != null)
                    {
                        // If a translation exists for the current culture, use it
                        translatedReportReasons.Add(new ReportReason { Id = reportReason.Id, Reason = localization.Description, ReasonType = reportReason.ReasonType });
                    }
                    else
                    {
                        // Otherwise, use the original ReportReason object
                        translatedReportReasons.Add(reportReason);
                    }
                }
                return translatedReportReasons;
            }

        }
        /// <summary>
        /// Obtem todas as razoes de denuncias de eventos
        /// </summary>
        /// <returns>Lista de razões de denuncia</returns>
        public async Task<List<ReportReason>> GetReportReasonsEventAsync()
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            List<ReportReason> reportReasons = await _context.ReportReason.Where(r => r.ReasonType == ReasonType.Event).ToListAsync<ReportReason>();
            if (culture == "en")
            {
                // If the culture is English, return the original ReportReason objects
                return reportReasons;
            }
            else
            {
                // Otherwise, get the translations for each ReportReason
                List<ReportReason> translatedReportReasons = new List<ReportReason>();
                foreach (ReportReason reportReason in reportReasons)
                {
                    ReportReasonLocalization localization = await _context.ReportReasonLocalizations.SingleOrDefaultAsync(r => r.ReportId == reportReason.Id && r.LocalizationCode == culture);
                    if (localization != null)
                    {
                        // If a translation exists for the current culture, use it
                        translatedReportReasons.Add(new ReportReason { Id = reportReason.Id, Reason = localization.Description, ReasonType = reportReason.ReasonType });
                    }
                    else
                    {
                        // Otherwise, use the original ReportReason object
                        translatedReportReasons.Add(reportReason);
                    }
                }
                return translatedReportReasons;
            }
        }

        /// <summary>
        /// Obtem todas as razoes de denuncias
        /// </summary>
        /// <returns>Lista de razões de denuncia</returns>
        public async Task<List<ReportReason>> GetReportReasonsAsync()
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            List<ReportReason> reportReasons = await _context.ReportReason.ToListAsync<ReportReason>();
            if (culture == "en")
            {
                // If the culture is English, return the original ReportReason objects
                return reportReasons;
            }
            else
            {
                // Otherwise, get the translations for each ReportReason
                List<ReportReason> translatedReportReasons = new List<ReportReason>();
                foreach (ReportReason reportReason in reportReasons)
                {
                    ReportReasonLocalization localization = await _context.ReportReasonLocalizations.SingleOrDefaultAsync(r => r.ReportId == reportReason.Id && r.LocalizationCode == culture);
                    if (localization != null)
                    {
                        // If a translation exists for the current culture, use it
                        translatedReportReasons.Add(new ReportReason { Id = reportReason.Id, Reason = localization.Description, ReasonType = reportReason.ReasonType });
                    }
                    else
                    {
                        // Otherwise, use the original ReportReason object
                        translatedReportReasons.Add(reportReason);
                    }
                }
                return translatedReportReasons;
            }
        }
        /// <summary>
        /// Adiciona uma denuncia de utilizador à base de dados
        /// </summary>
        ///<param name="report">Denuncia efetuada ao utilizador</param>
        /// <returns>Retorna a denuncia adicionada</returns>
        public async Task<bool> AddUserReport(ReportUser report)
        {
            if(await GetUserReport(report.Id) != null)
            {
                return false;
            }
            _context.Add(report);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }
        /// <summary>
        /// Adiciona uma denuncia de equipa à base de dados
        /// </summary>
        ///<param name="report">Denuncia efetuada à equipa</param>
        /// <returns>Retorna a denuncia adicionada</returns>
        public async Task<bool> AddTeamReport(ReportTeam report)
        {
            if (await GetTeamReport(report.Id) != null)
            {
                return false;
            }
            var team = await _teamService.GetTeamById(new Guid(report.ReportedTeamId.ToString()));
            if (report.ReporterId == team.OwnerId.ToString())
            {
                return false;
            }
            _context.Add(report);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }
        /// <summary>
        /// Adiciona uma denuncia de evento à base de dados
        /// </summary>
        ///<param name="report">Denuncia efetuada ao evento</param>
        /// <returns>Retorna a denuncia adicionada</returns>
        public async Task<bool> AddEventReport(ReportEvent report)
        {
            var @eventReport = await GetEventReport(report.Id);
            if (@eventReport != null)
            {
                return false;
            }
            var @event = await _eventService.GetEvent(new Guid(report.ReportedEventId.ToString()));
            if (report.ReporterId == @event.UserId)
            {
                return false;
            }
            _context.Add(report);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }

        /// <summary>
        /// Obtem todos os reports que foram feitos do tipo user
        /// </summary>
        /// <returns>Lista de utilizadores reportados</returns>
        public async Task<ICollection<ReportUser>> GetUserReports()
        {
            var list = await _context.ReportUser.Include(r => r.ReportedUser).Include(r => r.Reporter).Include(r => r.ReportReason).Where(r => r.ReportStatus == ReportStatus.Pending).ToListAsync<ReportUser>();
            if (list == null)
                return new List<ReportUser>();
            return list;
        }

        /// <summary>
        /// Obtem todos os reports que foram feitos do tipo equipa
        /// </summary>
        /// <returns>Lista de equipas reportadas</returns>
        public async Task<ICollection<ReportTeam>> GetTeamReports()
        {
            var list = await _context.ReportTeams.Include(r => r.ReportedTeam).Include(r => r.ReportReason).Include(r => r.Reporter).Where(r => r.ReportStatus == ReportStatus.Pending).ToListAsync<ReportTeam>();
            if (list == null)
                return new List<ReportTeam>();
            return list;
        }
        /// <summary>
        /// Obtem todos os reports que foram feitos do tipo evento
        /// </summary>
        /// <returns>Lista de eventos reportadas</returns>
        public async Task<ICollection<ReportEvent>> GetEventReports()
        {
            var list = await _context.ReportEvents.Include(r => r.ReportedEvent).Include(r => r.ReportReason).Include(r => r.Reporter).Where(r => r.ReportStatus == ReportStatus.Pending).ToListAsync<ReportEvent>();
            if (list == null)
                return new List<ReportEvent>();
            return list;
        }
        /// <summary>
        /// Obtem todos os reports que foram feitos e ja estão resolvidos
        /// </summary>
        /// <returns>Lista de denuncias resolvidas</returns>
        public async Task<ICollection<Report>> GetArchiveReports()
        {
            var archiveEvents = await _context.ReportEvents.Include(r => r.ReportedEvent).Include(r => r.ReportReason).Include(r => r.Reporter).Where(r => r.ReportStatus != ReportStatus.Pending).ToListAsync<ReportEvent>();
            var archiveTeams = await _context.ReportTeams.Include(r => r.ReportedTeam).Include(r => r.ReportReason).Include(r => r.Reporter).Where(r => r.ReportStatus != ReportStatus.Pending).ToListAsync<ReportTeam>();
            var archiveUsers = await _context.ReportUser.Include(r => r.ReportedUser).Include(r => r.Reporter).Include(r => r.ReportReason).Where(r => r.ReportStatus != ReportStatus.Pending).ToListAsync<ReportUser>();


            if (archiveEvents == null)
            {
                archiveEvents = new List<ReportEvent>();
            }
            if (archiveTeams == null)
            {
                archiveTeams = new List<ReportTeam>();

            }
            if (archiveUsers == null)
            {
                archiveUsers = new List<ReportUser>();

            }
            List<Report> list = new List<Report>();

            foreach (ReportEvent r in archiveEvents)
            {
                list.Add(r);
            }
            foreach (ReportTeam r in archiveTeams)
            {
                list.Add(r);
            }
            foreach (ReportUser r in archiveUsers)
            {
                list.Add(r);
            }
            return list;
        }

        /// <summary>
        /// Obtem os reports resolvidos de equipas
        /// </summary>
        /// <returns>Lista de denuncias resolvidas</returns>
        public async Task<ICollection<ReportTeam>> GetArchiveTeamReports()
        {
            var archiveTeams = await _context.ReportTeams.Include(r => r.ReportedTeam).Include(r => r.ReportReason).Include(r => r.Reporter).Where(r => r.ReportStatus != ReportStatus.Pending).ToListAsync<ReportTeam>();


            if (archiveTeams == null)
            {
                archiveTeams = new List<ReportTeam>();

            }

            return archiveTeams;
        }

        /// <summary>
        /// Obtem os reports resolvidos de eventos
        /// </summary>
        /// <returns>Lista de denuncias resolvidas</returns>
        public async Task<ICollection<ReportEvent>> GetArchiveEventReports()
        {
            var archiveEvents = await _context.ReportEvents.Include(r => r.ReportedEvent).Include(r => r.ReportReason).Include(r => r.Reporter).Where(r => r.ReportStatus != ReportStatus.Pending).ToListAsync<ReportEvent>();
            

            if (archiveEvents == null)
            {
                archiveEvents = new List<ReportEvent>();
            }


            return archiveEvents;
        }

        /// <summary>
        /// Obtem os reports resolvidos de utilizadores
        /// </summary>
        /// <returns>Lista de denuncias resolvidas</returns>
        public async Task<ICollection<ReportUser>> GetArchiveUserReports()
        {
            var archiveUsers = await _context.ReportUser.Include(r => r.ReportedUser).Include(r => r.Reporter).Include(r => r.ReportReason).Where(r => r.ReportStatus != ReportStatus.Pending).ToListAsync<ReportUser>();


            if (archiveUsers == null)
            {
                archiveUsers = new List<ReportUser>();

            }
            
            return archiveUsers;
        }


        /// <summary>
        /// Obtem um report de utilizador a partir de um certo id
        /// </summary>
        ///<param name="id">Id da denuncia</param>
        /// <returns>Denuncia de utilizador</returns>
        public async Task<ReportUser> GetUserReport(Guid id)
        {
            var user = await _context.ReportUser.Include(r => r.ReportedUser).Include(r => r.Reporter).Include(r => r.ReportReason).FirstOrDefaultAsync(r => r.Id == id);
            if (user == null)
                return null;
            return user;
        }

        /// <summary>
        /// Obtem um report de equipa a partir de um certo id
        /// </summary>
        ///<param name="id">Id da denuncia</param>
        /// <returns>Denuncia de equipa</returns>
        public async Task<ReportTeam> GetTeamReport(Guid id)
        {
            var team = await _context.ReportTeams.Include(r => r.ReportedTeam).Include(r => r.ReportReason).FirstOrDefaultAsync(r => r.Id == id);
            if (team == null)
                return null;
            return team;
        }

        /// <summary>
        /// Obtem um report de evento a partir de um certo id
        /// </summary>
        ///<param name="id">Id da denuncia</param>
        /// <returns>Denuncia de evento</returns>
        public async Task<ReportEvent> GetEventReport(Guid id)
        {
            var @event = await _context.ReportEvents.Include(r => r.ReportedEvent).Include(r => r.ReportReason).FirstOrDefaultAsync(r => r.Id == id);
            if (@event == null)
                return null;
            return @event;
        }
        /// <summary>
        /// Metodo para adicionar uma nova razão de denuncia à base de dados
        /// </summary>
        ///<param name="newReason">Nova razão a adicionar</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public async Task<bool> AddNewReason(ReportReason newReason)
        {

            await _context.ReportReason.AddAsync(newReason);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }

        /// <summary>
        /// Adiciona a tradução de uma razão de denúncia
        /// </summary>
        /// <param name="reportReasonLocalization">Objeto da tradução</param>
        /// <returns>true se conseguiu guardar a tradução, false caso nao tenha conseguido</returns>
        public async Task<bool> AddReasonLocalization(ReportReasonLocalization reportReasonLocalization)
        {
            if (reportReasonLocalization == null) return false;

            var originalReasonExist = await ExistReportReason(reportReasonLocalization.ReportId);

            if (!originalReasonExist) return false;

            try
            {
                _context.Add(reportReasonLocalization);
                var success = await _context.SaveChangesAsync();

                return success == 1;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Metodo para remover uma razão de denuncia à base de dados
        /// </summary>
        ///<param name="id">Id da razão a remover</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public async Task<bool> RemoveReportReason(Guid id)
        {
            var reason = await _context.ReportReason.FirstOrDefaultAsync(r => r.Id == id);
            if(reason == null)
            {
                return false;
            }
            _context.ReportReason.Remove(reason);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }
        /// <summary>/
        /// Metodo para rejeitar uma denuncia de utilizador
        /// </summary>
        ///<param name="id">Id da denuncia a rejeitar</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public async Task<bool> RejectUserReport(Guid id)
        {

            var report = await GetUserReport(id);

            if (report == null || report.ReportStatus != ReportStatus.Pending) return false;

            report.ReportStatus = ReportStatus.Rejected;
            _context.ReportUser.Update(report);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }
        /// <summary>
        /// Metodo para rejeitar uma denuncia de equipa
        /// </summary>
        ///<param name="id">Id da denuncia a rejeitar</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public async Task<bool> RejectTeamReport(Guid id)
        {
            var report = await GetTeamReport(id);

            if (report == null || report.ReportStatus != ReportStatus.Pending) return false;

            report.ReportStatus = ReportStatus.Rejected;
            _context.ReportTeams.Update(report);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }
        /// <summary>
        /// Metodo para rejeitar uma denuncia de evento
        /// </summary>
        ///<param name="id">Id da denuncia a rejeitar</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public async Task<bool> RejectEventReport(Guid id)
        {

            var report = await GetEventReport(id);

            if (report == null || report.ReportStatus != ReportStatus.Pending) return false;

            report.ReportStatus = ReportStatus.Rejected;
            _context.ReportEvents.Update(report);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }
        /// <summary>
        /// Metodo para aceitar uma denuncia de utilizador
        /// </summary>
        ///<param name="id">Id da denuncia a aceitar</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public async Task<bool> AcceptUserReport(Guid id)
        {
            var report = await GetUserReport(id);

            if(report.ReporterId == report.ReportedId)
            {
                _context.ReportUser.Remove(report);
                await _context.SaveChangesAsync();
                return false;
            }

            if (report == null || report.ReportStatus != ReportStatus.Pending) return false;

            var bannedSuccess = await _adminService.SuspendUser(Guid.Parse(report.ReportedId));

            if(!bannedSuccess) return false;

            report.ReportStatus = ReportStatus.Accepted;
            _context.ReportUser.Update(report);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }
        /// <summary>
        /// Metodo para rejeitar uma denuncia de equipa
        /// </summary>
        ///<param name="id">Id da denuncia a aceitar</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public async Task<bool> AcceptTeamReport(Guid id)
        {

            var report = await GetTeamReport(id);
            var team = report.ReportedTeam;
            if (report == null || team == null) { return false; }

            if (report.ReportStatus != ReportStatus.Pending) return false;

            var bannedSuccess = await _adminTeamService.BanTeam(team.TeamId);
            if (!bannedSuccess)
            {
                return false;
            }
            report.ReportStatus = ReportStatus.Accepted;
            _context.ReportTeams.Update(report);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }
        /// <summary>
        /// Metodo para rejeitar uma denuncia de evento
        /// </summary>
        ///<param name="id">Id da denuncia a aceitar</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public async Task<bool> AcceptEventReport(Guid id)
        {

            var report = await GetEventReport(id);

            if (report == null || report.ReportStatus != ReportStatus.Pending) return false;

            var @event = report.ReportedEvent;
            var bannedSuccess = await _adminEventService.BanEvent(@event.EventId);
            if(!bannedSuccess) { return false; }
            report.ReportStatus = ReportStatus.Accepted;
            _context.ReportEvents.Update(report);
            var success = await _context.SaveChangesAsync();
            return success > 0;
        }

        private async Task<bool> ExistReportReason(Guid reportReasonId)
        {
            if (reportReasonId == Guid.Empty) return false;

            return await _context.ReportReason.AnyAsync(c => c.Id == reportReasonId);
        }
    }
}


