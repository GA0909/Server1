
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;
using Server1.Models;
using Server1.Services;
using System.Collections.Generic;

namespace Server1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptDtoController : ControllerBase
    {
        private readonly IReceiptStorageService _storage;

        public ReceiptDtoController(IReceiptStorageService storage)
        {
            _storage = storage;
        }

        // POST api/receiptDto
        [HttpPost]
        public IActionResult Post([FromBody] ReceiptDto dto)
        {
            if (dto == null) return BadRequest();
            _storage.StoreReceipt(dto);
            return Ok();
        }

        // GET api/receiptDto
        [HttpGet]
        public ActionResult<IEnumerable<ReceiptDto>> Get()
        {
            return Ok(_storage.GetAllReceipts());
        }
    }
}
