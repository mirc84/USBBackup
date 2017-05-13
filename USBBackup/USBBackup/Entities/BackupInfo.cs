namespace USBBackup.Entities
{
    public class BackupInfo : DatabaseModel
    {
        private string _sourcePath;
        private string _targetPath;

        public virtual string SourcePath
        {
            get { return _sourcePath; }
            set
            {
                _sourcePath = value;
                OnPropertyChanged();
            }
        }

        public virtual string TargetPath
        {
            get { return _targetPath; }
            set
            {
                _targetPath = value;
                OnPropertyChanged();
            }
        }
    }
    public class BackupInfoMap : DatabaseModelMap<BackupInfo>
    {
        public BackupInfoMap()
        {
            Map(x => x.SourcePath);
            Map(x => x.TargetPath);
        }
    }
}