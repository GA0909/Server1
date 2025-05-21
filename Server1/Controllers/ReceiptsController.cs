using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;
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
    public class ReceiptsController : ControllerBase
    {
        private readonly IReceiptRepository _repo;
        public ReceiptsController(IReceiptRepository repo) => _repo = repo;

        [HttpPost]
        public IActionResult Post([FromBody] Receipt r)
        {
            var created = _repo.Add(r);
            return CreatedAtAction(null, created);
        }
    }
}
