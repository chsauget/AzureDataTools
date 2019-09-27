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
        public static string GetPBIToken(string PowerBILogin, string PowerBIPassword, string AuthenticationContextUrl, string PowerBIRessourceUrl, string ClientId)
        {
            UserCredential credential = new UserPasswordCredential(PowerBILogin, PowerBIPassword);

            // Authenticate using created credentials
            AuthenticationContext authenticationContext = new AuthenticationContext("https://login.microsoftonline.com/b2ed466b-1ad5-4ea5-b19e-10f2abc41e7c");

            Task<AuthenticationResult> authenticationResultTask = authenticationContext.AcquireTokenAsync(PowerBIRessourceUrl, ClientId, credential);

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
