using DBEntities.Models;
using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using static DTO.TraineeDTO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraineeController : ControllerBase
    {
        private readonly ITraineeBLL traineeBLL;
        public TraineeController(ITraineeBLL traineeBLL)
        {
            this.traineeBLL = traineeBLL;
        }


        // GET: api/<TraineeController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var trainees = await traineeBLL.GetAllTraineesAsync();
            return Ok(trainees);
        }

        // GET api/<TraineeController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            var trainee = await traineeBLL.GetTraineeByIdAsync(id);
            if (trainee == null)
            {
                return NotFound($"Trainee with id {id} was not found.");
            }
            return Ok(trainee);
        }

        // POST api/<TraineeController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] TraineeDTO trainee)
        {
            if (trainee == null)
            {
                return BadRequest("Trainee data is missing");
            }
            //צריך להוסיף בדיקה לפי סיסמא
            var trainee1 = await traineeBLL.GetTraineeByNameAsync(trainee.TraineeName);
            if (trainee1 != null)
            {
                return BadRequest($"Trainee with name {trainee.TraineeName} already exists.");
            }

            var newTrainee = new TraineeDTO
            {
                Idnumber= trainee.Idnumber,
                TraineeName = trainee.TraineeName,
                Age=trainee.Age,
                TraineeWeight=trainee.TraineeWeight,
                TraineeHeight=trainee.TraineeHeight,
                Gender = trainee.Gender,
                Phone = trainee.Phone,
                Email = trainee.Email,
                IsAdmin = trainee.IsAdmin,
                Password = trainee.Password,
                FitnessLevel = trainee.FitnessLevel,
                TrainingDays = trainee.TrainingDays,
                GoalId = trainee.GoalId,
                LoginDateTime=DateTime.Now
                // Do not set a value for ID, it will be automatically assigned
            };

            await traineeBLL.AddTraineeAsync(newTrainee);

            return CreatedAtAction(nameof(Get), new { id = newTrainee.TraineeId }, newTrainee);
        }

        // PUT api/<TraineeController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] TraineeDTO trainee)
        {
            if (trainee == null)
            {
                return BadRequest("Trainee data is missing");
            }
            //if (id != trainee.TraineeId)
            //{
            //    return BadRequest("Trainee id mismatch");
            //}
            var trainee1 = await traineeBLL.GetTraineeByIdAsync(id);
            if (trainee1 == null)
            {
                return NotFound($"Trainee with id {id} was not found.");
            }
            await traineeBLL.UpdateTraineeAsync(trainee, id);
            return Ok(trainee);
        }

        // DELETE api/<TraineeController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var trainee = await traineeBLL.GetTraineeByIdAsync(id);
            if (trainee == null)
            {
                return NotFound($"Trainee with id {id} was not found.");
            }
            await traineeBLL.DeleteTraineeAsync(id);
            return Ok($"Trainee with id {id} was deleted.");
        }
    }
}
