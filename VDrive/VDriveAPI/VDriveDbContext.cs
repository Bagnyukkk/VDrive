using Microsoft.EntityFrameworkCore;
using VDriveAPI.Entities;

namespace VDriveAPI
{
    public class VDriveDbContext : DbContext
    {
        public VDriveDbContext(DbContextOptions<VDriveDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<FileMetaData> Files { get; set; }
    }
}
