using System.Collections.Generic;
using System.Net.Http;

namespace nomsol.core.httprequest.models
{
    public class HttpCallModel
    {
        public string Url { get; set; }
        public string EndPoint { get; set; }
        public HttpMethod Method { get; set; }
        public Authenticator Authenticator { get; set; }
        public List<RequestParameters>? Parameters { get; set; }
    }

    public class Authenticator
    {
        public ApiAuthType AuthType { get; set; }
        public OAuthCredentials? OAuthCredentials { get; set; }
        public string? Token { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
    }

    public enum ApiAuthType
    {
        Header,
        Basic,
        Jwt,
        Oauth,
        None
    }

    public class RequestParameters
    {
        public ParameterType Type { get; set; }
        public Dictionary<string, string>? Values { get; set; }
        public object? requestBody { get; set; }
    }

    public enum ParameterType
    {
        Header,
        QueryString,
        RequestBody
    }

    public class HttpResponse
    {
        public string? Message { get; set; }
    }

    public class OAuthCredentials
    {
        public string? baseUrl { get; set; }
        public string? endPoint { get; set; }
        public string? clientId { get; set; }
        public string? clientSecret { get; set; }
        public string? authScope { get; set; }
    }
}
