namespace Functions.Infrastructure.Authentication
{
	internal class AuthenticationContext
	{
		public string AuthorityUrl { get; set; }
		public string ResourceUrl { get; set; }
		public string ClientId { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string ClientSecret { get; set; }
	}
}