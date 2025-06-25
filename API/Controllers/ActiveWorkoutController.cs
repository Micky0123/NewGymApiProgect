using Microsoft.AspNetCore.Mvc;
using DTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL;
using IBLL;
using DAL;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActiveWorkoutController : ControllerBase
    {
        private readonly ActiveWorkoutManager _activeWorkoutManager;
        // private readonly SchedulerManager _schedulerManager;

        private readonly IExerciseBLL _exerciseBLL;
        private readonly IPlanDayBLL _planDayBLL;
        private readonly IExercisePlanBLL _planExerciseBLL;
        private readonly IGraphEdgeBLL _graphEdgeBLL;
        private readonly IMuscleEdgeBLL _muscleEdgeBLL;
        private readonly IDeviceMuscleEdgeBLL _deviceMuscleEdgeBLL;
        private readonly ITraineeBLL _traineeBLL;


        // הקונסטרקטור מקבל את המנהלים דרך DI
        public ActiveWorkoutController(
            ActiveWorkoutManager activeWorkoutManager,
            //SchedulerManager schedulerManager,
            IExerciseBLL exerciseBLL, IPlanDayBLL planDayBLL, IExercisePlanBLL planPlanBLL, IGraphEdgeBLL graphEdgeBLL, IMuscleEdgeBLL muscleEdgeBLL, IDeviceMuscleEdgeBLL deviceMuscleEdgeBLL, ITraineeBLL traineeBLL)
        {
            _activeWorkoutManager = activeWorkoutManager;
            // _schedulerManager = schedulerManager;
            // דוגמה לאתחול פרמטרים, בפועל תביאי אותם מה-DB או מה-API
            this._exerciseBLL = exerciseBLL;
            _planDayBLL = planDayBLL;
            _planExerciseBLL = planPlanBLL;
            _graphEdgeBLL = graphEdgeBLL;
            _muscleEdgeBLL = muscleEdgeBLL;
            _deviceMuscleEdgeBLL = deviceMuscleEdgeBLL;
            _traineeBLL = traineeBLL;
        }

        // אתחול ראשוני של הסקדולר (פעם אחת בתחילת יום/מערכת)
        [HttpPost("initialize")]

        public async Task<IActionResult> InitializeScheduler([FromBody] SchedulerInitRequest req)
        {
            if (_activeWorkoutManager.IsInitialized)
                return BadRequest("Scheduler already initialized!");

            try
            {
                var exercises = await _exerciseBLL.GetAllExercisesAsync();
                var equipmentCountByExercise = exercises.ToDictionary(e => e.ExerciseId, e => e.Count ?? 0);
                var graphEdge = await _graphEdgeBLL.GetAllGraphEdgeAsync();
                var muscleEdge = await _muscleEdgeBLL.GetAllMuscleEdgeAsync();
                var deviceMuscleEdge = await _deviceMuscleEdgeBLL.GetAllDeviceMuscleEdgeAsync();

                _activeWorkoutManager.Initialize(
                    exercises, graphEdge, deviceMuscleEdge, muscleEdge,
                    equipmentCountByExercise, DateTime.Today.AddHours(7),
                    req.SlotMinutes, req.SlotCount);

                return Ok("Scheduler initialized successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //  לאפשר איפוס
        [HttpPost("reset")]
        public IActionResult ResetScheduler()
        {
            _activeWorkoutManager.ResetScheduler();
            return Ok("Scheduler reset successfully");
        }


        // התחלת אימון חדש למתאמן
        [HttpPost("start-workout")]
        public async Task<IActionResult> StartWorkout([FromBody] RunAlgorithmRequest request)
        {
            try
            {
                if (request == null || request.Trainee == null || request.planday == 0)
                    return BadRequest("Invalid data");

                var trainee = await _traineeBLL.GetTraineeByIdAsync(request.Trainee);
                // קבלת כל התרגילים של התוכנית היומית
                List<ExercisePlanDTO> exerciseOrder = await _planExerciseBLL.GetExercisesByPlanDayIdAsync(request.planday);

                if (exerciseOrder == null || !exerciseOrder.Any())
                    return NotFound("No exercises found for the selected plan day");

                // הרצת האלגוריתם
                await _activeWorkoutManager.StartWorkoutAsync(trainee, exerciseOrder, request.StartTime, request.planday);

                return Ok("Workout started for trainee " + trainee.TraineeId);
            }
            catch (ServerBusyException ex) // תפוס את החריגה הספציפית שלך
            {
                Console.Error.WriteLine($"שגיאת שרת עמוס בהתחלת אימון: {ex.Message}");
                return StatusCode(429, new ProblemDetails
                {
                    Type = "https://example.com/problems/server-busy",
                    Title = "שרת עמוס",
                    Status = 429,
                    Detail = ex.Message, // ההודעה מהחריגה המותאמת אישית
                    Instance = HttpContext.Request.Path
                });
            }
            catch (Exception ex) // תפוס כל חריגה כללית אחרת
            {
                Console.Error.WriteLine($"שגיאה כללית בהתחלת אימון: {ex.Message}");
                // ניתן לשקול כאן להחזיר ProblemDetails גם לשגיאות כלליות
                return StatusCode(500, $"אירעה שגיאה פנימית בשרת: {ex.Message}");
            }
        }

        // התחלת תרגיל
        [HttpPost("start-exercise")]
        public IActionResult StartExercise([FromBody] StartOrCompleteExerciseRequest req)
        {
            try
            {
                bool result = _activeWorkoutManager.StartExercise(req.TraineeId, req.ExerciseId, req.StartTime);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // סיום תרגיל
        [HttpPost("complete-exercise")]
        public async Task<IActionResult> CompleteExercise([FromBody] StartOrCompleteExerciseRequest req)
        {
            try
            {
                bool result = await _activeWorkoutManager.CompleteExercise(req.TraineeId, req.ExerciseId, req.StartTime);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // (אופציונלי) הדפסת מטריצת Scheduler
        [HttpGet("print-matrix")]
        public IActionResult PrintMatrix()
        {
            _activeWorkoutManager.PrintSchedulerMatrix();
            return Ok("Printed Scheduler Matrix to console/server logs.");
        }

        // ב-ActiveWorkoutController.cs
        [HttpGet("trainee/{traineeId}/updated-workout-plan")]
        public async Task<IActionResult> GetUpdatedWorkoutPlanForTrainee(int traineeId)
        {
            try
            {
                // תפתח פונקציה זו שתחזיר את ה-PathResultDTO המעודכן מהזיכרון/מצב השרת
                var updatedPlan = _activeWorkoutManager.GetUpdatedWorkoutPlan(traineeId);

                if (updatedPlan == null)
                {
                    return NotFound("No active workout plan found or plan completed.");
                }

                return Ok(updatedPlan); // מחזיר את כל ה-PathResultDTO המעודכן
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // --- נקודת קצה עבור GetUpdatedWorkoutPlan ---
        // זו תתאים לבקשת GET ל- /api/ActiveWorkout/GetUpdatedWorkoutPlan/{traineeId}
        [HttpGet("GetUpdatedWorkoutPlan/{traineeId}")]
        [ProducesResponseType(typeof(PathResult), StatusCodes.Status200OK)] // מציין את סוג התגובה בהצלחה
        [ProducesResponseType(StatusCodes.Status404NotFound)] // מציין תגובת 404
        public ActionResult<PathResult> GetUpdatedWorkoutPlan(int traineeId)
        {
            var result = _activeWorkoutManager.GetUpdatedWorkoutPlan(traineeId);
            if (result == null)
            {
                // חשוב: אם הפונקציה ב-manager מחזירה null כשלא נמצא אימון, צריך להחזיר NotFound()
                // זה יגרום לשגיאת 404 שהפרונטאנד שלך מצפה לה
                return NotFound($"No active workout found for trainee ID: {traineeId}.");
            }
            return Ok(result); // החזרת הנתונים עם קוד 200 OK
        }

        // --- נקודת קצה עבור GetNextExerciseInWorkout ---
        // זו תתאים לבקשת GET ל- /api/ActiveWorkout/GetNextExerciseInWorkout/{traineeId}
        [HttpGet("GetNextExerciseInWorkout/{traineeId}")]
        [ProducesResponseType(typeof(NextExerciseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<NextExerciseResponse> GetNextExerciseInWorkout(int traineeId)
        {
            var response = _activeWorkoutManager.GetNextExerciseInWorkout(traineeId);

            // במקרה ש-GetNextExerciseInWorkout מחזיר תגובה שמציינת שהאימון הושלם/לא נמצא,
            // נחזיר תמיד Ok, כי ה-NextExerciseResponse DTO כבר מכיל את המידע הזה.
            // הפרונטאנד יבדוק את שדה IsWorkoutComplete כדי לדעת את הסטטוס.
            return Ok(response);
        }

        // ***** NEW ENDPOINT *****
        [HttpGet("active-plan/{traineeId}")] // Example: GET /api/ActiveWorkout/active-plan/1
        public async Task<ActionResult<ActiveTrainingPlanResponse>> GetTraineeActiveTrainingPlan(int traineeId)
        {
            try
            {
                var activePlan = await _activeWorkoutManager.GetActiveTrainingPlanForTrainee(traineeId);

                if (activePlan == null)
                {
                    return NotFound($"No active training plan found for trainee with ID {traineeId}.");
                }

                return Ok(activePlan);
            }
            catch (Exception ex)
            {
                // Log the exception (use a proper logger in real app)
                Console.WriteLine($"Error in GetTraineeActiveTrainingPlan: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving the active training plan.");
            }
        }



        [HttpGet("all-trainees")]
        public async Task<ActionResult<IEnumerable<TraineeDTO>>> GetAllActiveTraineesIds()
        {
            try
            {
                //var trainees = await _traineeBLL.GetAllTraineesAsync(); // יש לוודא שפונקציה זו קיימת ב-ITraineeBLL ומחזירה רשימה של TraineeDTO
                List<TraineeDTO> trainees = await _activeWorkoutManager.GetAllActiveTraineesId(); // יש לוודא שפונקציה זו קיימת ב-ITraineeBLL ומחזירה רשימה של TraineeDTO
                //var traineeList = new List<TraineeDTO>();
                //foreach (var trainee in trainees)
                //{
                //    traineeList.Add(await _traineeBLL.GetTraineeByIdAsync(trainee));
                //}

                if (trainees == null)
                {
                    return NotFound("No trainees found in the system.");
                }
                return Ok(trainees);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllTrainees: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving trainees.");
            }
        }



        [HttpGet("all-active-workouts")]
        public ActionResult<IEnumerable<ActiveTrainingPlanResponse>> GetAllActiveWorkouts()
        {
            try
            {
                // נניח ש-ActiveWorkoutManager מכיל מתודה כזו
                // שתחזיר רשימה של כל האימונים הפעילים כרגע בזיכרון המערכת
                var activeWorkouts = _activeWorkoutManager.GetAllActiveWorkouts();

                if (activeWorkouts == null || !activeWorkouts.Any())
                {
                    return Ok(new List<ActiveTrainingPlanResponse>()); // החזר רשימה ריקה אם אין
                }
                return Ok(activeWorkouts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllActiveWorkouts: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving all active workouts.");
            }
        }

        //[HttpGet("all-active-trainees")]
        //public ActionResult<List<int>> GetAllActiveTraineesIds() => new List<int>(); // List<int> GetAllActiveTraineesIds()
}




// מודלים ל-Request
public class StartWorkoutRequest
{
    public TraineeDTO Trainee { get; set; }
    public List<ExercisePlanDTO> ExerciseOrder { get; set; }
    public DateTime StartTime { get; set; }
    public int PlanDayId { get; set; }
}

public class StartOrCompleteExerciseRequest
{
    public int TraineeId { get; set; }
    public int ExerciseId { get; set; }
    public DateTime StartTime { get; set; }
}

public class SchedulerInitRequest
{
    public int SlotMinutes { get; set; }
    public int SlotCount { get; set; }
}
    //public class RunAlgorithmRequest
    //{
    //    public int Trainee { get; set; }
    //    //public List<ExercisePlanDTO> ExerciseOrder { get; set; }
    //    public int planday { get; set; }
    //    public DateTime StartTime { get; set; }
    //}
}