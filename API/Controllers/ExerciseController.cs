using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExerciseController : ControllerBase
    {
        readonly IExerciseBLL exerciseBLL;
        public ExerciseController(IExerciseBLL exerciseBLL)
        {
            this.exerciseBLL = exerciseBLL;
        }


        // GET: api/<ExerciseController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var exercises = await exerciseBLL.GetAllExercisesAsync();
            return Ok(exercises);
        }

        // GET api/<ExerciseController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExerciseDTO>> Get(int id)
        {
            var exercise = await exerciseBLL.GetExerciseByIdAsync(id);
            if (exercise == null)
            {
                return NotFound($"Exercise with id {id} was not found.");
            }
            return Ok(exercise);
        }

        // POST api/<ExerciseController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] ExerciseDTO exercise)
        {
            if (exercise == null)
            {
                return BadRequest("Exercise data is missing");
            }
            var exercise1 = await exerciseBLL.GetExerciseByNameAsync(exercise.ExerciseName);
            if (exercise1 != null)
            {
                return BadRequest($"Exercise with name {exercise.ExerciseName} already exists.");
            }
            var newExercise = new ExerciseDTO
            {
                ExerciseName = exercise.ExerciseName
                // Do not set a value for ID, it will be automatically assigned
            };

            await exerciseBLL.AddExerciseAsync(newExercise);

            return CreatedAtAction(nameof(Get), new { id = newExercise.ExerciseId }, newExercise);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] ExerciseDTO exercise)
        {
            if (exercise == null)
            {
                return BadRequest("Exercise data is missing");
            }
            //if (id != exercise.ExerciseId)
            //{
            //    return BadRequest("Exercise id mismatch");
            //}
            var exercise1 = await exerciseBLL.GetExerciseByIdAsync(id);
            if (exercise1 == null)
            {
                return NotFound($"Exercise with id {id} was not found.");
            }
            await exerciseBLL.UpdateExerciseAsync(exercise, id);
            return Ok(exercise);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var exercise = await exerciseBLL.GetExerciseByIdAsync(id);
            if (exercise == null)
            {
                return NotFound($"Exercise with id {id} was not found.");
            }
            await exerciseBLL.DeleteExerciseAsync(id);
            return Ok($"Exercise with id {id} was deleted.");
        }
    }
}
