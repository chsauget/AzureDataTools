using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Functions.Infrastructure;
using Functions.Infrastructure.Config;
using Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.DependencyInjection;


namespace AzureDataToolsStandard
{
    public static class PowerBI
    {
        private static readonly AppSettings Settings =
             ServiceProviderConfiguration.GetServiceProvider().GetService<AppSettings>();

        [FunctionName("ExtractPowerBILogsFromO365AuditLogsAsync")]
        public static async Task<HttpResponseMessage> ExctractLog([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            Timer timer = await req.Content.ReadAsAsync<Timer>();

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

            log.Info($"{timer.StartDate} - {timer.EndDate} loaded.");
            foreach (var content in contents)
            {
                records.AddRange(await client.GetPowerBIRecordsAsync(content));
            }

            return req.CreateResponse(HttpStatusCode.OK, records);
        }

        public class Timer
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }

        }
    }
}
