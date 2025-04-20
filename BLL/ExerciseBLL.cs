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
    public class ExerciseBLL:IExerciseBLL
    {
        private readonly IExerciseDAL exerciseDAL;
        private readonly IMapper mapper;
        public  ExerciseBLL(IExerciseDAL exerciseDAL)
        {
            this.exerciseDAL = exerciseDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Exercise, ExerciseDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }


        public async Task AddExerciseAsync(ExerciseDTO exercise)
        {
            Exercise exercise1 = mapper.Map<Exercise>(exercise);
            await exerciseDAL.AddExerciseAsync(exercise1);
        }

        public async Task DeleteExerciseAsync(int id)
        {
            await exerciseDAL.DeleteExerciseAsync(id);
        }

        public async Task<List<ExerciseDTO>> GetAllExercisesAsync()
        {
            var list = await exerciseDAL.GetAllExercisesAsync();
            return mapper.Map<List<ExerciseDTO>>(list);
        }

        public async Task<ExerciseDTO> GetExerciseByIdAsync(int id)
        {
            Exercise exercise = await exerciseDAL.GetExerciseByIdAsync(id);
            return mapper.Map<ExerciseDTO>(exercise);
        }

        public async Task<ExerciseDTO> GetExerciseByNameAsync(string name)
        {
            Exercise exercise = await exerciseDAL.GetExerciseByNameAsync(name);
            return mapper.Map<ExerciseDTO>(exercise);
        }

        public async Task UpdateExerciseAsync(ExerciseDTO exercise, int id)
        {
            Exercise exercise1 = mapper.Map<Exercise>(exercise);
            await exerciseDAL.UpdateExerciseAsync(exercise1, id);
        }
    }
}
