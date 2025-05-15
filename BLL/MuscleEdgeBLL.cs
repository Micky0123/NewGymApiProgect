using AutoMapper;
using DAL;
using DBEntities.Models;
using DTO;
using IBLL;
using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL
{
    public class MuscleEdgeBLL : IMuscleEdgeBLL
    {
        private readonly IMuscleEdgeDAL muscleEdgeDAL;
        private readonly IMuscleDAL muscleDAL;
        private readonly IMapper mapper;

        public MuscleEdgeBLL(IMuscleEdgeDAL muscleEdgeDAL, IMuscleDAL muscleDAL)
        {
            this.muscleEdgeDAL = muscleEdgeDAL;
            this.muscleDAL = muscleDAL;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MuscleEdge, MuscleEdgeDTO>().ReverseMap();
            });
            mapper = new Mapper(config);
        }

        public async Task AddMuscleEdgeAsync(MuscleEdgeDTO muscleEdgeDTO)
        {
            MuscleEdge muscleEdge = mapper.Map<MuscleEdge>(muscleEdgeDTO);
            await muscleEdgeDAL.AddMuscleEdgeAsync(muscleEdge);
        }

        public async Task DeleteMuscleEdgeAsync(int id)
        {
            await muscleEdgeDAL.DeleteMuscleEdgeAsync(id);
        }

        public async Task<List<MuscleEdgeDTO>> GetAllMuscleEdgeAsync()
        {
            var list = await muscleEdgeDAL.GetAllMuscleEdgeAsync();
            return mapper.Map<List<MuscleEdgeDTO>>(list);
        }

        public async Task<List<MuscleEdgeDTO>> GetAllMuscleEdgeBymuscle1Async(int muscle1)
        {
            var list = await muscleEdgeDAL.GetAllMuscleEdgeBymuscle1Async(muscle1);
            return mapper.Map<List<MuscleEdgeDTO>>(list);
        }

        public async Task<List<MuscleEdgeDTO>> GetAllMuscleEdgeByMuscle2Async(int muscle2)
        {
            var list = await muscleEdgeDAL.GetAllMuscleEdgeByMuscle2Async(muscle2);
            return mapper.Map<List<MuscleEdgeDTO>>(list);
        }

        public async Task<MuscleEdgeDTO> GetMuscleEdgeByIdAsync(int id)
        {
            MuscleEdge muscleEdge = await muscleEdgeDAL.GetMuscleEdgeByIdAsync(id);
            return mapper.Map<MuscleEdgeDTO>(muscleEdge);
        }

        public async Task UpdateMuscleEdgeAsync(MuscleEdgeDTO muscleEdgeDTO, int id)
        {
            MuscleEdge muscleEdge = mapper.Map<MuscleEdge>(muscleEdgeDTO);
            await muscleEdgeDAL.UpdateMuscleEdgeAsync(muscleEdge, id);
        }

        public async Task<bool> GenerateMuscleEdgeAsync(List<string> largeMuscleList, List<string> smallMuscleList)
        {
            try
            {
                // Fetch all muscles
                var muscles = await muscleDAL.GetAllMusclesAsync();

                // Create edges between every two muscles (or based on specific logic)
                foreach (var muscle1 in muscles)
                {

                    foreach (var muscle2 in muscles) // Avoid duplicate pairs
                    {
                        // Check if muscle1 is in the large muscle list
                        if ((largeMuscleList.Any(m => m == muscle1.MuscleName)) || (smallMuscleList.Any(m => m == muscle1.MuscleName) &&
                                 smallMuscleList.Any(m => m == muscle2.MuscleName)))
                        {
                            // Add an edge from muscle1 (large) to muscle2
                            var muscleEdge = new MuscleEdge
                            {
                                MuscleId1 = muscle1.MuscleId,
                                MuscleId2 = muscle2.MuscleId
                            };

                            await muscleEdgeDAL.AddMuscleEdgeAsync(muscleEdge);
                        }
                    }
                }
                Console.WriteLine("Muscle edges generated successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while generating muscle edges: {ex.Message}");
                return false;
            }
        }
    }
}