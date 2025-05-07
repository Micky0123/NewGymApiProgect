using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingDurationController : ControllerBase
    {
        private readonly ITrainingDurationBLL trainingDurationBLL;

        public TrainingDurationController(ITrainingDurationBLL trainingDurationBLL)
        {
            this.trainingDurationBLL = trainingDurationBLL;
        }

        // GET: api/<TrainingDurationController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrainingDurationDTO>>> Get()
        {
            var trainingDurations = await trainingDurationBLL.GetAllTrainingDurationAsync();
            return Ok(trainingDurations);
        }

        // GET api/<TrainingDurationController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TrainingDurationDTO>> Get(int id)
        {
            var trainingDuration = await trainingDurationBLL.GetTrainingDurationByIdAsync(id);
            if (trainingDuration == null)
            {
                return NotFound($"TrainingDuration with id {id} was not found.");
            }
            return Ok(trainingDuration);
        }

        // POST api/<TrainingDurationController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] TrainingDurationDTO trainingDuration)
        {
            if (trainingDuration == null)
            {
                return BadRequest("TrainingDuration data is missing");
            }

            await trainingDurationBLL.AddTrainingDurationAsync(trainingDuration);

            return CreatedAtAction(nameof(Get), new { id = trainingDuration.TrainingDurationId }, trainingDuration);
        }

        // PUT api/<TrainingDurationController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] TrainingDurationDTO trainingDuration)
        {
            if (trainingDuration == null)
            {
                return BadRequest("TrainingDuration data is missing");
            }

            var existingTrainingDuration = await trainingDurationBLL.GetTrainingDurationByIdAsync(id);
            if (existingTrainingDuration == null)
            {
                return NotFound($"TrainingDuration with id {id} was not found.");
            }

            await trainingDurationBLL.UpdateTrainingDurationAsync(trainingDuration, id);
            return Ok(trainingDuration);
        }

        // DELETE api/<TrainingDurationController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var trainingDuration = await trainingDurationBLL.GetTrainingDurationByIdAsync(id);
            if (trainingDuration == null)
            {
                return NotFound($"TrainingDuration with id {id} was not found.");
            }

            await trainingDurationBLL.DeleteTrainingDurationAsync(id);
            return Ok($"TrainingDuration with id {id} was deleted.");
        }
    }
}