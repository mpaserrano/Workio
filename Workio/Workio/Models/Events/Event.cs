using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Workio.Models.Events
{
    /// <summary>
    /// Contem as propriedades de um evento
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Id do evento
        /// </summary>
        [Key]
        public Guid EventId { get; set; }

        /// <summary>
        /// Id do utilizador que criou o evento
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Objeto do utilizador que publicou o evento
        /// </summary>
        [ForeignKey("UserId")]
        public User? UserPublisher { get; set; }

        /// <summary>
        /// Titulo do evento
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        [MaxLength(64)]
        public string Title { get; set; }

        /// <summary>
        /// Guarda o nome da imagem do evento
        /// </summary>
        [DataType(DataType.Text)]
        public string? BannerPicturePath { get; set; }

        /// <summary>
        /// Recebe o ficheiro que será a imagem a guardar
        /// </summary>
        [NotMapped]
        public IFormFile? BannerPicturePathFile { get; set; }

        /// <summary>
        /// Descrição do evento
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        [MaxLength(124)]
        public string Description { get; set; }

        /// <summary>
        /// Texto pleno com as tags do evento
        /// </summary>
        [NotMapped]
        [MaxLength(200)]
        public string? Tags{ get; set; }

        /// <summary>
        /// Data de criação do evento que por padrão é registado o tempo atual
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data de início do evento
        /// </summary>
        [DataType(DataType.Date)]
        [CheckDateRange]
        public DateTime StartDate{ get; set; }

        /// <summary>
        /// Data de fim do evento
        /// </summary>
        [DataType(DataType.Date)]
        [IsDateAfterAttribute("StartDate", allowEqualDates: true)]
        public DateTime EndDate{ get; set; }

        /// <summary>
        /// Estado do evento relativamente a se está aberto, a decorrer ou já terminou.
        /// Por padrão é defenido como aberto na criação do evento.
        /// </summary>
        public EventState State { get; set; } = EventState.Open;
        /// <summary>
        /// Url ou website de origem do evento.
        /// </summary>
        [DataType(DataType.Text)]
        [RequiredIf("IsInPerson", false)]
        public string? Url { get; set; }

        /// <summary>
        /// Tem o aspeto
        /// </summary>
        public bool IsFeatured { get; set; }

        /// <summary>
        /// Indica se o evento ocorre num local fisíco
        /// </summary>
        public bool IsInPerson { get; set; }

        /// <summary>
        /// Indica se o evento ocorre num local fisíco
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        public bool IsBanned { get; set; } = false;

        /// <summary>
        /// Latitude da localização do evento fisíco
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Latitude em texto usada na view
        /// </summary>
        [RequiredIf("IsInPerson", true)]
        [NotMapped]
        public string? LatitudeText { set; get; }

        /// <summary>
        /// Longitude da localização do evento fisíco
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Longitude em texto utilizada na view
        /// </summary>
        [NotMapped]
        [RequiredIf("IsInPerson", true)]
        public string? LongitudeText { get; set; }

        /// <summary>
        /// Address of the event
        /// </summary>
        [RequiredIf("IsInPerson", true)]
        public string? Address { get; set; }

        /// <summary>
        /// Guarda as tags de um evento
        /// </summary>
        public virtual ICollection<EventTag> EventTags { get; set; } = new List<EventTag>();
        /// <summary>
        /// Guarda as reações a um evento
        /// </summary>
        public virtual ICollection<EventReactions> EventReactions { get; set; } = new List<EventReactions>();
        public virtual ICollection<InterestedTeam> InterestedTeams { get; set; } = new List<InterestedTeam>();
        public virtual ICollection<InterestedUser> InterestedUsers { get; set;} = new List<InterestedUser>();

    }



    /// <summary>
    /// Representa o estado de um evento
    /// </summary>
    public enum EventState
    {
        [Display(Name = "Open")]
        Open,
        [Display(Name = "On going")]
        OnGoing,
        [Display(Name = "Finish")]
        Finish
    }


}
