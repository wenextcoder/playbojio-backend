using System.Text;
using System.Text.RegularExpressions;

namespace PlayBojio.API.Utils;

public static class SlugHelper
{
    public static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        // Convert to lowercase
        string slug = title.ToLowerInvariant();

        // Replace spaces and special characters with hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        
        // Trim hyphens from start and end
        slug = slug.Trim('-');

        // Limit length
        if (slug.Length > 100)
            slug = slug.Substring(0, 100).TrimEnd('-');

        return slug;
    }

    public static string EnsureUniqueSlug(string baseSlug, Func<string, bool> slugExists)
    {
        string slug = baseSlug;
        int counter = 1;

        while (slugExists(slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}

