using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentId { get; set; }

        [Required]
        public int EventId { get; set; } // FK to Event

        [Required]
        public int ParticipantId { get; set; } // FK to Participant

        [Required]
        public int CategoryId { get; set; } // FK to Category

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; /

        [Column(TypeName = "decimal(10,2)")]
        public decimal AmountPaid { get; set; } = 0.00m;

        public int? BibNumber { get; set; } 

        [ForeignKey("EventId")]
        public Event Event { get; set; } = null!;

        [ForeignKey("ParticipantId")]
        public Participant Participant { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;

        //One-to-One with Result
        public Result? Result { get; set; }
    }
}