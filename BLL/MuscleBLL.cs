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
    public class MuscleBLL : IMuscleBLL
    {
        private readonly IMuscleDAL muscleDAL;
        private readonly IMapper mapper;
        public MuscleBLL(IMuscleDAL muscleDAL)
        {
            this.muscleDAL = muscleDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Muscle, MuscleDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddMuscleAsync(MuscleDTO muscle)
        {
            Muscle muscle1 = mapper.Map<Muscle>(muscle);
            await muscleDAL.AddMuscleAsync(muscle1);
        }

        public async Task DeleteMuscleAsync(int muscleId)
        {
            await muscleDAL.DeleteMuscleAsync(muscleId);
        }

        public async Task<List<MuscleDTO>> GetAllMusclesAsync()
        {
            var list = await muscleDAL.GetAllMusclesAsync();
            return mapper.Map<List<MuscleDTO>>(list);
        }

        public async Task<MuscleDTO> GetMuscleByIdAsync(int id)
        {
            Muscle muscle = await muscleDAL.GetMuscleByIdAsync(id);
            return mapper.Map<MuscleDTO>(muscle);
        }

        public async Task<MuscleDTO> GetMuscleByNameAsync(string name)
        {
            Muscle muscle = await muscleDAL.GetMuscleByNameAsync(name);
            return mapper.Map<MuscleDTO>(muscle);
        }

        public async Task UpdateMuscleAsync(MuscleDTO muscle, int muscleId)
        {
            Muscle muscle1 = mapper.Map<Muscle>(muscle);
            await muscleDAL.UpdateMuscleAsync(muscle1, muscleId);
        }
    }
}
