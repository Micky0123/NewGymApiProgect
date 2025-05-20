using AutoMapper;
using DAL;
using DBEntities.Models;
using DTO;
using IBLL;
using IDAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class GraphEdgeBLL : IGraphEdgeBLL
    {
        private readonly IGraphEdgeDAL graphEdgeDAL;
        private readonly IExerciseDAL exerciseDAL;
        private readonly IMapper mapper;


        public GraphEdgeBLL(IGraphEdgeDAL graphEdgeDAL, IExerciseDAL exerciseDAL)
        {
            this.graphEdgeDAL = graphEdgeDAL;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<GraphEdge, GraphEdgeDTO>().ReverseMap();
            });
            mapper = new Mapper(config);
            this.exerciseDAL = exerciseDAL;
        }
        public async Task AddGraphEdgeAsync(GraphEdgeDTO graphEdge)
        {
            GraphEdge graphEdge1 = mapper.Map<GraphEdge>(graphEdge);
            await graphEdgeDAL.AddGraphEdgeAsync(graphEdge1);
        }

        public async Task DeleteGraphEdgeAsync(int id)
        {
            await graphEdgeDAL.DeleteGraphEdgeAsync(id);
        }

        public async Task<List<GraphEdgeDTO>> GetAllGraphEdgeAsync()
        {
            var list = await graphEdgeDAL.GetAllGraphEdgeAsync();
            return mapper.Map<List<GraphEdgeDTO>>(list);
        }

        public async Task<List<GraphEdgeDTO>> GetAllGraphEdgeByDevaice1Async(int devaiceID1)
        {
            var list = await graphEdgeDAL.GetAllGraphEdgeByDevaice1Async(devaiceID1);
            return mapper.Map<List<GraphEdgeDTO>>(list);
        }

        public async Task<List<GraphEdgeDTO>> GetAllGraphEdgeByDevaice2Async(int devaiceID2)
        {
            var list = await graphEdgeDAL.GetAllGraphEdgeByDevaice2Async(devaiceID2);
            return mapper.Map<List<GraphEdgeDTO>>(list);
        }

        public async Task<GraphEdgeDTO> GetGraphEdgeByIdAsync(int id)
        {
            GraphEdge graphEdge = await graphEdgeDAL.GetGraphEdgeByIdAsync(id);
            return mapper.Map<GraphEdgeDTO>(graphEdge);
        }

        public async Task UpdateGraphEdgeAsync(GraphEdgeDTO graphEdge, int id)
        {
            GraphEdge graphEdge1 = mapper.Map<GraphEdge>(graphEdge);
            await graphEdgeDAL.UpdateGraphEdgeAsync(graphEdge1, id);
        }

        //יצירת הגרף
        public async Task<bool> GenerateGraphEdgesAsync()
        {
            try
            {
                // שליפת כל התרגילים והשרירים שלהם
                var exercises = await exerciseDAL.GetAllExercisesAsync();
                //var exercises = await _context.Exercises
                //.Include(e => e.Muscles)
                //.ToListAsync();

                // עבור כל תרגיל
                foreach (var exercise1 in exercises)
                {
                    // בדיקה כנגד כל תרגיל אחר
                    foreach (var exercise2 in exercises)
                    {
                        // דילוג אם מדובר באותו תרגיל
                        // דילוג אם מדובר באותו תרגיל
                        if (exercise1.ExerciseId == exercise2.ExerciseId)
                            continue;

                        var muscles1 = await exerciseDAL.GetAllMusclesByExerciseAsync(exercise1.ExerciseId);
                        var muscle2 = await exerciseDAL.GetAllMusclesByExerciseAsync(exercise2.ExerciseId);
                        //bool hasCommonMuscle = muscles1.Any(m1 => muscle2.Any(m2 => m1.MuscleId == m2.MuscleId));
                        // בדיקה אם יש שריר משותף
                        //bool hasCommonMuscle = exercise1.Muscles.Any(m1 => exercise2.Muscles.Any(m2 => m1.MuscleId == m2.MuscleId));
                        bool hasCommonMuscle= muscles1==muscle2;

                        if (hasCommonMuscle)
                        {
                            //בדיקה על מספר השרירים
                            var jointCOunt1 = await exerciseDAL.GetJointCount(exercise1.ExerciseId);
                            var jointCOunt2 = await exerciseDAL.GetJointCount(exercise2.ExerciseId);
                            if (jointCOunt1 >= jointCOunt2)
                            {
                                // יצירת קשת לטבלה GraphEdge
                                var graphEdge = new GraphEdge
                                {
                                    Device1Id = exercise1.ExerciseId,
                                    Device2Id = exercise2.ExerciseId
                                };

                                // שימוש ב-DAL להוספת הקשת
                                await graphEdgeDAL.AddGraphEdgeAsync(graphEdge);
                            }
                        }

                    }
                }
                Console.WriteLine("Graph edges created successfully.");
                // אם הכל עבר בהצלחה
                return true;
            }
            catch (Exception ex)
            {
                // טיפול בשגיאה
                Console.WriteLine($"An error occurred while generating graph edges: {ex.Message}");
                return false; // במקרה של כישלון
                throw; // אפשר לזרוק שוב את החריגה אם צריך לטפל בה במקום אחר
            }


        }

    }
}