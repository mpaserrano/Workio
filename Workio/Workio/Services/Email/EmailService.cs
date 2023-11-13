using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Workio.Configurations;
using Workio.Models;
using Workio.Services.Email.Interfaces;
using Workio.Services.Interfaces;
using Workio.Services.Teams;

namespace Workio.Services.Email
{
    /// <summary>
    /// Gerer o envio de emails.
    /// </summary>
    public class EmailService : IEmailService
    {
        /// <summary>
        /// Configuração do serviço de email.
        /// </summary>
        EmailSettings _emailSettings = null;
        private readonly CommonLocalizationService _localizationService;

        /// <summary>
        /// Inicializa as configurações do serviço de email.
        /// </summary>
        /// <param name="options">Configurações do serviço de email.</param>
        public EmailService(IOptions<EmailSettings> options, CommonLocalizationService localizationService)
        {
            _emailSettings = options.Value;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Envia o email um email de acordo com o conteúdo recebido.
        /// </summary>
        /// <param name="emailData">Conteúdo do email.</param>
        /// <returns>True se bem sucedido. False se mal sucedido.</returns>
        public bool SendEmail(EmailData emailData)
        {
            try
            {
                MimeMessage emailMessage = new MimeMessage();

                MailboxAddress emailFrom = new MailboxAddress(_emailSettings.Name, _emailSettings.EmailId);
                emailMessage.From.Add(emailFrom);

                MailboxAddress emailTo = new MailboxAddress(emailData.EmailToName, emailData.EmailToId);
                emailMessage.To.Add(emailTo);

                emailMessage.Subject = emailData.EmailSubject;

                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.TextBody = emailData.EmailBody;
                emailMessage.Body = emailBodyBuilder.ToMessageBody();

                SmtpClient emailClient = new SmtpClient();
                emailClient.Connect(_emailSettings.Host, _emailSettings.Port, _emailSettings.UseSSL);
                emailClient.Authenticate(_emailSettings.EmailId, _emailSettings.Password);
                emailClient.Send(emailMessage);
                emailClient.Disconnect(true);
                emailClient.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                return false;
            }
        }

        /// <summary>
        /// Envia um email para a recuperação da password de um utilizador.
        /// </summary>
        /// <param name="user">Utilizador que vai recuperar a password.</param>
        /// <param name="link">Link com o token para a recuperação da password.</param>
        /// <returns>True se bem sucedido. False se mal sucedido.</returns>
        public bool SendRecoverPasswordEmail(User user, string link)
        {
            try
            {
                MimeMessage emailMessage = new MimeMessage();
                MailboxAddress emailFrom = new MailboxAddress(_emailSettings.Name, _emailSettings.EmailId);
                emailMessage.From.Add(emailFrom);
                MailboxAddress emailTo = new MailboxAddress(user.Name, user.Email);
                emailMessage.To.Add(emailTo);
                emailMessage.Subject = _localizationService.Get("Recover Password");
                string languageCode = Thread.CurrentThread.CurrentCulture.Name;
                if(languageCode == null || string.IsNullOrEmpty(languageCode))
                    languageCode = "en";
                string templateFileName = $"RecoverPassword.{languageCode}.html";
                string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", templateFileName);
                string EmailTemplateText = File.ReadAllText(FilePath);
                EmailTemplateText = string.Format(EmailTemplateText, user.Name, link);
                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = EmailTemplateText;
                emailMessage.Body = emailBodyBuilder.ToMessageBody();
                SmtpClient emailClient = new SmtpClient();
                emailClient.Connect(_emailSettings.Host, _emailSettings.Port, _emailSettings.UseSSL);
                emailClient.Authenticate(_emailSettings.EmailId, _emailSettings.Password);
                emailClient.Send(emailMessage);
                emailClient.Disconnect(true);
                emailClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                return false;
            }
        }

        /// <summary>
        /// Envia um email a um utilizador para confirmar o seu email.
        /// </summary>
        /// <param name="email">Email para onde enviar.</param>
        /// <param name="link">Link com o token para a validação do email.</param>
        /// <returns>True se bem sucedido. False se mal sucedido.</returns>
        public bool ConfirmEmail(string email, string link)
        {
            try
            {
                MimeMessage emailMessage = new MimeMessage();
                MailboxAddress emailFrom = new MailboxAddress(_emailSettings.Name, _emailSettings.EmailId);
                emailMessage.From.Add(emailFrom);
                MailboxAddress emailTo = new MailboxAddress(email, email);
                emailMessage.To.Add(emailTo);
                emailMessage.Subject = _localizationService.Get("Confirm Email");
                string languageCode = Thread.CurrentThread.CurrentCulture.Name;
                if (languageCode == null || string.IsNullOrEmpty(languageCode))
                    languageCode = "en";
                string templateFileName = $"ConfirmEmail.{languageCode}.html";
                string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", templateFileName);
                string EmailTemplateText = File.ReadAllText(FilePath);
                EmailTemplateText = string.Format(EmailTemplateText, link);
                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = EmailTemplateText;
                emailMessage.Body = emailBodyBuilder.ToMessageBody();
                SmtpClient emailClient = new SmtpClient();
                emailClient.Connect(_emailSettings.Host, _emailSettings.Port, _emailSettings.UseSSL);
                emailClient.Authenticate(_emailSettings.EmailId, _emailSettings.Password);
                emailClient.Send(emailMessage);
                emailClient.Disconnect(true);
                emailClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Envia um email a um utilizador para alterar o seu email atual.
        /// </summary>
        /// <param name="email">Email para quem enviar o conteúdo.</param>
        /// <param name="link">Link com o token para confirma a alteração de email.</param>
        /// <returns>True se bem sucedido. False se mal sucedido.</returns>
        public bool ChangeEmail(string email, string link)
        {
            try
            {
                MimeMessage emailMessage = new MimeMessage();
                MailboxAddress emailFrom = new MailboxAddress(_emailSettings.Name, _emailSettings.EmailId);
                emailMessage.From.Add(emailFrom);
                MailboxAddress emailTo = new MailboxAddress(email, email);
                emailMessage.To.Add(emailTo);
                emailMessage.Subject = _localizationService.Get("Change Email");
                string languageCode = Thread.CurrentThread.CurrentCulture.Name;
                if (languageCode == null || string.IsNullOrEmpty(languageCode))
                    languageCode = "en";
                string templateFileName = $"ChangeEmail.{languageCode}.html";
                string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", templateFileName);
                string EmailTemplateText = File.ReadAllText(FilePath);
                EmailTemplateText = string.Format(EmailTemplateText, link);
                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = EmailTemplateText;
                emailMessage.Body = emailBodyBuilder.ToMessageBody();
                SmtpClient emailClient = new SmtpClient();
                emailClient.Connect(_emailSettings.Host, _emailSettings.Port, _emailSettings.UseSSL);
                emailClient.Authenticate(_emailSettings.EmailId, _emailSettings.Password);
                emailClient.Send(emailMessage);
                emailClient.Disconnect(true);
                emailClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Envia um email ao proprietário de um equipa quando recebe uma nova solicitação de adesão.
        /// </summary>
        /// <param name="ownerUser">Objeto User que é o proprietário da equipa.</param>
        /// <param name="requestedTeam">Objeto Team que é a equipa que foi solicitada.</param>
        /// <param name="link">Link a incluir de referencia há equipa no email.</param>
        /// <returns>True se bem sucedido. False se mal sucedido.</returns>
        public async Task<bool> SendNewMemberTeamRequestEmail(User ownerUser, Team requestedTeam, string link)
        {
            try
            {
                string ownerName = ownerUser.Name;
                string ownerEmail = ownerUser.Email;
                string teamName = requestedTeam.TeamName;

                //From
                MimeMessage emailMessage = new MimeMessage();
                MailboxAddress emailFrom = new MailboxAddress(_emailSettings.Name, _emailSettings.EmailId);
                emailMessage.From.Add(emailFrom);

                //To
                MailboxAddress emailTo = new MailboxAddress(ownerEmail, ownerEmail);
                emailMessage.To.Add(emailTo);

                //Content
                emailMessage.Subject = "New request to join your team";

                string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\NewMemberTeamRequestEmailTemplate.html";
                string EmailTemplateText = File.ReadAllText(FilePath);

                EmailTemplateText = string.Format(EmailTemplateText, ownerName, teamName, link);

                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = EmailTemplateText;
                emailMessage.Body = emailBodyBuilder.ToMessageBody();
                SmtpClient emailClient = new SmtpClient();
                emailClient.Connect(_emailSettings.Host, _emailSettings.Port, _emailSettings.UseSSL);
                emailClient.Authenticate(_emailSettings.EmailId, _emailSettings.Password);
                emailClient.Send(emailMessage);
                emailClient.Disconnect(true);
                emailClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Envia email ao utilizador aceite em uma equipa.
        /// </summary>
        /// <param name="requesterUser">Utilizador que solicitou a entrada numa equipa.</param>
        /// <param name="requestedTeam">Equipa solicitada para entrar.</param>
        /// <param name="link">Url para a equipa a incluir no email.</param>
        /// <returns>True: Se o email for enviado com sucesso. False: Se for mal sucedido.</returns>
        public async Task<bool> SendAcceptedInTeamEmail(User requesterUser, Team requestedTeam, string link)
        {
            try
            {
                string requesterName = requesterUser.Name;
                string requesterEmail = requesterUser.Email;
                string teamName = requestedTeam.TeamName;

                //From
                MimeMessage emailMessage = new MimeMessage();
                MailboxAddress emailFrom = new MailboxAddress(_emailSettings.Name, _emailSettings.EmailId);
                emailMessage.From.Add(emailFrom);

                //To
                MailboxAddress emailTo = new MailboxAddress(requesterEmail, requesterEmail);
                emailMessage.To.Add(emailTo);

                //Content
                emailMessage.Subject = "Your request was accepted";

                string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\AcceptedInTeamEmailTemplate.html";
                string EmailTemplateText = File.ReadAllText(FilePath);

                EmailTemplateText = string.Format(EmailTemplateText, requesterName, teamName, link);

                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = EmailTemplateText;
                emailMessage.Body = emailBodyBuilder.ToMessageBody();
                SmtpClient emailClient = new SmtpClient();
                emailClient.Connect(_emailSettings.Host, _emailSettings.Port, _emailSettings.UseSSL);
                emailClient.Authenticate(_emailSettings.EmailId, _emailSettings.Password);
                emailClient.Send(emailMessage);
                emailClient.Disconnect(true);
                emailClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Envia um email ao user quando é banido.
        /// </summary>
        /// <param name="bannedUser">Objeto do utilizador banido</param>
        /// <returns>True: Se o email for enviado com sucesso. False: Se for mal sucedido.</returns>
        public async Task<bool> SendBanUserEmail(User bannedUser)
        {
            try
            {
                string bannedUserName = bannedUser.Name;
                string bannedUserEmail = bannedUser.Email;
                string languageCode = bannedUser.Language.Code;

                //From
                MimeMessage emailMessage = new MimeMessage();
                MailboxAddress emailFrom = new MailboxAddress(_emailSettings.Name, _emailSettings.EmailId);
                emailMessage.From.Add(emailFrom);

                //To
                MailboxAddress emailTo = new MailboxAddress(bannedUserEmail, bannedUserEmail);
                emailMessage.To.Add(emailTo);

                //Content
                emailMessage.Subject = _localizationService.GetLocalizedString("Account Suspended", languageCode);

                string templateFileName = $"BanUserEmailTemplate.{languageCode}.html";
                string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", templateFileName);
                string EmailTemplateText = File.ReadAllText(FilePath);

                EmailTemplateText = string.Format(EmailTemplateText, bannedUserName);

                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = EmailTemplateText;
                emailMessage.Body = emailBodyBuilder.ToMessageBody();
                SmtpClient emailClient = new SmtpClient();
                emailClient.Connect(_emailSettings.Host, _emailSettings.Port, _emailSettings.UseSSL);
                emailClient.Authenticate(_emailSettings.EmailId, _emailSettings.Password);
                emailClient.Send(emailMessage);
                emailClient.Disconnect(true);
                emailClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Envia um email ao user quando a suspenção da conta é removida.
        /// </summary>
        /// <param name="bannedUser">Objeto do utilizador que deixou de estar banido.</param>
        /// <returns>True: Se o email for enviado com sucesso. False: Se for mal sucedido.</returns>
        public async Task<bool> SendUnbanUserEmail(User unbannedUser)
        {
            try
            {
                string unbannedUserName = unbannedUser.Name;
                string unbannedUserEmail = unbannedUser.Email;
                string languageCode = unbannedUser.Language.Code;

                //From
                MimeMessage emailMessage = new MimeMessage();
                MailboxAddress emailFrom = new MailboxAddress(_emailSettings.Name, _emailSettings.EmailId);
                emailMessage.From.Add(emailFrom);

                //To
                MailboxAddress emailTo = new MailboxAddress(unbannedUserEmail, unbannedUserEmail);
                emailMessage.To.Add(emailTo);

                //Content
                emailMessage.Subject = _localizationService.GetLocalizedString("Remove Ban Account", unbannedUser.Language.Code);
                string templateFileName = $"UnbanUserEmailTemplate.{languageCode}.html";
                string FilePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", templateFileName);
                string EmailTemplateText = File.ReadAllText(FilePath);

                EmailTemplateText = string.Format(EmailTemplateText, unbannedUserName);

                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = EmailTemplateText;
                emailMessage.Body = emailBodyBuilder.ToMessageBody();
                SmtpClient emailClient = new SmtpClient();
                emailClient.Connect(_emailSettings.Host, _emailSettings.Port, _emailSettings.UseSSL);
                emailClient.Authenticate(_emailSettings.EmailId, _emailSettings.Password);
                emailClient.Send(emailMessage);
                emailClient.Disconnect(true);
                emailClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Envia uma notificação por email
        /// </summary>
        /// <param name="notification">Notificação a enviar</param>
        /// <returns>true caso o email tenha sido enviado, false caso contrário</returns>
        public async Task<bool> SendNotificationEmail(Notification notification)
        {
            try
            {
                string requesterName = notification.User.Name;
                string requesterEmail = notification.User.Email;

                //From
                MimeMessage emailMessage = new MimeMessage();
                MailboxAddress emailFrom = new MailboxAddress(_emailSettings.Name, _emailSettings.EmailId);
                emailMessage.From.Add(emailFrom);

                //To
                MailboxAddress emailTo = new MailboxAddress(requesterEmail, requesterEmail);
                emailMessage.To.Add(emailTo);

                //Content
                emailMessage.Subject = _localizationService.GetLocalizedString("New Notification", notification.User.Language.Code);

                string FilePath = Directory.GetCurrentDirectory() + "\\Templates\\NotificationEmailTemplate.html";
                string EmailTemplateText = File.ReadAllText(FilePath);

                string textMail = $"{_localizationService.GetLocalizedString("Hi", notification.User.Language.Code)} <b>{notification.User.Name}</b>, {notification.Text} {_localizationService.GetLocalizedString("Click", notification.User.Language.Code)}: <a href='{notification.URL}'>{_localizationService.GetLocalizedString("_here", notification.User.Language.Code)}</a> {_localizationService.GetLocalizedString("toSeeMore", notification.User.Language.Code)}";

                EmailTemplateText = string.Format(EmailTemplateText, textMail);

                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.HtmlBody = EmailTemplateText;
                emailMessage.Body = emailBodyBuilder.ToMessageBody();
                SmtpClient emailClient = new SmtpClient();
                emailClient.Connect(_emailSettings.Host, _emailSettings.Port, _emailSettings.UseSSL);
                emailClient.Authenticate(_emailSettings.EmailId, _emailSettings.Password);
                emailClient.Send(emailMessage);
                emailClient.Disconnect(true);
                emailClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                //Log Exception Details
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
