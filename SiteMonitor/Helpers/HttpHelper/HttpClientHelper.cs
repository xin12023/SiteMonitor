using System.Net.Http;
using System.Security.Authentication;

namespace System.Net
{
    public static class HttpClientHelper
    {
        static HttpClientHelper()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.Expect100Continue = false;
        }
        public static HttpClientHandler CreateClientHandler(bool useCookies = true)
        {
            return new HttpClientHandler
            {
                UseProxy = false,
                UseCookies = useCookies,
                AllowAutoRedirect = false,
                ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls,
                AutomaticDecompression = DecompressionMethods.GZip,
            };
        }
        public static HttpClient Create(HttpClientHandler httpClientHandler = null, int timeOut = 30)
        {
            if (httpClientHandler == null)
            {
                httpClientHandler = CreateClientHandler();
            }
            return new HttpClient(httpClientHandler, true)
            {
                Timeout = System.TimeSpan.FromSeconds(timeOut),
            };
        }
    }
}
