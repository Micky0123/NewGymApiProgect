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
        //public async Task<IActionResult> InitializeScheduler([FromBody] SchedulerInitRequest req)
        //{

        //    try
        //    {
        //        //// שליפת נתונים אסינכרונית
        //        //var exercises = await _exerciseBLL.GetAllExercisesAsync();
        //        //var equipmentCountByExercise = exercises
        //        //    .ToDictionary(e => e.ExerciseId, e => e.Count ?? 0);
        //        //var graphEdge = await _graphEdgeBLL.GetAllGraphEdgeAsync();
        //        //var muscleEdge = await muscleEdgeBLL.GetAllMuscleEdgeAsync();
        //        //var deviceMuscleEdge = await _deviceMuscleEdgeBLL.GetAllDeviceMuscleEdgeAsync();

        //        //var slotMinutes = req.SlotMinutes;
        //        //int slotCount = req.SlotCount;

        //        //// אחראי לבצע אתחול דרך SchedulerManager (אם צריך)
        //        //var activeWorkoutManager = new ActiveWorkoutManager(_traineeBLL, exercises, graphEdge, deviceMuscleEdge, muscleEdge, equipmentCountByExercise, slotMinutes, slotCount, DateTime.Today.AddHours(7));
        //        ////_activeWorkoutManager.Initialize(exercises, graphEdge, deviceMuscleEdge, muscleEdge, equipmentCountByExercise, DateTime.Today.AddHours(7), slotMinutes, slotCount);
        //        ////return Ok("Scheduler initialized successfully");
        //        //var exercises = await _exerciseBLL.GetAllExercisesAsync();

        //        var exercises = await _exerciseBLL.GetAllExercisesAsync();
        //        var equipmentCountByExercise = exercises.ToDictionary(e => e.ExerciseId, e => e.Count ?? 0);
        //        var graphEdge = await _graphEdgeBLL.GetAllGraphEdgeAsync();
        //        var muscleEdge = await _muscleEdgeBLL.GetAllMuscleEdgeAsync();
        //        var deviceMuscleEdge = await _deviceMuscleEdgeBLL.GetAllDeviceMuscleEdgeAsync();

        //        _activeWorkoutManager.Initialize(
        //            exercises, graphEdge, deviceMuscleEdge, muscleEdge,
        //            equipmentCountByExercise, DateTime.Today.AddHours(7),
        //            req.SlotMinutes, req.SlotCount);

        //        return Ok("Scheduler initialized successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
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
        public async Task<IActionResult> StartWorkout([FromBody] StartWorkoutRequest req)
        {
            try
            {
                await _activeWorkoutManager.StartWorkoutAsync(req.Trainee, req.ExerciseOrder, req.StartTime, req.PlanDayId);
                return Ok("Workout started for trainee " + req.Trainee.TraineeId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
        public IActionResult CompleteExercise([FromBody] StartOrCompleteExerciseRequest req)
        {
            try
            {
                bool result = _activeWorkoutManager.CompleteExercise(req.TraineeId, req.ExerciseId, req.StartTime);
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
        //public List<ExerciseDTO> ExerciseList { get; set; }
        //public List<GraphEdgeDTO> ExerciseEdges { get; set; }
        //public List<DeviceMuscleEdgeDTO> ExerciseToMuscleEdges { get; set; }
        //public List<MuscleEdgeDTO> MuscleEdges { get; set; }
        //public Dictionary<int, int> EquipmentCountByExercise { get; set; }
        //public DateTime FirstSlotStart { get; set; }
        public int SlotMinutes { get; set; }
        public int SlotCount { get; set; }
    }

    //    [Route("api/[controller]")]
    //    [ApiController]
    //    public class ActiveWorkoutController : ControllerBase
    //    {
    //        private readonly ActiveWorkoutManager _activeWorkoutManager;

    //        public ActiveWorkoutController(ActiveWorkoutManager activeWorkoutManager)
    //        {
    //            _activeWorkoutManager = activeWorkoutManager;
    //        }

    //        // התחלת אימון חדש למתאמן
    //        [HttpPost("start-workout")]
    //        public async Task<IActionResult> StartWorkout([FromBody] StartWorkoutRequest req)
    //        {
    //            try
    //            {
    //                await _activeWorkoutManager.StartWorkoutAsync(req.Trainee, req.ExerciseOrder, req.StartTime, req.PlanDayId);
    //                return Ok("Workout started for trainee " + req.Trainee.TraineeId);
    //            }
    //            catch (Exception ex)
    //            {
    //                return BadRequest(ex.Message);
    //            }
    //        }

    //        // התחלת תרגיל
    //        [HttpPost("start-exercise")]
    //        public IActionResult StartExercise([FromBody] StartOrCompleteExerciseRequest req)
    //        {
    //            try
    //            {
    //                bool result = _activeWorkoutManager.StartExercise(req.TraineeId, req.ExerciseId, req.StartTime);
    //                return Ok(result);
    //            }
    //            catch (Exception ex)
    //            {
    //                return BadRequest(ex.Message);
    //            }
    //        }

    //        // סיום תרגיל
    //        [HttpPost("complete-exercise")]
    //        public IActionResult CompleteExercise([FromBody] StartOrCompleteExerciseRequest req)
    //        {
    //            try
    //            {
    //                bool result = _activeWorkoutManager.CompleteExercise(req.TraineeId, req.ExerciseId, req.StartTime);
    //                return Ok(result);
    //            }
    //            catch (Exception ex)
    //            {
    //                return BadRequest(ex.Message);
    //            }
    //        }

    //        // (אופציונלי) הדפסת מטריצת Scheduler
    //        [HttpGet("print-matrix")]
    //        public IActionResult PrintMatrix()
    //        {
    //            _activeWorkoutManager.PrintSchedulerMatrix();
    //            return Ok("Printed Scheduler Matrix to console/server logs.");
    //        }
    //    }

    //    // מודלים ל-Request
    //    public class StartWorkoutRequest
    //    {
    //        public TraineeDTO Trainee { get; set; }
    //        public List<ExercisePlanDTO> ExerciseOrder { get; set; }
    //        public DateTime StartTime { get; set; }
    //        public int PlanDayId { get; set; }
    //    }

    //    public class StartOrCompleteExerciseRequest
    //    {
    //        public int TraineeId { get; set; }
    //        public int ExerciseId { get; set; }
    //        public DateTime StartTime { get; set; }
    //    }
}