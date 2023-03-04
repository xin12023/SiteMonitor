
using SqlSugar;

namespace SiteMonitor.Models
{
    [SugarTable("siteconfig")]

    public class SiteConfig
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        /// <summary>
        /// 站点名称
        /// </summary>
        [SugarColumn(Length = 128)]
        public string? Name { get; set; }
        /// <summary>
        /// 日志文件名
        /// </summary>
        [SugarColumn(Length = 32)]
        public string? LogName { get; set; }
        /// <summary>
        /// 请求方式
        /// </summary>
        [SugarColumn(Length = 32)]
        public string? Method { get; set; }
        /// <summary>
        /// POST 请求数据
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string? Data { get; set; }
        /// <summary>
        /// 请求URL
        /// </summary>
        [SugarColumn(Length = 128)]
        public string? Url { get; set; }
        /// <summary>
        /// Cookie
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string? Cookies { get; set; }
        /// <summary>
        /// 最后检查时间
        /// </summary>
        public long? Lasttime { get; set; }
        /// <summary>
        /// POST 请求数据类型
        /// </summary>
        [SugarColumn(Length = 128)]
        public string? ContentType { get; set; }
        /// <summary>
        /// 协议头
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string? Headers { get; set; }
        /// <summary>
        /// 检查间隔(秒)
        /// </summary>
        public int? Interval { get; set; }
        /// <summary>
        /// 检查内容列表
        /// </summary>
        [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string? Methods { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; } = false;
    }
}
