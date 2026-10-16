using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int EnrolmentId { get; set; } // FK to Enrolment

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        public int OverallRank { get; set; }

        [Required]
        public int CategoryRank { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Official"; 
        public DateTime TimeRecorded { get; set; } = DateTime.UtcNow;

        [ForeignKey("EnrolmentId")]
        public Enrolment Enrolment { get; set; } = null!;
    }
}