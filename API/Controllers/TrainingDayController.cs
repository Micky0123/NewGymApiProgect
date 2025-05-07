using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingDayController : ControllerBase
    {
        private readonly ITrainingDayBLL trainingDayBLL;

        public TrainingDayController(ITrainingDayBLL trainingDayBLL)
        {
            this.trainingDayBLL = trainingDayBLL;
        }

        // GET: api/<TrainingDayController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrainingDayDTO>>> Get()
        {
            var trainingDays = await trainingDayBLL.GetAllTrainingDaysAsync();
            return Ok(trainingDays);
        }

        // GET api/<TrainingDayController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TrainingDayDTO>> Get(int id)
        {
            var trainingDay = await trainingDayBLL.GetTrainingDayByIdAsync(id);
            if (trainingDay == null)
            {
                return NotFound($"TrainingDay with id {id} was not found.");
            }
            return Ok(trainingDay);
        }

        // POST api/<TrainingDayController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] TrainingDayDTO trainingDay)
        {
            if (trainingDay == null)
            {
                return BadRequest("TrainingDay data is missing");
            }

            // Validate the range of training days
            if (trainingDay.MinNumberDays < 0 || trainingDay.MaxNumberDays < trainingDay.MinNumberDays)
            {
                return BadRequest("Invalid training days range.");
            }

            await trainingDayBLL.AddTrainingDayAsync(trainingDay);

            return CreatedAtAction(nameof(Get), new { id = trainingDay.TrainingDaysId }, trainingDay);
        }

        // PUT api/<TrainingDayController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] TrainingDayDTO trainingDay)
        {
            if (trainingDay == null)
            {
                return BadRequest("TrainingDay data is missing");
            }

            // Validate the range of training days
            if (trainingDay.MinNumberDays < 0 || trainingDay.MaxNumberDays < trainingDay.MinNumberDays)
            {
                return BadRequest("Invalid training days range.");
            }

            var existingTrainingDay = await trainingDayBLL.GetTrainingDayByIdAsync(id);
            if (existingTrainingDay == null)
            {
                return NotFound($"TrainingDay with id {id} was not found.");
            }

            await trainingDayBLL.UpdateTrainingDayAsync(trainingDay, id);
            return Ok(trainingDay);
        }

        // DELETE api/<TrainingDayController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var trainingDay = await trainingDayBLL.GetTrainingDayByIdAsync(id);
            if (trainingDay == null)
            {
                return NotFound($"TrainingDay with id {id} was not found.");
            }

            await trainingDayBLL.DeleteTrainingDayAsync(id);
            return Ok($"TrainingDay with id {id} was deleted.");
        }
    }
}