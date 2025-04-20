using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MuscleController : ControllerBase
    {
        private readonly IMuscleBLL muscleBLL;
        private readonly ISubMuscleBLL subMuscleBLL;
        public MuscleController(IMuscleBLL muscleBLL)
        {
            this.muscleBLL = muscleBLL;
        }


        // GET: api/<MuscleController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var muscles = await muscleBLL.GetAllMusclesAsync();
            return Ok(muscles);
        }

        // GET api/<MuscleController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            var muscle = await muscleBLL.GetMuscleByIdAsync(id);
            if (muscle == null)
            {
                return NotFound($"Muscle with id {id} was not found.");
            }
            return Ok(muscle);
        }

        // POST api/<MuscleController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] MuscleDTO muscle)
        {
            if (muscle == null)
            {
                return BadRequest("Muscle data is missing");
            }
            var muscle1 = await muscleBLL.GetMuscleByNameAsync(muscle.MuscleName);
            if (muscle1 != null)
            {
                return BadRequest($"Muscle with name {muscle.MuscleName} already exists.");
            }
            var newMuscle = new MuscleDTO
            {
                MuscleName = muscle.MuscleName
                // Do not set a value for ID, it will be automatically assigned
            };

            await muscleBLL.AddMuscleAsync(newMuscle);

            return CreatedAtAction(nameof(Get), new { id = newMuscle.MuscleId }, newMuscle);
        }

        // PUT api/<MuscleController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] MuscleDTO muscle)
        {
            if (muscle == null)
            {
                return BadRequest("Muscle data is missing");
            }
            //if (id != muscle.MuscleId)
            //{
            //    return BadRequest("Muscle id mismatch");
            //}
            var muscle1 = await muscleBLL.GetMuscleByIdAsync(id);
            if (muscle1 == null)
            {
                return NotFound($"Muscle with id {id} was not found.");
            }
            await muscleBLL.UpdateMuscleAsync(muscle, id);
            return Ok(muscle);
        }

        // DELETE api/<MuscleController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var muscle = await muscleBLL.GetMuscleByIdAsync(id);
            if (muscle == null)
            {
                return NotFound($"Muscle with id {id} was not found.");
            }
            // Check if the muscle is used in any submuscle
            //get all submuscles with the same muscle id
            var subMuscle = await subMuscleBLL.GetAllMuscleByMuscleIdAsync(id);
            if (subMuscle != null)
            {
                return BadRequest($"Muscle with id {id} is used in submuscle.");
            }

            // Check if the muscle is used in any exercise
            await muscleBLL.DeleteMuscleAsync(id);
            return Ok($"Muscle with id {id} was deleted.");
        }
    }
}
