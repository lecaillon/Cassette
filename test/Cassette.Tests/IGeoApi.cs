using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace Cassette.Tests
{
    public interface IGeoApi
    {
        [Get("/regions")]
        Task<List<Region>> GetRegionsAsync(string code = null);

        [Post("/regions")]
        Task CreateRegion(Region region);
    }

    public class Region
    {
        public string Code { get; set; }
        public string Nom { get; set; }
    }
}
