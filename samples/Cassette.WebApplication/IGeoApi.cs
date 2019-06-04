using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace Cassette.WebApplication
{
    public interface IGeoApi
    {
        [Headers(Cassette.Http.Cassette.ExcludeFromCache)]
        [Get("/regions")]
        Task<List<Region>> GetRegions();
    }

    public class Region
    {
        public string Code { get; set; }
        public string Nom { get; set; }
    }
}
