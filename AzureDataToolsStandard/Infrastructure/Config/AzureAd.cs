namespace Functions.Infrastructure.Config
{
	public class AzureAd
	{
		public string AuthorityUrl { get; set; }
		public string ClientId { get; set; }
		public string ClientSecret { get; set; }
		public string TenantId { get; set; }
        public bool UseMSI { get; set; }
    }
}