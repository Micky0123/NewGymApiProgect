using AutoMapper;
using DAL;
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
    public class SubMuscleBLL : ISubMuscleBLL
    {
        private readonly ISubMuscleDAL subMuscleDAL;
        private readonly IMapper mapper;
        public SubMuscleBLL(ISubMuscleDAL subMuscleDAL)
        {
            this.subMuscleDAL = subMuscleDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SubMuscle, SubMuscleDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }
        public async Task AddSubMuscleAsync(SubMuscleDTO subMuscle)
        {
            SubMuscle subMuscle1 = mapper.Map<SubMuscle>(subMuscle);
            await subMuscleDAL.AddSubMuscleAsync(subMuscle1);
        }

        public async Task DeleteSubMuscleAsync(int id)
        {
            await subMuscleDAL.DeleteSubMuscleAsync(id);
        }

        public async Task<List<SubMuscleDTO>> GetAllMuscleByMuscleIdAsync(int id)
        {
            // Get all submuscles by muscle id
            var list = await subMuscleDAL.GetAllSubMuscleByMuscleIdAsync(id);
            return mapper.Map<List<SubMuscleDTO>>(list);
        }

        public async Task<List<SubMuscleDTO>> GetAllSubMusclesAsync()
        {
            var list = await subMuscleDAL.GetAllSubMusclesAsync();
            return mapper.Map<List<SubMuscleDTO>>(list);
        }

        public async Task<SubMuscleDTO> GetSubMuscleByIdAsync(int id)
        {
            SubMuscle subMuscle = await subMuscleDAL.GetSubMuscleByIdAsync(id);
            return mapper.Map<SubMuscleDTO>(subMuscle);
        }

        public async Task<SubMuscleDTO> GetSubMuscleByNameAsync(string name)
        {
            SubMuscle subMuscle = await subMuscleDAL.GetSubMuscleByNameAsync(name);
            return mapper.Map<SubMuscleDTO>(subMuscle);
        }

        public async Task UpdateSubMuscleAsync(SubMuscleDTO subMuscle, int id)
        {
            SubMuscle subMuscle1 = mapper.Map<SubMuscle>(subMuscle);
            await subMuscleDAL.UpdateSubMuscleAsync(subMuscle1, id);
        }
    }
}
