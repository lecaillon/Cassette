using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.HttpClientFactory.QuickStart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeoController : ControllerBase
    {
        private readonly IGeoApi _geoApi;

        public GeoController(IGeoApi geoApi)
        {
            _geoApi = geoApi;
        }

        // GET api/geo/regions
        [HttpGet]
        [Route("regions")]
        public async Task<ActionResult<IEnumerable<Region>>> GetRegion()
        {
            return await _geoApi.GetRegions();
        }
    }
}
