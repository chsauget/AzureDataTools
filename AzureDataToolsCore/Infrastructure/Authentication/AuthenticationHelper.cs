namespace Functions.Infrastructure.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;


    internal static class AuthenticationHelper
    {
        internal static async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationContext authContext)
        {
            var oauthEndpoint = new Uri(authContext.AuthorityUrl);

            using (var client = new HttpClient())
            {
                var result = await client.PostAsync(oauthEndpoint, new FormUrlEncodedContent(new[]
                {
                        new KeyValuePair<string, string>("resource", authContext.ResourceUrl),
                        new KeyValuePair<string, string>("client_id", authContext.ClientId),
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("username", authContext.Username),
                        new KeyValuePair<string, string>("password", authContext.Password),
                        new KeyValuePair<string, string>("client_secret", authContext.ClientSecret),
                    }));

                var content = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AuthenticationResponse>(content);
            }
        }
    }
}