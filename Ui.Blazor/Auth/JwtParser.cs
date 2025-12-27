using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Ui.Blazor.Auth;

public static class JwtParser
{
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3) return Array.Empty<Claim>();

        var payload = parts[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs is null) return Array.Empty<Claim>();

        var claims = new List<Claim>();
        foreach (var kvp in keyValuePairs)
        {
            // roles can be string or array (depends on issuer)
            if (kvp.Value is JsonElement el && el.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in el.EnumerateArray())
                    claims.Add(new Claim(kvp.Key, item.ToString()));
            }
            else
            {
                claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty));
            }
        }

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
