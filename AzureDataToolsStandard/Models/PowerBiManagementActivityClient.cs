using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Functions.Helpers;
using Functions.Infrastructure;
using Functions.Infrastructure.Config;
using Microsoft.Extensions.DependencyInjection;

namespace Functions.Models
{
    public class O365ManagementActivityClient
    {
        public static readonly string AuditGeneralContentTypeName = "Audit.General";
        public static readonly string PowerBIWorkloadName = "PowerBI";
        public static readonly int RetriesMaxCount = 5;
        public static readonly double RetryWaitInSec = 1;
        private static readonly AppSettings Settings =
                ServiceProviderConfiguration.GetServiceProvider().GetService<AppSettings>();
        /// <summary>
        /// HTTP client.
        /// </summary>
        private HttpClient Client { get; set; }

        /// <summary>
        /// New instance of the O365ManagementActivityClient class.
        /// </summary>
        /// <param name="credentials"></param>
        public O365ManagementActivityClient()
        {
            Client = new HttpClient();
            Client.BaseAddress = new Uri(Settings.O365AuditLogs.BaseAddress);
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                ADALHelper.GetManagementToken(Settings.O365AuditLogs.Resource, Settings.AzureAd.Authority, Settings.O365AuditLogs.AppId, Settings.O365AuditLogs.AppSecret
                    )
                );
        }

        /// <summary>
        /// Get the tenant audit logs content list for the specified content type.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<Content>> GetContentsAsync(string contentType)
        {
            HttpResponseMessage response = null;
            int retries = 0;

            do
            {
                response = await Client.GetAsync($"subscriptions/content?contentType={contentType}");

                if (response.IsSuccessStatusCode)
                {
                    var contents = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IList<Content>>(contents);
                }
                else
                {
                    await Task.Delay((int)((int)(RetryWaitInSec * 1000)));
                }

                retries = retries + 1;
            } while (!response.IsSuccessStatusCode && retries < RetriesMaxCount);

            var content = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<RequestError>(content);
            var message = error != null ? error.Error.Message : response.ReasonPhrase;

            throw new Exception($"Cannot get content list. {message}");
        }

        /// <summary>
        /// Get the tenant audit logs content list created between the specified start and end time for the specified content type.
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async Task<IList<Content>> GetContentsAsync(string contentType, DateTime startTime, DateTime endTime)
        {
            HttpResponseMessage response = null;
            int retries = 0;
            var timeFormat = "yyyy-MM-ddTHH:mm";

            do
            {
                response = await Client.GetAsync($"subscriptions/content?contentType={contentType}&startTime={startTime.ToString(timeFormat)}&endTime={endTime.ToString(timeFormat)}");

                if (response.IsSuccessStatusCode)
                {
                    var contents = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IList<Content>>(contents);
                }
                else
                {
                    await Task.Delay((int)(RetryWaitInSec * 1000));
                }

                retries = retries + 1;
            } while (!response.IsSuccessStatusCode && retries < RetriesMaxCount);

            var content = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<RequestError>(content);
            var message = error != null ? error.Error.Message : response.ReasonPhrase;

            throw new Exception($"Cannot get content list. {message}");
        }

        /// <summary>
        /// Starts the subscription to the specified content type.
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<Subscription> StartSubscriptionAsync(string contentType)
        {
            HttpResponseMessage response = null;
            int retries = 0;

            do
            {
                response = await Client.PostAsync($"subscriptions/start?contentType={contentType}", new StringContent(string.Empty));

                if (response.IsSuccessStatusCode)
                {
                    var contents = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Subscription>(contents);
                }
                else
                {
                    await Task.Delay((int)(RetryWaitInSec * 1000));
                }

                retries = retries + 1;
            } while (!response.IsSuccessStatusCode && retries < RetriesMaxCount);

            var content = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<RequestError>(content);
            var message = error != null ? error.Error.Message : response.ReasonPhrase;

            throw new Exception($"Cannot start the subscription. {message}");
        }

        /// <summary>
        /// Stops the subscription to the specified content type.
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task StopSubscriptionAsync(string contentType)
        {
            HttpResponseMessage response = null;
            int retries = 0;

            do
            {
                response = await Client.PostAsync($"subscriptions/stop?contentType={contentType}", new StringContent(string.Empty));

                if (!response.IsSuccessStatusCode)
                    await Task.Delay((int)(RetryWaitInSec * 1000));

                retries = retries + 1;
            } while (!response.IsSuccessStatusCode && retries < RetriesMaxCount);

            var content = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<RequestError>(content);
            var message = error != null ? error.Error.Message : response.ReasonPhrase;

            throw new Exception($"Cannot start the subscription. {message}");
        }

        /// <summary>
        /// List the current subscriptions.
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<IList<Subscription>> ListSubscriptionsAsync()
        {
            HttpResponseMessage response = null;
            int retries = 0;

            do
            {
                response = await Client.GetAsync($"subscriptions/list");

                if (response.IsSuccessStatusCode)
                {
                    var contents = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IList<Subscription>>(contents);
                }
                else
                {
                    await Task.Delay((int)(RetryWaitInSec * 1000));
                }

                retries = retries + 1;
            } while (!response.IsSuccessStatusCode && retries < RetriesMaxCount);

            var content = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<RequestError>(content);
            var message = error != null ? error.Error.Message : response.ReasonPhrase;

            throw new Exception($"Cannot get the subscriptions list. {message}");
        }

        /// <summary>
        /// Gets a content records.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<IList<Record>> GetRecordsAsync(Content content)
        {
            HttpResponseMessage response = null;
            int retries = 0;

            do
            {
                response = await Client.GetAsync(new Uri(content.ContentUri, UriKind.Absolute));

                if (response.IsSuccessStatusCode)
                {
                    var records = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<IList<Record>>(records);
                }
                else
                {
                    await Task.Delay((int)(RetryWaitInSec * 1000));
                }

                retries = retries + 1;
            } while (!response.IsSuccessStatusCode && retries < RetriesMaxCount);

            var reponseContent = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<RequestError>(reponseContent);
            var message = error != null ? error.Error.Message : response.ReasonPhrase;

            throw new Exception($"Cannot get records. {message}");
        }

        /// <summary>
        /// Gets a content records for the PowerBI workload.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<IList<Record>> GetPowerBIRecordsAsync(Content content)
        {
            HttpResponseMessage response = null;
            int retries = 0;

            do
            {
                response = await Client.GetAsync(new Uri(content.ContentUri, UriKind.Absolute));

                if (response.IsSuccessStatusCode)
                {
                    var recordsJson = await response.Content.ReadAsStringAsync();
                    var records = JsonConvert.DeserializeObject<IList<Record>>(recordsJson);
                    var powerBiRecords = records.Where(i => i.Workload.Equals(PowerBIWorkloadName, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    return powerBiRecords;
                }
                else
                {
                    await Task.Delay((int)(RetryWaitInSec * 1000));
                }

                retries = retries + 1;
            } while (!response.IsSuccessStatusCode && retries < RetriesMaxCount);

            var reponseContent = await response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<RequestError>(reponseContent);
            var message = error != null ? error.Error.Message : response.ReasonPhrase;

            throw new Exception($"Cannot get records. {message}");
        }
    }
    public class Record
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("RecordType")]
        public int RecordType { get; set; }

        [JsonProperty("CreationTime")]
        public DateTime CreationTime { get; set; }

        [JsonProperty("Operation")]
        public string Operation { get; set; }

        [JsonProperty("OrganizationId")]
        public string OrganizationId { get; set; }

        [JsonProperty("UserType")]
        public int UserType { get; set; }

        [JsonProperty("UserKey")]
        public string UserKey { get; set; }

        [JsonProperty("Workload")]
        public string Workload { get; set; }

        [JsonProperty("UserId")]
        public string UserId { get; set; }

        [JsonProperty("ClientIP")]
        public string ClientIP { get; set; }

        [JsonProperty("UserAgent")]
        public string UserAgent { get; set; }

        [JsonProperty("Activity")]
        public string Activity { get; set; }

        [JsonProperty("ItemName")]
        public string ItemName { get; set; }

        [JsonProperty("WorkSpaceName")]
        public string WorkSpaceName { get; set; }

        [JsonProperty("DatasetName")]
        public string DatasetName { get; set; }

        [JsonProperty("ReportName")]
        public string ReportName { get; set; }

        [JsonProperty("WorkspaceId")]
        public string WorkspaceId { get; set; }

        [JsonProperty("ObjectId")]
        public string ObjectId { get; set; }

        [JsonProperty("DatasetId")]
        public string DatasetId { get; set; }

        [JsonProperty("ReportId")]
        public string ReportId { get; set; }

        [JsonProperty("DashboardName")]
        public string DashboardName { get; set; }

        [JsonProperty("DataClassification")]
        public string DataClassification { get; set; }

        [JsonProperty("DashboardId")]
        public string DashboardId { get; set; }

        [JsonProperty("Datasets")]
        public IList<Dataset> Datasets { get; set; }
    }
    public class Dataset
    {
        [JsonProperty("DatasetId")]
        public string DatasetId { get; set; }

        [JsonProperty("DatasetName")]
        public string DatasetName { get; set; }
    }
    public class RequestError
    {
        [JsonProperty("error")]
        public Error Error { get; set; }
    }

    public class Error
    {

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
    public class Content
    {
        [JsonProperty("contentUri")]
        public string ContentUri { get; set; }

        [JsonProperty("contentId")]
        public string ContentId { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("contentCreated")]
        public DateTime ContentCreated { get; set; }

        [JsonProperty("contentExpiration")]
        public DateTime ContentExpiration { get; set; }
    }
    public class Subscription
    {

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("webhook")]
        public Webhook Webhook { get; set; }
    }
    public class Webhook
    {

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("authId")]
        public string AuthId { get; set; }

        [JsonProperty("expiration")]
        public object Expiration { get; set; }
    }
}

