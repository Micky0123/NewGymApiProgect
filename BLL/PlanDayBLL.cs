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
    public class PlanDayBLL : IPlanDayBLL
    {
        private readonly IPlanDayDAL planDayDAL;
        private readonly IMapper mapper;

        public PlanDayBLL(IPlanDayDAL planDayDAL)
        {
            this.planDayDAL = planDayDAL;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PlanDay, PlanDayDTO>().ReverseMap();
            });
            mapper = new Mapper(config);
        }

        public async Task AddPlanDayAsync(PlanDayDTO planDay)
        {
            PlanDay day = mapper.Map<PlanDay>(planDay);
            await planDayDAL.AddPlanDayAsync(day);
        }

        public async Task DeletePlanDayAsync(int id)
        {
            await planDayDAL.DeletePlanDayAsync(id);
        }

        public async Task<List<PlanDayDTO>> GetAllPlanDaysAsync()
        {
            var days = await planDayDAL.GetAllPlanDaysAsync();
            return mapper.Map<List<PlanDayDTO>>(days);
        }

        public async Task<PlanDayDTO> GetPlanDayByIdAsync(int id)
        {
            PlanDay day = await planDayDAL.GetPlanDayByIdAsync(id);
            return mapper.Map<PlanDayDTO>(day);
        }

        public async Task<List<PlanDayDTO>> GetPlanDaysByTrainingPlanIdAndNotHistorical(int trainingPlanId)
        {
            var planDays = await planDayDAL.GetPlanDaysByTrainingPlanIdAndNotHistorical(trainingPlanId);
            return mapper.Map<List<PlanDayDTO>>(planDays);
        }
        public async Task<List<PlanDayDTO>> GetPlanDaysByTrainingPlanIdAndHistorical(int trainingPlanId)
        {
            var planDays = await planDayDAL.GetPlanDaysByTrainingPlanIdAndHistorical(trainingPlanId);
            return mapper.Map<List<PlanDayDTO>>(planDays);
        }

        //public async Task<PlanDayDTO> GetPlanDayByNameAsync(string name)
        //{
        //    PlanDay day = await planDayDAL.GetPlanDayByNameAsync(name);
        //    return mapper.Map<PlanDayDTO>(day);
        //}

        public async Task UpdatePlanDayAsync(PlanDayDTO planDay, int id)
        {
            PlanDay day = mapper.Map<PlanDay>(planDay);
            await planDayDAL.UpdatePlanDayAsync(day, id);
        }
    }
}