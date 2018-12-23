using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASPNetCoreInMemoryCacheSample.Controllers
{
    [Produces("application/json")]
    [Route("api/States")]
    public class StatesController : Controller
    {

        private readonly IMemoryCache _cache;

        public StatesController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        [HttpGet("{stateCode}")]
        public async Task<IActionResult> Get(string stateCode)
        {

            string state = string.Empty;
            if (!_cache.TryGetValue("CashedStatesList", out Dictionary<string, string> states))
            {
                Console.WriteLine("Cache miss....loading from database into cache");

                states =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        await System.IO.File.ReadAllTextAsync("StatesList.json"));

                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(25), // cache will expire in 25 seconds
                    SlidingExpiration = TimeSpan.FromSeconds(5) // caceh will expire if inactive for 5 seconds
                };

                if (states != null)
                    _cache.Set("CashedStatesList", states, options);
            }
            else
            {
                Console.WriteLine("Cache hit");
            }

            if (states != null)
            {
                state = states.GetValueOrDefault(stateCode);

                if (string.IsNullOrEmpty(state))
                {
                    state = "Not found, please try again.";
                }

            }

            if (string.IsNullOrEmpty(state))
            {
                return NoContent();
            }
            return Ok(state);
        }
    }
}