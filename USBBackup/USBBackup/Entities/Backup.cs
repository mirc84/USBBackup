using USBBackup.Strings;

namespace USBBackup.Entities
{
    public class Backup : DatabaseModel, IBackup
    {
        #region Properties

        public virtual Drive Drive { get; set; }

        private string _sourcePath;
        public virtual string SourcePath
        {
            get { return _sourcePath; }
            set
            {
                _sourcePath = value;
                OnPropertyChanged();
            }
        }

        private string _targetPath;
        public virtual string TargetPath
        {
            get { return _targetPath; }
            set
            {
                _targetPath = value;
                OnPropertyChanged();
            }
        }

        private bool _isInverse;
        public virtual bool IsInverse
        {
            get { return _isInverse; }
            set { _isInverse = value; }
        }

        private bool _isEnabled;
        public virtual bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isRunning;
        public virtual bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _isPaused;
        public virtual bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                _isPaused = value;
                OnPropertyChanged();
            }
        }

        private string _currentFile;
        public virtual string CurrentFile
        {
            get { return _currentFile; }
            set
            {
                _currentFile = value;
                OnPropertyChanged();
            }
        }

        private Size _bytesToWrite;
        public virtual Size BytesToWrite
        {
            get { return _bytesToWrite; }
            set
            {
                _bytesToWrite = value;
                OnPropertyChanged();
            }
        }

        private Size _writtenBytes;
        public virtual Size WrittenBytes
        {
            get { return _writtenBytes; }
            set
            {
                _writtenBytes = value;
                OnPropertyChanged();
            }
        }


        private Size _finishedBytes;
        public virtual Size FinishedBytes
        {
            get { return _finishedBytes; }
            set
            {
                _finishedBytes = value;
                OnPropertyChanged();
            }
        }

        private Size _currentFileBytes;
        public virtual Size CurrentFileBytes
        {
            get { return _currentFileBytes; }
            set
            {
                _currentFileBytes = value;
                OnPropertyChanged();
            }
        }

        private Size _currentFileWrittenBytes;
        public virtual Size CurrentFileWrittenBytes
        {
            get { return _currentFileWrittenBytes; }
            set
            {
                _currentFileWrittenBytes = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Non Public Methods

        protected override string Validate(string columnName)
        {
            switch (columnName)
            {
                case nameof(SourcePath):
                    if (SourcePath == null)
                        return new Loc(nameof(StringResource.Backup_Validation_SourceNotSet));
                    if (SourcePath.StartsWith(TargetPath) || TargetPath.StartsWith(SourcePath))
                        return new Loc(nameof(StringResource.Backup_Validation_SourceTargetEquals));
                    if (!SourcePath.StartsWith(Drive.DriveLetter) && TargetPath != null && !TargetPath.StartsWith(Drive.DriveLetter))
                        return new Loc(nameof(StringResource.Backup_Validation_NoPathToDevice));
                    return null;
                case nameof(TargetPath):
                    if (TargetPath == null)
                        return new Loc(nameof(StringResource.Backup_Validation_TargetNotSet));
                    if (SourcePath.StartsWith(TargetPath) || TargetPath.StartsWith(SourcePath))
                        return new Loc(nameof(StringResource.Backup_Validation_SourceTargetEquals));
                    if (!TargetPath.StartsWith(Drive.DriveLetter) && SourcePath != null && !SourcePath.StartsWith(Drive.DriveLetter))
                        return new Loc(nameof(StringResource.Backup_Validation_NoPathToDevice));
                    return null;
                default:
                    return base.Validate(columnName);
            }
        }

        #endregion
    }
}