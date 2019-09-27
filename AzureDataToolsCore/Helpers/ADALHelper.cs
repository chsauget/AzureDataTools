using Microsoft.Azure;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Threading.Tasks;

namespace Functions.Helpers
{
    public static class ADALHelper
    {
        //Retrieve the ADAL Token  https://docs.microsoft.com/fr-fr/power-bi/developer/get-azuread-access-token#access-token-for-non-power-bi-users-app-owns-data
        public static TokenCredentials GetToken(string PowerBILogin, string PowerBIPassword, string AuthenticationContextUrl, string PowerBIRessourceUrl, string ClientId)
        {
            UserCredential credential = new UserPasswordCredential(PowerBILogin, PowerBIPassword);

            // Authenticate using created credentials
            AuthenticationContext authenticationContext = new AuthenticationContext("https://login.microsoftonline.com/b2ed466b-1ad5-4ea5-b19e-10f2abc41e7c");

            Task <AuthenticationResult> authenticationResultTask = authenticationContext.AcquireTokenAsync(PowerBIRessourceUrl, ClientId, credential);

            AuthenticationResult authenticationResult = authenticationResultTask.Result;
            if (authenticationResult == null)
            {
                throw new Exception("Authentication Failed.");
                throw new Exception("Authentication Failed.");
            }
            else
            {
                return new TokenCredentials(authenticationResult.AccessToken, "Bearer");
            }

        }

        //Retrieve the ADAL Token 
        public static string GetSSASToken(string resourceURI, string authority, string clientId, string AppSecret)
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

        //Static method for token retrieval
        public static TokenCloudCredentials GetTokenCredentials(string authority, string resource, string appId, string appSecret, string subscriptionId)
        {
            AuthenticationContext authContext = new AuthenticationContext("https://login.microsoftonline.com/b2ed466b-1ad5-4ea5-b19e-10f2abc41e7c");
            ClientCredential credentials = new ClientCredential(appId, appSecret);
            var task = authContext.AcquireTokenAsync("https://management.core.windows.net/", credentials);
            task.Wait();
            TokenCloudCredentials _credentials = new TokenCloudCredentials(subscriptionId, task.Result.AccessToken);
            return _credentials;
        }

        //Retrieve the ADAL Token 
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
