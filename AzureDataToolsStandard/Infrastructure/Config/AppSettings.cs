namespace Functions.Infrastructure.Config
{
	public class AppSettings
	{
		public AzureAd AzureAd { get; set; }

        public O365AuditLogs O365AuditLogs { get; set; }

        public SSAS SSAS { get; set; }
        public string AzureSubScriptionId { get; set; }

		public string ResourceGroup { get; set; }

    }
}