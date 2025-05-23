using Microsoft.AspNetCore.Mvc;
using Server.Models;
using Server.Services;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repo;
        public ProductsController(IProductRepository repo) => _repo = repo;

        [HttpGet]
        public IActionResult Get() => Ok(_repo.GetAll());

        [HttpGet("{id:int}")]
        public IActionResult Get(int id)
        {
            var p = _repo.GetById(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Product p)
        {
            var created = _repo.Add(p);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public IActionResult Put(int id, [FromBody] Product p)
        {
            if (_repo.GetById(id) == null) return NotFound();
            _repo.Update(id, p);
            return NoContent();
        }
    }
}
