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
                base.Headers = _site.Headers;
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
                if (_site.Methods == null || string.IsNullOrEmpty(_site.Url))
                {
                    return new ResultInfo { State = ResultState.Fail, Msg = "检查项目配置错误!" };
                }
                if (!string.IsNullOrEmpty(_site.Lasttime))
                {
                    long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    long last = long.Parse(_site.Lasttime);
                    if (now - last < _site.Interval * 1000)
                    {
                        return (new ResultInfo { Msg = "上次检查时间未到,跳过检查!", State = ResultState.Skip });
                    }
                    _site.Lasttime = now.ToString();
                }
                if (string.IsNullOrEmpty(_site.Method) || _site.Method.ToUpper() is not "GET" and not "POST")
                {
                    return new ResultInfo { State = ResultState.Fail, Msg = "检查方式配置错误!" };
                }
                HttpResponseMessage? httpResponse;
                if (_site.Method.ToUpper() == "GET")
                {

                    httpResponse = await base.GetAsync(new Uri(_site.Url), CancellationToken.None, _site.Cookies ?? "");
                }
                else
                {
                    httpResponse = await base.PostAsync(new Uri(_site.Url), _site.Data ?? "", CancellationToken.None, _site.Cookies ?? "");
                }
                var iserror = false;
                var message = string.Empty;
                foreach (var item in _site.Methods)
                {
                    switch (item.Type.ToLower())
                    {
                        case "code":
                            if (item.Value == httpResponse.StatusCode.ToString())
                            {
                                iserror = true;
                                message += $"当前请求状态码:[ {httpResponse.StatusCode} ]{Environment.NewLine}";
                            }
                            break;
                        case "contains":
                            if (!httpResponse.Content.ToString().Contains(item.Value))
                            {
                                iserror = true;
                                message += $"内容包含检查:[ {item.Value} ]{Environment.NewLine}";
                            }
                            break;
                        case "notcontains":
                            if (httpResponse.Content.ToString().Contains(item.Value))
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
