using Microsoft.EntityFrameworkCore;
using ApartmentManagement.Model;
using System.Data.Common;
using ApartmentManagement.Data;
namespace ApartmentManagement.Data
{
    public class ApartmentDbContext : DbContext
    {
        private readonly DbConnection _dbConnection;
        public ApartmentDbContext(DbConnection dbConnection, DbContextOptions<ApartmentDbContext> options)
             : base(options)
        {
            _dbConnection = dbConnection;
        }

        // DbSet tương ứng với các bảng
        public DbSet<User> Users { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<Resident> Residents { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapping table + schema "PTTK"
            modelBuilder.Entity<User>().ToTable("users", "pttk");
            modelBuilder.Entity<Building>().ToTable("buildings", "pttk");
            modelBuilder.Entity<Apartment>().ToTable("apartments", "pttk");
            modelBuilder.Entity<Resident>().ToTable("residents", "pttk");
            modelBuilder.Entity<Bill>().ToTable("bills", "pttk");
            modelBuilder.Entity<Payment>().ToTable("payments", "pttk");
            modelBuilder.Entity<ServiceRequest>().ToTable("service_requests", "pttk");

            // Quan hệ Resident -> Resident (owner)
            modelBuilder.Entity<Resident>()
                .HasMany(r => r.members)
                .WithOne(r => r.owner)
                .HasForeignKey(r => r.owner_id)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ Apartment -> Resident (owner)
            modelBuilder.Entity<Apartment>()
                .HasOne(a => a.owner)
                .WithMany()
                .HasForeignKey(a => a.owner_id)
                .OnDelete(DeleteBehavior.SetNull);

            // Quan hệ Building -> User (manager)
            modelBuilder.Entity<Building>()
                .HasOne(b => b.manager)
                .WithMany(u => u.buildings_managed)
                .HasForeignKey(b => b.manager_id)
                .OnDelete(DeleteBehavior.SetNull);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrEmpty(_dbConnection.ConnectionString))
            {
                optionsBuilder.UseNpgsql(_dbConnection.ConnectionString); 
            }
            else
            {
                throw new InvalidOperationException("Connection string is null or empty.");
            }
        }
    }
}
