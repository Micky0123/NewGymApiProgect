using AutoMapper;
using DBEntities.Models;
using DTO;
using IBLL;
using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class GoalBLL : IGoalBLL
    {
        private readonly IGoalDAL goalDAL;
        private readonly IMapper mapper;

        public GoalBLL(IGoalDAL goalDAL)
        {
            this.goalDAL = goalDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Goal, GoalDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddGoalAsync(GoalDTO Goal)
        {
            Goal goal1 = mapper.Map<Goal>(Goal);
            await goalDAL.AddGoalAsync(goal1);
        }

        public async Task DeleteGoalAsync(int id)
        {
            await goalDAL.DeleteGoalAsync(id);
        }

        public async Task<List<GoalDTO>> GetAllGoalsAsync()
        {
            var list = await goalDAL.GetAllGoalsAsync();
            return mapper.Map<List<GoalDTO>>(list);
        }

        public async Task<GoalDTO> GetGoalByIdAsync(int id)
        {
            Goal goal = await goalDAL.GetGoalByIdAsync(id);
            return mapper.Map<GoalDTO>(goal);
        }

        public async Task<GoalDTO> GetGoalByNameAsync(string name)
        {
            Goal goal = await goalDAL.GetGoalByNameAsync(name);
            return mapper.Map<GoalDTO>(goal);
        }

        public async Task UpdateGoalAsync(GoalDTO Goal, int id)
        {
            Goal goal1 = mapper.Map<Goal>(Goal);
            await goalDAL.UpdateGoalAsync(goal1, id);
        }
    }
}
