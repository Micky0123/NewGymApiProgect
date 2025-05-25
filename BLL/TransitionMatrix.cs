//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BLL;
//using DBEntities.Models;
//using DTO;
//using IBLL;
//using Microsoft.Graph.Models;

//namespace BLL
//{
//    public class TransitionMatrix
//    {

//        private ExerciseTransition[,] transitionMatrix;// מטריצה דו-ממדית: [תרגיל יעד, תרגיל מקור]
//        private int exerciseCount; //מספר התרגילים
//                                   //private int timeSlotsCount; // כמות הסולטים
//                                   // private int slotDurationMinutes = 5; // ברירת מחדל: כל סלוט הוא 5 דקות
//        private List<ExerciseDTO> exercises; // שמירת רשימת התרגילים הכללית
//        /// אולי להוסיף כמות מכל תרגיל


//        // זה כשמהתחלים את התורים 
//        public Dictionary<int, QueueSlot> queueSlots = new Dictionary<int, QueueSlot>(); // מילון של תורים

//        public void initQueueSlot(int EquipmentCount, DateTime firstSlotStart, int slotMinutes, int slotCount)
//        {
//            for (int i = 0; i < exerciseCount; i++)
//            {
//                queueSlots[i] = new QueueSlot(EquipmentCount, firstSlotStart, slotCount, slotMinutes); //יצירת תור חדש
//            }
//        }

//        // אופטימיזציה: מציאת האינדקס של תרגיל במערך על ידי Dictionary
//        private Dictionary<int, int> exerciseIdToIndex;

//        // מטריצת הנגישות תיבנה רק פעם אחת ותישמר למשך כל זמן החיפוש
//        private Dictionary<int, HashSet<int>> reachabilityCache;

//        // שימוש במכניזם קשירות (Caching) מתקדם עבור BuildReachabilityMatrix
//        public Dictionary<int, HashSet<int>> BuildReachabilityMatrix(IEnumerable<Node> nodes)
//        {
//            // בדוק אם כבר חישבנו את המטריצה בעבר
//            if (reachabilityCache != null)
//                return reachabilityCache;

//            var nodesList = nodes.ToList();
//            var result = new Dictionary<int, HashSet<int>>();

//            foreach (var nodeA in nodesList)
//            {
//                var reachable = new HashSet<int>();
//                foreach (var nodeB in nodesList)
//                {
//                    if (nodeA.Id == nodeB.Id)
//                        continue;

//                    // בדוק ישירות מהמטריצה אם המעבר חוקי
//                    int fromIdx = GetExerciseIndex(nodeA.Id);
//                    int toIdx = GetExerciseIndex(nodeB.Id);

//                    if (fromIdx >= 0 && toIdx >= 0 && fromIdx < exerciseCount && toIdx < exerciseCount &&
//                        transitionMatrix[toIdx, fromIdx].LegalityValue > 0)
//                    {
//                        reachable.Add(nodeB.Id);
//                    }
//                }
//                result[nodeA.Id] = reachable;
//            }

//            // שמור במטמון לשימוש עתידי
//            reachabilityCache = result;
//            return result;
//        }

//        // אתחול המטריצה הדו-ממדית עם תורי הזמנים
//        public void InitTransitionMatrix(
//        List<ExerciseDTO> exercises,
//        List<GraphEdgeDTO> exerciseEdges,
//        List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
//        List<MuscleEdgeDTO> muscleEdges)
//        {
//            exerciseCount = exercises.Count;
//            transitionMatrix = new ExerciseTransition[exerciseCount, exerciseCount];
//            exerciseIdToIndex = exercises.Select((ex, idx) => new { ex.ExerciseId, idx })
//                                        .ToDictionary(x => x.ExerciseId, x => x.idx);

//            // בנה מילון: תרגיל -> שריר עיקרי
//            var exerciseToMuscle = exerciseToMuscleEdges
//                .GroupBy(e => e.DeviceId)
//                .ToDictionary(g => g.Key, g => g.First().MuscleId);

//            // בנה HashSet לכל קשת בין תרגילים
//            var directEdges = new HashSet<(int from, int to)>(
//                exerciseEdges.Select(e => (e.Device1Id, e.Device2Id))
//            );

//            // סט של קשתות שריר->שריר
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

//                    int legalityValue;

//                    if (to == from)
//                    {
//                        legalityValue = 0; // אותו תרגיל
//                    }
//                    else if (directEdges.Contains((fromId, toId)))
//                    {
//                        legalityValue = toMuscle >= 0 ? (1 << toMuscle) : 0;
//                    }
//                    else if (toMuscle >= 0 && fromMuscle >= 0 &&
//                             muscleConnections.TryGetValue(fromMuscle, out var neighbors) &&
//                             neighbors.Contains(toMuscle))
//                    {
//                        legalityValue = 1 << toMuscle;
//                    }
//                    else
//                    {
//                        legalityValue = -1;
//                    }

//                    transitionMatrix[to, from] = new ExerciseTransition(queueSlots[to])
//                    {
//                        LegalityValue = legalityValue
//                    };

//                }
//            }
//        }










//        public bool IsLegalTransition(int fromExerciseId, int toExerciseId, int timeSlot)
//        {
//            // אם זה אותו תרגיל, החזר false
//            if (fromExerciseId == toExerciseId)
//                return false;

//            int fromIdx = GetExerciseIndex(fromExerciseId);
//            int toIdx = GetExerciseIndex(toExerciseId);

//            // בדוק תקינות האינדקסים
//            if (fromIdx < 0 || toIdx < 0 || timeSlot < 0 || timeSlot >= timeSlotsCount)
//                return false;

//            // בדוק שהמעבר חוקי מבחינה פיזיולוגית
//            return transitionMatrix[toIdx, fromIdx].LegalityValue > 0;
//        }

//        // בדיקת זמינות של סלוט זמן מסוים במעבר מתרגיל לתרגיל
//        public bool IsSlotAvailable(int toExerciseId, int fromExerciseId, int timeSlot)
//        {
//            int toIndex = GetExerciseIndex(toExerciseId);
//            int fromIndex = GetExerciseIndex(fromExerciseId);

//            // אם אינדקס לא נמצא או חורג מהתחום, לא זמין
//            if (toIndex < 0 || toIndex >= exerciseCount ||
//                fromIndex < 0 || fromIndex >= exerciseCount)
//            {
//                return false;
//            }

//            return transitionMatrix[toIndex, fromIndex].IsSlotAvailable(timeSlot, maxInQueue);
//        }

//        // הוספת מתאמן לסלוט זמן מסוים
//        public void AddTraineeToSlot(int toExerciseId, int fromExerciseId, int timeSlot, Trainee trainee)
//        {
//            int toIndex = GetExerciseIndex(toExerciseId);
//            int fromIndex = GetExerciseIndex(fromExerciseId);

//            // אם האינדקסים תקינים, הוסף את המתאמן לסלוט
//            if (toIndex >= 0 && toIndex < exerciseCount &&
//                fromIndex >= 0 && fromIndex < exerciseCount)
//            {
//                transitionMatrix[toIndex, fromIndex].AddTraineeToSlot(timeSlot, trainee);
//            }
//        }

//        // הסרת מתאמן מסלוט זמן מסוים
//        public Trainee RemoveTraineeFromSlot(int toExerciseId, int fromExerciseId, int timeSlot)
//        {
//            int toIndex = GetExerciseIndex(toExerciseId);
//            int fromIndex = GetExerciseIndex(fromExerciseId);

//            if (toIndex >= 0 && toIndex < exerciseCount &&
//                fromIndex >= 0 && fromIndex < exerciseCount)
//            {
//                return transitionMatrix[toIndex, fromIndex].RemoveTraineeFromSlot(timeSlot);
//            }

//            return null;
//        }

//        //שימי ❤ זה נראה לי צריך להחזיר רשימה של מתאמנים 
//        // פונקציה - מוצאת את המתאמן שתופס סלוט מסוים במעבר
//        public Trainee GetOccupyingTraineeInSlot(int toExerciseId, int fromExerciseId, int timeSlot)
//        {
//            int toIndex = GetExerciseIndex(toExerciseId);
//            int fromIndex = GetExerciseIndex(fromExerciseId);

//            if (toIndex >= 0 && toIndex < exerciseCount &&
//                fromIndex >= 0 && fromIndex < exerciseCount)
//            {
//                return transitionMatrix[toIndex, fromIndex].GetCurrentTraineeInSlot(timeSlot);
//            }

//            return null;
//        }

//        // פונקציית עזר: המרת DateTime לאינדקס סלוט
//        public int TimeToSlot(DateTime startTime, DateTime slotTime)
//        {
//            return (int)((slotTime - startTime).TotalMinutes / slotDurationMinutes);
//        }

//        // פונקציית עזר: המרת אינדקס סלוט ל-DateTime 
//        public DateTime SlotToTime(DateTime startTime, int slotIndex)
//        {
//            return startTime.AddMinutes(slotIndex * slotDurationMinutes);
//        }

//        public int GetExerciseIndex(int exerciseId)
//        {
//            if (exerciseIdToIndex.TryGetValue(exerciseId, out int index))
//                return index;
//            return -1;
//        }


//    }
//}







////private ExerciseTransition[,] transitionMatrix;// מטריצה דו-ממדית: [תרגיל יעד, תרגיל מקור]
////private int exerciseCount; //מספר התרגילים
////private int timeSlotsCount; // כמות הסולטים
////private int slotDurationMinutes = 5; // ברירת מחדל: כל סלוט הוא 5 דקות
////private List<ExerciseDTO> exercises; // שמירת רשימת התרגילים הכללית
/////// אולי להוסיף כמות מכל תרגיל


////// אופטימיזציה: מציאת האינדקס של תרגיל במערך על ידי Dictionary
////private Dictionary<int, int> exerciseIdToIndex;

////// מטריצת הנגישות תיבנה רק פעם אחת ותישמר למשך כל זמן החיפוש
////private Dictionary<int, HashSet<int>> reachabilityCache;

////// שימוש במכניזם קשירות (Caching) מתקדם עבור BuildReachabilityMatrix
////public Dictionary<int, HashSet<int>> BuildReachabilityMatrix(IEnumerable<Node> nodes)
////{
////    // בדוק אם כבר חישבנו את המטריצה בעבר
////    if (reachabilityCache != null)
////        return reachabilityCache;

////    var nodesList = nodes.ToList();
////    var result = new Dictionary<int, HashSet<int>>();

////    foreach (var nodeA in nodesList)
////    {
////        var reachable = new HashSet<int>();
////        foreach (var nodeB in nodesList)
////        {
////            if (nodeA.Id == nodeB.Id)
////                continue;

////            // בדוק ישירות מהמטריצה אם המעבר חוקי
////            int fromIdx = GetExerciseIndex(nodeA.Id);
////            int toIdx = GetExerciseIndex(nodeB.Id);

////            if (fromIdx >= 0 && toIdx >= 0 && fromIdx < exerciseCount && toIdx < exerciseCount &&
////                transitionMatrix[toIdx, fromIdx].LegalityValue > 0)
////            {
////                reachable.Add(nodeB.Id);
////            }
////        }
////        result[nodeA.Id] = reachable;
////    }

////    // שמור במטמון לשימוש עתידי
////    reachabilityCache = result;
////    return result;
////}


////// אתחול המטריצה הדו-ממדית עם תורי הזמנים
////public void InitTable(List<ExerciseDTO> exerciseList, int slotsCount)
////{
////    exercises = exerciseList ?? throw new ArgumentNullException(nameof(exerciseList));
////    exerciseCount = exercises.Count;
////    timeSlotsCount = slotsCount;
////    transitionMatrix = new ExerciseTransition[exerciseCount, exerciseCount];

////    // אתחול מילון מיידי מזהה -> אינדקס
////    exerciseIdToIndex = new Dictionary<int, int>();
////    for (int i = 0; i < exercises.Count; i++)
////    {
////        exerciseIdToIndex[exercises[i].ExerciseId] = i;
////    }

////    // חישוב מטריצת החוקיות והתורים פעם אחת בלבד
////    for (int to = 0; to < exerciseCount; to++)
////    {
////        for (int from = 0; from < exerciseCount; from++)
////        {
////            int legalityValue;

////            if (to == from)
////            {
////                legalityValue = 0; // אותו תרגיל
////            }
////            else
////            {
////                // בדוק אם יש קשר ישיר בין התרגילים
////                bool hasDirectPath = exercises[from].Neighbors?.Contains(exercises[to]) ?? false;

////                // אם יש קשר ישיר, נשמור את קבוצת השריר
////                legalityValue = hasDirectPath ?
////                    (int)Math.Pow(2, (int)exercises[to].MuscleGroup) : -1;
////            }

////            // יצירת אובייקט המעבר עם תורי הזמנים
////            transitionMatrix[to, from] = new ExerciseTransition(timeSlotsCount)
////            {
////                LegalityValue = legalityValue
////            };
////        }
////    }
////}


////public bool IsLegalTransition(int fromExerciseId, int toExerciseId, int timeSlot)
////{
////    // אם זה אותו תרגיל, החזר false
////    if (fromExerciseId == toExerciseId)
////        return false;

////    int fromIdx = GetExerciseIndex(fromExerciseId);
////    int toIdx = GetExerciseIndex(toExerciseId);

////    // בדוק תקינות האינדקסים
////    if (fromIdx < 0 || toIdx < 0 || timeSlot < 0 || timeSlot >= timeSlotsCount)
////        return false;

////    // בדוק שהמעבר חוקי מבחינה פיזיולוגית
////    return transitionMatrix[toIdx, fromIdx].LegalityValue > 0;
////}

////// בדיקת זמינות של סלוט זמן מסוים במעבר מתרגיל לתרגיל
////public bool IsSlotAvailable(int toExerciseId, int fromExerciseId, int timeSlot, int maxInQueue = 1)
////{
////    int toIndex = GetExerciseIndex(toExerciseId);
////    int fromIndex = GetExerciseIndex(fromExerciseId);

////    // אם אינדקס לא נמצא או חורג מהתחום, לא זמין
////    if (toIndex < 0 || toIndex >= exerciseCount ||
////        fromIndex < 0 || fromIndex >= exerciseCount)
////    {
////        return false;
////    }

////    return transitionMatrix[toIndex, fromIndex].IsSlotAvailable(timeSlot, maxInQueue);
////}

////// הוספת מתאמן לסלוט זמן מסוים
////public void AddTraineeToSlot(int toExerciseId, int fromExerciseId, int timeSlot, Trainee trainee)
////{
////    int toIndex = GetExerciseIndex(toExerciseId);
////    int fromIndex = GetExerciseIndex(fromExerciseId);

////    // אם האינדקסים תקינים, הוסף את המתאמן לסלוט
////    if (toIndex >= 0 && toIndex < exerciseCount &&
////        fromIndex >= 0 && fromIndex < exerciseCount)
////    {
////        transitionMatrix[toIndex, fromIndex].AddTraineeToSlot(timeSlot, trainee);
////    }
////}

////// הסרת מתאמן מסלוט זמן מסוים
////public Trainee RemoveTraineeFromSlot(int toExerciseId, int fromExerciseId, int timeSlot)
////{
////    int toIndex = GetExerciseIndex(toExerciseId);
////    int fromIndex = GetExerciseIndex(fromExerciseId);

////    if (toIndex >= 0 && toIndex < exerciseCount &&
////        fromIndex >= 0 && fromIndex < exerciseCount)
////    {
////        return transitionMatrix[toIndex, fromIndex].RemoveTraineeFromSlot(timeSlot);
////    }

////    return null;
////}

//////שימי ❤ זה נראה לי צריך להחזיר רשימה של מתאמנים 
////// פונקציה - מוצאת את המתאמן שתופס סלוט מסוים במעבר
////public Trainee GetOccupyingTraineeInSlot(int toExerciseId, int fromExerciseId, int timeSlot)
////{
////    int toIndex = GetExerciseIndex(toExerciseId);
////    int fromIndex = GetExerciseIndex(fromExerciseId);

////    if (toIndex >= 0 && toIndex < exerciseCount &&
////        fromIndex >= 0 && fromIndex < exerciseCount)
////    {
////        return transitionMatrix[toIndex, fromIndex].GetCurrentTraineeInSlot(timeSlot);
////    }

////    return null;
////}

////// פונקציית עזר: המרת DateTime לאינדקס סלוט
////public int TimeToSlot(DateTime startTime, DateTime slotTime)
////{
////    return (int)((slotTime - startTime).TotalMinutes / slotDurationMinutes);
////}

////// פונקציית עזר: המרת אינדקס סלוט ל-DateTime 
////public DateTime SlotToTime(DateTime startTime, int slotIndex)
////{
////    return startTime.AddMinutes(slotIndex * slotDurationMinutes);
////}

////public int GetExerciseIndex(int exerciseId)
////{
////    if (exerciseIdToIndex.TryGetValue(exerciseId, out int index))
////        return index;
////    return -1;
////}
