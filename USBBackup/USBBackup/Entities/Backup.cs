namespace USBBackup.Entities
{
    public class Backup : DatabaseModel, IBackup
    {
        public virtual string SourcePath { get; set; }
        public virtual string TargetPath { get; set; }
        public virtual bool IsEnabled { get; set; }
        public virtual bool IsRunning { get; set; }

    }

    public class BackupInfoMap : DatabaseModelMap<Backup>
    {
        public BackupInfoMap()
        {
            Map(x => x.SourcePath);
            Map(x => x.TargetPath);
            Map(x => x.IsEnabled);
        }
    }
}