using System.Data.Entity;
using USBBackup.Entities;

namespace USBBackup.DatabaseAccess
{
    public class Database : DbContext
    {
        public Database()
        {
        }

        public DbSet<USBDeviceInfo> USBDeviceInfos { get; set; }
        public DbSet<BackupInfo> BackupInfos { get; set; }
    }
}
