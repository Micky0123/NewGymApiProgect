using AutoMapper;
using BLL;
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
        //private readonly ITraineeBLL traineeBLL;
        private readonly IMapper _mapper;
        private readonly ITraineeBLL _traineeBLL; // שיניתי לשם עקבי עם _
        private readonly CreateTrainingPlan _createTrainingPlan; // הוספנו את הזרקת השירות

        public TraineeController(ITraineeBLL traineeBLL, IMapper mapper, CreateTrainingPlan createTrainingPlan)
        {
            _traineeBLL = traineeBLL;
            _mapper = mapper;
            _createTrainingPlan = createTrainingPlan; // אתחול
        }

        // GET: api/<TraineeController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var trainees = await _traineeBLL.GetAllTraineesAsync();
            return Ok(trainees);
        }

        // GET api/<TraineeController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            var trainee = await _traineeBLL.GetTraineeByIdAsync(id);
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

            var trainee = await _traineeBLL.GetTraineeByNameAsync(loginRequest.Username);
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


        [HttpPost("Register")] // שיניתי את הראוט ל-"Register" כדי שיהיה יותר ברור
        public async Task<ActionResult> Register([FromBody] DTO.RegisterRequest request) // נשתמש ב-RegisterRequest DTO חדש
        {
            if (request == null)
            {
                Console.WriteLine("Register request data is null");
                return BadRequest("Register data is missing");
            }

            Console.WriteLine($"Received TraineeName: {request.TraineeName}, Email: {request.Email}");

            // 1. בדוק אם מתאמן עם אותו שם או ת"ז כבר קיים (מומלץ לבדוק לפי ID Number או Email במקום TraineeName)
            // נניח ש-GetTraineeByIdNumberAsync קיים ב-TraineeBLL
            var existingTraineeByIdNumber = await _traineeBLL.GetTraineeByIdNumberAsync(request.IdNumber);
            if (existingTraineeByIdNumber != null)
            {
                Console.WriteLine($"Trainee with ID number {request.IdNumber} already exists.");
                return Conflict($"Trainee with ID number {request.IdNumber} already exists."); // Conflict 409 הוא קוד סטטוס מתאים יותר
            }

            // ניתן גם לבדוק לפי אימייל:
            // var existingTraineeByEmail = await _traineeBLL.GetTraineeByEmailAsync(request.Email);
            // if (existingTraineeByEmail != null) { ... return Conflict(...) }

            try
            {
                // 2. מיפוי מ-RegisterRequest ל-TraineeDTO ויצירת מתאמן חדש
                var newTraineeDto = _mapper.Map<TraineeDTO>(request);
                newTraineeDto.LoginDateTime = DateTime.Now; // עדכון שדה ספציפי
                newTraineeDto.IsAdmin = false; // לוודא שמשתמש חדש אינו אדמין כברירת מחדל
                newTraineeDto.TraineeId = 0; // ודא שה-ID מאופס כדי שה-DB יקצה חדש

                // **חשוב:** כאן יש לבצע Hash לסיסמה לפני השמירה ב-DB!
                // לדוגמה: newTraineeDto.Password = HashPassword(request.Password);
                // כרגע נשאיר את זה כפי שזה, אך זו חולשה אבטחתית חמורה שיש לתקן בהקדם.

                // שמור את המתאמן ב-DB וקבל את ה-TraineeDTO המעודכן (עם ה-TraineeId שנוצר)
                var createdTrainee = await _traineeBLL.AddTraineeAsync(newTraineeDto);

                if (createdTrainee == null || createdTrainee.TraineeId == 0)
                {
                    // אם יצירת המתאמן נכשלה או לא הוחזר ID
                    Console.WriteLine("Failed to create trainee or retrieve trainee ID.");
                    return StatusCode(500, "Failed to register trainee. Please try again.");
                }

                // 3. יצירת תוכנית אימון דיפולטיבית עבור המתאמן החדש
                // משתמשים ב-TraineeId של המתאמן שנוצר זה עתה
                await _createTrainingPlan.addProgramExerciseAsync(
                    request.TrainingDays,
                    request.GoalId,
                    request.FitnessLevelId,
                    request.TrainingDuration,
                    createdTrainee.TraineeId // העברת ה-TraineeId החדש
                );

                Console.WriteLine($"Trainee {createdTrainee.TraineeName} successfully registered and default program created.");
                return CreatedAtAction(nameof(Get), new { id = createdTrainee.TraineeId }, createdTrainee); // מחזיר 201 Created
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error during registration: {ex.Message}");
                // ניתן להוסיף יותר פרטים לשגיאה אם צריך (לדוגמה, ex.InnerException?.Message)
                return StatusCode(500, $"An error occurred during registration: {ex.Message}");
            }
        }


        [HttpPost]

        public async Task<ActionResult> Post([FromBody] TraineeDTO trainee)
        {
            if (trainee == null)
            {
                Console.WriteLine("Trainee data is null");
                return BadRequest("Trainee data is missing");
            }

            Console.WriteLine($"Received TraineeName: {trainee.TraineeName}, Email: {trainee.Email}");

            var trainee1 = await _traineeBLL.GetTraineeByNameAsync(trainee.TraineeName);
            if (trainee1 != null)
            {
                Console.WriteLine("Trainee already exists.");
                return BadRequest($"Trainee with name {trainee.TraineeName} already exists.");
            }

            // שימוש ב-AutoMapper למיפוי אוטומטי
            var newTrainee = _mapper.Map<TraineeDTO>(trainee);
            newTrainee.LoginDateTime = DateTime.Now; // עדכון שדה ספציפי

            await _traineeBLL.AddTraineeAsync(newTrainee);

            Console.WriteLine("Trainee successfully added.");
            return CreatedAtAction(nameof(Get), new { id = newTrainee.TraineeId }, newTrainee);
        }

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

            var existingTrainee = await _traineeBLL.GetTraineeByIdAsync(id);
            if (existingTrainee == null)
            {
                return NotFound($"Trainee with id {id} was not found.");
            }

            // ודא ש-TraineeId לא מתעדכן
            trainee.TraineeId = id;

            await _traineeBLL.UpdateTraineeAsync(trainee, id);
            return Ok(trainee);
        }
        // DELETE api/<TraineeController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var trainee = await _traineeBLL.GetTraineeByIdAsync(id);
            if (trainee == null)
            {
                return NotFound($"Trainee with id {id} was not found.");
            }
            await _traineeBLL.DeleteTraineeAsync(id);
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