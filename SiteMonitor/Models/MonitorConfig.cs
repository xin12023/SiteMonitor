namespace SiteMonitor.Models
{
    public class MonitorConfig
    {
        /// <summary>
        /// TG机器人TOKEN
        /// </summary>
        public string? BotToken { get; set; }
        /// <summary>
        /// 要通知的组列表
        /// </summary>
        public string[]? GroupIds { get; set; }
        /// <summary>
        /// 站点配置列表,配置的文件名,去掉json
        /// </summary>
        public string[]? SiteConfigs { get; set; }
        /// <summary>
        /// 通知方式 all 全部都通知, success 只通知成功的 , failed 只通知失败的
        /// </summary>
        public string? NotifyType { get; set; }
        /// <summary>
        /// 通知间隔,单位分钟
        /// </summary>
        public int NotifyInterval { get; set; }
        /// <summary>
        /// 最后通知时间
        /// </summary>
        public string? LastNotifyTime { get; set; }
        /// <summary>
        /// 程序运行cron表达式
        /// </summary>
        public string? RunCron { get; set; }
        /// <summary>
        /// 日志目录
        /// </summary>
        public string? LogPath { get; set; }
        /// <summary>
        /// 日志文件名格式化格式(日期的处理)
        /// </summary>
        public string? LogNameFormat { get; set; }
        /// <summary>
        /// 日志备份数量
        /// </summary>
        public int LogCopies { get; set; }
        /// <summary>
        /// 是否启用监控
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// 后台任务的检查间隔
        /// </summary>
        public int Interval { get; set; }
        /// <summary>
        /// 日志等级
        /// </summary>
        public int LogLevel { get; set; }
    }
}