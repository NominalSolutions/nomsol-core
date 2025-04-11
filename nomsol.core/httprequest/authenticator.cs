using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace nomsol.core.httprequest
{
    public class Authenticator
    {
        private readonly string _baseUrl;
        private readonly string _endPoint;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _authScope;
        private readonly string _grantType;
        private string Token { get; set; }

        public Authenticator(string baseUrl, string endPoint, string clientId, string clientSecret, string authScope, string grantType = "client_credentials")
        {
            _baseUrl = baseUrl;
            _endPoint = endPoint;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _authScope = authScope;
            _grantType = grantType;
        }

        public async Task<string> GetAuthenticationHeaderAsync()
        {
            var token = string.IsNullOrEmpty(Token) ? await GetToken() : Token;
            token = token.StartsWith("Bearer ")
                            ? token.Replace("Bearer ", string.Empty)
                            : token;
            return token;
        }

        private async Task<string> GetToken()
        {
            string tokenEndpoint = $"{_baseUrl}{_endPoint}";

            using var client = new HttpClient();
            var parameters = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "scope", _authScope },
                { "grant_type", _grantType }
            };

            var content = new FormUrlEncodedContent(parameters);
            HttpResponseMessage response = await client.PostAsync(tokenEndpoint, content);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException("Failed to obtain token.");

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

            Token = tokenResponse != null ? $"{tokenResponse.TokenType} {tokenResponse.AccessToken}" : throw new InvalidOperationException("Invalid token response.");
            return Token;
        }
    }

    public class TokenResponse
    {
        [JsonPropertyName("token_type")]
        public string TokenType { get; private set; }

        [JsonPropertyName("access_token")]
        public string AccessToken { get; private set; }

        public TokenResponse(string tokenType, string accessToken)
        {
            TokenType = tokenType;
            AccessToken = accessToken;
        }

        // Implement value-based equality
        public override bool Equals(object obj)
        {
            return obj is TokenResponse other &&
                   TokenType == other.TokenType &&
                   AccessToken == other.AccessToken;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TokenType, AccessToken);
        }

        // Method to mimic 'with' functionality
        public TokenResponse With(string tokenType = null, string accessToken = null)
        {
            return new TokenResponse(tokenType ?? TokenType, accessToken ?? AccessToken);
        }
    }
}
