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
    public class PlanDayController : ControllerBase
    {
        private readonly IPlanDayBLL planDayBLL;

        public PlanDayController(IPlanDayBLL planDayBLL)
        {
            this.planDayBLL = planDayBLL;
        }

        [HttpGet]
        public async Task<ActionResult<List<PlanDayDTO>>> Get()
        {
            var days = await planDayBLL.GetAllPlanDaysAsync();
            return Ok(days);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlanDayDTO>> Get(int id)
        {
            var day = await planDayBLL.GetPlanDayByIdAsync(id);
            if (day == null)
                return NotFound();
            return Ok(day);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PlanDayDTO planDay)
        {
            if (planDay == null)
                return BadRequest("Invalid data.");
            await planDayBLL.AddPlanDayAsync(planDay);
            return Ok("Plan day added successfully.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] PlanDayDTO planDay)
        {
            if (planDay == null)
                return BadRequest("Invalid data.");
            await planDayBLL.UpdatePlanDayAsync(planDay, id);
            return Ok("Plan day updated successfully.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await planDayBLL.DeletePlanDayAsync(id);
            return Ok("Plan day deleted successfully.");
        }
     
    }
}