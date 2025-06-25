using BLL;
using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExercisePlanController : ControllerBase
    {
        private readonly IExercisePlanBLL exercisePlanBLL;

        public ExercisePlanController(IExercisePlanBLL exercisePlanBLL)
        {
            this.exercisePlanBLL = exercisePlanBLL;
        }

        [HttpGet]
        public async Task<ActionResult<List<ExercisePlanDTO>>> Get()
        {
            var plans = await exercisePlanBLL.GetAllExercisePlansAsync();
            return Ok(plans);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExercisePlanDTO>> Get(int id)
        {
            var plan = await exercisePlanBLL.GetExercisePlanByIdAsync(id);
            if (plan == null)
                return NotFound();
            return Ok(plan);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ExercisePlanDTO exercisePlan)
        {
            if (exercisePlan == null)
                return BadRequest("Invalid data.");
            await exercisePlanBLL.AddExercisePlanAsync(exercisePlan);
            return Ok("Exercise plan added successfully.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ExercisePlanDTO exercisePlan)
        {
            if (exercisePlan == null)
                return BadRequest("Invalid data.");
            await exercisePlanBLL.UpdateExercisePlanAsync(exercisePlan, id);
            return Ok("Exercise plan updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await exercisePlanBLL.DeleteExercisePlanAsync(id);
            return Ok("Exercise plan deleted successfully.");
        }

        // --- נקודת הקצה החדשה שביקשת ---
        [HttpGet("planDay/{planDayId}/exercises")]
        public async Task<ActionResult<List<ExercisePlanDTO>>> GetExercisesForPlanDay(int planDayId)
        {
            var exercises = await exercisePlanBLL.GetExercisesByPlanDayIdAsync(planDayId);
            if (exercises == null || !exercises.Any())
            {
                // ייתכן שתרצה להחזיר רשימה ריקה במקום NotFound אם זה מקרה לגיטימי
                return Ok(new List<ExercisePlanDTO>());
            }
            return Ok(exercises);
        }
    }
}