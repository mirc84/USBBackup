namespace USBBackup.Entities
{
    public class Backup : DatabaseModel, IBackup
    {
        private string _sourcePath;
        private string _targetPath;
        private bool _isEnabled;
        private bool _isRunning;
        private bool _isPaused;
        private string _currentFile;

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

        public virtual bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public virtual bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                OnPropertyChanged();
            }
        }

        public virtual bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                _isPaused = value;
                OnPropertyChanged();
            }
        }

        public virtual string CurrentFile
        {
            get { return _currentFile; }
            set
            {
                _currentFile = value;
                OnPropertyChanged();
            }
        }
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