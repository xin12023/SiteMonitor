using Microsoft.Extensions.Hosting;
using SiteMonitor.Helpers;
using SiteMonitor.Models;
using SqlSugar;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using TimeCrontab;

public class SiteBackgroundService : IHostedService, IDisposable
{
    private readonly LogHelper _logger;
    private Crontab _crontab;
    private CancellationTokenSource _cancellationTokenSource;
    private TgHelper _tgHelper;
    private readonly ISqlSugarClient _db;
    private RunConfig _runConfig;


    public SiteBackgroundService(ISqlSugarClient db, LogHelper logHelper, TgHelper tgHelper)
    {
        _logger = logHelper;
        _tgHelper = tgHelper;
        _db = db;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("启动监控服务");
        _cancellationTokenSource = new CancellationTokenSource();
        _runConfig = await _db.Queryable<RunConfig>().FirstAsync();
        _crontab = Crontab.Parse(_runConfig.RunCron, CronStringFormat.WithSecondsAndYears);
        _tgHelper.Token = _runConfig.TgToken;
        _ = RunTaskAsync(_cancellationTokenSource.Token);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.Information("停止监控服务");
        _cancellationTokenSource?.Cancel();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _logger.Information("监控服务销毁了");
        _cancellationTokenSource?.Dispose();
    }

    private async Task CheckSite(SiteConfig site)
    {
        _logger.Information($"开始检查站点 [{site.Name}]", tag: site.LogName);
        var checks = site.Methods;
        if (checks?.Length == 0)
        {
            _logger.Warning($"站点 [{site.Name}]配置文件中未配置检查方法", tag: site.LogName);
            return;
        }
        var monitor = new MonitorHelper(site);

        var result = await monitor.CheckSiteAsync();
        switch (result.State)
        {
            case ResultState.Skip:
                _logger.Information($"站点[{site.Name}]: {result.Msg}", tag: site.LogName);
                break;
            case ResultState.Success:
                _logger.Information($"站点 [{site.Name}] 检查结果: {result.Msg}", tag: site.LogName);
                break;
            default:
                await NotifyTg($"站点[ {site.Name} : {site.Url} ]{Environment.NewLine}检查结果: {Environment.NewLine}{result.Msg}");
                _logger.Warning($"站点[{site.Name}: {site.Url}] 检查结果: {result.Msg}", tag: site.LogName);
                break;
        }
        if (result.State !=  ResultState.Skip)
        {
            //更新最后检查时间
            site.Lasttime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            await _db.Updateable(site).UpdateColumns(it => new { it.Lasttime }).ExecuteCommandAsync();
        }
    }

    private async Task NotifyTg(string message)
    {
        var groups = _runConfig?.GroupIds?.Split(',') ?? null;

        if (groups?.Length == 0)
        {
            return;
        }
        _logger.Warning($"TG发送通知,会话 [{string.Join(",", groups)}] :  发送消息: {message}");
        var tasks = groups.Select(chatId => _tgHelper.SendNotifyAsync(message, chatId));
        await Task.WhenAll(tasks);
    }

    private async Task RunTaskAsync(CancellationToken cancellationToken)
    {
        _logger.Information("任务线程启动");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var checkSites = _db.Queryable<SiteConfig>().Where(it => it.Enable).ToList();

                if (checkSites.Count == 0)
                {
                    _logger.Warning($"未配置检查站点");
                }
                else
                {
                    var nextTime = _crontab.GetSleepMilliseconds(DateTime.Now);
                    if (nextTime > 30_000)
                    {
                        _logger.Information($"下次检查等待: {nextTime / 1000} 超过30秒");
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                    else
                    {
                        await Task.Delay((int)nextTime);
                        //根据机器性能自动处理并行
                        var maxDegreeOfParallelism = Environment.ProcessorCount;
                        await Task.WhenAll(checkSites.AsParallel().WithDegreeOfParallelism(maxDegreeOfParallelism).Select(site => CheckSite(site)));
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(10));
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
