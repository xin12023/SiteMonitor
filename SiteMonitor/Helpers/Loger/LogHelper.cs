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
        private readonly string _logPath;
        private readonly int _copiesCount;
        private readonly string _logFileNameFormat;
        private readonly string _defaultLogName = "run";
        private readonly LoggerType _logLevel;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        public LogHelper(string logPath = "logs", int copiesCount = 1, LoggerType logLevel = LoggerType.Trace, string logFileNameFormat = "yyyy-MM-dd")
        {
            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logPath);
            _copiesCount = copiesCount;
            _logLevel = logLevel;
            _logFileNameFormat = logFileNameFormat;

            if (!Directory.Exists(this._logPath))
            {
                Directory.CreateDirectory(this._logPath);
            }

            //Task.Factory.StartNew(() => WriteLogsAsync(), TaskCreationOptions.LongRunning);
            _ = Task.Run(() => WriteLogsAsync());
        }

        public void Log(string message, LoggerType loggerType = LoggerType.Warning, Exception? exception = null, string? tag = null)
        {
            _loggerInfos.Enqueue(new LogInfo
            {
                Tag = tag ?? _defaultLogName,
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
                    if ((int)logInfo.LoggerType < (int)_logLevel) continue;
                    var fileName = logInfo.Tag ?? _defaultLogName;
                    var filePath = Path.Combine(_logPath, $"{fileName}_{logInfo.Time.ToString(_logFileNameFormat)}.log");

                    // 等待可用的信号量
                    await _semaphoreSlim.WaitAsync();
                    try
                    {
                        await WriteLogToFileAsync(filePath, logInfo);
                        DeleteOldLogFiles(fileName);
                    }
                    finally
                    {
                        // 释放信号量
                        _semaphoreSlim.Release();
                    }
                }
                
                // 无日志时等待一段时间
                await Task.Delay(300);
            }
        }

        private void DeleteOldLogFiles(string fileName)
        {
            var files = Directory.EnumerateFiles(_logPath, $"{fileName}_*.log")
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .ToList();

            if (files.Count > _copiesCount)
            {
                for (int i = _copiesCount; i < files.Count; i++)
                {
                    File.Delete(files[i]);
                }
            }
        }
    }
}