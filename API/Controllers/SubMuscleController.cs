using DBEntities.Models;
using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubMuscleController : ControllerBase
    {
        private readonly ISubMuscleBLL subMuscleBLL;
        public SubMuscleController(ISubMuscleBLL subMuscleBLL)
        {
            this.subMuscleBLL = subMuscleBLL;
        }


        // GET: api/<SubMuscleController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var subMuscles = await subMuscleBLL.GetAllSubMusclesAsync();
            return Ok(subMuscles);
        }

        // GET api/<SubMuscleController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            var subMuscle = await subMuscleBLL.GetSubMuscleByIdAsync(id);
            if (subMuscle == null)
            {
                return NotFound($"SubMuscle with id {id} was not found.");
            }
            return Ok(subMuscle);
        }

        // POST api/<SubMuscleController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] SubMuscleDTO subMuscle)
        {
            if (subMuscle == null)
            {
                return BadRequest("SubMuscle data is missing");
            }
            var subMuscle1 = await subMuscleBLL.GetSubMuscleByNameAsync(subMuscle.SubMuscleName);
            if (subMuscle1 != null)
            {
                return BadRequest($"SubMuscle with name {subMuscle.SubMuscleName} already exists.");
            }
            var newSubMuscle = new SubMuscleDTO
            {
                SubMuscleName = subMuscle.SubMuscleName,
                MuscleId = subMuscle.MuscleId
                // Do not set a value for ID, it will be automatically assigned
            }; 

            await subMuscleBLL.AddSubMuscleAsync(newSubMuscle);

            return CreatedAtAction(nameof(Get), new { id = newSubMuscle.SubMuscleId }, newSubMuscle);
        }

        // PUT api/<SubMuscleController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] SubMuscleDTO subMuscle)
        {
            if (subMuscle == null)
            {
                return BadRequest("SubMuscle data is missing");
            }
            //if (id != subMuscle.SubMuscleId)
            //{
            //    return BadRequest("SubMuscle id mismatch");
            //}
            var subMuscle1 = await subMuscleBLL.GetSubMuscleByIdAsync(id);
            if (subMuscle1 == null)
            {
                return NotFound($"SubMuscle with id {id} was not found.");
            }
            await subMuscleBLL.UpdateSubMuscleAsync(subMuscle, id);
            return Ok(subMuscle);
        }

        // DELETE api/<SubMuscleController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var subMuscle = await subMuscleBLL.GetSubMuscleByIdAsync(id);
            if (subMuscle == null)
            {
                return NotFound($"SubMuscle with id {id} was not found.");
            }
            await subMuscleBLL.DeleteSubMuscleAsync(id);
            return Ok($"SubMuscle with id {id} was deleted.");
        }
    }
}
