using Newtonsoft.Json;
using SiteMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiteMonitor.Helpers
{
    public class MonitorHelper : HttpBase
    {
        public SiteConfig Site { get; }
        public MonitorHelper(SiteConfig site)
        {
            Site = site;
            if (!string.IsNullOrWhiteSpace(Site.Headers))
            {
                try
                {
                    base.Headers = JsonConvert.DeserializeObject<List<Headers>>(Site.Headers) ?? Enumerable.Empty<Headers>();
                }
                catch
                {
                    // 不用处理异常，内容为空时作为空协议头处理
                }
            }
        }

        public async Task<ResultInfo> CheckSiteAsync()
        {
            try
            {
                var checkError = CheckSiteConfig();
                if (checkError != null)
                {
                    return new ResultInfo { State = ResultState.Fail, Msg = checkError };
                }
                if (Site.Lasttime > 0 && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - Site.Lasttime < Site.Interval * 1000)
                {
                    return new ResultInfo { State = ResultState.Skip, Msg = checkError };
                }

                var sendRet = await SendRequestAsync();
                return CheckResponse(sendRet.usetime, sendRet.httpResponse);
            }
            catch (Exception ex)
            {
                return new ResultInfo { State = ResultState.Exception, Msg = ex.Message };
            }
        }

        private ResultInfo CheckResponse(long usetime, HttpResponseMessage httpResponse)
        {
            var httpContent = httpResponse.Content.ReadAsStringAsync().Result;
            List<Method> checkMethods;
            try
            {
                checkMethods = JsonConvert.DeserializeObject<List<Method>>(Site.Methods) ?? new List<Method>();
            }
            catch
            {
                return new ResultInfo { State = ResultState.Exception, Msg = "无法解析检查组，请检查当前配置" };
            }
            if (!checkMethods.Any())
            {
                return new ResultInfo { State = ResultState.Fail, Msg = "未配置有效的检查方式" };
            }

            var messages = new List<string>();
            foreach (var item in checkMethods)
            {
                switch (item.Type.ToLower())
                {
                    case "time":
                        if (usetime > int.Parse(item.Value))
                        {
                            messages.Add($"当前请求耗时:[ {usetime} ]已超标.");
                        }
                        break;
                    case "code":
                        if (item.Value == ((int)httpResponse.StatusCode).ToString())
                        {
                            messages.Add($"请求状态码: [ {httpResponse.StatusCode} ]");
                        }
                        break;

                    case "contains":
                        if (!httpContent.Contains(item.Value))
                        {
                            messages.Add($"内容包含检查值: [ {item.Value} ]");
                        }
                        break;

                    case "notcontains":
                        if (httpContent.Contains(item.Value))
                        {
                            messages.Add($"内容不包含值: [ {item.Value} ]");
                        }
                        break;
                    default:
                        break;
                }
            }
            if (messages.Any())
            {
                var message = string.Join(Environment.NewLine, messages);
                return new ResultInfo { State = ResultState.Exception, Msg = message };
            }
            return new ResultInfo { State = ResultState.Success, Msg = "检查结果都正常" };
        }

        private string? CheckSiteConfig()
        {
            if (Site.Methods == null || string.IsNullOrEmpty(Site.Url))
            {
                return "检查项目配置错误!";
            }
            
            if (string.IsNullOrWhiteSpace(Site.Url) || string.IsNullOrWhiteSpace(Site.Method) || !Site.Method.Equals("GET", StringComparison.OrdinalIgnoreCase) && !Site.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                return "检查项目或方式配置错误!";
            }
            return null;
        }
        private async Task<(long usetime, HttpResponseMessage httpResponse)> SendRequestAsync()
        {
            HttpResponseMessage httpResponse;
            var strattime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (Site.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                httpResponse = await GetAsync(new Uri(Site.Url), CancellationToken.None, Site.Cookies ?? "");
            }
            else
            {
                httpResponse = await PostAsync(new Uri(Site.Url), Site.Data ?? "", CancellationToken.None, Site.Cookies ?? "");
            }
            var usetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - strattime;
            return (usetime, httpResponse);
        }
    }
}
