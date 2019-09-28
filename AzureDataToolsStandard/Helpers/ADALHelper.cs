using Microsoft.Azure;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

using System;
using System.Threading.Tasks;

namespace Functions.Helpers
{
    public static class ADALHelper
    {
        public static string GetManagementToken(string resourceURI, string authority, string clientId, string AppSecret)
        {
            ClientCredential credential = new ClientCredential(clientId, AppSecret);
            // Authenticate using created credentials
            AuthenticationContext authenticationContext = new AuthenticationContext(authority);

            Task<AuthenticationResult> authenticationResultTask = authenticationContext.AcquireTokenAsync(resourceURI, credential);

            AuthenticationResult authenticationResult = authenticationResultTask.Result;
            if (authenticationResult == null)
            {
                throw new Exception("Authentication Failed.");
            }
            else
            {
                return authenticationResult.AccessToken;
            }
        }
    }
}
