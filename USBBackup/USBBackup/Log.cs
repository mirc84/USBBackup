using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;

namespace USBBackup
{
    public class Log
    {
        #region Fields

        private static LoggingConfiguration _config;
        private static LogLevel _minLogLevel;
        private static string _logPath;
        private static string _layout;
        private static Logger _backupLogger;
        private static Logger _applicationLogger;

        #endregion

        #region Constructor

        static Log()
        {
            _logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "USBBackup/Log");
            _minLogLevel = LogLevel.Debug;

            _config = new LoggingConfiguration();
            _layout = @"${date:format=dd.MM.yyyy HH\:mm\:ss} ${level} ${logger} ""${message}"" ${exception:format=toString}";

            var consoleTarget = new ColoredConsoleTarget()
            {
                Layout = _layout
            };
            _config.AddTarget("console", consoleTarget);
            var rule1 = new LoggingRule("*", _minLogLevel, consoleTarget);
            _config.LoggingRules.Add(rule1);

            AddLogTarget("Application");
            AddLogTarget("Backup");

            LogManager.Configuration = _config;
        }

        #endregion

        #region Properties

        public static ILogger Backup => _backupLogger ?? (_backupLogger = LogManager.GetLogger("Backup"));
        public static ILogger Application => _applicationLogger ?? (_applicationLogger = LogManager.GetLogger("Application"));

        #endregion

        #region Non Public Methods

        private static void AddLogTarget(string name)
        {
            var target = new FileTarget()
            {
                FileName = Path.Combine(_logPath, $"USBBackup.log"),
                Layout = _layout
            };
            _config.AddTarget(name, target);

            var rule = new LoggingRule(name, _minLogLevel, target);
            _config.LoggingRules.Add(rule);
        }

        #endregion    }
    }
}
