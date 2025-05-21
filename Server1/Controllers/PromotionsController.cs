using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server1.Models;
using Server1.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server1.Controllers
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
