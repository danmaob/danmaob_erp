namespace DanmaobErp.Api.Configuration;

public static class CorsOriginsParser
{
    private static bool IsValidOrigin(string origin)
    {
        if (origin.EndsWith('/'))
        {
            return false;
        }

        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        if (uri.AbsolutePath is not "/")
        {
            return false;
        }

        if (uri.Query.Length > 0)
        {
            return false;
        }

        if (uri.Fragment.Length > 0)
        {
            return false;
        }

        return true;
    }

    public static IReadOnlyList<string> Parse(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            throw new InvalidOperationException("Invalid configuration: Cors__AllowedOrigins must contain one or more absolute http or https origins, separated by commas, without paths or trailing slashes.");
        }

        var origins = rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (origins.Length == 0)
        {
            throw new InvalidOperationException("Invalid configuration: Cors__AllowedOrigins must contain one or more absolute http or https origins, separated by commas, without paths or trailing slashes.");
        }

        foreach (var origin in origins)
        {
            if (!IsValidOrigin(origin))
            {
                throw new InvalidOperationException("Invalid configuration: Cors__AllowedOrigins must contain one or more absolute http or https origins, separated by commas, without paths or trailing slashes.");
            }
        }

        return origins;
    }
}
