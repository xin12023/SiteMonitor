namespace SiteMonitor.Helpers.Loger
{
    public class LogInfo
    {
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string? Tag { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string? Message { get; set; }
        /// <summary>
        /// 异常
        /// </summary>
        public Exception? Exception { get; set; }
        public LoggerType LoggerType { get; set; }
    }
    public enum LoggerType
    {
        /// <summary>
        /// 跟踪
        /// </summary>
        Trace = 0,
        /// <summary>
        /// 调试
        /// </summary>
        Debug = 1,
        /// <summary>
        /// 消息
        /// </summary>
        Information = 2,
        /// <summary>
        /// 警告
        /// </summary>
        Warning = 3,
        /// <summary>
        /// 错误
        /// </summary>
        Error = 4,
        /// <summary>
        /// 致命错误
        /// </summary>
        Fatal = 5
    }
}
