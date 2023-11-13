using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Workio.Models;

namespace Workio.Services.ReportServices
{
    /// <summary>
    /// Interface para o serviço de denuncias, contêm os metodos de acesso a base de dados relacionados com as denuncias
    /// </summary>
    public interface IReportReasonService
    {
        /// <summary>
        /// Obtem todas as razoes de denuncias de utilizadores
        /// </summary>
        /// <returns>Lista de razões de denuncia</returns>
        Task<List<ReportReason>> GetReportReasonsUserAsync();
        /// <summary>
        /// Obtem todas as razoes de denuncias de equipas
        /// </summary>
        /// <returns>Lista de razões de denuncia</returns>
        Task<List<ReportReason>> GetReportReasonsTeamAsync();
        /// <summary>
        /// Obtem todas as razoes de denuncias de eventos
        /// </summary>
        /// <returns>Lista de razões de denuncia</returns>
        Task<List<ReportReason>> GetReportReasonsEventAsync();
        /// <summary>
        /// Obtem todas as razoes de denuncias
        /// </summary>
        /// <returns>Lista de razões de denuncia</returns>
        public Task<List<ReportReason>> GetReportReasonsAsync();
        /// <summary>
        /// Metodo para remover uma razão de denuncia à base de dados
        /// </summary>
        ///<param name="id">Id da razão a remover</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public Task<bool> RemoveReportReason(Guid id);
        /// <summary>
        /// Adiciona uma denuncia de utilizador à base de dados
        /// </summary>
        ///<param name="report">Denuncia efetuada ao utilizador</param>
        /// <returns>Retorna a denuncia adicionada</returns>
        Task<bool> AddUserReport(ReportUser report);
        /// <summary>
        /// Adiciona uma denuncia de equipa à base de dados
        /// </summary>
        ///<param name="report">Denuncia efetuada à equipa</param>
        /// <returns>Retorna a denuncia adicionada</returns>
        Task<bool> AddTeamReport(ReportTeam report);
        /// <summary>
        /// Adiciona uma denuncia de evento à base de dados
        /// </summary>
        ///<param name="report">Denuncia efetuada ao evento</param>
        /// <returns>Retorna a denuncia adicionada</returns>
        Task<bool> AddEventReport(ReportEvent report);
        /// <summary>
        /// Obtem todos os reports que foram feitos do tipo user
        /// </summary>
        /// <returns>Lista de utilizadores reportados</returns>
        public Task<ICollection<ReportUser>> GetUserReports();
        /// <summary>
        /// Obtem todos os reports que foram feitos do tipo equipa
        /// </summary>
        /// <returns>Lista de equipas reportadas</returns>
        public Task<ICollection<ReportTeam>> GetTeamReports();
        /// <summary>
        /// Obtem todos os reports que foram feitos do tipo evento
        /// </summary>
        /// <returns>Lista de eventos reportados</returns>
        public Task<ICollection<ReportEvent>> GetEventReports();
        /// <summary>
        /// Obtem um report de utilizador a partir de um certo id
        /// </summary>
        ///<param name="id">Id da denuncia</param>
        /// <returns>Denuncia de utilizador</returns>
        public Task<ReportUser> GetUserReport(Guid id);
        /// <summary>
        /// Obtem um report de equipa a partir de um certo id
        /// </summary>
        ///<param name="id">Id da denuncia</param>
        /// <returns>Denuncia de equipa</returns>
        public Task<ReportTeam> GetTeamReport(Guid id);
        /// <summary>
        /// Obtem um report de evento a partir de um certo id
        /// </summary>
        ///<param name="id">Id da denuncia</param>
        /// <returns>Denuncia de evento</returns>
        public Task<ReportEvent> GetEventReport(Guid id);
        /// <summary>
        /// Metodo para adicionar uma nova razão de denuncia à base de dados
        /// </summary>
        ///<param name="newReason">Nova razão a adicionar</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public Task<bool> AddNewReason(ReportReason newReason);
        /// <summary>
        /// Metodo para rejeitar uma denuncia de utilizador
        /// </summary>
        ///<param name="id">Id da denuncia a rejeitar</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public Task<bool> RejectUserReport(Guid id);
        /// <summary>
        /// Metodo para rejeitar uma denuncia de equipa
        /// </summary>
        ///<param name="id">Id da denuncia a rejeitar</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public Task<bool> RejectTeamReport(Guid id);
        /// <summary>
        /// Metodo para rejeitar uma denuncia de evento
        /// </summary>
        ///<param name="id">Id da denuncia a rejeitar</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public Task<bool> RejectEventReport(Guid id);
        /// <summary>
        /// Obtem todos os reports que foram feitos e ja estão resolvidos
        /// </summary>
        /// <returns>Lista de denuncias resolvidas</returns>
        public Task<ICollection<Report>> GetArchiveReports();
        /// <summary>
        /// Obtem os reports resolvidos de equipas
        /// </summary>
        /// <returns>Lista de denuncias resolvidas</returns>
        public Task<ICollection<ReportTeam>> GetArchiveTeamReports();
        /// <summary>
        /// Obtem os reports resolvidos de eventos
        /// </summary>
        /// <returns>Lista de denuncias resolvidas</returns>
        public Task<ICollection<ReportEvent>> GetArchiveEventReports();
        /// <summary>
        /// Obtem os reports resolvidos de utilizadores
        /// </summary>
        /// <returns>Lista de denuncias resolvidas</returns>
        public Task<ICollection<ReportUser>> GetArchiveUserReports();
        /// <summary>
        /// Metodo para aceitar uma denuncia de utilizador
        /// </summary>
        ///<param name="id">Id da denuncia a aceitar</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public Task<bool> AcceptUserReport(Guid id);
        /// <summary>
        /// Metodo para rejeitar uma denuncia de equipa
        /// </summary>
        ///<param name="id">Id da denuncia a aceitar</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public Task<bool> AcceptTeamReport(Guid id);
        /// <summary>
        /// Metodo para rejeitar uma denuncia de evento
        /// </summary>
        ///<param name="id">Id da denuncia a aceitar</param>
        /// <returns>Devolve true se a operação teve sucesso, e false caso contrário</returns>
        public Task<bool> AcceptEventReport(Guid id);
        /// <summary>
        /// Adiciona a tradução de uma razão de denúncia
        /// </summary>
        /// <param name="reportReasonLocalization">Objeto da tradução</param>
        /// <returns>true se conseguiu guardar a tradução, false caso nao tenha conseguido</returns>
        public Task<bool> AddReasonLocalization(ReportReasonLocalization reportReasonLocalization);

    }
}
