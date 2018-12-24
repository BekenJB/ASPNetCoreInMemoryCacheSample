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
                Console.WriteLine("Loading from database or json file into cache");

                states =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        await System.IO.File.ReadAllTextAsync("StatesList.json"));

                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(300), // cache will expire in 300 seconds or 5 minutes
                    SlidingExpiration = TimeSpan.FromSeconds(60) // caceh will expire if inactive for 60 seconds
                };

                if (states != null)
                    _cache.Set("CashedStatesList", states, options);
            }
            else
            {
                Console.WriteLine("***Data Found in Cache...***");
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