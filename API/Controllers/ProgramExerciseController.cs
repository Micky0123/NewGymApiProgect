using BLL;
using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramExerciseController : ControllerBase
    {
        private readonly IProgramExerciseBLL programExerciseBLL;

        public ProgramExerciseController(IProgramExerciseBLL programExerciseBLL)
        {
            this.programExerciseBLL = programExerciseBLL;
        }

        // GET: api/<ProgramExerciseController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<ProgramExerciseController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ProgramExerciseController>
        [HttpPost]
        public async Task PostAsync([FromBody] string value)
        {
            //string filePath = "@WorkoutData.xlsx"; // עדכן את הנתיב לקובץ שלך
            string filePath = value; // עדכן את הנתיב לקובץ שלך
            await programExerciseBLL.ReadDataFromExcelAsync(filePath);
        }

        // פעולה חדשה: הוספת תוכנית אימון
        [HttpPost("AddProgramExercise")]
        public async Task<IActionResult> AddProgramExerciseAsync([FromBody] ProgramExerciseRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var programExercise = new ProgramExerciseDTO
                {
                    // הנח שצריך לעדכן נתונים מ-ProgramExerciseRequest ל-ProgramExerciseDTO
                    // הוספת שדות לפי הצורך
                };
                string filePath = "WorkoutData.xlsx";
                await programExerciseBLL.AddProgramExerciseAsync(programExercise, request.DaysInWeek, request.Goal, request.Level, request.Time);
                return Ok("Program exercise added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // PUT api/<ProgramExerciseController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ProgramExerciseController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    // מחלקת עזר עבור בקשת הוספת תוכנית אימון
    public class ProgramExerciseRequest
    {
        //public string FilePath { get; set; }
        public int DaysInWeek { get; set; }
        public int Goal { get; set; }
        public int Level { get; set; }
        public int Time { get; set; }
    }
}