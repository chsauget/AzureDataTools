using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AnalysisServices;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AzureDataToolsStandard
{
    public static class SSAS
    {
        [FunctionName("SSAS")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            string _token = string.Empty;
            try
            {
                var ssasQuery = await req.Content.ReadAsAsync<SsasQuery>();

                if(ssasQuery.AppId is null)
                {
                    _token = await GetMsiTokenAsync();
                }
                else
                {
                    _token = await GetAppToken(ssasQuery.TenantId,ssasQuery.AppId,ssasQuery.AppSecret);
                }

                Server cube_server = new Server();
                var connectionStringTemplate = ssasQuery.connectionString; //"Provider=MSOLAP;Data Source=asazure://northeurope.asazure.windows.net/meetup;Initial Catalog=MeteoSampleModel;User ID=;Password={0};Persist Security Info=True;Impersonation Level=Impersonate";
                var connectionString = string.Format(CultureInfo.InvariantCulture, connectionStringTemplate, _token);
                cube_server.Connect(connectionString);
                var results = cube_server.Execute(ssasQuery.query);
                cube_server.Disconnect();
                foreach (XmlaResult result in results)
                {
                    foreach (XmlaMessage message in result.Messages)
                    {
                        if (message.GetType().Name == "XmlaError")
                        {
                            return req.CreateResponse(HttpStatusCode.BadRequest, message);
                        }
                    }
                }
                return req.CreateResponse(HttpStatusCode.OK, results);
            }
            catch(Exception e)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, e.Message);
            }
             
        }
        public class SsasQuery
        {
            public string connectionString { get; set; }
            public string query { get; set; }
            public string callBackUri { get; set; }

            public string TenantId { get; set; }
            public string AppId { get; set; }     
            public string AppSecret { get; set; }

        }
        private static async Task<string> GetAppToken(string TenantId, string AppId, string appSecret)
        {
            string resourceURI = "https://*.asazure.windows.net";

            string authority = "https://login.windows.net/" + TenantId;
            AuthenticationContext ac = new AuthenticationContext(authority);

            ClientCredential cred = new ClientCredential(AppId, appSecret);
            AuthenticationResult ar = await ac.AcquireTokenAsync(resourceURI, cred);

            return ar.AccessToken;
        }
        public static async Task<string> GetMsiTokenAsync()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://*.asazure.windows.net");
            return accessToken;
        }
    }
}
