using Microsoft.EntityFrameworkCore;
using ApartmentManagement.Model;

namespace ApartmentManagement.Data
{
    public class ApartmentDbContext : DbContext
    {
        public ApartmentDbContext(DbContextOptions<ApartmentDbContext> options)
            : base(options)
        {
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
            modelBuilder.Entity<User>().ToTable("users", "PTTK");
            modelBuilder.Entity<Building>().ToTable("buildings", "PTTK");
            modelBuilder.Entity<Apartment>().ToTable("apartments", "PTTK");
            modelBuilder.Entity<Resident>().ToTable("residents", "PTTK");
            modelBuilder.Entity<Bill>().ToTable("bills", "PTTK");
            modelBuilder.Entity<Payment>().ToTable("payments", "PTTK");
            modelBuilder.Entity<ServiceRequest>().ToTable("service_requests", "PTTK");

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
    }
}
