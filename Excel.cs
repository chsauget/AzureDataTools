using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Functions.Infrastructure.Config;
using System.Net.Http;
using Functions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AzureDataTools
{
    public static class Excel
    {
        [FunctionName("load-excel-sharepoint")]
        public static async Task<IActionResult> LoadExcelSharepoint([HttpTrigger(AuthorizationLevel.Function, "post", Route = "load/excel/sharepoint")]
            HttpRequestMessage req,
            ILogger log)
        {
            log.LogInformation($"");

            await req.Content.ReadAsAsync<DataWarehouseLocator>();

            return new OkResult();
        }
        public class ExcelConfiguration
        {
            public string path { get; set; }
            public string sheet { get; set; }

            public SharepointCredential sharepointCredential { get; set; }

        }
        public class SharepointCredential
        {
            public string accountName
        }
    }
}
