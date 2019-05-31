using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace Cassette.Tests.WebApplication
{
    public interface IGeoApi
    {
        [Get("/regions")]
        Task<List<Region>> GetRegions();
    }

    public class Region
    {
        public string Code { get; set; }
        public string Nom { get; set; }
    }
}
