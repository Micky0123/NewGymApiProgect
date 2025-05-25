//using DTO;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BLL
//{
//    public class SchedulerInit
//    {
//        //private TraineeDTO currentTrainee;
//        private ExerciseTransition[,] transitionMatrix;
//        private int exerciseCount;
//        private List<ExerciseDTO> exercises;
//        private Dictionary<int, QueueSlot> queueSlots;
//        private Dictionary<int, int> exerciseIdToIndex;
//        private Dictionary<int, HashSet<int>> reachabilityCache;


//        #region אתחול המערכת
//        public void Initialize(List<ExerciseDTO> exerciseList,
//                              List<GraphEdgeDTO> exerciseEdges,
//                              List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
//                              List<MuscleEdgeDTO> muscleEdges,
//                              int equipmentCount, DateTime firstSlotStart,
//                              int slotMinutes, int slotCount)
//        {
//            exercises = exerciseList;
//            exerciseCount = exercises.Count;

//            InitTransitionMatrix(exerciseList, exerciseEdges, exerciseToMuscleEdges, muscleEdges);
//            InitQueueSlots(equipmentCount, firstSlotStart, slotMinutes, slotCount);
//            BuildReachabilityMatrix();
//        }

//        private void InitTransitionMatrix(List<ExerciseDTO> exercises,
//                                        List<GraphEdgeDTO> exerciseEdges,
//                                        List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
//                                        List<MuscleEdgeDTO> muscleEdges)
//        {
//            transitionMatrix = new ExerciseTransition[exerciseCount, exerciseCount];
//            exerciseIdToIndex = exercises.Select((ex, idx) => new { ex.ExerciseId, idx })
//                                       .ToDictionary(x => x.ExerciseId, x => x.idx);

//            var exerciseToMuscle = exerciseToMuscleEdges
//                .GroupBy(e => e.DeviceId)
//                .ToDictionary(g => g.Key, g => g.First().MuscleId);

//            var directEdges = new HashSet<(int from, int to)>(
//                exerciseEdges.Select(e => (e.Device1Id, e.Device2Id))
//            );

//            var muscleConnections = muscleEdges
//                .GroupBy(m => m.MuscleId1)
//                .ToDictionary(g => g.Key, g => g.Select(x => x.MuscleId2).ToList());

//            for (int to = 0; to < exerciseCount; to++)
//            {
//                int toId = exercises[to].ExerciseId;
//                int toMuscle = exerciseToMuscle.ContainsKey(toId) ? exerciseToMuscle[toId] : -1;

//                for (int from = 0; from < exerciseCount; from++)
//                {
//                    int fromId = exercises[from].ExerciseId;
//                    int fromMuscle = exerciseToMuscle.ContainsKey(fromId) ? exerciseToMuscle[fromId] : -1;

//                    int legalityValue = CalculateLegalityValue(to, from, toId, fromId,
//                                                             toMuscle, fromMuscle,
//                                                             directEdges, muscleConnections);

//                    transitionMatrix[to, from] = new ExerciseTransition(queueSlots[to])
//                    {
//                        LegalityValue = legalityValue
//                    };
//                }
//            }
//        }

//        private int CalculateLegalityValue(int to, int from, int toId, int fromId,
//                                         int toMuscle, int fromMuscle,
//                                         HashSet<(int from, int to)> directEdges,
//                                         Dictionary<int, List<int>> muscleConnections)
//        {
//            if (to == from)
//                return 0; // אותו תרגיל

//            if (directEdges.Contains((fromId, toId)))
//                return toMuscle >= 0 ? (1 << toMuscle) : 0;

//            if (toMuscle >= 0 && fromMuscle >= 0 &&
//                muscleConnections.TryGetValue(fromMuscle, out var neighbors) &&
//                neighbors.Contains(toMuscle))
//                return 1 << toMuscle;

//            return -1; // לא חוקי
//        }

//        private void InitQueueSlots(int equipmentCount, DateTime firstSlotStart,
//                                   int slotMinutes, int slotCount)
//        {
//            queueSlots = new Dictionary<int, QueueSlot>();
//            for (int i = 0; i < exerciseCount; i++)
//            {
//                queueSlots[i] = new QueueSlot(equipmentCount, firstSlotStart, slotMinutes, slotCount);
//            }
//        }

//        private void BuildReachabilityMatrix()
//        {
//            if (reachabilityCache != null) return;

//            reachabilityCache = new Dictionary<int, HashSet<int>>();

//            for (int i = 0; i < exerciseCount; i++)
//            {
//                var reachable = new HashSet<int>();
//                for (int j = 0; j < exerciseCount; j++)
//                {
//                    if (i != j && transitionMatrix[j, i].LegalityValue > 0)
//                    {
//                        reachable.Add(exercises[j].ExerciseId);
//                    }
//                }
//                reachabilityCache[exercises[i].ExerciseId] = reachable;
//            }
//        }

//        #endregion


//    }
//}
