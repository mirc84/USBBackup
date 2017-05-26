using USBBackup.Strings;

namespace USBBackup.Entities
{
    public class Backup : DatabaseModel, IBackup
    {
        private string _sourcePath;
        private string _targetPath;
        private bool _isEnabled;
        private bool _isInverse;
        private bool _isRunning;
        private bool _isPaused;
        private string _currentFile;

        private long _bytesToWrite;
        private long _writtenBytes;
        private long _finishedBytes;
        private long _currentFileBytes;
        private long _currentFileWrittenBytes;

        public virtual Drive Drive { get; set; }

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

        public virtual bool IsInverse
        {
            get { return _isInverse; }
            set { _isInverse = value; }
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

        public virtual long BytesToWrite
        {
            get { return _bytesToWrite; }
            set
            {
                _bytesToWrite = value;
                OnPropertyChanged();
            }
        }

        public virtual long WrittenBytes
        {
            get { return _writtenBytes; }
            set
            {
                _writtenBytes = value;
                OnPropertyChanged();
            }
        }


        public virtual long FinishedBytes
        {
            get { return _finishedBytes; }
            set
            {
                _finishedBytes = value;
                OnPropertyChanged();
            }
        }

        public virtual long CurrentFileBytes
        {
            get { return _currentFileBytes; }
            set
            {
                _currentFileBytes = value;
                OnPropertyChanged();
            }
        }

        public virtual long CurrentFileWrittenBytes
        {
            get { return _currentFileWrittenBytes; }
            set
            {
                _currentFileWrittenBytes = value;
                OnPropertyChanged();
            }
        }

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
    }

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