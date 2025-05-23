using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;
using System;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoyaltyCardsController : ControllerBase
    {
        private readonly ILoyaltyCardRepository _loyaltyCardService;

        public LoyaltyCardsController(ILoyaltyCardRepository loyaltyCardService)
        {
            _loyaltyCardService = loyaltyCardService;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_loyaltyCardService.GetAll());

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var card = _loyaltyCardService.GetById(id);
            return card != null ? Ok(card) : NotFound();
        }

        [HttpPost]
        public IActionResult Create(LoyaltyCard loyaltyCard)
        {
            var created = _loyaltyCardService.Create(loyaltyCard);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, LoyaltyCard loyaltyCard)
        {
            var updated = _loyaltyCardService.Update(id, loyaltyCard);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var deleted = _loyaltyCardService.Delete(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
