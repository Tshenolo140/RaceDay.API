using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TableName { get; set; } = string.Empty;

        [Required]
        public int RecordId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Action { get; set; } = string.Empty; 

        [Required]
        [MaxLength(100)]
        public string ChangedBy { get; set; } = string.Empty;

        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;

       
        public string? OldData { get; set; }

        public string? NewData { get; set; }
    }
}