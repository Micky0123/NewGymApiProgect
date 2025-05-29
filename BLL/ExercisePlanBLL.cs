using AutoMapper;
using DAL;
using DBEntities.Models;
using DTO;
using IBLL;
using IDAL;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL
{
    public class ExercisePlanBLL : IExercisePlanBLL
    {
        private readonly IExercisePlanDAL exercisePlanDAL;
        private readonly IMapper mapper;

        public ExercisePlanBLL(IExercisePlanDAL exercisePlanDAL)
        {
            this.exercisePlanDAL = exercisePlanDAL;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ExercisePlan, ExercisePlanDTO>().ReverseMap();
            });
            mapper = new Mapper(config);
        }

        public async Task AddExercisePlanAsync(ExercisePlanDTO exercisePlan)
        {
           ExercisePlan plan = mapper.Map<ExercisePlan>(exercisePlan);
            await exercisePlanDAL.AddExercisePlanAsync(plan);
        }

        public async Task DeleteExercisePlanAsync(int id)
        {
            await exercisePlanDAL.DeleteExercisePlanAsync(id);
        }

        public async Task<List<ExercisePlanDTO>> GetAllExercisePlansAsync()
        {
            var plans = await exercisePlanDAL.GetAllExercisePlansAsync();
            return mapper.Map<List<ExercisePlanDTO>>(plans);
        }

        public async Task<ExercisePlanDTO> GetExercisePlanByIdAsync(int id)
        {
            ExercisePlan plan = await exercisePlanDAL.GetExercisePlanByIdAsync(id);
            return mapper.Map<ExercisePlanDTO>(plan);
        }
        public async Task<List<ExercisePlanDTO>> GetExercisesByPlanDayIdAsync(int planDayId)
        {
            List< ExercisePlan> plan = await exercisePlanDAL.GetExercisesByPlanDayIdAsync(planDayId);
            return mapper.Map<List<ExercisePlanDTO>>(plan);
        }

        //public async Task<ExercisePlanDTO> GetExercisePlanByNameAsync(string name)
        //{
        //    ExercisePlan plan = await exercisePlanDAL.GetExercisePlanByNameAsync(name);
        //    return mapper.Map<ExercisePlanDTO>(plan);
        //}

        public async Task UpdateExercisePlanAsync(ExercisePlanDTO exercisePlan, int id)
        {
            DBEntities.Models.ExercisePlan plan = mapper.Map<DBEntities.Models.ExercisePlan>(exercisePlan);
            await exercisePlanDAL.UpdateExercisePlanAsync(plan, id);
        }
    }
}