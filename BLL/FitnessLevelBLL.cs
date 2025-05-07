using DTO;
using IBLL;
using IDAL;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using DBEntities.Models;

namespace BLL
{
    public class FitnessLevelBLL : IFitnessLevelBLL
    {
        private readonly IFitnessLevelDAL fitnessLevelDAL;
        private readonly IMapper mapper;

        public FitnessLevelBLL(IFitnessLevelDAL fitnessLevelDAL)
        {
            this.fitnessLevelDAL = fitnessLevelDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<FitnessLevel, FitnessLevelDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddFitnessLevelAsync(FitnessLevelDTO fitnessLevel)
        {
            FitnessLevel fitnessLevelEntity = mapper.Map<FitnessLevel>(fitnessLevel);
            await fitnessLevelDAL.AddFitnessLevelAsync(fitnessLevelEntity);
        }

        public async Task DeleteFitnessLevelAsync(int id)
        {
            await fitnessLevelDAL.DeleteFitnessLevelAsync(id);
        }

        public async Task<List<FitnessLevelDTO>> GetAllFitnessLevelAsync()
        {
            var list = await fitnessLevelDAL.GetAllFitnessLevelAsync();
            return mapper.Map<List<FitnessLevelDTO>>(list);
        }

        public async Task<FitnessLevelDTO> GetFitnessLevelByIdAsync(int id)
        {
            FitnessLevel fitnessLevelEntity = await fitnessLevelDAL.GetFitnessLevelByIdAsync(id);
            return mapper.Map<FitnessLevelDTO>(fitnessLevelEntity);
        }

        public async Task UpdateFitnessLevelAsync(FitnessLevelDTO fitnessLevel, int id)
        {
            FitnessLevel fitnessLevelEntity = mapper.Map<FitnessLevel>(fitnessLevel);
            await fitnessLevelDAL.UpdateFitnessLevelAsync(fitnessLevelEntity, id);
        }
    }
}