//using AutoMapper;
//using DAL;
//using DBEntities.Models;
//using DTO;
//using IBLL;
//using IDAL;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BLL
//{
//    public class TrainingProgramBLL : ITrainingProgramBLL
//    {
//        private readonly ITrainingProgramDAL trainingProgramDAL;
//        private readonly IMapper mapper;

//        public TrainingProgramBLL(ITrainingProgramDAL trainingProgramDAL)
//        {
//            this.trainingProgramDAL = trainingProgramDAL;
//            var config = new MapperConfiguration(cfg =>
//            {
//                cfg.CreateMap<TrainingProgram, TrainingProgramDTO>().ReverseMap();
//            });
//            mapper = new Mapper(config);
//        }

//        public async Task AddTrainingProgramAsync(TrainingProgramDTO trainingProgram)
//        {
//            TrainingProgram program = mapper.Map<TrainingProgram>(trainingProgram);
//            await trainingProgramDAL.AddTrainingProgramAsync(program);
//        }

//        public async Task DeleteTrainingProgramAsync(int id)
//        {
//            await trainingProgramDAL.DeleteTrainingProgramAsync(id);
//        }

//        public async Task<List<TrainingProgramDTO>> GetAllTrainingProgramsAsync()
//        {
//            var programs = await trainingProgramDAL.GetAllTrainingProgramsAsync();
//            return mapper.Map<List<TrainingProgramDTO>>(programs);
//        }

//        public async Task<TrainingProgramDTO> GetTrainingProgramByIdAsync(int id)
//        {
//            TrainingProgram program = await trainingProgramDAL.GetTrainingProgramByIdAsync(id);
//            return mapper.Map<TrainingProgramDTO>(program);
//        }

//        public async Task<TrainingProgramDTO> GetTrainingProgramByNameAsync(string name)
//        {
//            TrainingProgram program = await trainingProgramDAL.GetTrainingProgramByNameAsync(name);
//            return mapper.Map<TrainingProgramDTO>(program);

//        }

//        public async Task UpdateTrainingProgramAsync(TrainingProgramDTO trainingProgram, int id)
//        {
//            TrainingProgram program = mapper.Map<TrainingProgram>(trainingProgram);
//            await trainingProgramDAL.UpdateTrainingProgramAsync(program, id);
//        }
//    }
//}

