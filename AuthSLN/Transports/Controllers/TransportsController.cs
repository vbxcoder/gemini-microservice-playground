using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace Transports.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransportsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public TransportsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var db = _redis.GetDatabase();
            const string cacheKey = "transports";

            var cachedTransports = db.StringGet(cacheKey);
            if (!cachedTransports.IsNullOrEmpty)
            {
                // Cache hit: return data from cache
                var transports = JsonSerializer.Deserialize<List<Transport>>(cachedTransports);
                return Ok(new { Source = "Cache", Data = transports });
            }

            // Cache miss: create data, store in cache, and return
            var newTransports = new List<Transport>
            {
                new Transport { Id = 1, Name = "Truck A-123" },
                new Transport { Id = 2, Name = "Ship B-456" },
                new Transport { Id = 3, Name = "Plane C-789" }
            };

            var serializedTransports = JsonSerializer.Serialize(newTransports);
            db.StringSet(cacheKey, serializedTransports, TimeSpan.FromMinutes(1));

            return Ok(new { Source = "Database (mock)", Data = newTransports });
        }
    }

    public class Transport
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
