using DeviceManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Data
{
    public class DeviceContext(DbContextOptions<DeviceContext> options) : IdentityDbContext(options)
    {
        public DbSet<Device> Devices { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<DeviceType> DeviceTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Device>()
                .HasOne(d => d.Technician)
                .WithMany(t => t.Devices)
                .HasForeignKey(d => d.TechnicianId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DeviceType>()
                .HasData(
                    new DeviceType { Id = 1, Name = "Laptop" },
                    new DeviceType { Id = 2, Name = "Desktop" },
                    new DeviceType { Id = 3, Name = "Tablet" },
                    new DeviceType { Id = 4, Name = "Smartphone" }
                );
        }
    }
}
