using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace src
{
    public class SearchResult
    {
        public string title { get; set; }
        public string url { get; set; }
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
                searchResults.AddRange(GetCodecademy(q));
                searchResults.Add(new SearchResult { Title = $"Learn more about {q}", Url = "https://codecademy.com"});
            }

            await Task.Delay(1);
            return new OkObjectResult(searchResults);
        }

        private static IEnumerable<SearchResult> GetCodecademy(string query)
        {
            //const string url = "https://www.codecademy.com/catalog.json";
            return new List<SearchResult>();
        }

        private static IEnumerable<SearchResult> GetStackOverflow(string query)
        {
            // https://api.stackexchange.com/2.2/tags/python/wikis?site=stackoverflow
        }
    }
}
