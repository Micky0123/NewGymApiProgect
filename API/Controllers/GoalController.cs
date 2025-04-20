using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoalController : ControllerBase
    {
        readonly IGoalBLL goalBLL;
        public GoalController(IGoalBLL goalBLL)
        {
            this.goalBLL = goalBLL;
        }

        // GET: api/<GoalController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var goals = await goalBLL.GetAllGoalsAsync();
            return Ok(goals);
        }

        // GET api/<GoalController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GoalDTO>> Get(int id)
        {
            var goal = await goalBLL.GetGoalByIdAsync(id);
            if (goal == null)
            {
                return NotFound($"goal with id {id} was not found.");
            }
            return Ok(goal);
        }

        // POST api/<GoalController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] GoalDTO goal)
        {
            if (goal == null)
            {
                return BadRequest("goal data is missing");
            }
            var goal1 = await goalBLL.GetGoalByNameAsync(goal.GoalName);
            if (goal1 != null)
            {
                return BadRequest($"goal with name {goal.GoalName} already exists.");
            }
            var newGoal = new GoalDTO
            {
                GoalName = goal.GoalName
                // אל תציב ערך ל-ID, הוא יוגדר אוטומטית
            };

            await goalBLL.AddGoalAsync(newGoal);

            return CreatedAtAction(nameof(Get), new { id = newGoal.GoalId }, newGoal);
        }

        // PUT api/<GoalController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] GoalDTO goalDTO)
        {
            if (goalDTO == null)
            {
                return BadRequest("goal data is missing");
            }
            //if (id != category.CategoryId)
            //{
            //    return BadRequest("Category id mismatch");
            //}
            var goal1 = await goalBLL.GetGoalByIdAsync(id);
            if (goal1 == null)
            {
                return NotFound($"goal with id {id} was not found.");
            }
            await goalBLL.UpdateGoalAsync(goalDTO, id);
            return Ok(goalDTO);
        }

        // DELETE api/<GoalController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var goal = await goalBLL.GetGoalByIdAsync(id);
            if (goal == null)
            {
                return NotFound($"goal with id {id} was not found.");
            }
            await goalBLL.DeleteGoalAsync(id);
            return Ok($"goal with id {id} was deleted.");
        }
    }
}
