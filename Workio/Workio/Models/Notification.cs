using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace Workio.Models
{
    /// <summary>
    /// Modelo de uma notificação que é enviada para o utilizador
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Id da notificação
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Mensagem da notificação
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Link dos detalhes (ver mais)
        /// </summary>
        public string URL { get; set; }
        /// <summary>
        /// Identificador de se a mensagem já foi ou não lida
        /// </summary>
        public bool IsRead { get; set; } = false;
        /// <summary>
        /// Id do user a que esta notificação corresponde
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// Objeto do utilizador (destinatário da notificação)
        /// </summary>
        public User User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsValid()
        {
            if(string.IsNullOrWhiteSpace(Text) || string.IsNullOrEmpty(Text)) return false;
            if (User == null || UserId == Guid.Empty.ToString() || User.Id != UserId) return false;

            return true;

        }
    }
}
