namespace USBBackup.Entities
{
    class DriveInfoMap : DatabaseModelMap<Drive>
    {
        public DriveInfoMap()
        {
            Map(x => x.DriveLetter);
            Map(x => x.Name);
            Map(x => x.DeviceID);
            Map(x => x.Model);
            Map(x => x.PNPDeviceID);
            Map(x => x.Description);
            HasMany(x => x.Backups).Inverse().KeyColumn("Drive_id").Cascade.AllDeleteOrphan().Not.LazyLoad();
        }
    }
}
