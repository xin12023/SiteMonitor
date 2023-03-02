namespace SiteMonitor.Models
{
    public class SiteConfig
    {
        /// <summary>
        /// 站点名称
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 日志文件名
        /// </summary>
        public string? LogName { get; set; }
        /// <summary>
        /// 请求方式
        /// </summary>
        public string? Method { get; set; }
        /// <summary>
        /// POST 请求数据
        /// </summary>
        public string? Data { get; set; }
        /// <summary>
        /// 请求URL
        /// </summary>
        public string? Url { get; set; }
        /// <summary>
        /// Cookie
        /// </summary>
        public string? Cookies { get; set; }
        /// <summary>
        /// 最后检查时间
        /// </summary>
        public string? Lasttime { get; set; }
        /// <summary>
        /// POST 请求数据类型
        /// </summary>
        public string? ContentType { get; set; }
        /// <summary>
        /// 协议头
        /// </summary>
        public Headers[]? Headers { get; set; }
        /// <summary>
        /// 检查间隔(秒)
        /// </summary>
        public int Interval { get; set; }
        /// <summary>
        /// 当前检查次数
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 检查总耗时
        /// </summary>
        public int CountTime { get; set; }
        /// <summary>
        /// 平均耗时
        /// </summary>
        public int AverageTime { get; set; }
        /// <summary>
        /// 检查内容列表
        /// </summary>
        public Method[]? Methods { get; set; }
    }
    public class Method
    {
        /// <summary>
        /// 检查方式  Code 状态码, Time 耗时, Content 内容包含, NotContains 内容不包含
        /// </summary>
        public string? Type { get; set; }
        /// <summary>
        /// 检查方式的值
        /// </summary>
        public string? Value { get; set; }
        /// <summary>
        /// 周期错误次
        /// </summary>
        public int Error { get; set; }
    }

    public class Headers
    {
        /// <summary>
        /// 检查方式  Code 状态码, Time 耗时, Content 内容包含, NotContains 内容不包含
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 检查方式的值
        /// </summary>
        public string? Value { get; set; }

    }


}
