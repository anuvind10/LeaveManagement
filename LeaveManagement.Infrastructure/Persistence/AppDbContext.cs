using LeaveManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<Approval> Approvals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure LeaveRequest entity
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.NoOfDays)
                    .HasPrecision(5, 2); // Max 999.99 days

                entity.Property(e => e.Reason)
                    .HasMaxLength(500);

                // Configure relationship: LeaveRequest has many Approvals
                entity.HasMany(e => e.Approvals)
                    .WithOne()
                    .HasForeignKey(a => a.LeaveRequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Approval entity
            modelBuilder.Entity<Approval>(entity =>
            {
                entity.HasKey(e => e.ApprovalId);
                entity.Property(e => e.LeaveRequestId)
                    .IsRequired();
                entity.Property(e => e.Comments)
                    .HasMaxLength(500);
            });
        }
    }
}
