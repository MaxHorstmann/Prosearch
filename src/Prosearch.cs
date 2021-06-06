using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;

namespace src
{
    public class SearchResult
    {
        public int rank { get; set; }
        public string provider { get; set; }
        public string providerImageUrl { get; set; }
        public string title { get; set; }
        public string url { get; set; }
    }

    public class CodecademySearchResults
    {
        public CodecademyEntities entities { get; set; }
    }

    public class CodecademyEntities
    {
        public CodecademyPaths paths { get; set; }
    }

    public class CodecademyPaths
    {
        public IDictionary<string, CodecademyPath> byId { get; set;  }
    }

    public class CodecademyPath
    {
        public string id { get; set; }
        public string title { get; set; }
        public string slug { get; set; }
    }
    
    public class StackOverflowSearchResults
    {
        public IEnumerable<StackOverflowSearchResult> items { get; set; }
    }

    public class StackOverflowSearchResult
    {
        public string name { get; set; }
        public int count { get; set; }
    }
    
    public class UdemySearchResults
    {
        public IEnumerable<UdemySearchResult> results { get; set; }
    }
    
    public class UdemySearchResult
    {
        public int id { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string headline { get; set; }
        public string image_240x135 { get; set; }
    }
    
    public static class Prosearch
    {

        [FunctionName("Prosearch")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var searchResults = new List<SearchResult>();
            foreach (var q in req.Query["q"])
            {
                searchResults.AddRange(await GetStackOverflow(q));
                searchResults.AddRange(await GetCodecademy(q));
                searchResults.AddRange(await GetUdemy(q));
            }
            searchResults = searchResults.OrderBy(m => m.rank).ToList(); 
            return new OkObjectResult(searchResults);
        }

        private static async Task<IEnumerable<SearchResult>> GetCodecademy(string query)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var url = $"https://www.codecademy.com/catalog/language/{query}.json";
                    var searchResults = 
                        JsonSerializer.Deserialize<CodecademySearchResults>(await httpClient.GetStringAsync(url));
                    var r = 1;
                    return searchResults.entities.paths.byId
                        .Where(m => m.Value.title.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                        .Select(m => new SearchResult
                        {
                            rank = r++,
                            provider = "codecademy",
                            providerImageUrl = "http://s3.amazonaws.com/codecademy-blog/assets/logo_blue_dark.png",
                            title = m.Value.title,
                            url = $"https://www.codecademy.com/learn/paths/{m.Value.slug}"
                        });
                }
            }
            catch (Exception)
            {
                return new SearchResult[0];
            }
        }

        private static async Task<IEnumerable<SearchResult>> GetStackOverflow(string query)
        {
            try
            {
                var handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
                using (var httpClient = new HttpClient(handler))
                {
//              httpClient.DefaultRequestHeaders.Authorization =
//                  new AuthenticationHeaderValue("Basic", token);
                    var url = $"https://api.stackexchange.com/2.2/tags?sort=popular&inname={query}&site=stackoverflow";
                    var json = await httpClient.GetStringAsync(url);
                    var searchResults = 
                        JsonSerializer.Deserialize<StackOverflowSearchResults>(json);
                    var r = 1;
                    return searchResults.items.Select(m => new SearchResult
                    {
                        rank = r++,
                        provider = "stackoverflow",
                        providerImageUrl = "https://stackoverflow.design/assets/img/logos/so/logo-stackoverflow.svg",
                        title = $"{m.count} [{m.name}] questions",
                        url = $"https://stackoverflow.com/tags/{m.name}/info"
                    });
                }
            }
            catch (Exception)
            {
                return new SearchResult[0];
            }
        }

        private static async Task<IEnumerable<SearchResult>> GetUdemy(string query)
        {
            const string token =
                "YOUR_TOKEN";
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Basic", token);
                    var url = $"https://www.udemy.com/api-2.0/courses/?search={query}";
                    var searchResults =
                        JsonSerializer.Deserialize<UdemySearchResults>(await httpClient.GetStringAsync(url));
                    var r = 1;
                    return searchResults.results.Select(m => new SearchResult
                    {
                        rank = r++,
                        provider = "udemy",
                        providerImageUrl = "https://www.udemy.com/staticx/udemy/images/v6/logo-coral.svg",
                        title = m.title,
                        url = $"https://www.udemy.com{m.url}"
                    });
                }
            }
            catch (Exception)
            {
                return new SearchResult[0];
            }
        }
    }
}
