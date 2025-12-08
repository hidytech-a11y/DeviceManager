using DeviceManager.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Data
{
    // Inherit IdentityDbContext so Identity tables store in the same DB
    public class DeviceContext : IdentityDbContext<Microsoft.AspNetCore.Identity.IdentityUser>
    {
        public DeviceContext(DbContextOptions<DeviceContext> options)
            : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<DeviceType> DeviceTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // IMPORTANT - Identity needs this

            modelBuilder.Entity<Device>()
                .HasOne(d => d.Technician)
                .WithMany(t => t.Devices)
                .HasForeignKey(d => d.TechnicianId)
                .OnDelete(DeleteBehavior.SetNull);

            // seed some device types (optional)
            modelBuilder.Entity<DeviceType>().HasData(
                new DeviceType { Id = 1, Name = "Laptop" },
                new DeviceType { Id = 2, Name = "Desktop" },
                new DeviceType { Id = 3, Name = "Tablet" },
                new DeviceType { Id = 4, Name = "Smartphone" }
            );
        }
    }
}
