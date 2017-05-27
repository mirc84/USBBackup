using System.ComponentModel;

namespace USBBackup
{
    public interface IBackup : IDataErrorInfo
    {
        bool IsEnabled { get; set; }
        bool IsRunning { get; set; }
        bool IsPaused { get; set; }
        string SourcePath { get; set; }
        string TargetPath { get; set; }

        string CurrentFile { get; set; }
        Size BytesToWrite { get; set; }
        Size WrittenBytes { get; set; }
        Size FinishedBytes { get; set; }
        Size CurrentFileBytes { get; set; }
        Size CurrentFileWrittenBytes { get; set; }
    }
}