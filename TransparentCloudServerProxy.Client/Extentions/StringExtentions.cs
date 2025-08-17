using System;

namespace TransparentCloudServerProxy.Client.Extentions {
    public static class StringExtentions {
        public static Uri NormalizeHostUri(this string input) {
            if (string.IsNullOrWhiteSpace(input)) {
                return null;
            }

            if (!input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !input.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
                input = "https://" + input;
            }

            if (!Uri.TryCreate(input, UriKind.Absolute, out var uri)) {
                return null;
            }

            var builder = new UriBuilder(uri);
            if (!builder.Path.EndsWith("/")) {
                builder.Path += "/";
            }

            return builder.Uri;
        }
    }
}
