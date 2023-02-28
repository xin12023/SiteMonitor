using System.Collections.Concurrent;

namespace SiteMonitor.Helpers
{
    public class LogHelper
    {

        private readonly ConcurrentQueue<LogInfo> loggerInfos = new ConcurrentQueue<LogInfo>();
        private readonly string logPath;
        private readonly string logNameStart;
        private readonly int copiesCount;
        private readonly string logFileNameFormat;
        private readonly LoggerType logLevel;
        private readonly string logFilePath;
        private readonly string logFileNameFaormat;

        public LogHelper(string logPath = "logs", string logNameStart = "log", int copiesCount = 1,  LoggerType logLevel = LoggerType.Trace, string logFileNameFormat = "yyyy-MM-dd")
        {
            this.logPath = logPath;
            this.logNameStart = logNameStart;
            this.copiesCount = copiesCount;
            this.logLevel = logLevel;
            this.logFileNameFormat = logFileNameFormat;

            var baseDirectory = AppContext.BaseDirectory;
            var logDirectory = Path.Combine(baseDirectory, logPath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            logFilePath = Path.Combine(logDirectory, $"{logNameStart}-{DateTime.UtcNow.ToString(logFileNameFormat)}.log");

        }

        public void Clear()
        {
            loggerInfos.Clear();
        }

        public void Log(string message, LoggerType loggerType = LoggerType.Warning)
        {

            loggerInfos.Enqueue(new LogInfo
            {
                Exception = null,
                Message = message,
                LoggerType = loggerType,
                Time = DateTime.UtcNow,
            });
        }
        public void Log(Exception exception, string? message = null)
        {
            loggerInfos.Enqueue(new LogInfo
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
            loggerInfos.Enqueue(new LogInfo
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

            var currentDate = DateTime.UtcNow.AddHours(8).ToString(logFileNameFaormat);
            var currentFileName = $"{logNameStart}_{currentDate}.log";


            var logFilePath = Path.Combine(AppContext.BaseDirectory, logPath);
            Directory.CreateDirectory(logFilePath);
            var filePath = Path.Combine(logFilePath, currentFileName);
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }


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


            using (var writer = new StreamWriter(filePath, true))
            {
                while (loggerInfos.TryDequeue(out var loggerInfo))
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