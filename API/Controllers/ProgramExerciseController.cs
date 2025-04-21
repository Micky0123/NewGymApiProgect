using IBLL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramExerciseController : ControllerBase
    {
        readonly IProgramExerciseBLL programExerciseBLL;
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
}
