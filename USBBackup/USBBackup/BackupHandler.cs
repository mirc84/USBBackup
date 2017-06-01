using System;
using System.Collections.Generic;
using System.Linq;
using USBBackup.Entities;

namespace USBBackup
{
    public delegate void BackupHandledEventHandler(IBackup backup);

    public delegate void BackupStateChangedEventHandler(BackupState state);

    public class BackupHandler
    {
        #region Fields

        private readonly Dictionary<IBackup, BackupRunner> _backupRunners;
        private BackupState _state;

        #endregion

        #region Constructor

        public BackupHandler()
        {
            _backupRunners = new Dictionary<IBackup, BackupRunner>();
        }

        #endregion

        #region Events

        public event BackupHandledEventHandler BackupStarted;
        public event BackupHandledEventHandler BackupFinished;
        public event BackupHandledEventHandler CleanupStarted;
        public event BackupHandledEventHandler CleanupFinished;
        public event BackupStateChangedEventHandler StateChanged;

        #endregion

        #region Public Methods
        
        public void HandleBackup(Drive drive)
        {
            foreach (var backup in drive.Backups)
            {
                HandleBackup(backup);
            }
        }

        public void HandleBackup(IBackup backup, bool force = false)
        {
            if (backup.Error != null || !backup.Drive.IsAttached || backup.IsRunning || (!backup.IsEnabled && !force))
                return;

            var backupRunner = PrepareBackup(backup);
            backupRunner.AppendExecuteBackup();

            if (Properties.Settings.Default.CleanupRemovedFile)
                backupRunner.AppendRecycleExecute();
        }
        
        public void RecycleBackupFiles(IBackup backup)
        {
            if (!backup.IsEnabled || backup.Error != null)
                return;

            var backupRunner = PrepareBackup(backup);
            backupRunner.AppendRecycleExecute();
        }

        public void HandleBackup(Backup backup, string changedPath)
        {
            if (backup.Error != null)
                return;

            var backupRunner = PrepareBackup(backup);
            backupRunner.AppendHandleChangedPath(changedPath);
        }

        public void PauseResumeBackups()
        {
            switch (_state)
            {
                case BackupState.Idle:
                    return;
                case BackupState.Running:
                    PauseBackups();
                    break;
                case BackupState.Paused:
                    ResumeBackups();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Backup state {_state} unknown.");
            }
        }

        public void PauseBackups()
        {
            Log.Backup.Info("Pausing all backups");

            foreach (var backupRunner in _backupRunners)
            {
                backupRunner.Key.IsPaused = true;
                backupRunner.Value.PauseWaitHandle.Reset();
            }
            OnStateChanged();
        }

        public void PauseBackup(IBackup backup)
        {
            Log.Backup.Info($"Pausing backup from '{backup.SourcePath}' to '{backup.TargetPath}'");
            _backupRunners.TryGetValue(backup, out BackupRunner backupRunner);
            backupRunner?.PauseWaitHandle?.Reset();
            backupRunner?.PauseCancellationTokenSource?.Cancel();
            backup.IsPaused = true;
            OnStateChanged();
        }

        public void ResumeBackups()
        {
            Log.Backup.Info("Resuming all backups");
            foreach (var backupRunner in _backupRunners)
            {
                backupRunner.Key.IsPaused = false;
                backupRunner.Value.PauseWaitHandle.Set();
            }
            OnStateChanged();
        }

        public void ResumeBackup(IBackup backup)
        {
            Log.Backup.Info($"Resuming backup from '{backup.SourcePath}' to '{backup.TargetPath}'");
            backup.IsPaused = false;
            _backupRunners.TryGetValue(backup, out BackupRunner backupRunner);
            backupRunner?.PauseWaitHandle?.Set();
            OnStateChanged();
        }

        public void CancelBackup(IBackup backup, bool hardCancel)
        {
            Log.Backup.Info($"Cancelling backup from '{backup.SourcePath}' to '{backup.TargetPath}'");
            _backupRunners.TryGetValue(backup, out BackupRunner backupRunner);
            backupRunner?.CancellationTokenSource?.Cancel();
            backupRunner?.PauseWaitHandle?.Set();
            if (!hardCancel)
                return;

            backupRunner?.PauseCancellationTokenSource?.Cancel();
        }

        public void CancelBackups(bool hardCancel)
        {
            Log.Backup.Info("Cancelling all backups");
            foreach (var backupRunner in _backupRunners.Values)
            {
                backupRunner.CancellationTokenSource.Cancel();
                if (hardCancel)
                    backupRunner.PauseCancellationTokenSource?.Cancel();
                
                backupRunner.PauseWaitHandle.Set();
            }

            foreach (var backupRunner in _backupRunners.Values)
                backupRunner.Task.Wait();

            OnStateChanged();
        }

        #endregion

        #region Non Public Methods

        private BackupRunner PrepareBackup(IBackup backup)
        {
            if (!_backupRunners.TryGetValue(backup, out BackupRunner backupRunner))
            {
                backupRunner = new BackupRunner(backup);
                _backupRunners[backup] = backupRunner;
                backupRunner.BackupStarted += OnBackupStarted;
                backupRunner.BackupFinished += OnBackupFinished;
                backupRunner.CleanupStarted += OnCleanupStarted;
                backupRunner.CleanupFinished += OnCleanupFinished;
            }
            // Set WaitHandle to not pause new backup process
            backupRunner.PauseWaitHandle.Set();
            return backupRunner;
        }

        private void OnBackupStarted(IBackup backup)
        {
            BackupStarted?.Invoke(backup);
            OnStateChanged();
        }

        private void OnBackupFinished(IBackup backup)
        {
            BackupFinished?.Invoke(backup);
            OnStateChanged();
        }

        private void OnCleanupStarted(IBackup backup)
        {
            CleanupStarted?.Invoke(backup);
            OnStateChanged();
        }

        private void OnCleanupFinished(IBackup backup)
        {
            CleanupFinished?.Invoke(backup);
            OnStateChanged();
        }

        private void OnStateChanged()
        {
            _state = default(BackupState);
            if (_backupRunners.All(x => x.Key.IsPaused))
                _state = BackupState.Paused;
            else if (_backupRunners.Any(x => x.Key.IsRunning))
                _state = BackupState.Running;
            StateChanged?.Invoke(_state);
        }

        #endregion
    }
}