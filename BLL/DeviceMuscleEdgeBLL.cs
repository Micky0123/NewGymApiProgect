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
    public class DeviceMuscleEdgeBLL : IDeviceMuscleEdgeBLL
    {

        private readonly IDeviceMuscleEdgeDAL deviceMuscleEdgeDAL;
        private readonly IExerciseDAL exerciseDAL;
        private readonly IMuscleDAL muscleDAL;
        private readonly IMapper mapper;
        public DeviceMuscleEdgeBLL(IDeviceMuscleEdgeDAL deviceMuscle, IExerciseDAL exerciseDAL,IMuscleDAL muscleDAL)
        {
            this.deviceMuscleEdgeDAL = deviceMuscle;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<GraphEdge, GraphEdgeDTO>().ReverseMap();
            });
            mapper = new Mapper(config);
            this.exerciseDAL = exerciseDAL;
            this.muscleDAL=muscleDAL;
        }

        public async Task AddDeviceMuscleEdgeAsync(DeviceMuscleEdgeDTO deviceMuscleEdge)
        {
            DeviceMuscleEdge deviceMuscleEdge1 = mapper.Map<DeviceMuscleEdge>(deviceMuscleEdge);
            await deviceMuscleEdgeDAL.AddDeviceMuscleEdgeAsync(deviceMuscleEdge1);
        }

        public async Task DeleteDeviceMuscleEdgeAsync(int id)
        {
            await deviceMuscleEdgeDAL.DeleteDeviceMuscleEdgeAsync(id);
        }

        public async Task<List<DeviceMuscleEdgeDTO>> GetAllDeviceMuscleEdgeAsync()
        {
            var list = await deviceMuscleEdgeDAL.GetAllDeviceMuscleEdgeAsync();
            return mapper.Map<List<DeviceMuscleEdgeDTO>>(list);
        }

        public async Task<List<DeviceMuscleEdgeDTO>> GetAllDeviceMuscleEdgeByDevaiceAsync(int devaice)
        {
            var list = await deviceMuscleEdgeDAL.GetAllDeviceMuscleEdgeByDevaiceAsync(devaice);
            return mapper.Map<List<DeviceMuscleEdgeDTO>>(list);
        }

        public async Task<List<DeviceMuscleEdgeDTO>> GetAllDeviceMuscleEdgeByMuscleAsync(int muscle)
        {
            var list = await deviceMuscleEdgeDAL.GetAllDeviceMuscleEdgeByMuscleAsync(muscle);
            return mapper.Map<List<DeviceMuscleEdgeDTO>>(list);
        }

        public async Task<DeviceMuscleEdgeDTO> GetDeviceMuscleEdgeByIdAsync(int id)
        {
            DeviceMuscleEdge deviceMuscleEdge = await deviceMuscleEdgeDAL.GetDeviceMuscleEdgeByIdAsync(id);
            return mapper.Map<DeviceMuscleEdgeDTO>(deviceMuscleEdge);
        }

        public async Task UpdateDeviceMuscleEdgeAsync(DeviceMuscleEdgeDTO deviceMuscleEdge, int id)
        {
            DeviceMuscleEdge deviceMuscleEdge1 = mapper.Map<DeviceMuscleEdge>(deviceMuscleEdge);
            await deviceMuscleEdgeDAL.UpdateDeviceMuscleEdgeAsync(deviceMuscleEdge1, id);
        }

        //יצירת הגרף
        public async Task<bool> GenerateDeviceMuscleEdgeAsync()
        {
            try
            {
                // שליפת כל התרגילים והשרירים שלהם
                var exercises = await exerciseDAL.GetAllExercisesAsync();
                var muscle= await muscleDAL.GetAllMusclesAsync();


                foreach (var exercise in exercises)
                {
                    var muscleList = await exerciseDAL.GetAllMusclesByExerciseAsync(exercise.ExerciseId);
                    foreach(var muscle1 in muscleList)
                    {
                            // יצירת קשת לטבלה GraphEdge
                            var deviceMuscleEdge = new DeviceMuscleEdge
                            {
                                DeviceId = exercise.ExerciseId,
                                MuscleId = muscle1.MuscleId
                            };

                            // שימוש ב-DAL להוספת הקשת
                            await deviceMuscleEdgeDAL.AddDeviceMuscleEdgeAsync(deviceMuscleEdge);
                    }
                }
                Console.WriteLine("Graph deviceMuscle Edge created successfully.");
                // אם הכל עבר בהצלחה
                return true;
            }
            catch (Exception ex)
            {
                // טיפול בשגיאה
                Console.WriteLine($"An error occurred while generating graph deviceMuscle edges: {ex.Message}");
                return false; // במקרה של כישלון
                throw; // אפשר לזרוק שוב את החריגה אם צריך לטפל בה במקום אחר
            }


        }
    }
}
