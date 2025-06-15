using AutoMapper;
using BLL;
using DAL;
using DBEntities.Models;
using DocumentFormat.OpenXml.Spreadsheet;
using DTO;
using IBLL;
using IDAL;
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
        private readonly IPlanDayBLL planDayBLL;
        private readonly IMapper mapper;


        public TrainingPlanController(ITrainingPlanBLL trainingPlanBLL, IPlanDayBLL planDayBLL, IPlanDayDAL planDayDAL)
        {
            this.trainingPlanBLL = trainingPlanBLL;
            this.planDayBLL = planDayBLL;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TrainingPlan, TrainingPlanDTO>().ReverseMap();
                cfg.CreateMap<PlanDay, PlanDayDTO>().ReverseMap();
            });
            mapper = new Mapper(config);
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

        //[HttpGet("active/{traineeId}")]
        //public async Task<ActionResult<List<PlanDayDTO>>> GetActivePlans(int traineeId)
        //{
        //    var plans = await trainingPlanBLL.GetAllActiveTrainingPlansOfTrainee(traineeId);

        //    if (plans == null)
        //    {
        //        return NotFound("No historical training plans found for this trainee.");
        //    }

        //    var allPlanDays = new List<PlanDayDTO>();

        //    var planDaysForCurrentPlan = await planDayBLL.GetPlanDaysByTrainingPlanIdAndNotHistorical(plans.TrainingPlanId);

        //    // Add all found plan days to our aggregated list
        //    if (planDaysForCurrentPlan != null && planDaysForCurrentPlan.Any())
        //    {
        //        allPlanDays.AddRange(planDaysForCurrentPlan);
        //    }
        //    //}

        //    // After iterating through all historical training plans, check if any plan days were collected
        //    if (allPlanDays == null || !allPlanDays.Any()) // Use !Any() for checking if list is empty
        //    {
        //        return NotFound("No historical plan days found for any historical training plan of this trainee.");
        //    }
        //    return Ok(allPlanDays);
        //}
        ////[HttpGet("history/{traineeId}")]
        ////public async Task<ActionResult<List<TrainingPlanDTO>>> GetHistoryPlans(int traineeId)
        ////{
        ////    var plans = await trainingPlanBLL.GetAllHistoryTrainingPlansOfTrainee(traineeId);

        ////    //if (plans == null || plans.Count == 0)
        ////    //    return NotFound("No history training plans found for this trainee.");
        ////    return Ok(plans);
        ////}

        //// ב-Controller שלך (לדוגמה, TrainingPlanController.cs)

        //[HttpGet("history/{traineeId}")]
        //public async Task<ActionResult<List<PlanDayDTO>>> GetHistoryPlans(int traineeId)
        //{
        //    // קבל את התוכנית ההיסטורית הבודדת (או null)
        //    //var historyTrainingPlan = await trainingPlanBLL.GetAllHistoryTrainingPlansOfTrainee(traineeId);

        //    //if (historyTrainingPlan == null)
        //    //{
        //    //    // אם לא נמצאה תוכנית אימונים היסטורית עבור המתאמן
        //    //    return NotFound("No historical training plan found for this trainee.");
        //    //}

        //    //// קבל את רשימת PlanDayDTO עבור TrainingPlanId של התוכנית ההיסטורית שנמצאה
        //    //// (בהנחה ש-GetPlanDaysByTrainingPlanIdAndHistorical עדיין מחזירה רשימה)
        //    //var planDaysForCurrentPlan = await planDayBLL.GetPlanDaysByTrainingPlanIdAndHistorical(historyTrainingPlan.TrainingPlanId);

        //    //// אם אין PlanDays המשויכים לתוכנית ההיסטורית הזו
        //    //if (planDaysForCurrentPlan == null || !planDaysForCurrentPlan.Any())
        //    //{
        //    //    return NotFound($"No plan days found for the historical training plan (ID: {historyTrainingPlan.TrainingPlanId}) of this trainee.");
        //    //}

        //    //// החזר את רשימת ה-PlanDayDTO
        //    //return Ok(planDaysForCurrentPlan);
        //    var plans = await trainingPlanBLL.GetAllActiveTrainingPlansOfTrainee(traineeId);

        //    if (plans == null)
        //    {
        //        return NotFound("No historical training plans found for this trainee.");
        //    }

        //    var allPlanDays = new List<PlanDayDTO>();

        //    var planDaysForCurrentPlan = await planDayBLL.GetPlanDaysByTrainingPlanIdAndHistorical(plans.TrainingPlanId);

        //    // Add all found plan days to our aggregated list
        //    if (planDaysForCurrentPlan != null && planDaysForCurrentPlan.Any())
        //    {
        //        allPlanDays.AddRange(planDaysForCurrentPlan);
        //    }
        //    //}

        //    // After iterating through all historical training plans, check if any plan days were collected
        //    if (allPlanDays == null || !allPlanDays.Any()) // Use !Any() for checking if list is empty
        //    {
        //        //return NotFound("No historical plan days found for any historical training plan of this trainee.");
        //        return Ok(new List<PlanDayDTO>());
        //    }
        //    return Ok(allPlanDays);
        //}


        // עבור תוכנית פעילה
        [HttpGet("active/{traineeId}")]
        public async Task<ActionResult<TrainingPlanDTO>> GetActivePlans(int traineeId)
        {
            // קבל את ה-TrainingPlan כולל PlanDays
            //var activePlanEntity = await trainingPlanBLL.GetActiveTrainingPlanDTO(traineeId);

            //if (activePlanEntity == null)
            //{
            //    // אם אין תוכנית פעילה, החזר תגובה עם אובייקטים ריקים/null
            //    return Ok(new SinglePlanResponse { TrainingPlan = null, PlanDays = new List<PlanDayDTO>() });
            //}

            //// מפה את ה-TrainingPlan ל-DTO
            //var trainingPlanDto = mapper.Map<TrainingPlanDTO>(activePlanEntity);
            //// מפה את ה-PlanDays ל-DTOs
            //var planDaysDtos = mapper.Map<List<PlanDayDTO>>(activePlanEntity.TrainingPlanId);

            //// החזר אובייקט שמכיל את שניהם
            //return Ok(new SinglePlanResponse
            //{
            //    TrainingPlan = trainingPlanDto,
            //    PlanDays = planDaysDtos
            //});
            // 1. שלוף את ה-TrainingPlan
            var trainingPlanEntity = await trainingPlanBLL.GetActiveTrainingPlanDTO(traineeId);

            if (trainingPlanEntity == null)
            {
                return NotFound($"No active training plan found for trainee ID: {traineeId}");
            }

            // 2. שלוף את ה-PlanDays עבור ה-TrainingPlan הספציפי
            var planDayEntities = await trainingPlanBLL.GetPlanDaysForTrainingPlan(trainingPlanEntity.TrainingPlanId);

            // 3. בנה את ה-DTO ואחד את הנתונים
            var trainingPlanDto = new TrainingPlanDTO
            {
                TrainingPlanId = trainingPlanEntity.TrainingPlanId,
                TraineeId = trainingPlanEntity.TraineeId,
                GoalId = trainingPlanEntity.GoalId,
                TrainingDays = trainingPlanEntity.TrainingDays,
                TrainingDurationId = trainingPlanEntity.TrainingDurationId,
                FitnessLevelId = trainingPlanEntity.FitnessLevelId,
                StartDate = trainingPlanEntity.StartDate,
                EndDate = trainingPlanEntity.EndDate,
                IsActive = trainingPlanEntity.IsActive,
                // המרת רשימת ה-PlanDay Entities לרשימת PlanDay DTOs
                PlanDays = planDayEntities.Select(pd => new PlanDayDTO
                {
                    PlanDayId = pd.PlanDayId,
                    TrainingPlanId = pd.TrainingPlanId,
                    ProgramName = pd.ProgramName,
                    DayOrder = pd.DayOrder,
                    CreationDate = pd.CreationDate,
                    IsDefaultProgram = pd.IsDefaultProgram,
                    ParentProgramId = pd.ParentProgramId,
                    IsHistoricalProgram = pd.IsHistoricalProgram
                    // אם יש PlanDay.ExercisePlans, תצטרך לכלול גם אותם כאן לתוך ExercisePlanDto
                }).ToList()
            };

            return Ok(trainingPlanDto);
        }

        // עבור תוכניות היסטוריות
        [HttpGet("history/{traineeId}")]
        public async Task<ActionResult<List<MultiplePlansResponse>>> GetHistoryPlans(int traineeId)
        {
            // קבל את רשימת ה-TrainingPlan כולל PlanDays עבור כל אחת
            var historyPlanEntities = await trainingPlanBLL.GetAllHistoryTrainingPlansDTO(traineeId);

            var responseList = new List<MultiplePlansResponse>();

            if (historyPlanEntities != null && historyPlanEntities.Any())
            {
                foreach (var planEntity in historyPlanEntities)
                {
                    var trainingPlanDto = mapper.Map<TrainingPlanDTO>(planEntity);
                    var planDaysDtos = mapper.Map<List<PlanDayDTO>>(planEntity.TrainingPlanId);

                    responseList.Add(new MultiplePlansResponse
                    {
                        TrainingPlan = trainingPlanDto,
                        PlanDays = planDaysDtos
                    });
                }
            }

            // החזר רשימה של אובייקטים המכילים גם את התוכנית וגם את ימיה
            return Ok(responseList);
        }

        public class SinglePlanResponse
        {
            public TrainingPlanDTO? TrainingPlan { get; set; }
            public List<PlanDayDTO>? PlanDays { get; set; }
        }

        public class MultiplePlansResponse
        {
            public TrainingPlanDTO TrainingPlan { get; set; }
            public List<PlanDayDTO> PlanDays { get; set; } = new List<PlanDayDTO>();
        }
    }
}