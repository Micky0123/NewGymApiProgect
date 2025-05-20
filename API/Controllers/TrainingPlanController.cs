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
    public class TrainingPlanController : ControllerBase
    {
        private readonly ITrainingPlanBLL trainingPlanBLL;

        public TrainingPlanController(ITrainingPlanBLL trainingPlanBLL)
        {
            this.trainingPlanBLL = trainingPlanBLL;
        }

        [HttpGet]
        public async Task<ActionResult<List<TrainingPlanDTO>>> Get()
        {
            var plans = await trainingPlanBLL.GetAllTrainingPlansAsync();
            return Ok(plans);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TrainingPlanDTO>> Get(int id)
        {
            var plan = await trainingPlanBLL.GetTrainingPlanByIdAsync(id);
            if (plan == null)
                return NotFound();
            return Ok(plan);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrainingPlanDTO trainingPlan)
        {
            if (trainingPlan == null)
                return BadRequest("Invalid data.");
            await trainingPlanBLL.AddTrainingPlanAsync(trainingPlan);
            return Ok("Training plan added successfully.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TrainingPlanDTO trainingPlan)
        {
            if (trainingPlan == null)
                return BadRequest("Invalid data.");
            await trainingPlanBLL.UpdateTrainingPlanAsync(trainingPlan, id);
            return Ok("Training plan updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await trainingPlanBLL.DeleteTrainingPlanAsync(id);
            return Ok("Training plan deleted successfully.");
        }
    }
}