using DTO;
using Microsoft.Graph.Models;
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
        public SchedulerManager()
        {
            scheduler = new BacktrackingScheduler();
            // אתחול (פעם אחת בלבד)
            scheduler.Initialize(
                exerciseList: new List<ExerciseDTO>(),
                exerciseEdges: new List<GraphEdgeDTO>(),
                exerciseToMuscleEdges: new List<DeviceMuscleEdgeDTO>(),
                muscleEdges: new List<MuscleEdgeDTO>(),
                equipmentCount: 5,
                firstSlotStart: DateTime.Now,
                slotMinutes: 15,
                slotCount: 10
            );
        }

        // קריאה לאלגוריתם - אפשר לקרוא כמה פעמים שרוצים
        public PathResult RunAlgorithmForTrainee(TraineeDTO trainee, List<int> exerciseOrder, DateTime startTime)
        {
            return scheduler.FindOptimalPath(trainee, exerciseOrder, startTime);
        }
    }
}
