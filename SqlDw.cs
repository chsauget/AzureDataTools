using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Sql.Fluent;
using Functions.Infrastructure.Config;
using System.Net.Http;
using Functions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AzureDataTools
{
    public static class SqlDw
    {
        [FunctionName("pause-datawarehouse")]
        public static async Task<IActionResult> Pause([HttpTrigger(AuthorizationLevel.Function, "post", Route = "datawarehouse/pause")]
            HttpRequestMessage req,
            ILogger log)
        {
            log.LogInformation($"{typeof(SqlDw).Name}/Pause triggered.");

            var dwhLocator = await req.Content.ReadAsAsync<DataWarehouseLocator>();
            var settings = ServiceProviderConfiguration.GetServiceProvider().GetService<AppSettings>();

            var warehouse = GetDataWarehouseReference(settings, dwhLocator);

            await warehouse.PauseDataWarehouseAsync();

            return new OkResult();
        }

        [FunctionName("resume-datawarehouse")]
        public static async Task<IActionResult> Resume(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "datawarehouse/resume")]
                    HttpRequestMessage req,
            ILogger log)
        {
            log.LogInformation($"{typeof(SqlDw).Name}/Resume triggered.");

            var dwhLocator = await req.Content.ReadAsAsync<DataWarehouseLocator>();
            var settings = ServiceProviderConfiguration.GetServiceProvider().GetService<AppSettings>();

            var warehouse = GetDataWarehouseReference(settings, dwhLocator);

            await warehouse.ResumeDataWarehouseAsync();

            return new OkResult();
        }

        [FunctionName("state-datawarehouse")]
        public static async Task<IActionResult> State(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "datawarehouse/state")]
                    HttpRequestMessage req,
            ILogger log)
        {
            log.LogInformation($"{typeof(SqlDw).Name}/State triggered.");

            var dwhLocator = await req.Content.ReadAsAsync<DataWarehouseLocator>();
            var settings = ServiceProviderConfiguration.GetServiceProvider().GetService<AppSettings>();

            var warehouse = GetDataWarehouseReference(settings, dwhLocator);
            return new OkObjectResult(new { Status = warehouse.Status });
        }
        private static ISqlWarehouse GetDataWarehouseReference(AppSettings settings, DataWarehouseLocator dwhLocator)
        {
            // Authenticate
            var credentials = SdkContext.AzureCredentialsFactory
                                        .FromServicePrincipal(settings.AzureAd.ClientId, settings.AzureAd.ClientSecret,
                                            settings.AzureAd.TenantId, AzureEnvironment.AzureGlobalCloud);

            var azure = Azure.Configure()
                             .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                             .Authenticate(credentials)
                             .WithSubscription(settings.AzureSubScriptionId);

            var warehouse = azure.SqlServers
                                 .GetByResourceGroup(settings.ResourceGroup, dwhLocator.SqlServer)
                                 .Databases
                                 .Get(dwhLocator.Database)
                                 .AsWarehouse();

            return warehouse;
        }

        public class DataWarehouseLocator
        {
            public string SqlServer { get; set; }
            public string Database { get; set; }
        }
    }
}
