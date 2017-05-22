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
        long BytesToWrite { get; set; }
        long WrittenBytes { get; set; }
        long FinishedBytes { get; set; }
        long CurrentFileBytes { get; set; }
        long CurrentFileWrittenBytes { get; set; }
    }
}