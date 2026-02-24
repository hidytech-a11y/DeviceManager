using DeviceManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceManager.Data
{
    public class DeviceContext(DbContextOptions<DeviceContext> options) : IdentityDbContext<IdentityUser>(options)
    {
        public DbSet<Device> Devices { get; set; }
        public DbSet<Technician> Technicians { get; set; }
        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Diagnosis> Diagnoses { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DeviceHistory> DeviceHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Technician soft delete filter
            modelBuilder.Entity<Technician>()
                .HasQueryFilter(t => !t.IsDeleted);

            // Device -> Technician (Set to null when technician is deleted)
            modelBuilder.Entity<Device>()
                .HasOne(d => d.Technician)
                .WithMany(t => t.Devices)
                .HasForeignKey(d => d.TechnicianId)
                .OnDelete(DeleteBehavior.SetNull);

            // Diagnosis -> Device (Cascade delete - delete diagnoses when device is deleted)
            modelBuilder.Entity<Diagnosis>()
                .HasOne(d => d.Device)
                .WithMany(dev => dev.Diagnoses)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Diagnosis -> Technician (Set to null when technician is deleted)
            modelBuilder.Entity<Diagnosis>()
                .HasOne(d => d.Technician)
                .WithMany()
                .HasForeignKey(d => d.TechnicianId)
                .OnDelete(DeleteBehavior.SetNull);

            // DeviceHistory -> Device (Cascade delete - delete history when device is deleted)
            modelBuilder.Entity<DeviceHistory>()
                .HasOne(h => h.Device)
                .WithMany()
                .HasForeignKey(h => h.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification -> Device (Set to null when device is deleted)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Device)
                .WithMany()
                .HasForeignKey(n => n.DeviceId)
                .OnDelete(DeleteBehavior.SetNull);

            // Seed DeviceTypes
            modelBuilder.Entity<DeviceType>().HasData(
                new DeviceType { Id = 1, Name = "Laptop" },
                new DeviceType { Id = 2, Name = "Desktop" },
                new DeviceType { Id = 3, Name = "Tablet" },
                new DeviceType { Id = 4, Name = "Smartphone" }
            );
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            EnforceDeviceStatusRules();
            HandleTechnicianDeletion();
            HandleTechnicianSoftDelete();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void HandleTechnicianDeletion()
        {
            var deletedTechnicianIds = ChangeTracker.Entries<Technician>()
                .Where(e => e.State == EntityState.Deleted)
                .Select(e => e.Entity.Id)
                .ToList();

            if (!deletedTechnicianIds.Any())
                return;

            var affectedDevices = ChangeTracker.Entries<Device>()
                .Where(e =>
                    e.Entity.TechnicianId != null &&
                    deletedTechnicianIds.Contains(e.Entity.TechnicianId.Value))
                .Select(e => e.Entity)
                .ToList();

            foreach (var device in affectedDevices)
            {
                device.TechnicianId = null;
                device.Status = "Inactive";
                Entry(device).State = EntityState.Modified;
            }
        }

        private void EnforceDeviceStatusRules()
        {
            var deviceEntries = ChangeTracker.Entries<Device>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .ToList();

            foreach (var entry in deviceEntries)
            {
                var device = entry.Entity;

                if (device.TechnicianId == null)
                {
                    device.Status = DeviceStatus.Inactive;
                }
                else
                {
                    device.Status = DeviceStatus.Active;
                }
            }
        }

        private void HandleTechnicianSoftDelete()
        {
            var deletedTechs = ChangeTracker.Entries<Technician>()
                .Where(e => e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in deletedTechs)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;

                foreach (var device in entry.Entity.Devices)
                {
                    device.TechnicianId = null;
                }
            }
        }
    }
}