namespace USBBackup.Entities
{
    public class BackupInfo : DatabaseModel
    {
        private string _sourcePath;
        private string _targetPath;

        public string SourcePath
        {
            get { return _sourcePath; }
            set
            {
                _sourcePath = value;
                OnPropertyChanged();
            }
        }

        public string TargetPath
        {
            get { return _targetPath; }
            set
            {
                _targetPath = value;
                OnPropertyChanged();
            }
        }
    }
}