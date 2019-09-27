using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Globalization;
using Microsoft.AnalysisServices.Tabular;

namespace AzureDataTools
{
    public static class SSAS
    {
        [FunctionName("SSAS")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string token = GetToken("https://login.windows.net/18907841-8c13-49e8-9fc3-7f8b8fcbc2f5"
            , "https://northeurope.asazure.windows.net"
            , "f2280bac-61b9-4feb-9f69-c4e5e5f32644"
            , "PgJ1hVBq2ClnLSag85/QCO-a-/?0NOwG");
            log.LogInformation("C# HTTP trigger function processed a request.");

            var connectionStringTemplate = "Provider=MSOLAP;Data Source=asazure://northeurope.asazure.windows.net/meetup;Initial Catalog=MeteoSampleModel;Password={0};Persist Security Info=True;Impersonation Level=Impersonate";
            var connectionString = string.Format(CultureInfo.InvariantCulture, connectionStringTemplate, token);
            //Connection to SSAS Tab
            Server cube_server = new Server();
            cube_server.Connect(connectionString);
            log.LogInformation("Connected to AAS Server");

            return (ActionResult)new OkResult();
        }

        //Static method for token retrieval
        public static string GetToken(string authority, string resource, string appId, string appSecret)
        {
            AuthenticationContext authContext = new AuthenticationContext(authority);
            ClientCredential credentials = new ClientCredential(appId, appSecret);
            var task = authContext.AcquireTokenAsync(resource, credentials);
            task.Wait();
            return task.Result.AccessToken;
        }
    }
}
