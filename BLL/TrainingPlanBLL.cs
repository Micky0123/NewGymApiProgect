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
    public class TrainingPlanBLL : ITrainingPlanBLL
    {
        private readonly ITrainingPlanDAL trainingPlanDAL;
        private readonly IMapper mapper;

        public TrainingPlanBLL(ITrainingPlanDAL trainingPlanDAL)
        {
            this.trainingPlanDAL = trainingPlanDAL;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TrainingPlan, TrainingPlanDTO>().ReverseMap();
            });
            mapper = new Mapper(config);
        }

        public async Task AddTrainingPlanAsync(TrainingPlanDTO trainingPlan)
        {
            TrainingPlan plan = mapper.Map<TrainingPlan>(trainingPlan);
            await trainingPlanDAL.AddTrainingPlanAsync(plan);
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
    }
}