using AutoMapper;
using DAL;
using DBEntities.Models;
using DTO;
using IBLL;
using IDAL;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL
{
    public class TrainingPlanBLL : ITrainingPlanBLL
    {
        private readonly ITrainingPlanDAL trainingPlanDAL;
        private readonly IMapper mapper;

        public TrainingPlanBLL(ITrainingPlanDAL trainingPlanDAL)
        {
            this.trainingPlanDAL = trainingPlanDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TrainingPlan, TrainingPlanDTO>().ReverseMap();
                // הגדרת מיפוי מ-PlanDay ל-PlanDayDto
                cfg.CreateMap<PlanDay, PlanDayDTO>(); // זה המיפוי שחסר לך!

                // אם יש לך ExercisePlans בתוך PlanDay, תצטרך גם את זה:
                // CreateMap<ExercisePlan, ExercisePlanDto>();
            });
            mapper = new Mapper(configTaskConverter);
        }

        //public async Task AddTrainingPlanAsync(TrainingPlanDTO trainingPlan)
        //{
        //    TrainingPlan plan = mapper.Map<TrainingPlan>(trainingPlan);
        //    await trainingPlanDAL.AddTrainingPlanAsync(plan);
        //}

        public async Task<int> AddTrainingPlanAsync(TrainingPlanDTO trainingPlan)
        {
            TrainingPlan plan = mapper.Map<TrainingPlan>(trainingPlan);
            return await trainingPlanDAL.AddTrainingPlanAsync(plan);
        }



        public async Task DeleteTrainingPlanAsync(int id)
        {
            await trainingPlanDAL.DeleteTrainingPlanAsync(id);
        }

        public async Task<List<TrainingPlanDTO>> GetAllTrainingPlansAsync()
        {
            var plans = await trainingPlanDAL.GetAllTrainingPlansAsync();
            return mapper.Map<List<TrainingPlanDTO>>(plans);
        }

        public async Task<TrainingPlanDTO> GetTrainingPlanByIdAsync(int id)
        {
            TrainingPlan plan = await trainingPlanDAL.GetTrainingPlanByIdAsync(id);
            return mapper.Map<TrainingPlanDTO>(plan);
        }

        //public async Task<TrainingPlanDTO> GetTrainingPlanByNameAsync(string name)
        //{
        //    TrainingPlan plan = await trainingPlanDAL.GetTrainingPlanByNameAsync(name);
        //    return mapper.Map<TrainingPlanDTO>(plan);
        //}

        public async Task UpdateTrainingPlanAsync(TrainingPlanDTO trainingPlan, int id)
        {
            TrainingPlan plan = mapper.Map<TrainingPlan>(trainingPlan);
            await trainingPlanDAL.UpdateTrainingPlanAsync(plan, id);
        }
        //public async Task<TrainingPlanDTO> GetAllActiveTrainingPlansOfTrainee(int traineeId)
        //{
        //    var plans = await trainingPlanDAL.GetAllActiveTrainingPlansOfTrainee(traineeId);
        //    return mapper.Map<TrainingPlanDTO>(plans);
        //}

        public async Task<TrainingPlanDTO> GetActiveTrainingPlanDTO(int traineeId)
        {
            // זה יחזיר TrainingPlan כולל PlanDays טעונים
            var plan = await trainingPlanDAL.GetActiveTrainingPlanWithDaysOfTrainee(traineeId);

            if (plan == null)
            {
                return null; // אם לא נמצאה תוכנית פעילה
            }

            // מפה רק את TrainingPlan ל-TrainingPlanDTO
            // את ה-PlanDays נשלח בנפרד מה-Controller
            return mapper.Map<TrainingPlanDTO>(plan);
        }

        //public async Task<TrainingPlanDTO> GetAllHistoryTrainingPlansOfTrainee(int traineeId)
        //{
        //    var plans = await trainingPlanDAL.GetAllHistoryTrainingPlansOfTrainee(traineeId);
        //    return mapper.Map<TrainingPlanDTO>(plans);
        //}
        //public async Task<TrainingPlanDTO?> GetAllHistoryTrainingPlansOfTrainee(int traineeId)
        //{
        //    var plan = await trainingPlanDAL.GetAllHistoryTrainingPlansOfTrainee(traineeId);

        //    if (plan == null)
        //    {
        //        return null; // אם לא נמצאה תוכנית ב-DAL, החזר null גם מה-BLL
        //    }

        //    return mapper.Map<TrainingPlanDTO>(plan);
        //}

        // פונקציה נוספת לשליפת PlanDays עבור TrainingPlanId ספציפי
        public async Task<List<PlanDayDTO>> GetPlanDaysForTrainingPlan(int trainingPlanId)
        {
            await using var ctx = new GymDbContext();
            var planDays = await ctx.PlanDays
                                     .Where(pd => pd.TrainingPlanId == trainingPlanId && !pd.IsHistoricalProgram) // assuming active plan days are not historical
                                     .ToListAsync();
            return mapper.Map<List<PlanDayDTO>>(planDays);
        }

        // עבור תוכניות היסטוריות, כנראה שעדיין נרצה רשימה של DTOs
        public async Task<List<TrainingPlanDTO>> GetAllHistoryTrainingPlansDTO(int traineeId)
        {
            // זה יחזיר List<TrainingPlan> כולל PlanDays טעונים
            var plans = await trainingPlanDAL.GetAllHistoryTrainingPlansWithDaysOfTrainee(traineeId);

            // מפה את רשימת ה-TrainingPlan ל-TrainingPlanDTO
            return mapper.Map<List<TrainingPlanDTO>>(plans);
        }
    }
}