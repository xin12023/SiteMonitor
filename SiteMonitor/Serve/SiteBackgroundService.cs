using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SiteMonitor.Helpers;
using SiteMonitor.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using TimeCrontab;

public class SiteBackgroundService : IHostedService, IDisposable
{
    private readonly IOptionsMonitor<MonitorConfig> _configMonitor;
    private readonly LogHelper _logger;
    private MonitorConfig _config;
    private IDisposable _configReloadToken;
    private Crontab _crontab;
    private CancellationTokenSource _cancellationTokenSource;
    private TgHelper _tgHelper;

    public SiteBackgroundService(IOptionsMonitor<MonitorConfig> configMonitor, LogHelper logHelper, TgHelper tgHelper)
    {
        _configMonitor = configMonitor;
        _logger = logHelper;
        _config = _configMonitor.CurrentValue;
        _tgHelper = tgHelper;
        _tgHelper.Token = _config.BotToken;
        if (!string.IsNullOrEmpty(_config.RunCron))
        {
            _crontab = Crontab.Parse(_config.RunCron, CronStringFormat.WithSecondsAndYears);
        }

    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("启动监控服务");
        _cancellationTokenSource = new CancellationTokenSource();

        _configReloadToken = _configMonitor.OnChange(config =>
        {
            _config = config;
            _tgHelper.Token = _config.BotToken;
            if (!string.IsNullOrEmpty(_config.RunCron))
            {
                _crontab = Crontab.Parse(_config.RunCron, CronStringFormat.WithSecondsAndYears);
            }
            _logger.Information($"修改了配置文件 {Newtonsoft.Json.JsonConvert.SerializeObject(_config)}");
        });

        _logger.Information($"服务配置当前为: {Newtonsoft.Json.JsonConvert.SerializeObject(_config)}");

        _ = RunTaskAsync(_cancellationTokenSource.Token);
    }
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Information("停止监控服务");
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _logger.Information("监控服务销毁了");
        _configReloadToken?.Dispose();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
    }

    private async Task CheckSite(string sitePath)
    {
        var siteConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<SiteConfig>(System.IO.File.ReadAllText(sitePath));
        if (siteConfig == null)
        {
            _logger.Warning($"站点配置文件解析失败: {sitePath}");
            return;
        }
        _logger.Information($"开始检查站点 [ {siteConfig.Name} ]", tag: siteConfig.LogName);
        var checks = siteConfig.Methods;
        if (checks?.Length == 0)
        {
            _logger.Warning($"站点 [ {siteConfig.Name} ]配置文件中未配置检查方法", tag: siteConfig.LogName);
            return;
        }
        var monitor = new MonitorHelper(siteConfig);

        var result = await monitor.CheckSiteAsync();
        switch (result.State)
        {
            case ResultState.Skip:
                _logger.Information($"站点[ {siteConfig.Name} ]: {result.Msg}", tag: siteConfig.LogName);
                return;
            case ResultState.Fail:
            case ResultState.Exception:
                _logger.Warning($"站点[ {siteConfig.Name}: {siteConfig.Url} ] 检查结果: {result.Msg}", tag: siteConfig.LogName);
                await NotifyTg($"站点[ {siteConfig.Name}: {siteConfig.Url} ]{Environment.NewLine}检查结果: {Environment.NewLine}{result.Msg}");
                break;
            case ResultState.Success:
                _logger.Information($"站点 [ {siteConfig.Name} ] 检查结果: {result.Msg}", tag: siteConfig.LogName);
                break;
        }
    }

    private async Task NotifyTg(string message)
    {
        if (_config.GroupIds?.Length == 0)
        {
            _logger.Error("通知服务配置错误,未配置通知接收ID");
            return;
        }
        var tasks = _config.GroupIds.Select(chatId => _tgHelper.SendNotifyAsync(message, chatId));
        await Task.WhenAll(tasks);
    }


    private async Task WaitInterval()
    {
        await Task.Delay(TimeSpan.FromSeconds(_config.Interval * 1000));
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
                    if (_config.SiteConfigs?.Length == 0)
                    {
                        _logger.Warning($"未配置检查站点");
                        await WaitInterval();
                        continue;
                    }

                    var nextTime = _crontab.GetSleepMilliseconds(DateTime.Now);
                    if (nextTime > 30_000)
                    {
                        _logger.Information($"下次检查等待: {nextTime / 1000} 超过30秒");
                        await Task.Delay(30_000);
                        continue;
                    }
                    await Task.Delay((int)nextTime);

                    foreach (var site in _config.SiteConfigs)
                    {
                        if (string.IsNullOrEmpty(site))
                        {
                            continue;
                        }
                        var sitePath = Path.Combine(AppContext.BaseDirectory, _config.SiteFolder ?? "SiteConfigs", $"{site}.json");
                        if (Directory.Exists(sitePath))
                        {
                            _logger.Warning($"站点配置文件不存在: {sitePath}");
                            continue;
                        }
                        //启动新的线程任务 CheckSite
                        _ = Task.Factory.StartNew(() => CheckSite(sitePath));
                    }
                }
                await WaitInterval();
            }
        }
        catch (Exception ex)
        {
            _logger.Error("监控服务启动异常", exception: ex);
        }
        finally
        {
            _logger.Information("任务线程停止了");
        }
    }


}
