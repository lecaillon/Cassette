using System.Collections.Generic;
using System.Threading.Tasks;
using Cassette;
using Refit;

namespace AspNetCore.HttpClientFactory.QuickStart
{
    public interface IGeoApi
    {
        [Headers(CassetteOptions.Refit.NoRecord)]
        [Get("/regions")]
        Task<List<Region>> GetRegions();
    }

    public class Region
    {
        public string Code { get; set; }
        public string Nom { get; set; }
    }
}
