using Microsoft.Extensions.Options;
using SiteMonitor.Models;

public class SiteBackgroundService : IHostedService, IDisposable
{
    private MonitorConfig _config;
    private readonly IOptionsMonitor<MonitorConfig> _configMonitor;
    private IDisposable _configReloadToken;
    private CancellationTokenSource _cancellationTokenSource;

    public SiteBackgroundService(IOptionsMonitor<MonitorConfig> configMonitor)
    {
        _configMonitor = configMonitor;
        _config = _configMonitor.CurrentValue;
    }
    public async Task StartAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("启动监控");
        _cancellationTokenSource = new CancellationTokenSource();
        _configReloadToken = _configMonitor.OnChange(config =>
        {
            _config = config;
            Console.WriteLine("config发生了变化");
        });
        Console.WriteLine($"当前配置为：{_config}");
        await RunTaskAsync(_cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        Console.WriteLine("监控销毁");
        _configReloadToken?.Dispose();
        _cancellationTokenSource?.Cancel();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("监控停止");
        _cancellationTokenSource?.Cancel();
        return Task.CompletedTask;
    }

    private async Task RunTaskAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_config.Enable)
            {






                // 在这里编写你的定时任务逻辑
            }
            await Task.Delay(TimeSpan.FromSeconds(_config.Interval), cancellationToken);
        }
    }
}
