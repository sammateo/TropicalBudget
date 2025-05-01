using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TropicalBudget.Controllers
{
    [Authorize]
    public class RoadmapController : Controller
    {
        public IActionResult Index()
        {
            return View("ViewRoadmap");
        }
    }
}
