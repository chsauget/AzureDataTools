namespace Functions.Infrastructure.Config
{
	public class AzureAd
	{
		public string AuthorityUrl { get; set; }
        public string PowerBIRessourceUrl { get; set; }
		public string ClientId { get; set; }
		public string PowerBILogin { get; set; }
		public string PowerBIPassword { get; set; }
		public string ClientSecret { get; set; }
		public string TenantId { get; set; }
	}
}