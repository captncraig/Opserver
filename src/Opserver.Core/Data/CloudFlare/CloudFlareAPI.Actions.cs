﻿using System.Text.RegularExpressions;

namespace StackExchange.Opserver.Data.Cloudflare
{
    public partial class CloudflareAPI
    {
        public bool PurgeFile(string url)
        {
            var zone = GetZoneFromUrl(url);
            if (zone == null) return false;

            var otherUrl = url.StartsWith("http:")
                ? Regex.Replace(url, "^http:", "https:")
                : Regex.Replace(url, "^https:", "http:");

            return Module.PurgeFiles(zone, new[] {url, otherUrl});
        }
    }
}
