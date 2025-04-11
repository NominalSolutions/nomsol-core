using System.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using nomsol.core.httprequest.models;

namespace nomsol.core.httprequest
{
    public class HttpRequestHandler : IDisposable
    {
        private readonly HttpClient _client;
        private bool _disposed = false;

        public HttpRequestHandler(HttpCallModel apiRequest)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(apiRequest.Url),
                Timeout = TimeSpan.FromMinutes(3)
            };

            // Setup authenticator
            if (apiRequest.Authenticator != null)
            {
                switch (apiRequest.Authenticator.AuthType)
                {
                    case ApiAuthType.Basic:
                        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiRequest.Authenticator.User}:{apiRequest.Authenticator.Password}"));
                        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                        break;
                    case ApiAuthType.Jwt:
                        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiRequest.Authenticator.Token);
                        break;
                    case ApiAuthType.Header:
                        _client.DefaultRequestHeaders.Add("Authorization", apiRequest.Authenticator.Token);
                        break;
                    case ApiAuthType.Oauth:
                        var authenticator = new Authenticator(
                        apiRequest.Authenticator.OAuthCredentials.baseUrl,
                        apiRequest.Authenticator.OAuthCredentials.endPoint,
                        apiRequest.Authenticator.OAuthCredentials.clientId,
                        apiRequest.Authenticator.OAuthCredentials.clientSecret,
                        apiRequest.Authenticator.OAuthCredentials.authScope);
                        var token = authenticator.GetAuthenticationHeaderAsync().Result;
                        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); 
                        break;
                    default:
                        break;
                }
            }
        }

        public async Task<T> TriggerHttpRequest<T>(HttpCallModel apiRequest)
        {
            try
            {
                var requestUri = apiRequest.EndPoint;
                var requestMessage = new HttpRequestMessage(apiRequest.Method, requestUri);

                // Handle parameters
                if (apiRequest.Parameters != null)
                {
                    foreach (var param in apiRequest.Parameters)
                    {
                        switch (param.Type)
                        {
                            case ParameterType.Header:
                                foreach (var header in param.Values)
                                {
                                    requestMessage.Headers.Add(header.Key, header.Value);
                                }
                                break;
                            case ParameterType.QueryString:
                                var query = string.Join("&", param.Values.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                                requestMessage.RequestUri = new Uri($"{requestUri}?{query}", UriKind.RelativeOrAbsolute);
                                break;
                            case ParameterType.RequestBody:
                                if (param.requestBody != null)
                                {
                                    var jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
                                    var jsonContent = JsonSerializer.Serialize(param.requestBody, jsonOptions);
                                    requestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                                }
                                break;
                        }
                    }
                }

                var response = await _client.SendAsync(requestMessage);
                if (response.IsSuccessStatusCode)
                {
                    JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    return JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync(), jsonSerializerOptions) ?? throw new ArgumentException("Response content deserialization failed.");
                }
                else
                {
                    throw new HttpRequestException($"Request failed with status code {response.StatusCode} and message: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Request failed", ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _client.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
