using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionRepository _repo;
        public PromotionsController(IPromotionRepository repo) => _repo = repo;

        [HttpGet]
        public IActionResult Get() => Ok(_repo.GetAll());

        [HttpPost]
        public IActionResult Post([FromBody] Promotion promo)
        {
            var created = _repo.Add(promo);
            return CreatedAtAction(null, created);
        }
    }
}
