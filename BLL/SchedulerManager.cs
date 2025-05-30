using DTO;
using IBLL;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class SchedulerManager
    {
        private BacktrackingScheduler scheduler;
        //public SchedulerManager(List<ExerciseDTO> exerciseList, List<GraphEdgeDTO> exerciseEdges, List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
        public SchedulerManager(ITraineeBLL traineeBLL, List<ExerciseDTO> exerciseList, List<GraphEdgeDTO> exerciseEdges, List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
            List<MuscleEdgeDTO> muscleEdges, Dictionary<int, int> equipmentCountByExercise, int slotMinutes, int slotCount, DateTime firstSlotStart)
        {
           // scheduler = new BacktrackingScheduler();
            scheduler = new BacktrackingScheduler(traineeBLL);
            // אתחול (פעם אחת בלבד)
            scheduler.Initialize(
                exerciseList: exerciseList,
                exerciseEdges: exerciseEdges,
                exerciseToMuscleEdges: exerciseToMuscleEdges,
                muscleEdges: muscleEdges,
                equipmentCountByExercise: equipmentCountByExercise,
                firstSlotStart: firstSlotStart,
                slotMinutes: slotMinutes,
                slotCount: slotCount
            );
        }
        //public void Print()
        //{
        //    scheduler.PrintTransitionMatrixToConsole();
        //}


        //private BacktrackingScheduler scheduler;

        //public SchedulerManager(ITraineeBLL traineeBLL, List<ExerciseDTO> exerciseList, List<GraphEdgeDTO> exerciseEdges, List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
        //    List<MuscleEdgeDTO> muscleEdges, Dictionary<int, int> equipmentCountByExercise, int slotMinutes, int slotCount, DateTime firstSlotStart)
        //{
        //    scheduler = new BacktrackingScheduler(traineeBLL);
        //    // אתחול (פעם אחת בלבד)
        //    scheduler.Initialize(
        //        exerciseList: exerciseList,
        //        exerciseEdges: exerciseEdges,
        //        exerciseToMuscleEdges: exerciseToMuscleEdges,
        //        muscleEdges: muscleEdges,
        //        equipmentCountByExercise: equipmentCountByExercise,
        //        firstSlotStart: firstSlotStart,
        //        slotMinutes: slotMinutes,
        //        slotCount: slotCount
        //    );
        //}
        ////public SchedulerManager(ITraineeBLL traineeBLL )
        ////{
        ////    scheduler = new BacktrackingScheduler(traineeBLL);
        ////    //// אתחול (פעם אחת בלבד)
        ////    //scheduler.Initialize(
        ////    //    exerciseList: exerciseList,
        ////    //    exerciseEdges: exerciseEdges,
        ////    //    exerciseToMuscleEdges:exerciseToMuscleEdges,
        ////    //    muscleEdges:muscleEdges,
        ////    //    equipmentCountByExercise: equipmentCountByExercise,
        ////    //    firstSlotStart: firstSlotStart,
        ////    //    slotMinutes: slotMinutes,
        ////    //    slotCount: slotCount
        ////    //);
        ////}
        //public void Initialize(
        //      List<ExerciseDTO> exerciseList,
        //      List<GraphEdgeDTO> exerciseEdges,
        //      List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
        //      List<MuscleEdgeDTO> muscleEdges,
        //      Dictionary<int, int> equipmentCountByExercise,
        //      DateTime firstSlotStart,
        //      int slotMinutes,
        //      int slotCount)
        //{
        //    scheduler.Initialize(
        //        exerciseList: exerciseList,
        //        exerciseEdges: exerciseEdges,
        //        exerciseToMuscleEdges: exerciseToMuscleEdges,
        //        muscleEdges: muscleEdges,
        //        equipmentCountByExercise: equipmentCountByExercise,
        //        firstSlotStart: firstSlotStart,
        //        slotMinutes: slotMinutes,
        //        slotCount: slotCount
        //    );
        //}
        public void Print()
        {
            scheduler.PrintTransitionMatrixToConsole();
        }
        // קריאה לאלגוריתם - לכל מתאמן בכניסה לחדר כושר
        public async Task<PathResult> RunAlgorithmForTrainee(TraineeDTO trainee, List<ExercisePlanDTO> exerciseOrder, DateTime startTime)
        {
            return await scheduler.FindOptimalPath(trainee, exerciseOrder, startTime);
        }
    }
}
