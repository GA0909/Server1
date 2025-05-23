using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;
using System;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptsController : ControllerBase
    {
        private readonly IReceiptService _receiptService;

        public ReceiptsController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        [HttpPost]
        public IActionResult CreateReceipt([FromBody] Receipt receipt)
        {
            var createdReceipt = _receiptService.CreateReceipt(receipt);
            return CreatedAtAction(nameof(GetReceiptById), new { id = createdReceipt.Id }, createdReceipt);
        }

        [HttpGet]
        public IActionResult GetAllReceipts()
        {
            return Ok(_receiptService.GetAllReceipts());
        }

        [HttpGet("{id}")]
        public IActionResult GetReceiptById(Guid id)
        {
            var receipt = _receiptService.GetReceiptById(id);
            return receipt != null ? Ok(receipt) : NotFound();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateReceipt(Guid id, [FromBody] Receipt receipt)
        {
            var updated = _receiptService.UpdateReceipt(id, receipt);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteReceipt(Guid id)
        {
            var deleted = _receiptService.DeleteReceipt(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
