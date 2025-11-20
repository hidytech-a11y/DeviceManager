using DeviceManager.Models;
using Microsoft.EntityFrameworkCore;

namespace DeviceManager.Data
{
    public class DeviceContext : DbContext
    {
        public DeviceContext(DbContextOptions<DeviceContext> options)
            : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }

        public DbSet<Technician> Technicians { get; set; }
    }
}
