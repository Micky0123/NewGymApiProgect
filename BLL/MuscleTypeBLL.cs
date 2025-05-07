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
    public class MuscleTypeBLL : IMuscleTypeBLL
    {
        private readonly IMuscleTypeDAL muscleTypeDAL;
        private readonly IMapper mapper;

        public MuscleTypeBLL(IMuscleTypeDAL muscleTypeDAL)
        {
            this.muscleTypeDAL = muscleTypeDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MuscleType, MuscleTypeDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddMuscleTypeAsync(MuscleTypeDTO muscleType)
        {
            MuscleType muscleType1 = mapper.Map<MuscleType>(muscleType);
            await muscleTypeDAL.AddMuscleTypeAsync(muscleType1);
        }

        public async Task DeleteMuscleTypeAsync(int id)
        {
            await muscleTypeDAL.DeleteMuscleTypeAsync(id);
        }

        public async Task<List<MuscleTypeDTO>> GetAllMusclesTypeAsync()
        {
            var list = await muscleTypeDAL.GetAllMuscleTypesAsync();
            return mapper.Map<List<MuscleTypeDTO>>(list);
        }

        public async Task<MuscleTypeDTO> GetMuscleTypeByIdAsync(int id)
        {
            MuscleType muscleType1 = await muscleTypeDAL.GetMuscleTypeByIdAsync(id);
            return mapper.Map<MuscleTypeDTO>(muscleType1);
        }

        public async Task<MuscleTypeDTO> GetMuscleTypeByNameAsync(string name)
        {
            // Note: This method is not implemented in the DAL, so we'll throw an exception
            throw new NotImplementedException();
        }

        public async Task UpdateMuscleTypeAsync(MuscleTypeDTO muscleType, int id)
        {
            MuscleType muscleType1 = mapper.Map<MuscleType>(muscleType);
            await muscleTypeDAL.UpdateMuscleTypeAsync(muscleType1);
        }
    }
}