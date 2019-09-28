using Microsoft.Azure;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

using System;
using System.Threading.Tasks;

namespace Functions.Helpers
{
    public static class ADALHelper
    {
        public static async Task<string> GetAppToken(string authority, string resourceURI, string clientId, string AppSecret)
        {
            ClientCredential credential = new ClientCredential(clientId, AppSecret);
            // Authenticate using created credentials
            AuthenticationContext authenticationContext = new AuthenticationContext(authority);

            AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenAsync(resourceURI, credential);

            if (authenticationResult == null)
            {
                throw new Exception("Authentication Failed.");
            }
            else
            {
                return authenticationResult.AccessToken;
            }
        }
      
        public static async Task<string> GetMsiTokenAsync(string Resource)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(Resource);
            return accessToken;
        }
    }
}
