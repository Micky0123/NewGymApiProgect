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
                cfg.CreateMap<PlanDay, PlanDayDTO>(); 
            });
            mapper = new Mapper(configTaskConverter);
        }

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

        public async Task UpdateTrainingPlanAsync(TrainingPlanDTO trainingPlan, int id)
        {
            TrainingPlan plan = mapper.Map<TrainingPlan>(trainingPlan);
            await trainingPlanDAL.UpdateTrainingPlanAsync(plan, id);
        }

        public async Task<TrainingPlanDTO> GetActiveTrainingPlanDTO(int traineeId)
        {
            var plan = await trainingPlanDAL.GetActiveTrainingPlanWithDaysOfTrainee(traineeId);

            if (plan == null)
            {
                return null; // אם לא נמצאה תוכנית פעילה
            }

            return mapper.Map<TrainingPlanDTO>(plan);
        }

        // פונקציה נוספת לשליפת PlanDays עבור TrainingPlanId ספציפי
        public async Task<List<PlanDayDTO>> GetPlanDaysForTrainingPlan(int trainingPlanId)
        {
            await using var ctx = new GymDbContext();
            var planDays = await ctx.PlanDays
                                     .Where(pd => pd.TrainingPlanId == trainingPlanId && !pd.IsHistoricalProgram)
                                     .ToListAsync();
            return mapper.Map<List<PlanDayDTO>>(planDays);
        }

        public async Task<List<TrainingPlanDTO>> GetAllHistoryTrainingPlansDTO(int traineeId)
        {
            var plans = await trainingPlanDAL.GetAllHistoryTrainingPlansWithDaysOfTrainee(traineeId);
            return mapper.Map<List<TrainingPlanDTO>>(plans);
        }
    }
}