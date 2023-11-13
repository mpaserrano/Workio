using System.ComponentModel.DataAnnotations;

namespace Workio.Models.Admin.Logs
{
    /// <summary>
    /// Representa os atributos basicos e essenciaís para um log.
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Id do registo do log
        /// </summary>
        [Key]
        public Guid LogId { get; set; }

        /// <summary>
        /// Data e hora do momento em que a ação foi realizada
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
