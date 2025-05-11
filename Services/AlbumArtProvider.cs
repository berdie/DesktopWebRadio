using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace DesktopWebRadio.Services
{
    public class AlbumArtProvider
    {
        private readonly string discogsApiKey;
        private readonly string discogsApiSecret;
        private readonly string discogsUserAgent;
        private readonly string lastfmApiKey;
        private readonly HttpClient httpClient;

        public AlbumArtProvider(string discogsApiKey, string discogsApiSecret, string discogsUserAgent, string lastfmApiKey)
        {
            this.discogsApiKey = discogsApiKey;
            this.discogsApiSecret = discogsApiSecret;
            this.discogsUserAgent = discogsUserAgent;
            this.lastfmApiKey = lastfmApiKey;

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(discogsUserAgent);
        }

        public async Task<string> GetAlbumArtUrl(string artist, string title, bool useDiscogs = true)
        {
            if (string.IsNullOrEmpty(artist) || string.IsNullOrEmpty(title))
                return null;

            try
            {
                if (useDiscogs)
                {
                    // Try Discogs API first
                    string discogsUrl = await GetDiscogsAlbumArt(artist, title);
                    if (!string.IsNullOrEmpty(discogsUrl))
                        return discogsUrl;
                }

                // Fall back to Last.fm API
                return await GetLastFmAlbumArt(artist, title);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<string> GetDiscogsAlbumArt(string artist, string title)
        {
            try
            {
                // Build the search query
                string query = HttpUtility.UrlEncode($"{artist} {title}");
                string url = $"https://api.discogs.com/database/search?q={query}&type=release&key={discogsApiKey}&secret={discogsApiSecret}";

                // Make the request
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                JObject result = JObject.Parse(json);

                // Check if we got any results
                JArray results = (JArray)result["results"];
                if (results != null && results.Count > 0)
                {
                    // Get the first result's cover image
                    foreach (var item in results)
                    {
                        JToken coverImageToken = item["cover_image"];
                        if (coverImageToken != null && !string.IsNullOrEmpty(coverImageToken.ToString()))
                        {
                            return coverImageToken.ToString();
                        }
                    }
                }

                // No suitable cover found
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<string> GetLastFmAlbumArt(string artist, string title)
        {
            try
            {
                // Build the search query
                string encodedArtist = HttpUtility.UrlEncode(artist);
                string encodedTitle = HttpUtility.UrlEncode(title);
                string url = $"http://ws.audioscrobbler.com/2.0/?method=track.getInfo&api_key={lastfmApiKey}&artist={encodedArtist}&track={encodedTitle}&format=json";

                // Make the request
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                JObject result = JObject.Parse(json);

                // Navigate through the JSON response to find album art
                JToken track = result["track"];
                if (track != null)
                {
                    JToken album = track["album"];
                    if (album != null)
                    {
                        JArray images = (JArray)album["image"];
                        if (images != null && images.Count > 0)
                        {
                            // Try to get the largest image available (typically the last one)
                            for (int i = images.Count - 1; i >= 0; i--)
                            {
                                JToken image = images[i];
                                string size = image["size"]?.ToString();
                                string imageUrl = image["#text"]?.ToString();

                                if (!string.IsNullOrEmpty(imageUrl) && 
                                    (size == "large" || size == "extralarge" || size == "mega"))
                                {
                                    return imageUrl;
                                }
                            }

                            // If we didn't find a large image, use any available image
                            foreach (var image in images)
                            {
                                string imageUrl = image["#text"]?.ToString();
                                if (!string.IsNullOrEmpty(imageUrl))
                                {
                                    return imageUrl;
                                }
                            }
                        }
                    }
                }

                // No album art found
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
