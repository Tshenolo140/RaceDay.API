using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs
{
    public class CreateEventDto
    {
        [Required] public string EventName { get; set; } = string.Empty;
        [Required] public DateTime EventDate { get; set; }
        [Required] public string Location { get; set; } = string.Empty;
        [Required] public int MaxParticipants { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required] public int EventId { get; set; }
        [Required] public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required] public string AgeGroup { get; set; } = string.Empty;
        [Required] public string GenderSpecific { get; set; } = string.Empty; // Male, Female, Open
    }

    public class CreateEnrolmentDto
    {
        [Required] public int EventId { get; set; }
        [Required] public int CategoryId { get; set; }
    }

    public class CreateResultDto
    {
        [Required] public int EnrolmentId { get; set; }
        [Required] public TimeSpan FinishTime { get; set; }
        [Required] public int OverallRank { get; set; }
        [Required] public int CategoryRank { get; set; }
        [Required] public string Status { get; set; } = "Official";
    }

    public class UpdateProfileDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Organisation { get; set; }
        public string? EmergencyContact { get; set; }
        public string? EmergencyPhone { get; set; }
    }
}