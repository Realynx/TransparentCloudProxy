using System;

namespace TransparentCloudServerProxy.Client.Extentions {
    public static class StringExtentions {
        public static Uri NormalizeHostUri(this string input) {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            input = input.Trim();

            // Add scheme if missing
            if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !input.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
                input = "https://" + input;
            }

            // Wrap IPv6 addresses in brackets if needed
            var uriWithoutScheme = input.Substring(input.IndexOf("://") + 3);
            if (System.Net.IPAddress.TryParse(uriWithoutScheme, out var ip) && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) {
                // Only wrap if not already wrapped
                if (!uriWithoutScheme.StartsWith("[") && !uriWithoutScheme.Contains("]")) {
                    input = input.Replace(uriWithoutScheme, $"[{uriWithoutScheme}]");
                }
            }

            // Try create the Uri
            if (!Uri.TryCreate(input, UriKind.Absolute, out var uri))
                return null;

            // Ensure path ends with '/'
            var builder = new UriBuilder(uri);
            if (!builder.Path.EndsWith("/"))
                builder.Path += "/";

            return builder.Uri;
        }

    }
}
