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
    public class TrainingDayBLL : ITrainingDayBLL
    {
        private readonly ITrainingDayDAL trainingDayDAL;
        private readonly IMapper mapper;
        public TrainingDayBLL(ITrainingDayDAL trainingDayDAL)
        {
            this.trainingDayDAL = trainingDayDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TrainingDay, TrainingDayDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddTrainingDayAsync(TrainingDayDTO trainingDay)
        {
            TrainingDay trainingDayEntity = mapper.Map<TrainingDay>(trainingDay);
            await trainingDayDAL.AddTrainingDayAsync(trainingDayEntity);
        }

        public async Task DeleteTrainingDayAsync(int id)
        {
            await trainingDayDAL.DeleteTrainingDayAsync(id);
        }

        public async Task<List<TrainingDayDTO>> GetAllTrainingDaysAsync()
        {
            var list = await trainingDayDAL.GetAllTrainingDaysAsync();
            return mapper.Map<List<TrainingDayDTO>>(list);
        }

        public async Task<TrainingDayDTO> GetTrainingDayByIdAsync(int id)
        {
            TrainingDay trainingDay = await trainingDayDAL.GetTrainingDayByIdAsync(id);
            return mapper.Map<TrainingDayDTO>(trainingDay);
        }

        //public async Task<TrainingDayDTO> GetTrainingDayByNameAsync(string name)
        //{
        //    TrainingDay trainingDay = await trainingDayDAL.GetTrainingDayByNameAsync(name);
        //    return mapper.Map<TrainingDayDTO>(trainingDay);
        //}

        public async Task UpdateTrainingDayAsync(TrainingDayDTO trainingDay, int id)
        {
            TrainingDay trainingDayEntity = mapper.Map<TrainingDay>(trainingDay);
            await trainingDayDAL.UpdateTrainingDayAsync(trainingDayEntity, id);
        }
    }
}