using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;

namespace SiteMonitor.Helpers
{
    public class LogHelper
    {
        private readonly ConcurrentQueue<LogInfo> _loggerInfos = new ConcurrentQueue<LogInfo>();

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();


        public string LogPath { get; set; } = "logs";

        public int CopiesCount { get; set; } = 1;

        public string LogFileNameFormat { get; set; } = "yyyy-MM-dd";

        public string DefaultLogName { get; set; } = "run";

        public long MaxLogFileSize { get; set; } = 1024 * 1024 * 10;

        public LoggerType LogLevel { get; set; } = LoggerType.Trace;



        public LogHelper()
        {
            if (!Directory.Exists(this.LogPath))
            {
                Directory.CreateDirectory(this.LogPath);
            }

            Task.Run(WriteLogsAsync, _cancellationTokenSource.Token);
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


        private async Task WriteLogsAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                if (_loggerInfos.TryDequeue(out var logInfo))
                {
                    if ((int)logInfo.LoggerType < (int)LogLevel)
                    {
                        continue;
                    }
                    var fileName = logInfo.Tag ?? DefaultLogName;
                    var filePath = Path.Combine(LogPath, $"{fileName}_{logInfo.Time.ToString(LogFileNameFormat)}.log");
                    await WriteLogToFileAsync(filePath, logInfo);
                    DeleteOldLogFiles(fileName);
                }
                else
                {
                    await Task.Delay(100); // 暂停一段时间，避免线程空转消耗资源
                }
            }
        }

        private async Task WriteLogToFileAsync(string filePath, LogInfo logInfo)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                using var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, useAsync: true);
                using var writer = new StreamWriter(stream);

                if (new FileInfo(filePath).Length >= MaxLogFileSize)
                {
                    BackupLogFile(filePath);
                    using var newStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, useAsync: true);
                    using var newWriter = new StreamWriter(newStream);
                    await WriteLogInfoAsync(newWriter, logInfo);
                }
                else
                {
                    await WriteLogInfoAsync(writer, logInfo);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private void BackupLogFile(string filePath)
        {
            var backupFileName = $"{Path.GetFileNameWithoutExtension(filePath)}_{DateTime.Now:ddHHmmssfff}{Path.GetExtension(filePath)}";
            var backupFilePath = Path.Combine(LogPath, backupFileName);
            File.Replace(filePath, backupFilePath, null);
        }

        private async Task WriteLogInfoAsync(TextWriter writer, LogInfo logInfo)
        {
            await writer.WriteLineAsync($"[{logInfo.Time:yyyy-MM-dd HH:mm:ss.fff}] {logInfo.LoggerType}: {logInfo.Message}");
            if (logInfo.Exception != null)
            {
                await writer.WriteLineAsync(logInfo.Exception.ToString());
            }
        }

        private void DeleteOldLogFiles(string fileName)
        {
            var files = Directory.EnumerateFiles(LogPath, $"{fileName}_*.log")
                .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                .Skip(CopiesCount)
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