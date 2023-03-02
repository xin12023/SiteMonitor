using SiteMonitor.Models;

namespace SiteMonitor.Helpers
{
    public class TgHelper : HttpBase
    {
        public string? Token { get; set; }

        public async Task<ResultInfo> SendNotifyAsync(string message, string chatId)
        {
            if (string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(chatId))
            {
                return new ResultInfo() { State = ResultState.Fail, Msg = "TG通信配置不正确!" };
            }
            try
            {
                string url = $"https://api.telegram.org/bot{Token}/sendMessage?parse_mode=Markdown";
                string postData = $"{{\"chat_id\":\"{chatId}\",\"text\":\"{message}\"}}";
                using HttpResponseMessage httpResponse = await PostAsync(new Uri(url), postData,CancellationToken.None, contentType: "application/json", charSet: "UTF-8");
                string result = await httpResponse.Content.ReadAsStringAsync();
                return new ResultInfo { State = ResultState.Success, Msg = "发送成功", Result = result };
            }
            catch (Exception ex)
            {
                return new ResultInfo() { State = ResultState.Exception, Msg = ex.Message };
            }
        }
    }
}
