using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Functions.Infrastructure.Config;
using Functions.Infrastructure;
using System.Net.Http;
using Functions.Models;
using System.Collections.Generic;

using System.Linq;

namespace AzureDataTools
{
    public static class PowerBI
    {
        private static readonly AppSettings Settings =
             ServiceProviderConfiguration.GetServiceProvider().GetService<AppSettings>();

        [FunctionName("ExtractPowerBILogsFromO365AuditLogsAsync")]
        public static async Task<IActionResult> ExctractLog([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, ILogger log)
        {
            Timer timer = await req.Content.ReadAsAsync<Timer>();

            var records = await GetContentsByPeriodAsync(timer, log);

            log.LogInformation($"{records.Count} records found.");

            return new OkObjectResult(records);

        }
        public static async Task<List<Record>> GetContentsByPeriodAsync(Timer timer, ILogger log)
        {
            O365ManagementActivityClient client = new O365ManagementActivityClient();
            var subscriptions = await client.ListSubscriptionsAsync();
            var auditGeneralSubscription = subscriptions.FirstOrDefault(i => i.ContentType.Equals(O365ManagementActivityClient.AuditGeneralContentTypeName,
                StringComparison.InvariantCultureIgnoreCase));

            if (auditGeneralSubscription == null)
            {
                await client.StartSubscriptionAsync(O365ManagementActivityClient.AuditGeneralContentTypeName);
            }

            List<Content> contents = new List<Content>();
            List<Record> records = new List<Record>();

            contents.AddRange(await client.GetContentsAsync(O365ManagementActivityClient.AuditGeneralContentTypeName, timer.StartDate, timer.EndDate));

            log.LogInformation($"{timer.StartDate} - {timer.EndDate} loaded.");
            foreach (var content in contents)
            {
                records.AddRange(await client.GetPowerBIRecordsAsync(content));
            }

            return records;
        }

        public class Timer
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }

        }
    }
}
