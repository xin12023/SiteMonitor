using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace SiteMonitor.Helpers
{
    public class LogHelper
    {
        private readonly ConcurrentQueue<LogInfo> _loggerInfos = new ConcurrentQueue<LogInfo>();
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        public string LogPath { get; set; } = "logs";

        public int CopiesCount { get; set; } = 1;

        public string LogFileNameFormat { get; set; } = "yyyy-MM-dd";

        public string DefaultLogName { get; set; } = "run";

        public LoggerType LogLevel { get; set; } = LoggerType.Trace;



        public LogHelper()
        {
            if (!Directory.Exists(this.LogPath))
            {
                Directory.CreateDirectory(this.LogPath);
            }

            //Task.Factory.StartNew(() => WriteLogsAsync(), TaskCreationOptions.LongRunning);
            _ = Task.Run(() => WriteLogsAsync());
        }

        public void Log(string message, LoggerType loggerType = LoggerType.Warning, Exception? exception = null, string? tag = null)
        {
            _loggerInfos.Enqueue(new LogInfo
            {
                Tag = tag ?? DefaultLogName,
                Exception = exception,
                Message = message,
                LoggerType = loggerType,
                Time = DateTime.UtcNow,
            });
        }

        public void Trace(string message, Exception? exception = null, string? tag = null) => Log(message, LoggerType.Trace, exception, tag);
        public void Debug(string message, Exception? exception = null, string? tag = null) => Log(message, LoggerType.Debug, exception, tag);
        public void Information(string message, Exception? exception = null, string? tag = null) => Log(message, LoggerType.Information, exception, tag);
        public void Warning(string message, Exception? exception = null, string? tag = null) => Log(message, LoggerType.Warning, exception, tag);
        public void Error(string message, Exception? exception = null, string? tag = null) => Log(message, LoggerType.Error, exception, tag);
        public void Fatal(string message, Exception? exception = null, string? tag = null) => Log(message, LoggerType.Fatal, exception, tag);

        private async Task WriteLogToFileAsync(string filePath, LogInfo logInfo)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                using var writer = File.AppendText(filePath);
                await writer.WriteLineAsync($"[{logInfo.Time:yyyy-MM-dd HH:mm:ss.fff}] {logInfo.LoggerType}: {logInfo.Message}");
                if (logInfo.Exception != null)
                {
                    await writer.WriteLineAsync(logInfo.Exception.ToString());
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task WriteLogsAsync()
        {
            while (true)
            {
                while (_loggerInfos.TryDequeue(out var logInfo))
                {
                    if ((int)logInfo.LoggerType < (int)LogLevel) continue;
                    var fileName = logInfo.Tag ?? DefaultLogName;
                    var filePath = Path.Combine(LogPath, $"{fileName}_{logInfo.Time.ToString(LogFileNameFormat)}.log");
                    await WriteLogToFileAsync(filePath, logInfo);
                    DeleteOldLogFiles(fileName);
                }
                // 无日志时等待一段时间
                await Task.Delay(300);
            }
        }

        private void DeleteOldLogFiles(string fileName)
        {
            var files = Directory.EnumerateFiles(LogPath, $"{fileName}_*.log")
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .ToList();

            if (files.Count > CopiesCount)
            {
                for (int i = CopiesCount; i < files.Count; i++)
                {
                    File.Delete(files[i]);
                }
            }
        }
    }
}