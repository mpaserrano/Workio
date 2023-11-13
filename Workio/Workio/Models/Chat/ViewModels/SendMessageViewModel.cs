namespace Workio.Models.Chat.ViewModels
{
    /// <summary>
    /// Modelo para ser utilizado para enviar mensagens
    /// </summary>
    public class SendMessageViewModel
    {
        /// <summary>
        /// Id da conversa
        /// </summary>
        public Guid ChatRoomId { get; set; }
        /// <summary>
        /// Conteúdo da mensagem
        /// </summary>
        public string Text { get; set; }
    }
}
