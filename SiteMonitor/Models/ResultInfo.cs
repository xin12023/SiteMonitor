using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace SiteMonitor.Models
{
    public class ResultInfo
    {
        public ResultInfo(ResultState state, string msg, object result, int httpCode)
        {
            State = state;
            Msg = msg;
            Result = result;
            HttpCode = httpCode;
        }

        public ResultInfo()
        {
            this.State = ResultState.Success;
        }
        public ResultInfo(string msg, ResultState resultState = ResultState.Fail)
        {
            this.Msg = msg;
            this.State = resultState;
        }
        public ResultInfo(object? result = null)
        {
            this.State = ResultState.Success;
            this.Result = result;
        }
        public ResultState? State { get; set; }
        public string? Msg { get; set; }
        public object? Result { get; set; }
        public int? Count { get; set; }
        public int? HttpCode { get; set; }

        public static ResultInfo? FromString(string text)
        {
            try
            {
                JObject job = JObject.Parse(text);
                int v = job["state"].Value<int>();
                ResultInfo resultInfo = new ResultInfo()
                {
                    State = (ResultState)v,
                    Msg = job["msg"]?.Value<string>(),
                    Result = job["result"]?.Value<object>(),
                    Count = job["count"]?.Value<int?>(),
                    HttpCode = job["httpcode"]?.Value<int?>(),
                };
                return resultInfo;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    public enum ResultState
    {
        /// <summary>
        /// 异常
        /// </summary>
        [Display(Name = "异常")]
        Exception = -1,
        /// <summary>
        /// 失败
        /// </summary>
        [Display(Name = "失败")]
        Fail = 0,
        /// <summary>
        /// 成功
        /// </summary>
        [Display(Name = "成功")]
        Success = 1,
        /// <summary>
        /// 跳过的检查
        /// </summary>
        [Display(Name = "跳过")]
        Skip = 2,
    }

}
