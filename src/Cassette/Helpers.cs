using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Cassette
{
    internal static class Helpers
    {
        public static async Task<string> GetCassetteKey(this HttpRequestMessage httpRequest, CassetteOptions options)
        {
            var request = await httpRequest.ToRequest();
            return Cassette.GetKey(request, options);
        }

        public static async Task<Request> ToRequest(this HttpRequestMessage httpRequest)
        {
            string requestBody = await httpRequest.Content.ToStringAsync();
            var request = new Request
            {
                Method = httpRequest.Method.ToString(),
                Uri = httpRequest.RequestUri.ToString(),
                Version = httpRequest.Version,
                Headers = httpRequest.Headers.ToDictionary(x => x.Key, v => v.Value),
                Body = requestBody
            };
            if (httpRequest.Content != null)
            {
                request.ContentHeaders = httpRequest.Content.Headers.ToDictionary(x => x.Key, v => v.Value);
            }

            return request;
        }

        public static async Task<Response> ToResponse(this HttpResponseMessage httpResponse)
        {
            string responseBody = await httpResponse.Content.ToStringAsync();

            return new Response
            {
                Status = httpResponse.StatusCode,
                ReasonPhrase = httpResponse.ReasonPhrase,
                Version = httpResponse.Version,
                Headers = httpResponse.Headers.ToDictionary(x => x.Key, v => v.Value),
                ContentHeaders = httpResponse.Content.Headers.ToDictionary(x => x.Key, v => v.Value),
                Body = responseBody
            };
        }

        public static HttpResponseMessage Replay(this byte[] bytes) => bytes.ToCassette().Replay();

        public static HttpRequestMessage ToHttpRequestMessage(this Request request)
        {
            var httpRequest = new HttpRequestMessage
            {
                Method = new HttpMethod(request.Method),
                RequestUri = new Uri(request.Uri),
                Version = request.Version,
                Content = request.Body.ToHttpContent()
            };

            foreach (var header in request.Headers)
            {
                httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (httpRequest.Content != null)
            {
                foreach (var header in request.ContentHeaders)
                {
                    httpRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return httpRequest;
        }

        public static HttpContent ToHttpContent(this string body)
        {
            if (!string.IsNullOrEmpty(body))
            {
                return new ByteArrayContent(Encoding.UTF8.GetBytes(body));
            }

            return null;
        }

        private static Cassette ToCassette(this byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                return (Cassette)formatter.Deserialize(stream);
            }
        }

        private static async Task<string> ToStringAsync(this HttpContent content)
        {
            if (content is null)
            {
                return null;
            }

            return await content.ReadAsStringAsync();
        }
    }
}
