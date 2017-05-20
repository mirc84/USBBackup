namespace USBBackup
{
    public interface IBackup
    {
        bool IsEnabled { get; set; }
        bool IsRunning { get; set; }
        bool IsPaused { get; set; }
        string SourcePath { get; set; }
        string TargetPath { get; set; }
        string CurrentFile { get; set; }
    }
}