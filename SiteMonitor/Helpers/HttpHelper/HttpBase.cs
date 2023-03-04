using SiteMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SiteMonitor.Helpers
{
    public class HttpBase : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IWebProxy? _webProxy;
        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36";

        public string AccessToken { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public string UserAgent { get; set; } = DefaultUserAgent;

        public IEnumerable<Headers> Headers { get; set; } = Enumerable.Empty<Headers>();

        public int TimeoutSeconds { get; set; } = 60;

        public HttpBase(IWebProxy? webProxy = null)
        {
            _webProxy = webProxy;
            _httpClient = _webProxy != null ? new HttpClient(new HttpClientHandler { UseProxy = true, Proxy = webProxy }, true) : new HttpClient();
        }


        public async Task<HttpResponseMessage> PostAsync(Uri uri, string content, CancellationToken cancellationToken, string cookie = null, string contentType = "application/x-www-form-urlencoded", string charSet = "utf-8")
        {
            return await SendAsync(HttpMethod.Post, uri, new StringContent(content, Encoding.GetEncoding(charSet), contentType), contentType, charSet,  cancellationToken, cookie);
        }

        public async Task<HttpResponseMessage> PostAsync(Uri uri, byte[] body, CancellationToken cancellationToken, string? cookie = null, string contentType = "application/x-www-form-urlencoded", string charSet = "utf-8")
        {
            return await SendAsync(HttpMethod.Post, uri, new ByteArrayContent(body), contentType, charSet, cancellationToken, cookie);
        }

        public async Task<HttpResponseMessage> PostAsync(Uri uri, ByteArrayContent byteArrayContent, CancellationToken cancellationToken, string cookie = null)
        {
            return await SendAsync(HttpMethod.Post, uri, byteArrayContent, byteArrayContent.Headers.ContentType.MediaType, null, cancellationToken, cookie);
        }

        public async Task<HttpResponseMessage> GetAsync(Uri uri, CancellationToken cancellationToken, string? cookie = null)
        {
            return await SendAsync(HttpMethod.Get, uri, null, null, null, cancellationToken, cookie);
        }

        private async Task<HttpResponseMessage> SendAsync(HttpMethod httpMethod, Uri uri, HttpContent content, string contentType, string charSet, CancellationToken cancellationToken = default, string cookie = null)
        {
            try
            {
                uri ??= new Uri(string.Empty, UriKind.Relative);
                using var httpRequestMessage = new HttpRequestMessage(httpMethod, uri);
                if (!string.IsNullOrWhiteSpace(cookie))
                {
                    httpRequestMessage.Headers.Add("Cookie", cookie);
                }
                if (content != null)
                {
                    var parsedContentType = MediaTypeHeaderValue.Parse(contentType);
                    content.Headers.ContentType = charSet is null ? parsedContentType : new MediaTypeHeaderValue(parsedContentType.MediaType) { CharSet = charSet };
                    httpRequestMessage.Content = content;
                }
                AddHeadersToRequest(httpRequestMessage);
                return await _httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void AddHeadersToRequest(HttpRequestMessage httpRequestMessage)
        {
            httpRequestMessage.Headers.UserAgent.ParseAdd(UserAgent);

            if (!string.IsNullOrWhiteSpace(AccessToken))
            {
                httpRequestMessage.Headers.Add("AccessToken", AccessToken);
            }

            if (!string.IsNullOrWhiteSpace(Token))
            {
                httpRequestMessage.Headers.Add("Token", Token);
            }

            if (Headers.Any() && Headers?.Where(h => !string.IsNullOrWhiteSpace(h.Name)).Any() == true)
            {
                foreach (var item in Headers.Where(h => !string.IsNullOrWhiteSpace(h.Name)))
                {
                    httpRequestMessage.Headers.Add(item.Name, item.Value);
                }
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }


    public class Headers
    {
        public string? Name { get; set; }
        /// <summary>
        /// 检查方式的值
        /// </summary>
        public string? Value { get; set; }
    }
}
