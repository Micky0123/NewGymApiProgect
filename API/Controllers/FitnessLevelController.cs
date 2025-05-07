using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FitnessLevelController : ControllerBase
    {
        private readonly IFitnessLevelBLL fitnessLevelBLL;

        public FitnessLevelController(IFitnessLevelBLL fitnessLevelBLL)
        {
            this.fitnessLevelBLL = fitnessLevelBLL;
        }

        // GET: api/<FitnessLevelController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FitnessLevelDTO>>> Get()
        {
            var fitnessLevels = await fitnessLevelBLL.GetAllFitnessLevelAsync();
            return Ok(fitnessLevels);
        }

        // GET api/<FitnessLevelController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FitnessLevelDTO>> Get(int id)
        {
            var fitnessLevel = await fitnessLevelBLL.GetFitnessLevelByIdAsync(id);
            if (fitnessLevel == null)
            {
                return NotFound($"FitnessLevel with id {id} was not found.");
            }
            return Ok(fitnessLevel);
        }

        // POST api/<FitnessLevelController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] FitnessLevelDTO fitnessLevel)
        {
            if (fitnessLevel == null)
            {
                return BadRequest("FitnessLevel data is missing");
            }

            await fitnessLevelBLL.AddFitnessLevelAsync(fitnessLevel);

            return CreatedAtAction(nameof(Get), new { id = fitnessLevel.FitnessLevelId }, fitnessLevel);
        }

        // PUT api/<FitnessLevelController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] FitnessLevelDTO fitnessLevel)
        {
            if (fitnessLevel == null)
            {
                return BadRequest("FitnessLevel data is missing");
            }

            var existingFitnessLevel = await fitnessLevelBLL.GetFitnessLevelByIdAsync(id);
            if (existingFitnessLevel == null)
            {
                return NotFound($"FitnessLevel with id {id} was not found.");
            }

            await fitnessLevelBLL.UpdateFitnessLevelAsync(fitnessLevel, id);
            return Ok(fitnessLevel);
        }

        // DELETE api/<FitnessLevelController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var fitnessLevel = await fitnessLevelBLL.GetFitnessLevelByIdAsync(id);
            if (fitnessLevel == null)
            {
                return NotFound($"FitnessLevel with id {id} was not found.");
            }

            await fitnessLevelBLL.DeleteFitnessLevelAsync(id);
            return Ok($"FitnessLevel with id {id} was deleted.");
        }
    }
}