using System.Text;
using System.Security.Cryptography;

namespace Main.Helpers;

public static class SlugHelper
{
    public static string GenerateHashSlug(Guid id)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(id.ToString()));
            string base64 = Convert.ToBase64String(hashBytes);

            // Make it URL-safe (remove special chars)
            string slug = base64.Replace("/", "").Replace("+", "").Replace("=", "");

            // Take first 8 characters for uniqueness
            return slug.Substring(0, 8);
        }
    }
}
