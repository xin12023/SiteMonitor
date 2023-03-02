using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace System.Net
{
    public static class HttpRequestHelper
    {
        public static HttpRequestMessage AddStringContent(this HttpRequestMessage httpRequestMessage, string postData, string contentType = "application/x-www-form-urlencoded", string charSet = "UTF-8")
        {
            StringContent stringContent = new StringContent(postData);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            if (!string.IsNullOrEmpty(charSet))
            {
                stringContent.Headers.ContentType.CharSet = charSet;
            }
            httpRequestMessage.Content = stringContent;
            return httpRequestMessage;
        }
        public static HttpRequestMessage AddStringContent(this HttpRequestMessage httpRequestMessage, byte[] postData, string contentType = "application/x-www-form-urlencoded", string charSet = "UTF-8")
        {
            ByteArrayContent byteContent = new ByteArrayContent(postData);
            byteContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

            if (!string.IsNullOrEmpty(charSet))
            {
                byteContent.Headers.ContentType.CharSet = charSet;
            }
            httpRequestMessage.Content = byteContent;
            return httpRequestMessage;
        }
    }
}
