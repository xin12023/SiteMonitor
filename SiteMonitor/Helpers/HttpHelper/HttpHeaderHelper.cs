using System.Net.Http.Headers;

namespace System.Net
{
    public static class HttpHeaderHelper
    {
        public static HttpRequestHeaders AddUserAgent(this HttpRequestHeaders headers, string value = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)")
        {
            headers.Add("User-Agent", value);
            return headers;
        }
        public static HttpRequestHeaders AddAccept(this HttpRequestHeaders headers, string value = "*/*")
        {
            headers.Add("Accept", value);
            return headers;
        }
        public static HttpRequestHeaders AddCookie(this HttpRequestHeaders headers, string value = "*/*")
        {
            headers.Add("Cookie", value);
            return headers;
        }
    }
}
