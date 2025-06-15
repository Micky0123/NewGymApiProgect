using AutoMapper;
using DBEntities.Models;
using DTO;
using IBLL;
using Microsoft.AspNetCore.Identity.Data;
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
        private readonly IMapper _mapper;

        public TraineeController(ITraineeBLL traineeBLL, IMapper mapper)
        {
            this.traineeBLL = traineeBLL;
            _mapper = mapper;

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
        // פעולת Login
        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest("Username or password is missing.");
            }

            Console.WriteLine($"Received Username: {loginRequest.Username}, Password: {loginRequest.Password}");

            var trainee = await traineeBLL.GetTraineeByNameAsync(loginRequest.Username);
            if (trainee == null || trainee.Password != loginRequest.Password)
            {
                Console.WriteLine("Invalid credentials");
                return Unauthorized("Invalid username or password.");
            }

            return Ok(new
            {
                Message = "Login successful",
                Password = trainee.TraineeId,
                traineeName = trainee.TraineeName,
                IsAdmin = trainee.IsAdmin
            });
        }



        [HttpPost]
        //public async Task<ActionResult> Post([FromBody] TraineeDTO trainee)
        //{
        //    if (trainee == null)
        //    {
        //        Console.WriteLine("Trainee data is null");
        //        return BadRequest("Trainee data is missing");
        //    }

        //    Console.WriteLine($"Received TraineeName: {trainee.TraineeName}, Email: {trainee.Email}");

        //    var trainee1 = await traineeBLL.GetTraineeByNameAsync(trainee.TraineeName);
        //    if (trainee1 != null)
        //    {
        //        Console.WriteLine("Trainee already exists.");
        //        return BadRequest($"Trainee with name {trainee.TraineeName} already exists.");
        //    }

        //    var newTrainee = new TraineeDTO
        //    {
        //        Idnumber = trainee.Idnumber,
        //        TraineeName = trainee.TraineeName,
        //        Age = trainee.Age,
        //        TraineeWeight = trainee.TraineeWeight,
        //        TraineeHeight = trainee.TraineeHeight,
        //        Gender = trainee.Gender,
        //        Phone = trainee.Phone,
        //        Email = trainee.Email,
        //        IsAdmin = trainee.IsAdmin,
        //        Password = trainee.Password,
        //        FitnessLevelId = trainee.FitnessLevelId,
        //        TrainingDays = trainee.TrainingDays,
        //        GoalId = trainee.GoalId,
        //        TrainingDuration = trainee.TrainingDuration,
        //        LoginDateTime = DateTime.Now
        //    };

        //    await traineeBLL.AddTraineeAsync(newTrainee);

        //    Console.WriteLine("Trainee successfully added.");
        //    return CreatedAtAction(nameof(Get), new { id = newTrainee.TraineeId }, newTrainee);
        //}
        public async Task<ActionResult> Post([FromBody] TraineeDTO trainee)
        {
            if (trainee == null)
            {
                Console.WriteLine("Trainee data is null");
                return BadRequest("Trainee data is missing");
            }

            Console.WriteLine($"Received TraineeName: {trainee.TraineeName}, Email: {trainee.Email}");

            var trainee1 = await traineeBLL.GetTraineeByNameAsync(trainee.TraineeName);
            if (trainee1 != null)
            {
                Console.WriteLine("Trainee already exists.");
                return BadRequest($"Trainee with name {trainee.TraineeName} already exists.");
            }

            // שימוש ב-AutoMapper למיפוי אוטומטי
            var newTrainee = _mapper.Map<TraineeDTO>(trainee);
            newTrainee.LoginDateTime = DateTime.Now; // עדכון שדה ספציפי

            await traineeBLL.AddTraineeAsync(newTrainee);

            Console.WriteLine("Trainee successfully added.");
            return CreatedAtAction(nameof(Get), new { id = newTrainee.TraineeId }, newTrainee);
        }

        // PUT api/<TraineeController>/5
        //[HttpPut("{id}")]
        //public async Task<ActionResult> Put(int id, [FromBody] TraineeDTO trainee)
        //{
        //    if (trainee == null)
        //    {
        //        return BadRequest("Trainee data is missing");
        //    }
        //    //if (id != trainee.TraineeId)
        //    //{
        //    //    return BadRequest("Trainee id mismatch");
        //    //}
        //    var trainee1 = await traineeBLL.GetTraineeByIdAsync(id);
        //    if (trainee1 == null)
        //    {
        //        return NotFound($"Trainee with id {id} was not found.");
        //    }
        //    await traineeBLL.UpdateTraineeAsync(trainee, id);
        //    return Ok(trainee);
        //}
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] TraineeDTO trainee)
        {
            if (trainee == null)
            {
                return BadRequest("Trainee data is missing");
            }

            // ודא שהתואם בין ID שנשלח ל-TraineeId
            if (trainee.TraineeId != 0 && trainee.TraineeId != id)
            {
                return BadRequest("Trainee ID mismatch.");
            }

            var existingTrainee = await traineeBLL.GetTraineeByIdAsync(id);
            if (existingTrainee == null)
            {
                return NotFound($"Trainee with id {id} was not found.");
            }

            // ודא ש-TraineeId לא מתעדכן
            trainee.TraineeId = id;

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



    // מחלקה עבור בקשת Login
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
