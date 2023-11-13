using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Workio.Models
{
    public class RequestEntityStatus
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Id")]
        public Guid? UserId { get; set; }

        [Display(Name = "Reason")]
        [Required(ErrorMessage = "É necessário preencher o motivo.")]
        [MaxLength(124)]
        public string Motivation { get; set; }
        [Display(Name = "File")]
        [Required(ErrorMessage = "Apenas são aceites ficheiros em formato.pdf!")]
        public string FilePath { get; set; }

        public string? OriginalFileName { get; set; }

        public string? AlteredFileName { get; set; }

        public RequestState RequestState { get; set; } = RequestState.Pending;

        public DateTime RequestDate { get; set; } = DateTime.Now;
        [NotMapped]
        public IFormFile File { get; set; }
    }

    public enum RequestState
    {
        Pending, Approved, Rejected
    }
}

