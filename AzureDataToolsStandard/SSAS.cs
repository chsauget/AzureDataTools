using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Functions.Helpers;
using Functions.Infrastructure;
using Functions.Infrastructure.Config;
using Microsoft.AnalysisServices;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Extensions.DependencyInjection;

namespace AzureDataToolsStandard
{
    public static class SSAS
    {
        private static readonly AppSettings Settings =
             ServiceProviderConfiguration.GetServiceProvider().GetService<AppSettings>();
        [FunctionName("SSAS")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            string _token = string.Empty;
            try
            {
                var ssasQuery = await req.Content.ReadAsAsync<SsasQuery>();

                if(ssasQuery.AppId is null)
                {
                    _token = await ADALHelper.GetMsiTokenAsync(Settings.SSAS.Resource);
                }
                else
                {
                    _token = await ADALHelper.GetAppToken(Settings.AzureAd.AuthorityUrl, Settings.SSAS.Resource, ssasQuery.AppId,ssasQuery.AppSecret);
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
                return req.CreateResponse(HttpStatusCode.BadRequest, e.InnerException);
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

    }
}
