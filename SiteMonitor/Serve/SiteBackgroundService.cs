using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SiteMonitor.Helpers;
using SiteMonitor.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SiteBackgroundService : IHostedService, IDisposable
{
    private readonly IOptionsMonitor<MonitorConfig> _configMonitor;
    private readonly LogHelper _logger;
    private MonitorConfig _config;
    private IDisposable _configReloadToken;
    private CancellationTokenSource _cancellationTokenSource;

    public SiteBackgroundService(IOptionsMonitor<MonitorConfig> configMonitor, LogHelper logHelper)
    {
        _configMonitor = configMonitor;
        _logger = logHelper;
        _config = _configMonitor.CurrentValue;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("启动监控服务");

        _cancellationTokenSource = new CancellationTokenSource();

        _configReloadToken = _configMonitor.OnChange(config =>
        {
            _config = config;
            _logger.Information($"修改了配置文件 {Newtonsoft.Json.JsonConvert.SerializeObject(_config)}");
        });

        _logger.Information($"服务配置当前为: {Newtonsoft.Json.JsonConvert.SerializeObject(_config)}");

        _ = RunTaskAsync(_cancellationTokenSource.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Information("停止监控服务");
    }

    public void Dispose()
    {
        _logger.Information("监控服务销毁了");
        _configReloadToken?.Dispose();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

    private async Task RunTaskAsync(CancellationToken cancellationToken)
    {
        _logger.Information("任务线程启动");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_config.Enable)
                {
                    //_logger.Information($"Running monitor task at {DateTimeOffset.UtcNow}");

                    // Run your periodic task logic here.

                    await Task.Delay(TimeSpan.FromSeconds(_config.Interval), cancellationToken);
                    //_logger.Information($"Monitor task completed at {DateTimeOffset.UtcNow}");
                }

                await Task.Delay(TimeSpan.FromSeconds(_config.Interval), cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            //忽略异常任务
        }
        finally
        {
            _logger.Information("任务线程停止了");
        }
    }
}
