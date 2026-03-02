using LeaveManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveAudit> LeaveAudits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<LeaveAudit>().ToTable("LeaveAudits");

            // Configure LeaveRequest entity
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.NoOfDays)
                    .HasPrecision(5, 2); // Max 999.99 days

                entity.Property(e => e.Reason)
                    .HasMaxLength(500);

                // Configure relationship: LeaveRequest has many LeaveAudits
                entity.HasMany(e => e.LeaveAudits)
                    .WithOne()
                    .HasForeignKey(a => a.LeaveRequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure LeaveAudit entity
            modelBuilder.Entity<LeaveAudit>(entity =>
            {
                entity.HasKey(e => e.AuditId);
                entity.Property(e => e.AuditId)
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.LeaveRequestId)
                    .IsRequired();
                entity.Property(e => e.Comments)
                    .HasMaxLength(500);
                entity.Property(e => e.Action);
            });
        }
    }
}
