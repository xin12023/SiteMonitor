using System.Collections.Concurrent;

namespace SiteMonitor.Helpers.Loger
{
    public class LogHelper
    {
        public ConcurrentQueue<LogInfo> LoggerInfos { get; private set; } = new ConcurrentQueue<LogInfo>();
        /// <summary>
        /// 日志目录
        /// </summary>
        private string logPath;
        /// <summary>
        /// 日志文件开头名
        /// </summary>
        private string logNameStart;
        /// <summary>
        /// 备份数量
        /// </summary>
        private int copiesCount;
        /// <summary>
        /// 当前文件名
        /// </summary>
        private string logfileName;
        /// <summary>
        /// 日志写出等级
        /// </summary>
        private LoggerType logLevel;

        /// <summary>
        /// 日志文件名的格式化方式
        /// </summary>
        private string logFileNameFaormat;



        private void InitializeLog(string logPath, string logNameStart, int copiesCount, LoggerType level, string logFileNameFaormat)
        {
            this.logPath = logPath;
            if (!File.Exists(Path.Combine(AppContext.BaseDirectory, logPath)))
            {
                Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, logPath));
            }
            this.logNameStart = logNameStart;
            this.copiesCount = copiesCount;
            this.logFileNameFaormat = logFileNameFaormat;
        }

        public LogHelper(string logPath = "logs", string logNameStart = "log", int copiesCount = 1, LoggerType level = LoggerType.Trace, string logFileNameFaormat = "yyyy-MM-dd")
        {
            InitializeLog(logPath, logNameStart, copiesCount, level, logFileNameFaormat);
        }

        public LogHelper()
        {
            InitializeLog("logs", "log", 1, LoggerType.Trace, "yyyy-MM-dd");
        }


        public void Clear()
        {
            LoggerInfos = new ConcurrentQueue<LogInfo>();
        }

        public void Log(string message, LoggerType loggerType = LoggerType.Warning)
        {

            LoggerInfos.Enqueue(new LogInfo
            {
                Exception = null,
                Message = message,
                LoggerType = loggerType,
                Time = DateTime.UtcNow,
            });
        }
        public void Log(Exception exception, string? message = null)
        {
            LoggerInfos.Enqueue(new LogInfo
            {
                Exception = exception,
                Message = message ?? exception.Message,
                LoggerType = LoggerType.Error,
                Time = DateTime.UtcNow,
            });
        }
        public void Trace(string message, Exception? exception = null, string? tag = null)
        {
            Log(message, exception, LoggerType.Trace, DateTime.UtcNow, tag: tag);
        }
        public void Debug(string message, Exception? exception = null, string? tag = null)
        {
            Log(message, exception, LoggerType.Debug, DateTime.UtcNow, tag: tag);
        }
        public void Information(string message, Exception? exception = null, string? tag = null)
        {
            Log(message, exception, LoggerType.Information, DateTime.UtcNow, tag: tag);
        }
        public void Warning(string message, Exception? exception = null, string? tag = null)
        {
            Log(message, exception, LoggerType.Warning, DateTime.UtcNow, tag: tag);
        }
        public void Error(string message, Exception? exception = null, string? tag = null)
        {
            Log(message, exception, LoggerType.Error, DateTime.UtcNow, tag: tag);
        }
        public void Fatal(string message, Exception? exception = null, string? tag = null)
        {
            Log(message, exception, LoggerType.Fatal, DateTime.UtcNow, tag: tag);
        }
        public void Log(string message, Exception exception, LoggerType loggerType, DateTime dateTime, string tag = null)
        {
            LoggerInfos.Enqueue(new LogInfo
            {
                Tag = tag,
                Exception = exception,
                Message = message,
                LoggerType = loggerType,
                Time = dateTime,
            });
            WriteLog();
        }

        private void WriteLog()
        {
            // Get current log file name based on current date
            var currentDate = DateTime.UtcNow.AddHours(8).ToString(logFileNameFaormat);
            var currentFileName = $"{logNameStart}_{currentDate}.log";

            // Create log file if it does not exist
            var logFilePath = Path.Combine(AppContext.BaseDirectory, logPath);
            Directory.CreateDirectory(logFilePath);
            var filePath = Path.Combine(logFilePath, currentFileName);
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            // Remove oldest log files if there are too many
            var logFiles = Directory.GetFiles(logFilePath, $"{logNameStart}_*.log")
                .OrderByDescending(f => f)
                .Skip(copiesCount - 1)
                .ToList();
            foreach (var file in logFiles)
            {
                if (Path.GetFileName(file) != currentFileName)
                {
                    File.Delete(file);
                }
            }

            // Write pending log messages to the log file
            using (var writer = new StreamWriter(filePath, true))
            {
                while (LoggerInfos.TryDequeue(out var loggerInfo))
                {
                    if (loggerInfo.LoggerType >= logLevel)
                    {
                        writer.WriteLine($"{loggerInfo.Time.ToString()} [{loggerInfo.LoggerType}] {loggerInfo.Tag ?? ""}: {loggerInfo.Message}");
                        if (loggerInfo.Exception != null)
                        {
                            writer.WriteLine(loggerInfo.Exception.ToString());
                        }
                    }
                }
            }
        }
    }
}