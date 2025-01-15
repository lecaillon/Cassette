using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace Cassette.Tests;

public interface IGeoApi
{
    [Headers(CassetteOptions.Refit.NoRecord)]
    [Get("/regions")]
    Task<List<Region>> GetRegions();

    [Headers(CassetteOptions.Refit.ExcludeLastUriSegment)]
    [Get("/regions/{code}")]
    Task<Region> GetRegion(string code);

    [Get("/regions/{code}/departements")]
    Task<List<Departement>> GetDepartements(string code);
}

public class Region
{
    public string Code { get; set; }
    public string Nom { get; set; }
}

public class Departement
{
    public string Nom { get; set; }
    public string Code { get; set; }
    public string CodeRegion { get; set; }
}
