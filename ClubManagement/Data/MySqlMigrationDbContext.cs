
using ClubManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubManagement.Data
{
    public class MySqlMigrationDbContext : DbContext
    {
        public MySqlMigrationDbContext(
            DbContextOptions<MySqlMigrationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Club> Clubs { get; set; } = null!;
        public DbSet<Member> Members { get; set; } = null!;
        public DbSet<ActivityAttendance> ActivityAttendances { get; set; } = null!;
        public DbSet<ClubActivity> Activities { get; set; } = null!;
        public DbSet<Attendance> Attendances { get; set; } = null!;
        public DbSet<FileStorage> FileStorages { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; } = null!;
        public DbSet<Contact> Contacts { get; set; } = null!;
        public DbSet<MemberRegistration> MemberRegistrations { get; set; } = null!;
        public DbSet<ActivityRegistration> ActivityRegistrations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Member>()
                .HasOne(m => m.Club)
                .WithMany(c => c.Members)
                .HasForeignKey(m => m.ClubId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClubActivity>()
                .HasOne(a => a.Club)
                .WithMany(c => c.Activities)
                .HasForeignKey(a => a.ClubId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Member)
                .WithMany(m => m.Attendances)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Activity)
                .WithMany(a => a.Attendances)
                .HasForeignKey(a => a.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);
         
            modelBuilder.Entity<ActivityAttendance>()
                .HasOne(a => a.Activity)
                .WithMany(a => a.ActivityAttendances)
                .HasForeignKey(a => a.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<ActivityRegistration>()
                .HasOne(a => a.Activity)
                .WithMany()
                .HasForeignKey(a => a.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);
           
            modelBuilder.Entity<FileStorage>()
                .HasOne(f => f.Activity)
                .WithMany()
                .HasForeignKey(f => f.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
