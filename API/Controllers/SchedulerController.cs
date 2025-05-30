using BLL;
using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulerController : ControllerBase
    {
        //private SchedulerManager _schedulerManager;
        private readonly IExerciseBLL _exerciseBLL;
        private readonly IPlanDayBLL _planDayBLL;
        private readonly IExercisePlanBLL _planExerciseBLL;
        private readonly IGraphEdgeBLL _graphEdgeBLL;
        private readonly IMuscleEdgeBLL muscleEdgeBLL;
        private readonly IDeviceMuscleEdgeBLL _deviceMuscleEdgeBLL;
        private readonly ITraineeBLL _traineeBLL;

        public static SchedulerManager SharedSchedulerManager { get; set; }

        // אפשרות 1: יצירת מופע בכל קריאה
        public SchedulerController(IExerciseBLL exerciseBLL, IPlanDayBLL planDayBLL, IExercisePlanBLL planPlanBLL, IGraphEdgeBLL graphEdgeBLL, IMuscleEdgeBLL muscleEdgeBLL, IDeviceMuscleEdgeBLL deviceMuscleEdgeBLL, ITraineeBLL traineeBLL)
        {
            // דוגמה לאתחול פרמטרים, בפועל תביאי אותם מה-DB או מה-API
            this._exerciseBLL = exerciseBLL;
            _planDayBLL = planDayBLL;
            _planExerciseBLL = planPlanBLL;
            _graphEdgeBLL = graphEdgeBLL;
            this.muscleEdgeBLL = muscleEdgeBLL;
            _deviceMuscleEdgeBLL = deviceMuscleEdgeBLL;
            _traineeBLL = traineeBLL;
        }
        [HttpPost("init")]
        public async void Init(int slotMinutes1, int slotCount1)
        {
            // שליפת נתונים אסינכרונית
            var exercises = await _exerciseBLL.GetAllExercisesAsync();
            var equipmentCountByExercise = exercises
                .ToDictionary(e => e.ExerciseId, e => e.Count ?? 0);
            var graphEdge = await _graphEdgeBLL.GetAllGraphEdgeAsync();
            var muscleEdge = await muscleEdgeBLL.GetAllMuscleEdgeAsync();
            var deviceMuscleEdge = await _deviceMuscleEdgeBLL.GetAllDeviceMuscleEdgeAsync();

            int slotMinutes = slotMinutes1;
            int slotCount = slotCount1;

            // יצירת האובייקט כאן!
            SharedSchedulerManager = new SchedulerManager(_traineeBLL, exercises, graphEdge, deviceMuscleEdge, muscleEdge, equipmentCountByExercise, slotMinutes, slotCount, DateTime.Today.AddHours(7));
        }

        // דוגמה: קריאה לאלגוריתם לכל מתאמן
        [HttpPost("runAlgorithm")]
        public async Task<ActionResult<PathResult>> RunAlgorithm([FromBody] RunAlgorithmRequest request)
        {
            if (request == null || request.Trainee == null || request.planday == 0)
                return BadRequest("Invalid data");

            var trainee = await _traineeBLL.GetTraineeByIdAsync(request.Trainee);
            // קבלת כל התרגילים של התוכנית היומית
            List<ExercisePlanDTO> exerciseOrder = await _planExerciseBLL.GetExercisesByPlanDayIdAsync(request.planday);

            if (exerciseOrder == null || !exerciseOrder.Any())
                return NotFound("No exercises found for the selected plan day");

            // הרצת האלגוריתם
            var result =await SharedSchedulerManager.RunAlgorithmForTrainee(trainee, exerciseOrder, request.StartTime);
            return Ok(result);
        }

        [HttpPost("log")]
        public async Task<IActionResult> Print()
        {
            if (SharedSchedulerManager == null)
            {
                return BadRequest("SchedulerManager is not initialized. Please call /init first.");
            }
            SharedSchedulerManager.Print();
            return Ok("Matrix printed to console.");
        }
    }

    // מחלקת עזר לבקשה
    public class RunAlgorithmRequest
    {
        public int Trainee { get; set; }
        //public List<ExercisePlanDTO> ExerciseOrder { get; set; }
        public int planday {  get; set; }
        public DateTime StartTime { get; set; }
    }
}
