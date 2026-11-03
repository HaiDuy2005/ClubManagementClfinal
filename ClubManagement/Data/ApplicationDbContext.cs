using ClubManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ClubManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Club> Clubs { get; set; }

        public DbSet<Member> Members { get; set; }
        public DbSet<ActivityAttendance> ActivityAttendances { get; set; }

        public DbSet<ClubActivity> Activities { get; set; }

        public DbSet<Attendance> Attendances { get; set; }

        public DbSet<FileStorage> FileStorages { get; set; }

        public DbSet<Admin> Admins { get; set; }

        public DbSet<Contact> Contacts { get; set; }

        public DbSet<MemberRegistration> MemberRegistrations { get; set; }

        public DbSet<ActivityRegistration> ActivityRegistrations { get; set; }
    }
}