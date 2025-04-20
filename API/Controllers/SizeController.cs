using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SizeController : ControllerBase
    {
        private readonly ISizeBLL sizeBLL;
        public SizeController(ISizeBLL sizeBLL)
        {
            this.sizeBLL = sizeBLL;
        }


        // GET: api/<SizeController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var sizes = await sizeBLL.GetAllSizesAsync();
            return Ok(sizes);
        }

        // GET api/<SizeController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            var size = await sizeBLL.GetSizeByIdAsync(id);
            if (size == null)
            {
                return NotFound($"Size with id {id} was not found.");
            }
            return Ok(size);
        }

        // POST api/<SizeController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] SizeDTO size)
        {
            if (size == null)
            {
                return BadRequest("Size data is missing");
            }
            var size1 = await sizeBLL.GetSizeByNameAsync(size.MuscleGroupName);
            if (size1 != null)
            {
                return BadRequest($"Size with name {size.MuscleGroupName} already exists.");
            }
            var newSize = new SizeDTO
            {
                MuscleGroupName = size.MuscleGroupName
                // Do not set a value for ID, it will be automatically assigned
            };

            await sizeBLL.AddSizeAsync(newSize);

            return CreatedAtAction(nameof(Get), new { id = newSize.MuscleGroupId }, newSize);
        }

        // PUT api/<SizeController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] SizeDTO size)
        {
            if (size == null)
            {
                return BadRequest("Size data is missing");
            }
            var size1 = await sizeBLL.GetSizeByIdAsync(id);
            if (size1 == null)
            {
                return NotFound($"Size with id {id} was not found.");
            }
            await sizeBLL.UpdateSizeAsync(size, id);
            return Ok(size);
        }

        // DELETE api/<SizeController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var size = await sizeBLL.GetSizeByIdAsync(id);
            if (size == null)
            {
                return NotFound($"Size with id {id} was not found.");
            }
            await sizeBLL.DeleteSizeAsync(id);
            return Ok($"Size with id {id} was deleted.");
        }
    }
}
