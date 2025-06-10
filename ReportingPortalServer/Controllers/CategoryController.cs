using Microsoft.AspNetCore.Mvc;
using Models;
using ReportingPortalServer.Services;
using ReportingPortalServer.Data;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController(ICategoryService categoryService, ApplicationDbContext context) : ControllerBase
    {
        private readonly ICategoryService _categoryService = categoryService;
        private readonly ApplicationDbContext _context = context;

        [HttpGet]
        public IActionResult GetAll()
            => Ok(_categoryService.GetAll(_context));

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var category = _categoryService.GetById(id, _context);
            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpPost]
        public IActionResult Add(Category category)
        {
            var created = _categoryService.Add(category, _context);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
    }
}