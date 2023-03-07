namespace SiteMonitor.Models
{
    public class ReturnResult
    {

        /// <summary>
        ///数据状态一切正常的状态码 200:成功，400：失败
        /// </summary>
        public int Code { get; set; } = 200;

        /// <summary>
        /// 状态信息
        /// </summary>
        public String Msg { get; set; } = "成功";

        /// <summary>
        /// 数据详情
        /// </summary>
        public Object? Data { get; set; }

        /// <summary>
        /// 数据总条数
        /// </summary>
        public int Count { get; set; } = 0;
    }
}
