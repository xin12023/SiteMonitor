using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SiteMonitor.Helpers;
using SiteMonitor.Models;
using SqlSugar;
using System;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Threading;
using System.Threading.Tasks;
using TimeCrontab;

public class SiteBackgroundService : IHostedService, IDisposable
{
    //private readonly IOptionsMonitor<MonitorConfig> _configMonitor;
    //private MonitorConfig _config;
    private readonly LogHelper _logger;
    private IDisposable _configReloadToken;
    private Crontab _crontab;
    private CancellationTokenSource _cancellationTokenSource;
    private TgHelper _tgHelper;
    readonly ISqlSugarClient _db;
    private RunConfig _runConfig;

    public SiteBackgroundService(ISqlSugarClient db, LogHelper logHelper, TgHelper tgHelper)
    {
        _logger = logHelper;
        _tgHelper = tgHelper;
        _db = db;
        //_config = _configMonitor.CurrentValue;
        //_tgHelper.Token = _config.BotToken;
        //if (!string.IsNullOrEmpty(_config.RunCron))
        //{
        //    _crontab = Crontab.Parse(_config.RunCron, CronStringFormat.WithSecondsAndYears);
        //}

    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Information("启动监控服务");
        _cancellationTokenSource = new CancellationTokenSource();
        _runConfig = await _db.Queryable<RunConfig>().FirstAsync();
        _ = RunTaskAsync(_cancellationTokenSource.Token);

        //_configReloadToken = _configMonitor.OnChange(config =>
        //{
        //    _config = config;
        //    _tgHelper.Token = _config.BotToken;
        //    if (!string.IsNullOrEmpty(_config.RunCron))
        //    {
        //        _crontab = Crontab.Parse(_config.RunCron, CronStringFormat.WithSecondsAndYears);
        //    }
        //    _logger.Information($"修改了配置文件 {Newtonsoft.Json.JsonConvert.SerializeObject(_config)}");
        //});

        //_logger.Information($"服务配置当前为: {Newtonsoft.Json.JsonConvert.SerializeObject(_config)}");
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

    private async Task CheckSite(SiteConfig site)
    {
        _logger.Information($"开始检查站点 [ {site.Name} ]", tag: site.LogName);
        var checks = site.Methods;
        if (checks?.Length == 0)
        {
            _logger.Warning($"站点 [ {site.Name} ]配置文件中未配置检查方法", tag: site.LogName);
            return;
        }
        var monitor = new MonitorHelper(site);

        var result = await monitor.CheckSiteAsync();
        switch (result.State)
        {
            case ResultState.Skip:
                _logger.Information($"站点[ {site.Name} ]: {result.Msg}", tag: site.LogName);
                return;
            case ResultState.Fail:
            case ResultState.Exception:
                _logger.Warning($"站点[ {site.Name}: {site.Url} ] 检查结果: {result.Msg}", tag: site.LogName);
                await NotifyTg($"站点[ {site.Name}: {site.Url} ]{Environment.NewLine}检查结果: {Environment.NewLine}{result.Msg}");
                break;
            case ResultState.Success:
                _logger.Information($"站点 [ {site.Name} ] 检查结果: {result.Msg}", tag: site.LogName);
                break;
        }
    }

    private async Task NotifyTg(string message)
    {
        var groups = _runConfig.GroupIds.Split(',') ?? null;

        if (groups?.Length == 0)
        {
            _logger.Error("通知服务配置错误,未配置通知接收ID");
            return;
        }
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
                        await Task.Delay(30_000);
                        continue;
                    }
                    await Task.Delay((int)nextTime);
                    foreach (var site in checkSites)
                    {
                        _ = Task.Factory.StartNew(() => CheckSite(site));
                    }
                }
                await Task.Delay(10_000);
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
