namespace USBBackup.Entities
{
    public class BackupInfoMap : DatabaseModelMap<Backup>
    {
        public BackupInfoMap()
        {
            Map(x => x.SourcePath);
            Map(x => x.TargetPath);
            Map(x => x.IsEnabled);
            Map(x => x.IsInverse);
            References(x => x.Drive).Column("Drive_id");
        }
    }
}