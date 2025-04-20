using DTO;
using DBEntities.Models;
using IBLL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        readonly ICategoryBLL categoryBLL;
        public CategoryController(ICategoryBLL categoryBLL)
        {
            this.categoryBLL = categoryBLL;
        }


        // GET: api/<CatedoryController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var categories = await categoryBLL.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDTO>> Get(int id)
        {
            var category = await categoryBLL.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound($"category with id {id} was not found.");
            }
            return Ok(category);
        }

        // POST api/<CategoryController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CategoryDTO category)
        {
            if (category == null)
            {
                return BadRequest("category data is missing");
            }
            var category1 = await categoryBLL.GetCategoryByNameAsync(category.CategoryName);
            if (category1 != null)
            {
                return BadRequest($"category with name {category.CategoryName} already exists.");
            }
            var newCategory = new CategoryDTO
            {
                CategoryName = category.CategoryName
                // אל תציב ערך ל-ID, הוא יוגדר אוטומטית
            };

            await categoryBLL.AddCategoryAsync(newCategory);

            return CreatedAtAction(nameof(Get), new { id = newCategory.CategoryId }, newCategory);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] CategoryDTO category)
        {
            if (category == null)
            {
                return BadRequest("Category data is missing");
            }
            //if (id != category.CategoryId)
            //{
            //    return BadRequest("Category id mismatch");
            //}
            var category1 = await categoryBLL.GetCategoryByIdAsync(id);
            if (category1 == null)
            {
                return NotFound($"Category with id {id} was not found.");
            }
            await categoryBLL.UpdateCategoryAsync(category, id);
            return Ok(category);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var category = await categoryBLL.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound($"Category with id {id} was not found.");
            }
            await categoryBLL.DeleteCategoryAsync(id);
            return Ok($"Category with id {id} was deleted.");
        }
    }
}
