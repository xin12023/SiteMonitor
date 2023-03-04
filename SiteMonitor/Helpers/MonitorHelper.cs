using Newtonsoft.Json;
using SiteMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SiteMonitor.Helpers
{
    public class MonitorHelper : HttpBase
    {
        public SiteConfig? _site { get; set; }

        public MonitorHelper(SiteConfig site)
        {
            _site = site;
            if (_site.Headers != null)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(_site.Headers))
                    {
                        base.Headers = JsonConvert.DeserializeObject<List<Headers>>(_site.Headers) ?? Enumerable.Empty<Headers>();
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public async Task<ResultInfo> CheckSiteAsync()
        {
            try
            {
                if (_site == null)
                {
                    return (new ResultInfo { Msg = "配置为空无法进行检查工作!", State = ResultState.Fail });
                }
                if (_site.Lasttime > 0 && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _site.Lasttime < _site.Interval * 1000)
                {
                    return (new ResultInfo { Msg = "暂时不在检查时间内!", State = ResultState.Skip });
                }
                if (_site.Methods == null || string.IsNullOrEmpty(_site.Url))
                {
                    return new ResultInfo { State = ResultState.Fail, Msg = "检查项目配置错误!" };
                }
                if (string.IsNullOrWhiteSpace(_site.Method) || _site.Method.ToUpper() is not "GET" and not "POST")
                {
                    return new ResultInfo { State = ResultState.Fail, Msg = "检查方式配置错误!" };
                }
                HttpResponseMessage? httpResponse;
                var strattime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (_site.Method.ToUpper() == "GET")
                {
                    httpResponse = await base.GetAsync(new Uri(_site.Url), CancellationToken.None, _site.Cookies ?? "");
                }
                else
                {
                    httpResponse = await base.PostAsync(new Uri(_site.Url), _site.Data ?? "", CancellationToken.None, _site.Cookies ?? "");
                }
                var usetime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - strattime;

                var iserror = false;
                var message = string.Empty;
                var httpContent = httpResponse.Content.ToString();
                List<Method>? checkMethods;
                try
                {
                    checkMethods = JsonConvert.DeserializeObject<List<Models.Method>>(_site.Methods);
                }
                catch (Exception)
                {
                    return new ResultInfo { State = ResultState.Exception, Msg = "无法处理的检查列表.请检查当前配置" };
                }
                if (checkMethods  == null)
                {
                    return new ResultInfo { State = ResultState.Fail, Msg = "未配置有效的检查方式" };
                }
                foreach (var item in checkMethods)
                {
                    switch (item.Type.ToLower())
                    {
                        case "time":

                            if (usetime > int.Parse(item.Value))
                            {
                                iserror = true;
                                message += $"当前请求耗时:[ {usetime} ]已超标.{Environment.NewLine}";
                            }
                            break;
                        case "code":
                            if (item.Value == httpResponse.StatusCode.ToString())
                            {
                                iserror = true;
                                message += $"当前请求状态码:[ {httpResponse.StatusCode} ]{Environment.NewLine}";
                            }
                            break;
                        case "contains":
                            if (!httpContent.Contains(item.Value))
                            {
                                iserror = true;
                                message += $"内容包含检查:[ {item.Value} ]{Environment.NewLine}";
                            }
                            break;
                        case "notcontains":
                            if (httpContent.Contains(item.Value))
                            {
                                iserror = true;
                                message += $"内容不包含:[ {item.Value} ]{Environment.NewLine}";
                            }
                            break;
                        default:
                            break;
                    }
                }
                if (iserror)
                {
                    return new ResultInfo { State = ResultState.Exception, Msg = message };
                }
                return new ResultInfo { State = ResultState.Success, Msg = message };
            }
            catch (Exception ex)
            {
                return new ResultInfo { State = ResultState.Exception, Msg = ex.Message };
            }
        }
    }
}
