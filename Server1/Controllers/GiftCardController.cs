using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;
using System;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GiftCardsController : ControllerBase
    {
        private readonly IGiftCardRepository _giftCardRepository;

        public GiftCardsController(IGiftCardRepository giftCardRepository)
        {
            _giftCardRepository = giftCardRepository;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_giftCardRepository.GetAll());

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var giftCard = _giftCardRepository.GetById(id);
            return giftCard != null ? Ok(giftCard) : NotFound();
        }

        [HttpPost]
        public IActionResult Create(GiftCard giftCard)
        {
            var created = _giftCardRepository.Create(giftCard);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, GiftCard giftCard)
        {
            var updated = _giftCardRepository.Update(id, giftCard);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var deleted = _giftCardRepository.Delete(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
