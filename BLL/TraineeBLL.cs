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
    public class TraineeBLL : ITraineeBLL
    {
        private readonly ITraineeDAL traineeDAL;
        private readonly IMapper mapper;
        public TraineeBLL(ITraineeDAL traineeDAL)
        {
            this.traineeDAL = traineeDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Trainee, TraineeDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }
        public async Task AddTraineeAsync(TraineeDTO Trainee)
        {
            Trainee trainee1 = mapper.Map<Trainee>(Trainee);
            await traineeDAL.AddTraineeAsync(trainee1);
        }

        public async Task DeleteTraineeAsync(int id)
        {
            await traineeDAL.DeleteTraineeAsync(id);
        }

        public async Task<List<TraineeDTO>> GetAllTraineesAsync()
        {
            var list = await traineeDAL.GetAllTraineesAsync();
            return mapper.Map<List<TraineeDTO>>(list);
        }

        public async Task<TraineeDTO> GetTraineeByIdAsync(int id)
        {
            Trainee trainee = await traineeDAL.GetTraineeByIdAsync(id);
            return mapper.Map<TraineeDTO>(trainee);
        }

        public async Task<TraineeDTO> GetTraineeByNameAsync(string name)
        {
            Trainee trainee = await traineeDAL.GetTraineeByNameAsync(name);
            return mapper.Map<TraineeDTO>(trainee);
        }

        public async Task UpdateTraineeAsync(TraineeDTO Trainee, int id)
        {
            Trainee trainee = mapper.Map<Trainee>(Trainee);
            await traineeDAL.UpdateTraineeAsync(trainee, id);
        }
    }
}

