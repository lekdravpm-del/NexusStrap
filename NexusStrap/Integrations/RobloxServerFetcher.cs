/*
 *  NexusStrap
 *  Copyright (c) NexusStrap Team
 *
 *  This file is part of NexusStrap and is distributed under the terms of the
 *  GNU Affero General Public License, version 3 or later.
 *
 *  SPDX-License-Identifier: AGPL-3.0-or-later
 */

using System.Collections.Concurrent;
using System.Net.Http.Headers;

namespace NexusStrap.Integrations
{
    public class RobloxServerFetcher
    {
        private const string LOG_IDENT = "RobloxServerFetcher";
        private readonly HttpClient _client;

        // Static mapping of Roblox datacenter IP blocks (128.116.x.0/24) to their regions.
        // Source: publicly known Roblox server IP ranges (BTRoblox serverdetails.js).
        private static readonly Dictionary<string, string> ServerRegionsByIp = new(StringComparer.OrdinalIgnoreCase)
        {
            ["128.116.1.0"] = "Los Angeles, United States",
            ["128.116.5.0"] = "Frankfurt, Germany",
            ["128.116.11.0"] = "Ashburn, United States",
            ["128.116.13.0"] = "Paris, France",
            ["128.116.21.0"] = "Amsterdam, Netherlands",
            ["128.116.22.0"] = "Atlanta, United States",
            ["128.116.31.0"] = "London, United Kingdom",
            ["128.116.32.0"] = "New York, United States",
            ["128.116.33.0"] = "London, United Kingdom",
            ["128.116.44.0"] = "Frankfurt, Germany",
            ["128.116.45.0"] = "Miami, United States",
            ["128.116.46.0"] = "Singapore, Singapore",
            ["128.116.47.0"] = "San Jose, United States",
            ["128.116.48.0"] = "Chicago, United States",
            ["128.116.50.0"] = "Singapore, Singapore",
            ["128.116.51.0"] = "Sydney, Australia",
            ["128.116.53.0"] = "Ashburn, United States",
            ["128.116.54.0"] = "Singapore, Singapore",
            ["128.116.55.0"] = "Tokyo, Japan",
            ["128.116.56.0"] = "Ashburn, United States",
            ["128.116.57.0"] = "San Jose, United States",
            ["128.116.63.0"] = "Los Angeles, United States",
            ["128.116.64.0"] = "San Jose, United States",
            ["128.116.67.0"] = "San Jose, United States",
            ["128.116.74.0"] = "Ashburn, United States",
            ["128.116.80.0"] = "Ashburn, United States",
            ["128.116.81.0"] = "San Jose, United States",
            ["128.116.84.0"] = "Chicago, United States",
            ["128.116.86.0"] = "Sao Paulo, Brazil",
            ["128.116.87.0"] = "Ashburn, United States",
            ["128.116.88.0"] = "Chicago, United States",
            ["128.116.95.0"] = "Dallas, United States",
            ["128.116.97.0"] = "Singapore, Singapore",
            ["128.116.99.0"] = "Atlanta, United States",
            ["128.116.102.0"] = "Ashburn, United States",
            ["128.116.104.0"] = "Mumbai, India",
            ["128.116.105.0"] = "San Jose, United States",
            ["128.116.115.0"] = "Seattle, United States",
            ["128.116.116.0"] = "Los Angeles, United States",
            ["128.116.117.0"] = "San Jose, United States",
            ["128.116.119.0"] = "London, United Kingdom",
            ["128.116.120.0"] = "Tokyo, Japan",
            ["128.116.123.0"] = "Frankfurt, Germany",
            ["128.116.127.0"] = "Miami, United States",
        };

        private readonly string _serverCacheFilePath = Path.Combine(Paths.Cache, "server_cache.json");
        private ConcurrentDictionary<long, ConcurrentDictionary<string, ServerInstance>> _serverCache = new();

        public RobloxServerFetcher()
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                MaxConnectionsPerServer = 20
            };

            _client = new HttpClient(handler);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.UserAgent.ParseAdd("Roblox/NexusStrap");

            try
            {
                Directory.CreateDirectory(Paths.Cache);

                if (File.Exists(_serverCacheFilePath))
                {
                    using FileStream fs = File.OpenRead(_serverCacheFilePath);
                    var loadedCache = JsonSerializer.Deserialize<ConcurrentDictionary<long, ConcurrentDictionary<string, ServerInstance>>>(fs);

                    if (loadedCache != null)
                    {
                        _serverCache = loadedCache;
                        App.Logger.WriteLine(LOG_IDENT, $"Loaded {_serverCache.Count} games from disk.");
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public static string GetRegionForAddress(string address)
        {
            try
            {
                var parts = address.Split('.');

                if (parts.Length == 4)
                {
                    string block = $"{parts[0]}.{parts[1]}.{parts[2]}.0";

                    if (ServerRegionsByIp.TryGetValue(block, out var region))
                        return region;
                }
            }
            catch
            {
                // ignore malformed addresses
            }

            return "Unknown";
        }

        public static IReadOnlyList<string> GetKnownRegions() =>
            ServerRegionsByIp.Values.Distinct().OrderBy(r => r, StringComparer.OrdinalIgnoreCase).ToList();

        public Task<(List<string> regions, Dictionary<int, string> datacenterMap)?> GetDatacentersAsync()
        {
            var regions = GetKnownRegions().ToList();
            var emptyMap = new Dictionary<int, string>();
            return Task.FromResult<(List<string> regions, Dictionary<int, string> datacenterMap)?>((regions, emptyMap));
        }

        private async Task<HttpResponseMessage> SendJoinRequestWithRetriesAsync(long placeId, string jobId, string roblosecurity)
        {
            int attempt = 0;
            const int maxAttempts = 3;

            while (true)
            {
                attempt++;
                var joinReq = new HttpRequestMessage(HttpMethod.Post, "https://gamejoin.roblox.com/v1/join-game-instance");
                joinReq.Headers.Add("Referer", $"https://roblox.com/games/{placeId}");
                joinReq.Headers.Add("Origin", "https://roblox.com");
                joinReq.Headers.Add("Cookie", $".ROBLOSECURITY={roblosecurity}");

                joinReq.Content = new StringContent(JsonSerializer.Serialize(new
                {
                    placeId,
                    isTeleport = false,
                    gameId = jobId,
                    gameJoinAttemptId = jobId
                }), Encoding.UTF8, "application/json");

                try
                {
                    var resp = await _client.SendAsync(joinReq).ConfigureAwait(false);

                    if (resp.StatusCode == HttpStatusCode.Unauthorized || resp.StatusCode == HttpStatusCode.Forbidden)
                        return resp;

                    if (((int)resp.StatusCode == 429 || (int)resp.StatusCode >= 500) && attempt < maxAttempts)
                    {
                        await Task.Delay(500 * attempt).ConfigureAwait(false);
                        continue;
                    }

                    return resp;
                }
                catch (HttpRequestException) when (attempt < maxAttempts)
                {
                    await Task.Delay(250 * attempt).ConfigureAwait(false);
                }
            }
        }

        private string TryExtractServerAddress(JsonElement root)
        {
            string result = string.Empty;

            if (!root.TryGetProperty("joinScript", out var joinScript) || joinScript.ValueKind != JsonValueKind.Object)
                return result;

            if (joinScript.TryGetProperty("UdmuxEndpoints", out var endpoints) && endpoints.ValueKind == JsonValueKind.Array)
            {
                foreach (var endpoint in endpoints.EnumerateArray())
                {
                    if (endpoint.TryGetProperty("Address", out var addrProp))
                    {
                        string? addr = addrProp.GetString();

                        if (!string.IsNullOrEmpty(addr) && !addr.StartsWith("10."))
                            return addr;
                    }
                }
            }

            if (joinScript.TryGetProperty("MachineAddress", out var machineProp) && machineProp.ValueKind == JsonValueKind.String)
                result = machineProp.GetString() ?? string.Empty;

            return result;
        }

        public async Task<bool> ValidateCookieAsync(string roblosecurityCookie)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roblosecurityCookie)) return false;

                var request = new HttpRequestMessage(HttpMethod.Get, "https://users.roblox.com/v1/users/authenticated");
                request.Headers.Add("Cookie", $".ROBLOSECURITY={roblosecurityCookie}");

                var response = await _client.SendAsync(request);
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }

        public async Task<FetchResult> FetchServerInstancesAsync(long placeId, string roblosecurity, string cursor = "", int sortOrder = 2)
        {
            string url = $"https://games.roblox.com/v1/games/{placeId}/servers/Public?sortOrder={sortOrder}&excludeFullGames=true&limit=100&cursor={cursor}";

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(roblosecurity))
                req.Headers.Add("Cookie", $".ROBLOSECURITY={roblosecurity}");

            var response = await _client.SendAsync(req).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return new FetchResult();

            using var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (!jsonDoc.RootElement.TryGetProperty("data", out var dataElement)) return new FetchResult();

            string nextCursor = jsonDoc.RootElement.TryGetProperty("nextPageCursor", out var cElem) ? cElem.GetString() ?? "" : "";

            var instances = new ConcurrentBag<ServerInstance>();
            var placeCache = _serverCache.GetOrAdd(placeId, _ => new ConcurrentDictionary<string, ServerInstance>());
            var newServers = new ConcurrentBag<ServerInstance>();

            foreach (var serverElem in dataElement.EnumerateArray())
            {
                string jobId = serverElem.GetProperty("id").GetString() ?? "";
                int playing = serverElem.GetProperty("playing").GetInt32();
                int maxPlayers = serverElem.GetProperty("maxPlayers").GetInt32();

                if (playing >= maxPlayers) continue;

                int? ping = serverElem.TryGetProperty("ping", out var pingProp) && pingProp.ValueKind == JsonValueKind.Number ? pingProp.GetInt32() : null;
                int? fps = serverElem.TryGetProperty("fps", out var fpsProp) && fpsProp.ValueKind == JsonValueKind.Number ? fpsProp.GetInt32() : null;

                if (placeCache.TryGetValue(jobId, out var cached))
                {
                    cached.Playing = playing;
                    cached.MaxPlayers = maxPlayers;
                    cached.Ping = ping;
                    cached.Fps = fps;
                    instances.Add(cached);
                    continue;
                }

                var server = new ServerInstance
                {
                    Id = jobId,
                    Playing = playing,
                    MaxPlayers = maxPlayers,
                    Ping = ping,
                    Fps = fps,
                    FirstSeen = DateTime.UtcNow
                };

                newServers.Add(server);
                instances.Add(server);
            }

            if (!string.IsNullOrWhiteSpace(roblosecurity))
            {
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 8 };
                await Parallel.ForEachAsync(newServers, parallelOptions, async (server, ct) =>
                {
                    try
                    {
                        var joinResp = await SendJoinRequestWithRetriesAsync(placeId, server.Id, roblosecurity);
                        using var parsed = JsonDocument.Parse(await joinResp.Content.ReadAsStringAsync());

                        string address = TryExtractServerAddress(parsed.RootElement);
                        server.Region = GetRegionForAddress(address);

                        if (server.Region != "Unknown") placeCache[server.Id] = server;
                    }
                    catch { }
                });
            }

            SaveServerCache();

            return new FetchResult
            {
                Servers = instances.ToList(),
                NextCursor = nextCursor
            };
        }

        private void SaveServerCache()
        {
            try
            {
                foreach (var gameCache in _serverCache.Values)
                {
                    foreach (var key in gameCache.Where(e => e.Value.FirstSeen == null || DateTime.UtcNow - e.Value.FirstSeen.Value > TimeSpan.FromDays(7)).Select(e => e.Key).ToList())
                        gameCache.TryRemove(key, out _);

                    while (gameCache.Count > 1000)
                    {
                        string? oldest = gameCache.OrderBy(e => e.Value.FirstSeen ?? DateTime.MinValue).Select(e => e.Key).FirstOrDefault();
                        if (oldest == null)
                            break;

                        gameCache.TryRemove(oldest, out _);
                    }
                }

                using FileStream fs = File.Create(_serverCacheFilePath);
                JsonSerializer.Serialize(fs, _serverCache);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }
    }
}
