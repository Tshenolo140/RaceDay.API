using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public int EventId { get; set; } //FK

        [Required]
        [MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; } 

        [Required]
        [MaxLength(30)]
        public string AgeGroup { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string GenderSpecific { get; set; } = string.Empty; 

        [ForeignKey("EventId")]
        public Event Event { get; set; } = null!;

        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}