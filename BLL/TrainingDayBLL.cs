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
    public class TrainingDayBLL:ITrainingDayBLL
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

        public Task AddTrainingDayAsync(TrainingDayDTO trainingDay)
        {
            throw new NotImplementedException();
        }


        public Task DeleteTrainingDayAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<TrainingDayDTO>> GetAllTrainingDaysAsync()
        {
            throw new NotImplementedException();
        }

        public Task<TrainingDayDTO> GetTrainingDayByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<TrainingDayDTO> GetTrainingDayByNameAsync(string name)
        {
            throw new NotImplementedException();
        }


        public Task UpdateTrainingDayAsync(TrainingDayDTO trainingDay, int id)
        {
            throw new NotImplementedException();
        }
    }
}
