using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

        public DbSet<Organiser> Organisers { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Unique Constraints
            modelBuilder.Entity<Organiser>().HasIndex(o => o.Email).IsUnique();
            modelBuilder.Entity<Participant>().HasIndex(p => p.Email).IsUnique();
            modelBuilder.Entity<Enrolment>().HasIndex(e => e.BibNumber).IsUnique();
            modelBuilder.Entity<Enrolment>().HasIndex(e => new { e.EventId, e.ParticipantId }).IsUnique();
            modelBuilder.Entity<Result>().HasIndex(r => r.EnrolmentId).IsUnique();

            //Check Constraints
            modelBuilder.Entity<Event>().ToTable(t => t.HasCheckConstraint("CK_Event_MaxParticipants", "[MaxParticipants] > 0"));
            modelBuilder.Entity<Event>().ToTable(t => t.HasCheckConstraint("CK_Event_Status", "[Status] IN ('Upcoming', 'Ongoing', 'Completed', 'Cancelled')"));
            modelBuilder.Entity<Participant>().ToTable(t => t.HasCheckConstraint("CK_Participant_Gender", "[Gender] IN ('Male', 'Female', 'Other')"));
            modelBuilder.Entity<Enrolment>().ToTable(t => t.HasCheckConstraint("CK_Enrolment_PaymentStatus", "[PaymentStatus] IN ('Pending', 'Completed', 'Refunded')"));
            modelBuilder.Entity<Result>().ToTable(t => t.HasCheckConstraint("CK_Result_Status", "[Status] IN ('Official', 'Disqualified', 'Did Not Finish', 'Pending')"));

            // --- Relationships & Cascade Deletes ---

            // Event -> Organiser (Cascade)
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Category -> Event (Cascade)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enrolment -> Event (Cascade)
            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Event)
                .WithMany() 
                .HasForeignKey(en => en.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enrolment -> Participant (Cascade)
            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Participant)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(en => en.ParticipantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enrolment -> Category 
            modelBuilder.Entity<Enrolment>()
                .HasOne(en => en.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(en => en.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Result -> Enrolment (Cascade)
            modelBuilder.Entity<Result>()
                .HasOne(r => r.Enrolment)
                .WithOne(en => en.Result)
                .HasForeignKey<Result>(r => r.EnrolmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}