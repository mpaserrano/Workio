using Workio.Models;

namespace Workio.Services.Email.Interfaces
{
    /// <summary>
    /// Gerer o envio de emails.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Envia o email um email de acordo com o conteúdo recebido.
        /// </summary>
        /// <param name="emailData">Conteúdo do email.</param>
        /// <returns>True se bem sucedido. False se mal sucedido.</returns>
        bool SendEmail(EmailData emailData);

        /// <summary>
        /// Envia um email para a recuperação da password de um utilizador.
        /// </summary>
        /// <param name="user">Utilizador que vai recuperar a password.</param>
        /// <param name="link">Link com o token para a recuperação da password.</param>
        /// <returns>True se bem sucedido. False se mal sucedido.</returns>
        bool SendRecoverPasswordEmail(User user, string link);

        /// <summary>
        /// Envia um email a um utilizador para confirmar o seu email.
        /// </summary>
        /// <param name="email">Email para onde enviar.</param>
        /// <param name="link">Link com o token para a validação do email.</param>
        /// <returns>True se bem sucedido. False se mal sucedido.</returns>
        bool ConfirmEmail(string email, string link);

        /// <summary>
        /// Envia um email a um utilizador para alterar o seu email atual.
        /// </summary>
        /// <param name="email">Email para quem enviar o conteúdo.</param>
        /// <param name="link">Link com o token para confirma a alteração de email.</param>
        /// <returns>True se bem sucedido. False se mal sucedido.</returns>
        bool ChangeEmail(string email, string link);

        /// <summary>
        /// Envia um email ao proprietário de um equipa quando recebe uma nova solicitação de adesão.
        /// </summary>
        /// <param name="ownerUser">Objeto User que é o proprietário da equipa.</param>
        /// <param name="requestedTeam">Objeto Team que é a equipa que foi solicitada.</param>
        /// <param name="link">Link a incluir de referencia há equipa no email.</param>
        /// <returns>True se bem sucedido. False se mal sucedido.</returns>
        Task<bool> SendNewMemberTeamRequestEmail(User ownerUser, Team requestedTeam, string link);

        /// <summary>
        /// Envia email ao utilizador aceite em uma equipa.
        /// </summary>
        /// <param name="requesterUser">Utilizador que solicitou a entrada numa equipa.</param>
        /// <param name="requestedTeam">Equipa solicitada para entrar.</param>
        /// <param name="link">Url para a equipa a incluir no email.</param>
        /// <returns>True: Se o email for enviado com sucesso. False: Se for mal sucedido.</returns>
        Task<bool> SendAcceptedInTeamEmail(User requesterUser, Team requestedTeam, string link);

        /// <summary>
        /// Envia um email ao user quando é banido.
        /// </summary>
        /// <param name="bannedUser">Objeto do utilizador banido</param>
        /// <returns>True: Se o email for enviado com sucesso. False: Se for mal sucedido.</returns>
        Task<bool> SendBanUserEmail(User bannedUser);

        /// <summary>
        /// Envia um email ao user quando a suspenção da conta é removida.
        /// </summary>
        /// <param name="bannedUser">Objeto do utilizador que deixou de estar banido.</param>
        /// <returns>True: Se o email for enviado com sucesso. False: Se for mal sucedido.</returns>
        Task<bool> SendUnbanUserEmail(User unbannedUser);
        /// <summary>
        /// Envia uma notificação por email
        /// </summary>
        /// <param name="notification">Notificação a enviar</param>
        /// <returns>true caso o email tenha sido enviado, false caso contrário</returns>
        public Task<bool> SendNotificationEmail(Notification notification);
    }
}
