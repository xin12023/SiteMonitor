namespace SiteMonitor.Models
{
    public class Method
    {
        /// <summary>
        /// 检查方式  Code 等于状态码, Time 最低耗时, Content 内容包含, NotContains 内容不包含
        /// </summary>
        public string? Type { get; set; }
        /// <summary>
        /// 检查方式的值
        /// </summary>
        public string? Value { get; set; }
    }


}
