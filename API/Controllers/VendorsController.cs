using API.Dtos;
using Infa;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("[controller]")]
public class VendorsController(SatinRoadDatabase db) : ControllerBase
{
    public const int FeaturedTheshold = 100;

    [HttpGet(nameof(GetFeatured))]
    public List<FeaturedVendorResponse> GetFeatured()
    {
        var counts = db.Orders()
            .GroupBy(o => o.VendorId)
            .Select(g => new { VendorId = g.Key, OrderCount = g.Count() })
            .Where(x => x.OrderCount > FeaturedTheshold)
            .ToList();
        
        var VendorIds = counts.Select(c => c.VendorId).ToList();
        var users = db.Users()
            .Where(u => VendorIds.Contains(u.Id) && !u.IsShutDown)
            .ToList();
        
        return counts
            .Join(users, c => c.VendorId, u => u.Id, (c, u) => new FeaturedVendorResponse(u.Id, u.Username, c.OrderCount))
            .OrderByDescending(v => v.OrderCount)
            .ToList();
    }
}