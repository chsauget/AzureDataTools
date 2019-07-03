using System.Security;

namespace AzureDataTools
{
    internal static class StringExtensions
    {
        public static SecureString AsSecureString(this string password)
        {
            var securePassword = new SecureString();

            foreach (char c in password)
            {
                securePassword.AppendChar(c);
            }

            return securePassword;
        }
    }
}
