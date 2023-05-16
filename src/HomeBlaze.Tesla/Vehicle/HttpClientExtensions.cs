using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using TeslaAuth;

namespace HomeBlaze.Tesla
{
    public static class HttpClientExtensions
    {
        public static async Task<Tokens?> AuthorizeAsync(
            this HttpClient httpClient, 
            string refreshToken, 
            string accessToken,
            DateTimeOffset tokenExpirationDate,
            CancellationToken cancellationToken)
        {
            Tokens? tokens = null;
           
            if (refreshToken != null && tokenExpirationDate - DateTimeOffset.Now < TimeSpan.FromHours(1))
            {
                var auth = new TeslaAuthHelper("HomeBlaze");
                tokens = await auth.RefreshTokenAsync(refreshToken, cancellationToken);
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return tokens;
        }
    }
}