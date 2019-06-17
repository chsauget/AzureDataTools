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
using Microsoft.SharePoint.Client;
using System.Net;
using ExcelDataReader;
using System.Data;
using System.Text;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureDataTools
{
    public static class Excel
    {
        static Excel()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        [FunctionName("load-excel-sharepoint")]
        public static async Task<IActionResult> LoadExcelSharepoint([HttpTrigger(AuthorizationLevel.Function, "post", Route = "load/excel/sharepoint")]
            HttpRequestMessage req,
            ILogger log)
        {
            log.LogInformation($"");

            ExcelConfiguration conf = await req.Content.ReadAsAsync<ExcelConfiguration>();
            var requestUri = new Uri(conf.path);
            var cred = new SharePointOnlineCredentials(conf.sharepointLogin, conf.sharepointPassword.AsSecureString());

            var excelFileContent = await DownloadFile(requestUri, cred);

            var dataSet = BuildDataset(excelFileContent);

            if (!dataSet.Tables.Contains(conf.sheet))
            {
                return new BadRequestObjectResult($"Unable to find {conf.sheet} sheet");
            }
            return new OkObjectResult(JsonConvert.SerializeObject(dataSet.Tables[conf.sheet]));
        }
        [FunctionName("load-blob-sharepoint")]
        public static async Task<IActionResult> LoadExcelBlob([HttpTrigger(AuthorizationLevel.Function, "post", Route = "load/excel/blob")]
            HttpRequestMessage req,
            ILogger log)
        {
            
            log.LogInformation($"");

            ExcelConfiguration conf = await req.Content.ReadAsAsync<ExcelConfiguration>();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(conf.blobConnectionString);

            CloudBlobClient serviceClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = serviceClient.GetContainerReference(conf.blobContainerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(conf.path);

            var ms = new MemoryStream();
            await blob.DownloadToStreamAsync(ms);
            var dataSet = BuildDataset(ms.ToArray());
            
            if (!dataSet.Tables.Contains(conf.sheet))
            {
                return new BadRequestObjectResult($"Unable to find {conf.sheet} sheet");
            }
            return new OkObjectResult(JsonConvert.SerializeObject(dataSet.Tables[conf.sheet]));


        }
        public class ExcelConfiguration
        {
            public string path { get; set; }
            public string sheet { get; set; }

            public string sharepointLogin { get; set; }
            public string sharepointPassword { get; set; }

            public string blobConnectionString { get; set; }
            public string blobContainerName { get; set; }
        }
        private static async Task<byte[]> DownloadFile(Uri fileUri, SharePointOnlineCredentials credentials)
        {
            const string SPOIDCRL = "SPOIDCRL";

            var authCookie = credentials.GetAuthenticationCookie(fileUri);
            var fedAuthString = authCookie.TrimStart($"{SPOIDCRL}=".ToCharArray());
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(fileUri, new Cookie(SPOIDCRL, fedAuthString));

            var securityHandler = new HttpClientHandler
            {
                Credentials = credentials,
                CookieContainer = cookieContainer
            };

            using (var httpClient = new HttpClient(securityHandler))
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, fileUri))
                {
                    var response = await httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsByteArrayAsync();

                    return content;
                }
            }
        }
        private static DataSet BuildDataset(byte[] rawContent)
        {
            using (var ms = new MemoryStream(rawContent))
            {
                ms.Position = 0;

                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(ms);
                DataSet result = excelReader.AsDataSet(new ExcelDataSetConfiguration
                {
                    UseColumnDataType = true,

                    ConfigureDataTable = tableReader => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                });

                return result;
            }
        }

    }
}
