using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cassette;

[Serializable]
internal class Cassette
{
    public Request Request { get; set; }
    public Response Response { get; set; }
    public DateTimeOffset RecordedAt { get; set; }

    public byte[] ToByteArray()
    {
        return JsonSerializer.SerializeToUtf8Bytes(this);
    }

    public static async Task<Cassette> Record(HttpRequestMessage httpRequest, HttpResponseMessage httpResponse)
    {
        ArgumentNullException.ThrowIfNull(httpRequest);
        ArgumentNullException.ThrowIfNull(httpResponse);

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

        using (var hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA1))
        {
            hasher.AppendData(Encoding.UTF8.GetBytes(requestMethod + requestUri));

            if (request.Body != null && !request.Headers.ContainsKey(CassetteOptions.ExcludeRequestBody))
            {
                hasher.AppendData(request.Body);
            }

            return options.KeyPrefix is null ? "" : options.KeyPrefix + options.KeySeparator
                 + requestMethod + options.KeySeparator
                 + requestUri.Replace("http://", "http//").Replace("https://", "https//") + options.KeySeparator
                 + Convert.ToBase64String(hasher.GetHashAndReset());
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
    public byte[] Body { get; set; }
    public Version Version { get; set; }
}

[Serializable]
internal class Response
{
    public HttpStatusCode Status { get; set; }
    public string ReasonPhrase { get; set; }
    public Dictionary<string, IEnumerable<string>> Headers { get; set; }
    public Dictionary<string, IEnumerable<string>> ContentHeaders { get; set; }
    public byte[] Body { get; set; }
    public Version Version { get; set; }
}
