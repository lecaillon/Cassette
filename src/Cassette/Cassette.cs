using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cassette
{
    [Serializable]
    internal class Cassette
    {
        public Request Request { get; set; }
        public Response Response { get; set; }
        public DateTimeOffset RecordedAt { get; set; }

        public byte[] ToByteArray()
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, this);
                return stream.ToArray();
            }
        }

        public static async Task<Cassette> Record(HttpRequestMessage httpRequest, HttpResponseMessage httpResponse)
        {
            if (httpRequest is null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            if (httpResponse is null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            return new Cassette
            {
                Request = await httpRequest.ToRequest(),
                Response = await httpResponse.ToResponse(),
                RecordedAt = DateTimeOffset.UtcNow
            };
        }

        public HttpResponseMessage Replay()
        {
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = Response.Status,
                Version = Response.Version,
                ReasonPhrase = Response.ReasonPhrase,
                RequestMessage = Request.ToHttpRequestMessage(),
                Content = Response.Body.ToHttpContent()
            };

            foreach (var header in Response.Headers)
            {
                httpResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (httpResponse.Content != null)
            {
                foreach (var header in Response.ContentHeaders)
                {
                    httpResponse.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return httpResponse;
        }

        public static string GetKey(Request request, CassetteOptions options)
        {
            if (request.Headers.ContainsKey(CassetteOptions.NoRecord))
            {
                return null;
            }

            string requestMethod = request.Method;
            string requestUri = request.Headers.ContainsKey(CassetteOptions.ExcludeLastUriSegment) ? request.GetUriWithoutLastSegment() : request.Uri;
            string requestBody = request.Headers.ContainsKey(CassetteOptions.ExcludeRequestBody) ? string.Empty : request.Body;
            var bytes = Encoding.UTF8.GetBytes(requestMethod + requestUri + requestBody);

            using (var sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(bytes);
                return options.KeyPrefix is null ? "" : options.KeyPrefix + options.KeySeparator
                     + requestMethod + options.KeySeparator
                     + requestUri.Replace("http://", "http//").Replace("https://", "http//") + options.KeySeparator
                     + Convert.ToBase64String(hash);
            }
        }
    }

    [Serializable]
    internal class Request
    {
        public string Method { get; set; }
        public string Uri { get; set; }
        public Dictionary<string, IEnumerable<string>> Headers { get; set; }
        public Dictionary<string, IEnumerable<string>> ContentHeaders { get; set; }
        public string Body { get; set; }
        public Version Version { get; set; }
    }

    [Serializable]
    internal class Response
    {
        public HttpStatusCode Status { get; set; }
        public string ReasonPhrase { get; set; }
        public Dictionary<string, IEnumerable<string>> Headers { get; set; }
        public Dictionary<string, IEnumerable<string>> ContentHeaders { get; set; }
        public string Body { get; set; }
        public Version Version { get; set; }
    }
}
