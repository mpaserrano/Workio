using Workio.Models;

namespace Workio.Services.Interfaces
{
    /// <summary>
    /// Interface para o serviço dos utilizadores
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Use este método para obter um utilizador com um determinado Id.
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Utilizador</returns>
        Task<User> GetUser(Guid id);
        /// <summary>
        /// Obtem todas as habilidades e recomendações para um utilizador com um Id especifico.
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Habilidades e recomendações das habilidades</returns>
        Task<List<SkillModel>> GetUserSkills(Guid id);

        /// <summary>
        /// Obtem os dados necessários relacionados com o perfil de um utilizador
        /// </summary>
        /// <param name="id">Id do utilizador</param>
        /// <returns>Utilizador</returns>
        public Task<User> GetUserProfile(Guid id);

        Task<UserPreferences> GetUserPreferences(string userId);

        /// <summary>
        /// Apaga toda a informação do utilizador do website
        /// </summary>
        /// <param name="userId">Id do utilizador</param>
        /// <returns>True se apagou com sucesso</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task<bool> DeleteUserData(string userId);

        Task<bool> SetUserPreferences(UserPreferences preferences);

        /// <summary>
        /// Recebe uma habilidade e adiciona-a a um utilizador na base de dados
        /// </summary>
        /// <param name="id">Id do utilizador dono da habilidade</param>
        /// <param name="skill">Habilidade</param>
        /// <returns>Habilidade que foi adicionada na base de dados</returns>
        Task<SkillModel> AddSkill(Guid id, SkillModel skill);
        /// <summary>
        /// Recebe uma skill e atualiza-se na base de dados
        /// </summary>
        /// <param name="skill">Skill do utilizador</param>
        /// <returns>true se foi atualizada com sucesso, falso caso contrário</returns>
        /// <exception cref="Exception">Caso o utilizador não esteja logado é retornada uma exceção</exception>
        Task<bool> EditSkill(SkillModel skill);
        /// <summary>
        /// Recebe um id de uma skill e vai remove-la
        /// </summary>
        /// <param name="id">Id da skill do utilizador logado</param>
        /// <returns>true se removeu com sucesso, false caso contrário</returns>
        Task<bool> DeleteSkill(Guid id);

        /// <summary>
        /// Obtem todas as experiências de um utilizador
        /// </summary>
        /// <param name="id">Id do utilizador a procurar</param>
        /// <returns>Experiências do utilizador</returns>
        Task<List<ExperienceModel>> GetUserExperience(Guid id);
        /// <summary>
        /// Recebe os dados de um experiência e adiciona-a ao utilizador com sessão iniciada na base de dados
        /// </summary>
        /// <param name="experience">Experiência</param>
        /// <returns>Experiência que foi adicionada</returns>
        Task<ExperienceModel> AddExperience(ExperienceModel experience);
        /// <summary>
        /// Recebe uma experiência atualizada e atualiza-a na base de dados
        /// </summary>
        /// <param name="experience">Experiência</param>
        /// <returns>Retorna true se foi atualizada com sucesso, 0 se não foi atualizada</returns>
        Task<bool> EditExperience(ExperienceModel experience);
        /// <summary>
        /// Remove uma experiência com um determinado id
        /// </summary>
        /// <param name="id">Id da experiência</param>
        /// <returns>true se conseguiu remover, false se ocorreu um error ao remover</returns>
        Task<bool> DeleteExperience(Guid id);

        Task<SkillModel> AddEndorsement(Guid sk);

        Task<SkillModel> RemoveEndorsement(Guid id);
        Task<List<User>> GetUsersAsync();

        /// <summary>
        /// Verifica se o utilizador com sessão iniciada está bloqueado por outro
        /// </summary>
        /// <param name="id">Id do outro utilizador</param>
        /// <returns>true se está bloqueado, false caso contrário</returns>
        Task<bool> IsBlockedByUser(Guid id);

        /// <summary>
        /// Verifica se o utilizador´com sessão iniciada já bloqueou o outro utilizador
        /// </summary>
        /// <param name="id">Id do outro utilizador</param>
        /// <returns>true se já bloqueou esses utilizador, false caso contrário</returns>
        Task<bool> IsAlreadyBlocked(Guid id);

        /// <summary>
        /// Verifica se o utilizador´com sessão iniciada é Admin/Mod/Entidade
        /// </summary>
        /// <returns>true se for, false caso contrário</returns>
        public Task<bool> IsCurrentUserModAdminEntity();

        /// <summary>
        /// Altera o idioma de preferencia guardado pelo utilizador
        /// </summary>
        /// <param name="code">Código do idioma (i.e.: Portugal seria pt</param>
        /// <returns>true se alterou com sucesso, false caso contrário</returns>
        public Task<bool> ChangeSavedLanguage(string code);

    }
}
