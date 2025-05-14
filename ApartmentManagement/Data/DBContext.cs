using Microsoft.EntityFrameworkCore;
using ApartmentManagement.Model;
using System.Data.Common;
using ApartmentManagement.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ApartmentManagement.Core.Singleton;

namespace ApartmentManagement.Data
{
    public class ApartmentDbContext : DbContext
    {
        private readonly DbConnection _dbConnection;
        internal readonly string _schema;
        // Constructor với schema được truyền vào
        public ApartmentDbContext(DbConnection dbConnection, DbContextOptions<ApartmentDbContext> options, string schema)
            : base(options)
        {
            _schema = schema;
            _dbConnection = dbConnection;
        }

        // Các DbSet cho các thực thể
        public DbSet<User> Users { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<Resident> Residents { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        // Cấu hình các thực thể trong OnModelCreating
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Áp dụng schema vào bảng
            modelBuilder.Entity<User>().ToTable("users", _schema);
            modelBuilder.Entity<Building>().ToTable("buildings", _schema);
            modelBuilder.Entity<Apartment>().ToTable("apartments", _schema);
            modelBuilder.Entity<Resident>().ToTable("residents", _schema);
            modelBuilder.Entity<Bill>().ToTable("bills", _schema);
            modelBuilder.Entity<Payment>().ToTable("payments", _schema);
            modelBuilder.Entity<ServiceRequest>().ToTable("service_requests", _schema);
            modelBuilder.Entity<PaymentDetail>().ToTable("paymentsdetail", _schema);  // Thêm bảng PaymentDetail

            // Quan hệ giữa các thực thể
            modelBuilder.Entity<Resident>()
                .HasMany(r => r.members)
                .WithOne(r => r.owner)
                .HasForeignKey(r => r.owner_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Apartment>()
                .HasOne(a => a.owner)
                .WithMany()
                .HasForeignKey(a => a.owner_id)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Building>()
                .HasOne(b => b.manager)
                .WithMany(u => u.buildings_managed)
                .HasForeignKey(b => b.manager_id)
                .OnDelete(DeleteBehavior.SetNull);

            // Quan hệ giữa Payment và PaymentDetail
            modelBuilder.Entity<PaymentDetail>()
                .HasKey(pd => new { pd.bill_id, pd.payment_id });  // Khoá chính là sự kết hợp giữa bill_id và payment_id

            modelBuilder.Entity<PaymentDetail>()
                .HasOne(pd => pd.bill)  // Liên kết với bảng Bill
                .WithMany(b => b.payment_details)
                .HasForeignKey(pd => pd.bill_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentDetail>()
                .HasOne(pd => pd.payment)  // Liên kết với bảng Payment
                .WithMany(p => p.payment_details)
                .HasForeignKey(pd => pd.payment_id)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }


        // Cấu hình kết nối đến cơ sở dữ liệu
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

        // Override phương thức SaveChanges để thêm thông tin thời gian
        public override int SaveChanges()
        {
            foreach (var entry in ChangeTracker.Entries<Apartment>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.created_at = DateTime.UtcNow;
                    entry.Entity.updated_at = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.updated_at = DateTime.UtcNow;
                }
            }

            return base.SaveChanges();
        }
    }
}
