using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.HttpClientFactory.QuickStart.Controllers
{
    [Route("regions")]
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
        public async Task<ActionResult<IEnumerable<Region>>> GetRegion()
        {
            return await _geoApi.GetRegions();
        }

        // GET api/geo/regions/{code}
        [HttpGet]
        [Route("{code}")]
        public async Task<ActionResult<Region>> GetRegion(string code)
        {
            return await _geoApi.GetRegion(code);
        }

        // GET api/geo/regions/{code}/departements
        [HttpGet]
        [Route("{code}/departements")]
        public async Task<ActionResult<IEnumerable<Departement>>> GetDepartements(string code)
        {
            return await _geoApi.GetDepartements(code);
        }
    }
}
