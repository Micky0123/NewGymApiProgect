using AutoMapper;
using DAL;
using DBEntities.Models;
using DocumentFormat.OpenXml.Wordprocessing;
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
    public class TrainingDurationBLL : ITrainingDurationBLL
    {
        private readonly ITrainingDurationDAL trainingDurationDAL;
        private readonly IMapper mapper;

        public TrainingDurationBLL(ITrainingDurationDAL trainingDurationDAL)
        {
            this.trainingDurationDAL = trainingDurationDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<TrainingDuration, TrainingDurationDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddTrainingDurationAsync(TrainingDurationDTO trainingDuration)
        {
            TrainingDuration trainingDurationEntity = mapper.Map<TrainingDuration>(trainingDuration);
            await trainingDurationDAL.AddTrainingDurationAsync(trainingDurationEntity);
        }

        public async Task DeleteTrainingDurationAsync(int id)
        {
            await trainingDurationDAL.DeleteTrainingDurationAsync(id);
        }

        public async Task<List<TrainingDurationDTO>> GetAllTrainingDurationAsync()
        {
            var list = await trainingDurationDAL.GetAllTrainingDurationAsync();
            return mapper.Map<List<TrainingDurationDTO>>(list);
        }

        public async Task<TrainingDurationDTO> GetTrainingDurationByIdAsync(int id)
        {
            TrainingDuration trainingDurationEntity = await trainingDurationDAL.GetTrainingDurationByIdAsync(id);
            return mapper.Map<TrainingDurationDTO>(trainingDurationEntity);
        }

        public async Task UpdateTrainingDurationAsync(TrainingDurationDTO trainingDuration, int id)
        {
            TrainingDuration trainingDurationEntity = mapper.Map<TrainingDuration>(trainingDuration);
            await trainingDurationDAL.UpdateTrainingDurationAsync(trainingDurationEntity, id);
        }

        public async Task<TrainingDurationDTO> GetTrainingDurationByValue(int time)
        {
            TrainingDuration trainingDurationEntity = await trainingDurationDAL.GetTrainingDurationByValue(time);
            return mapper.Map<TrainingDurationDTO>(trainingDurationEntity);
        }
    }
}
