using DBEntities.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Vml;
using DTO;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class BacktrackingScheduler
    {
        #region Fields - שדות
        private TraineeDTO currentTrainee;
        private ExerciseTransition[,] transitionMatrix;
        private int exerciseCount;
        private List<ExerciseDTO> exercises;
        private List<ExercisePlanDTO> explan;
        private Dictionary<int, QueueSlot> queueSlots;
        private Dictionary<int, int> exerciseIdToIndex;
        private Dictionary<int, HashSet<int>> reachabilityCache;
        Dictionary<int, TraineeExerciseStatus> traineesExerciseStatus = new();
        //private readonly ILogger<BacktrackingScheduler> _logger;

        public BacktrackingScheduler()
        {
        }
        public static int SlotMinutes { get; private set; }


        // מטמון לתוצאות חישוב
        private Dictionary<(int mask, int lastNodeId, long timeMinutes), CachedResult> memo = new();

        // מבנה לשמירת תוצאות במטמון
        private struct CachedResult
        {
            public bool Found;
            public int NumAlternatives;
            public List<int> BestPath;
            public DateTime EndTime;
        }
        #endregion

        #region אתחול המערכת
        // מאתחל את המערכת עם כל הרשומות והמבנים הנדרשים לעבודה
        public void Initialize(List<ExerciseDTO> exerciseList,
                              List<GraphEdgeDTO> exerciseEdges,
                              List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
                              List<MuscleEdgeDTO> muscleEdges,
                              Dictionary<int, int> equipmentCountByExercise,
                              DateTime firstSlotStart,
                              int slotMinutes, int slotCount)
        {
            exercises = exerciseList;
            exerciseCount = exercises.Count;

            SlotMinutes = slotMinutes;
            InitQueueSlots(equipmentCountByExercise, firstSlotStart, slotMinutes, slotCount);

            InitTransitionMatrix(exerciseList, exerciseEdges, exerciseToMuscleEdges, muscleEdges);
           // InitQueueSlots(equipmentCountByExercise, firstSlotStart, slotMinutes, slotCount);
            BuildReachabilityMatrix();
        }
        // בונה את מטריצת המעברים בין תרגילים, כולל חישוב חוקיות המעבר בין תרגיל אחד לאחר.
        private void InitTransitionMatrix(List<ExerciseDTO> exercises,
                                        List<GraphEdgeDTO> exerciseEdges,
                                        List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
                                        List<MuscleEdgeDTO> muscleEdges)
        {
            transitionMatrix = new ExerciseTransition[exerciseCount, exerciseCount];
            exerciseIdToIndex = exercises.Select((ex, idx) => new { ex.ExerciseId, idx })
                                       .ToDictionary(x => x.ExerciseId, x => x.idx);

            var exerciseToMuscle = exerciseToMuscleEdges
                .GroupBy(e => e.DeviceId)
                .ToDictionary(g => g.Key, g => g.First().MuscleId);

            var directEdges = new HashSet<(int from, int to)>(
                exerciseEdges.Select(e => (e.Device1Id, e.Device2Id))
            );

            var muscleConnections = muscleEdges
                .GroupBy(m => m.MuscleId1)
                .ToDictionary(g => g.Key, g => g.Select(x => x.MuscleId2).ToList());

            for (int to = 0; to < exerciseCount; to++)
            {
                int toId = exercises[to].ExerciseId;
                int toMuscle = exerciseToMuscle.ContainsKey(toId) ? exerciseToMuscle[toId] : -1;

                for (int from = 0; from < exerciseCount; from++)
                {
                    int fromId = exercises[from].ExerciseId;
                    int fromMuscle = exerciseToMuscle.ContainsKey(fromId) ? exerciseToMuscle[fromId] : -1;

                    int legalityValue = CalculateLegalityValue(to, from, toId, fromId,
                                                             toMuscle, fromMuscle,
                                                             directEdges, muscleConnections);

                    transitionMatrix[to, from] = new ExerciseTransition(queueSlots[to])
                    {
                        LegalityValue = legalityValue
                    };
                }
            }
        }
        // מחשב את ערך החוקיות (LegalityValue) של מעבר בין שני תרגילים, בהתאם לחיבורים ישירים ולקשרי שרירים.
        private int CalculateLegalityValue(int to, int from, int toId, int fromId,
                                         int toMuscle, int fromMuscle,
                                         HashSet<(int from, int to)> directEdges,
                                         Dictionary<int, List<int>> muscleConnections)
        {
            if (to == from)
                return 0; // אותו תרגיל

            if (directEdges.Contains((fromId, toId)))
                return toMuscle >= 0 ? (1 << toMuscle) : 0;//מכניס את הערך של 2 בחזקת הערך של השריר

            if (toMuscle >= 0 && fromMuscle >= 0 &&
                muscleConnections.TryGetValue(fromMuscle, out var neighbors) &&
                neighbors.Contains(toMuscle))
                return 1 << toMuscle;

            return -1; // לא חוקי
        }
        // מאתחל את כל תורי התרגילים (QueueSlots) עבור כל התרגילים, בהתאם למספר עמדות, זמן התחלה ומרווחי זמן
        private void InitQueueSlots(Dictionary<int, int> equipmentCountByExercise,  DateTime firstSlotStart,
                                   int slotMinutes, int slotCount)
        {
            //queueSlots = new Dictionary<int, QueueSlot>();
            //for (int i = 0; i < exerciseCount; i++)
            //{
            //    queueSlots[i] = new QueueSlot(equipmentCount, firstSlotStart, slotMinutes, slotCount);
            //}
            queueSlots = new Dictionary<int, QueueSlot>();
            for (int i = 0; i < exerciseCount; i++)
            {
                int exerciseId = exercises[i].ExerciseId;
                int equipmentCount = equipmentCountByExercise.ContainsKey(exerciseId)
                                        ? equipmentCountByExercise[exerciseId]
                                        : 1; // ערך ברירת מחדל אם לא הוגדר
                queueSlots[i] = new QueueSlot(equipmentCount, firstSlotStart, slotMinutes, slotCount);
            }
        }
        // בונה מטריצת Reachability המאפשרת לדעת אילו תרגילים נגישים מכל תרגיל אחר.
        private void BuildReachabilityMatrix()
        {
            if (reachabilityCache != null) return;

            reachabilityCache = new Dictionary<int, HashSet<int>>();

            for (int i = 0; i < exerciseCount; i++)
            {
                var reachable = new HashSet<int>();
                for (int j = 0; j < exerciseCount; j++)
                {
                    if (i != j && transitionMatrix[j, i].LegalityValue > 0)
                    {
                        reachable.Add(exercises[j].ExerciseId);
                    }
                }
                reachabilityCache[exercises[i].ExerciseId] = reachable;
            }
        }

        #endregion
        public void PrintTransitionMatrixToConsole()
        {
            if (transitionMatrix == null || exercises == null)
            {
                Console.WriteLine("Transition matrix or exercises are not initialized.");
                return;
            }

            // הגדרת רוחב תא קבוע
            int cellWidth = 12;

            // רשימת שמות התרגילים (שורות ועמודות)
            var names = exercises.Select(e => e.ExerciseName ?? e.ExerciseId.ToString()).ToList();

            // הדפסת כותרות עמודות (מימין לשמאל!)
            string header = "".PadLeft(cellWidth); // רווח ראשון לשם שורה
            for (int col = 0; col < exercises.Count; col++)
            {
                header += AlignText(names[col], cellWidth);
            }
            Console.WriteLine(header);

            // הדפסת שורות
            for (int row = 0; row < exercises.Count; row++)
            {
                string line = AlignText(names[row], cellWidth); // שם התרגיל (שורה)
                for (int col = 0; col < exercises.Count; col++)
                {
                    var trans = transitionMatrix[row, col];
                    string cell;
                    if (trans == null)
                    {
                        cell = AlignText("-", cellWidth);
                    }
                    else
                    {
                        int legality = trans.LegalityValue;
                        int eqCount = trans.QueueSlots?.EquipmentCount ?? 0;
                        cell = AlignText($"{legality}/E{eqCount}", cellWidth);
                    }
                    line += cell;
                }
                Console.WriteLine(line);
            }

            // פונקציה פנימית ליישור טקסט
            string AlignText(string text, int width)
            {
                // אם יש עברית - תיישר לימין, אחרת לשמאל
                bool isHebrew = text.Any(c => c >= 0x0590 && c <= 0x05FF);
                if (isHebrew)
                    return text.PadLeft(width);
                else
                    return text.PadRight(width);
            }
        }

        #region האלגוריתם הראשי
        // מחשב את המסלול האופטימלי עבור מתאמן, כולל סדר התרגילים וזמני התחלה/סיום.
        public PathResult FindOptimalPath(TraineeDTO trainee, List<ExercisePlanDTO> exerciseOrder, DateTime startTime)
        {
            //איתחול הסטטוס התרגילים של המתאמן
            traineesExerciseStatus[trainee.TraineeId] = new TraineeExerciseStatus
            {
                Trainee = trainee,
                Exercises = exerciseOrder
                    .Select((exId, idx) => new ExerciseStatusEntry
                    {
                        ExerciseId = exId.ExerciseId,
                        OrderInList = idx,
                        IsDone = false,
                        PerformedAt = null
                    }).ToList()
            };

            currentTrainee = trainee;
            memo.Clear();

            var result = BacktrackWithPriority(exerciseOrder, 0, new List<int>(),
                                             0, startTime, out DateTime endTime);

            if (result.found)
            {
                return new PathResult
                {
                    Trainee = trainee,
                    ExerciseIdsInPath = CreateExerciseEntries(result.bestPath),
                    StartTime = startTime,
                    EndTime = endTime,
                    AlternativesUsed = result.numAlternatives
                };
            }

            return null;
        }

        // אלגוריתם רקורסיבי עיקרי למציאת מסלול אופטימלי, תוך שמירה על חוקיות וזמינות.
        // משתמש בזיכרון (memoization) להאצת חישוב.
        private (bool found, int numAlternatives, List<int> bestPath, DateTime endTime)
            BacktrackWithPriority(List<ExercisePlanDTO> exerciseOrder, int mask, List<int> currentPath,
                                int currentAlternatives, DateTime currentTime, out DateTime endTime, bool isReschedulingAnotherTrainee = false)
        {
            // בדיקת מטמון
            int lastNodeId = currentPath.Count > 0 ? currentPath.Last() : -1;
            var memoKey = (mask, lastNodeId, currentTime.Ticks / TimeSpan.TicksPerMinute);

            if (memo.TryGetValue(memoKey, out var cachedResult))
            {
                if (cachedResult.Found && cachedResult.NumAlternatives <= currentAlternatives)
                {
                    endTime = cachedResult.EndTime;
                    return (cachedResult.Found, cachedResult.NumAlternatives, cachedResult.BestPath, cachedResult.EndTime);
                }
                // אחרת תמשיך לבדוק, אולי תמצא מסלול עם פחות חלופות
            }

            // תנאי סיום - כל התרגילים בוצעו
            if (mask == (1 << exerciseOrder.Count) - 1)
            {
                endTime = currentTime;
                var result = (true, currentAlternatives, new List<int>(currentPath), currentTime);

                // עדכון memo רק אם המסלול הזה טוב יותר
                if (!memo.ContainsKey(memoKey) ||
                    (memo[memoKey].Found && memo[memoKey].NumAlternatives > result.Item2) ||
                    !memo[memoKey].Found)
                {
                    memo[memoKey] = new CachedResult
                    {
                        Found = true,
                        NumAlternatives = result.Item2,
                        BestPath = result.Item3,
                        EndTime = endTime
                    };
                }

                return result;
            }


            // נבדוק את כל האסטרטגיות ונבחר את הטובה ביותר
            (bool found, int numAlternatives, List<int> bestPath, DateTime endTime) bestResult = (false, int.MaxValue, null, DateTime.MinValue);

            // אסטרטגיה 1: נסה תרגילים רגילים
            var strategy1Result = TryRegularExercises(exerciseOrder, mask, currentPath, currentAlternatives, currentTime, lastNodeId);
            if (strategy1Result.found && strategy1Result.numAlternatives == 0)
            {
                endTime = strategy1Result.endTime;
                memo[memoKey] = new CachedResult
                {
                    Found = true,
                    NumAlternatives = 0,
                    BestPath = new List<int>(strategy1Result.bestPath),
                    EndTime = endTime
                };
                return strategy1Result;
            }
            if (strategy1Result.found && strategy1Result.numAlternatives < bestResult.numAlternatives)
                bestResult = strategy1Result;

            // אסטרטגיה 2: תרגיל חילופי
            var strategy2Result = TryAlternativeExercises(exerciseOrder, mask, currentPath, currentAlternatives, currentTime, lastNodeId);
            if (strategy2Result.found && strategy2Result.numAlternatives < bestResult.numAlternatives)
                bestResult = strategy2Result;

            // אסטרטגיה 3: סידור מתאמנים אחרים (רק אם לא ברקורסיה)
            if (!isReschedulingAnotherTrainee)
            {
                var strategy3Result = TryReschedulingOthers(exerciseOrder, mask, currentPath, currentAlternatives, currentTime, lastNodeId);
                if (strategy3Result.found && strategy3Result.numAlternatives < bestResult.numAlternatives)
                    bestResult = strategy3Result;
            }


            // אם מצאנו מסלול, נעדכן את ה־memo ונחזיר אותו
            if (bestResult.found)
            {
                endTime = bestResult.endTime;
                if (!memo.ContainsKey(memoKey) ||
                    (memo[memoKey].Found && memo[memoKey].NumAlternatives > bestResult.numAlternatives) ||
                    !memo[memoKey].Found)
                {
                    memo[memoKey] = new CachedResult
                    {
                        Found = true,
                        NumAlternatives = bestResult.numAlternatives,
                        BestPath = new List<int>(bestResult.bestPath),
                        EndTime = bestResult.endTime
                    };
                }
                return bestResult;
            }

            // לא נמצא מסלול
            endTime = DateTime.MinValue;
            return (false, int.MaxValue, null, endTime);

        }

        #endregion

        #region אסטרטגיות חיפוש
        // מנסה לבצע את התרגילים המקוריים לפי הסדר, כל עוד אפשרי וזמין
        private (bool found, int numAlternatives, List<int> bestPath, DateTime endTime)
            TryRegularExercises(List<ExercisePlanDTO> exerciseOrder, int mask, List<int> currentPath,
                              int currentAlternatives, DateTime currentTime, int lastNodeId)
        {
            bool foundAny = false;
            int minAlternatives = int.MaxValue;
            List<int> bestPath = null;
            DateTime bestEndTime = DateTime.MinValue;
            List<int> exerciseIds = exerciseOrder.Select(e => e.ExerciseId).ToList();

            for (int i = 0; i < exerciseOrder.Count; i++)
            {
                if (IsExerciseDone(mask, i)) continue;

                int nodeId = exerciseOrder[i].ExerciseId;
                var exercisePlan = exerciseOrder[i];

                // בדיקות חוקיות
                if (!IsLegalTransition(lastNodeId, nodeId)) continue;
                if (!CanReachAllRemainingExercises(nodeId, exerciseIds, mask)) continue;

                // בדיקת זמינות
                if (IsSlotAvailable(nodeId, currentTime, exercisePlan.TimesMax))
                {
                    var duration = GetExerciseDuration(exercisePlan);
                    AddTraineeToSlot(nodeId, currentTime, duration, currentTrainee);

                    currentPath.Add(nodeId);
                    MarkExerciseAsDone(currentTrainee, nodeId, currentTime);

                    var nextTime = currentTime.Add(duration);

                    var result = BacktrackWithPriority(exerciseOrder, MarkExerciseDone(mask, i),
                                                     currentPath, currentAlternatives, nextTime,
                                                     out DateTime candidateEndTime);

                    if (result.found)
                    {
                        foundAny = true;
                        if (result.numAlternatives < minAlternatives)
                        {
                            minAlternatives = result.numAlternatives;
                            bestPath = new List<int>(result.bestPath);
                            bestEndTime = candidateEndTime;
                        }
                    }

                    // נקה אחרי כשלון
                    RemoveTraineeFromSlot(nodeId, currentTime, duration);
                    currentPath.RemoveAt(currentPath.Count - 1);
                    UndoMarkExerciseAsDone(currentTrainee, nodeId);
                }
            }

            return (foundAny, minAlternatives, bestPath, bestEndTime);
        }

        // מנסה לבצע תרגילים חילופיים במקום תרגילים שלא ניתן לבצע
        private (bool found, int numAlternatives, List<int> bestPath, DateTime endTime)
            TryAlternativeExercises(List<ExercisePlanDTO> exerciseOrder, int mask, List<int> currentPath,
                                  int currentAlternatives, DateTime currentTime, int lastNodeId)
        {
            bool foundAny = false;
            int minAlternatives = int.MaxValue;
            List<int> bestPath = null;
            DateTime bestEndTime = DateTime.MinValue;

            List<int> exerciseIds = exerciseOrder.Select(e => e.ExerciseId).ToList();

            for (int i = 0; i < exerciseOrder.Count; i++)
            {
                if (IsExerciseDone(mask, i)) continue;

                int originalNodeId = exerciseOrder[i].ExerciseId;
                var during = exerciseOrder[i].TimesMax;
                // נזהה את התרגיל הבא (אם יש)
                int nextIdx = -1;
                for (int j = i + 1; j < exerciseOrder.Count; j++)
                {
                    if (!IsExerciseDone(mask, j))
                    {
                        nextIdx = exerciseOrder[j].ExerciseId;
                        break;
                    }
                }
                // אלטרנטיבות לפי הפונקציה החדשה:
                var alternatives = GetAlternativeExercises(lastNodeId, originalNodeId, nextIdx);


                foreach (int altNodeId in alternatives)
                {
                    if (!IsLegalTransition(lastNodeId, altNodeId)) continue;
                    if (!CanReachAllRemainingExercises(altNodeId, exerciseIds, mask)) continue;

                    if (IsSlotAvailable(altNodeId, currentTime, during))
                    {
                        //var duration = GetExerciseDuration(altNodeId);
                        var duration = TimeSpan.FromMinutes(during);
                        AddTraineeToSlot(altNodeId, currentTime, duration, currentTrainee);

                        currentPath.Add(altNodeId);
                        var nextTime = currentTime.Add(duration);
                        MarkExerciseAsDone(currentTrainee, altNodeId, currentTime);

                        var result = BacktrackWithPriority(exerciseOrder, MarkExerciseDone(mask, i),
                                                         currentPath, currentAlternatives + 1, nextTime,
                                                         out DateTime candidateEndTime);

                        if (result.found)
                        {
                            foundAny = true;
                            if (result.numAlternatives < minAlternatives)
                            {
                                minAlternatives = result.numAlternatives;
                                bestPath = new List<int>(result.bestPath);
                                bestEndTime = candidateEndTime;
                            }
                        }

                        // נקה אחרי כשלון
                        RemoveTraineeFromSlot(altNodeId, currentTime, duration);
                        currentPath.RemoveAt(currentPath.Count - 1);
                        UndoMarkExerciseAsDone(currentTrainee, altNodeId);
                    }
                }
            }
            return (foundAny, minAlternatives, bestPath, bestEndTime);
        }

        // מנסה לסדר מחדש מתאמנים אחרים כדי לפנות תרגיל למתאמן הנוכחי, כולל טיפול באלטרנטיבות 
        private (bool found, int numAlternatives, List<int> bestPath, DateTime endTime)
            TryReschedulingOthers(List<ExercisePlanDTO> exerciseOrder, int mask, List<int> currentPath,
                                 int currentAlternatives, DateTime currentTime, int lastNodeId)
        {
            bool foundAny = false;
            int minAlternatives = int.MaxValue;
            List<int> bestPath = null;
            DateTime bestEndTime = DateTime.MinValue;

            for (int i = 0; i < exerciseOrder.Count; i++)
            {
                if (IsExerciseDone(mask, i)) continue;

                int nodeId = exerciseOrder[i].ExerciseId;
                var during = exerciseOrder[i].TimesMax;

                var alternatives = new List<int> { nodeId };
                int nextNodeId = GetNextExerciseId(exerciseOrder, mask, i);
                alternatives.AddRange(GetAlternativeExercises(lastNodeId, nodeId, nextNodeId));

                foreach (var alt in alternatives)
                {
                    //צריך לבדוק נראה לי שצריך את כל המתאמנים 
                    var occupyingTrainee = GetOccupyingTraineeInSlot(alt, currentTime);
                    foreach (var ocTrainee in occupyingTrainee)
                    {
                        if (ocTrainee != null && ocTrainee.TraineeId != currentTrainee.TraineeId)
                        {
                            var othersRemainingExercisesID = GetRemainingExercisesForTrainee(ocTrainee, currentTime);
                            List<ExercisePlanDTO> othersRemainingExercises = exerciseOrder
                                                .Where(e => othersRemainingExercisesID.Contains(e.ExerciseId))
                                                .ToList();
                            // קריאה לאלגוריתם הראשי עבור המתאמן השני, עם דגל שמונע ממנו להזיז אחרים
                            var result = BacktrackWithPriority(
                                othersRemainingExercises,
                                0,
                                new List<int>(),
                                0,
                                currentTime,
                                out DateTime _,
                                isReschedulingAnotherTrainee: true
                            );

                            if (result.found)
                            {
                                ApplyNewExerciseOrderToTrainee(ocTrainee, result.bestPath, currentTime);

                                //var duration = GetExerciseDuration(alt);
                                var duration = TimeSpan.FromMinutes(during);
                                AddTraineeToSlot(alt, currentTime, duration, currentTrainee);
                                currentPath.Add(alt);

                                MarkExerciseAsDone(currentTrainee, nodeId, currentTime);

                                var res = BacktrackWithPriority(exerciseOrder, MarkExerciseDone(mask, i),
                                                             currentPath, currentAlternatives + 2,
                                                             currentTime.Add(duration),
                                                             out DateTime candidateEndTime);

                                if (res.found)
                                {
                                    foundAny = true;
                                    if (result.numAlternatives < minAlternatives)
                                    {
                                        minAlternatives = result.numAlternatives;
                                        bestPath = new List<int>(result.bestPath);
                                        bestEndTime = candidateEndTime;
                                    }
                                    break;
                                }

                                RemoveTraineeFromSlot(alt, currentTime, duration);
                                currentPath.RemoveAt(currentPath.Count - 1);
                                UndoApplyNewExerciseOrderToTrainee(ocTrainee, currentTime);
                                UndoMarkExerciseAsDone(currentTrainee, nodeId);

                            }
                        }

                    }
                }
            }
            return (foundAny, minAlternatives, bestPath, bestEndTime);
        }

        #endregion

        #region פונקציות עזר - בדיקות חוקיות

        // בודק האם תרגיל מסוים כבר בוצע
        private bool IsExerciseDone(int mask, int exerciseIndex)
        {
            return (mask & (1 << exerciseIndex)) != 0;
        }

        // מסמן תרגיל כבוצע במפת הסיום (mask)
        private int MarkExerciseDone(int mask, int exerciseIndex)
        {
            return mask | (1 << exerciseIndex);
        }

        // ###בודק אם מותר לעבור בין שני תרגילים בזמן נתון 
        private bool IsLegalTransition(int fromNodeId, int toNodeId)
        {
            if (fromNodeId == -1) return true; // התחלה

            int fromIdx = GetExerciseIndex(fromNodeId);
            int toIdx = GetExerciseIndex(toNodeId);

            if (fromIdx < 0 || toIdx < 0) return false;

            return transitionMatrix[toIdx, fromIdx].LegalityValue > 0;
        }

        // בודק האם ניתן להגיע מכל תרגיל נוכחי לכל התרגילים שנותרו
        private bool CanReachAllRemainingExercises(int currentNodeId, List<int> exerciseOrder, int mask)
        {
            if (!reachabilityCache.TryGetValue(currentNodeId, out var reachable))
                return false;

            for (int i = 0; i < exerciseOrder.Count; i++)
            {
                if (IsExerciseDone(mask, i)) continue;

                int exerciseId = exerciseOrder[i];
                if (exerciseId != currentNodeId && !reachable.Contains(exerciseId))
                    return false;
            }

            return true;
        }

        //מחזיר את האינדקס של תרגיל לפי מזהה
        private int GetExerciseIndex(int exerciseId)
        {
            return exerciseIdToIndex.TryGetValue(exerciseId, out int index) ? index : -1;
        }

        #endregion

        #region פונקציות עזר - ניהול סלוטים

        // בודק האם יש סלוט פנוי לתרגיל בזמן מסוים
        private bool IsSlotAvailable(int exerciseId, DateTime startTime, int during)
        {
            int exerciseIndex = GetExerciseIndex(exerciseId);
            if (exerciseIndex < 0) return false;

            var queueSlot = queueSlots[exerciseIndex];
            if (!queueSlot.SlotsByStartTime.TryGetValue(startTime, out var slot))
                return false;
            //צריך לבדוק האם נותנים לפי משך זמן של תרגיל או משך זמן של מתאמן 
            //var duration = GetExerciseDuration(exerciseId);
            var duration = TimeSpan.FromMinutes(during);
            var slotsNeeded = GetSlotsNeeded(duration);

            // בדוק זמינות של כל הסלוטים הנדרשים
            var currentSlotTime = startTime;
            for (int i = 0; i < slotsNeeded; i++)
            {
                if (!queueSlot.SlotsByStartTime.TryGetValue(currentSlotTime, out var checkSlot) ||
                    checkSlot.ExercisesByTrainee.Count >= queueSlot.EquipmentCount)
                {
                    return false;
                }
                currentSlotTime = checkSlot.EndTime;
            }

            return true;
        }

        // מוסיף מתאמן לסלוט של תרגיל בזמן נתון
        private void AddTraineeToSlot(int exerciseId, DateTime startTime, TimeSpan duration, TraineeDTO trainee)
        {
            int exerciseIndex = GetExerciseIndex(exerciseId);
            var queueSlot = queueSlots[exerciseIndex];
            var slotsNeeded = GetSlotsNeeded(duration);

            queueSlot.AddTraineeToSlot(startTime, slotsNeeded, trainee);
        }

        // מסיר מתאמן מסלוט של תרגיל
        private void RemoveTraineeFromSlot(int exerciseId, DateTime startTime, TimeSpan duration)
        {
            int exerciseIndex = GetExerciseIndex(exerciseId);
            var queueSlot = queueSlots[exerciseIndex];
            var slotsNeeded = GetSlotsNeeded(duration);

            var currentSlotTime = startTime;
            for (int i = 0; i < slotsNeeded; i++)
            {
                if (queueSlot.SlotsByStartTime.TryGetValue(currentSlotTime, out var slot))
                {
                    var traineeToRemove = slot.ExercisesByTrainee.Keys.FirstOrDefault(t => t.TraineeId == currentTrainee.TraineeId);
                    if (traineeToRemove != null)
                    {
                        slot.ExercisesByTrainee.Remove(traineeToRemove);
                    }
                }
                currentSlotTime = slot?.EndTime ?? currentSlotTime.AddMinutes(duration.TotalMinutes);
            }
        }

        // מחזיר את המתאמן שתופס סלוט מסוים, אם קיים
        private List<TraineeDTO> GetOccupyingTraineeInSlot(int exerciseId, DateTime startTime)
        {
            int exerciseIndex = GetExerciseIndex(exerciseId);
            var queueSlot = queueSlots[exerciseIndex];

            //if (queueSlot.SlotsByStartTime.TryGetValue(startTime, out var slot))
            //{
            //    return slot.ExercisesByTrainee.Keys.FirstOrDefault();
            //}

            //return null;

            if (queueSlot.SlotsByStartTime.TryGetValue(startTime, out var slot))
            {
                return slot.ExercisesByTrainee.Keys.ToList();
            }

            return new List<TraineeDTO>(); // מחזיר רשימה ריקה אם אין סלוט כזה
        }

        // מחשב כמה סלוטים דרושים למשך התרגיל
        private int GetSlotsNeeded(TimeSpan duration)
        {
            return (int)Math.Ceiling(duration.TotalMinutes / SlotMinutes);
        }

        #endregion

        #region פונקציות עזר - תרגילים חילופיים

        // מחפש תרגילים חילופיים אפשריים לפי מעבר חוקי במטריצת המעברים
        private List<int> GetAlternativeExercises(int prevExerciseId, int currentExerciseId, int nextExerciseId)
        {
            var alternatives = new List<int>();

            int prevIdx = GetExerciseIndex(prevExerciseId);
            int currIdx = GetExerciseIndex(currentExerciseId);
            int nextIdx = GetExerciseIndex(nextExerciseId);

            if (prevIdx < 0 || currIdx < 0 || nextIdx < 0)
                return alternatives;

            // הערך שצריך לחפש
            int referenceValue = transitionMatrix[currIdx, prevIdx].LegalityValue;

            var possibleAlternatives = new List<int>();
            // שלב א: מציאת כל האלטרנטיביים בשורה של התרגיל הקודם שיש להם את אותו ערך כמו המשבצת המקורית
            for (int rowIdx = 0; rowIdx < exerciseCount; rowIdx++)
            {
                if (rowIdx == currIdx) continue; // לא התרגיל הנוכחי
                if (transitionMatrix[rowIdx, prevIdx].LegalityValue == referenceValue)
                {
                    possibleAlternatives.Add(rowIdx);
                }
            }

            // שלב ב: עבור כל האפשריים, בדוק אם בתרגיל הבא (בשורה שלו) יש ערך חוקי (>0 או שונה מ-(-1) ו-0)
            foreach (int altIdx in possibleAlternatives)
            {
                int transitionValue = transitionMatrix[nextIdx, altIdx].LegalityValue;
                if (transitionValue != 0 && transitionValue != -1)
                {
                    alternatives.Add(exercises[altIdx].ExerciseId);
                }
            }

            return alternatives;
        }

        #endregion

        #region פונקציות עזר - סידור מחדש של מתאמנים

        // מחזיר את מזהה התרגיל הבא שטרם בוצע 
        //private int GetNextExerciseId(List<ExercisePlan> exerciseOrder, int mask, int currentIdx)
        //{
        //    for (int j = currentIdx + 1; j < exerciseOrder.Count; j++)
        //    {
        //        if (!IsExerciseDone(mask, j))
        //            return exerciseOrder[j].ExerciseId;
        //    }
        //    return -1;
        //}
        private int GetNextExerciseId(List<ExercisePlanDTO> exerciseOrder, int mask, int currentIdx)
        {
            int count = exerciseOrder.Count;
            for (int offset = 1; offset <= count; offset++)
            {
                int j = (currentIdx + offset) % count;
                if (!IsExerciseDone(mask, j))
                    return exerciseOrder[j].ExerciseId;
            }
            return -1;
        }

        // מחזיר רשימת תרגילים שעדיין לא בוצעו עבור מתאמן
        private List<int> GetRemainingExercisesForTrainee(TraineeDTO trainee, DateTime currentTime)
        {
            if (!traineesExerciseStatus.TryGetValue(trainee.TraineeId, out TraineeExerciseStatus status))
                return new List<int>();
            // בחר רק תרגילים שלא בוצעו
            return status.Exercises
                .Where(e => !e.IsDone)
                .OrderBy(e => e.OrderInList)
                .Select(e => e.ExerciseId)
                .ToList();
        }

        // מחליף למתאמן את סדר התרגילים בפועל, כולל אפשרות לשחזור (Undo)
        private void ApplyNewExerciseOrderToTrainee(TraineeDTO trainee, List<int> newOrder, DateTime currentTime)
        {
            if (!traineesExerciseStatus.TryGetValue(trainee.TraineeId, out TraineeExerciseStatus status))
                return;

            // שמור מצב קודם ל-Undo
            status.History.Push(status.Exercises.Select(e => new ExerciseStatusEntry
            {
                ExerciseId = e.ExerciseId,
                OrderInList = e.OrderInList,
                IsDone = e.IsDone,
                PerformedAt = e.PerformedAt
            }).ToList());

            // צור סדר חדש - שומר על אותם IsDone/PerformedAt לפי ExerciseId
            var dictById = status.Exercises.ToDictionary(e => e.ExerciseId);
            status.Exercises = newOrder
                .Select((exId, idx) =>
                    dictById.ContainsKey(exId)
                        ? new ExerciseStatusEntry
                        {
                            ExerciseId = exId,
                            OrderInList = idx,
                            IsDone = dictById[exId].IsDone,
                            PerformedAt = dictById[exId].PerformedAt
                        }
                        : new ExerciseStatusEntry
                        {
                            ExerciseId = exId,
                            OrderInList = idx,
                            IsDone = false,
                            PerformedAt = null
                        }
                ).ToList();
        }

        // מחזיר למתאמן את סדר התרגילים הקודם (Undo)
        private void UndoApplyNewExerciseOrderToTrainee(TraineeDTO trainee, DateTime currentTime)
        {
            if (!traineesExerciseStatus.TryGetValue(trainee.TraineeId, out TraineeExerciseStatus status))
                return;

            if (status.History.Count > 0)
                status.Exercises = status.History.Pop();
        }
        #endregion

        #region פונקציות עזר - כלליות

        // מבטל סימון של תרגיל כבוצע אצל מתאמן
        private void UndoMarkExerciseAsDone(TraineeDTO trainee, int exerciseId)
        {
            if (!traineesExerciseStatus.TryGetValue(trainee.TraineeId, out TraineeExerciseStatus status))
                return;

            var ex = status.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
            if (ex != null)
            {
                ex.IsDone = false;
                ex.PerformedAt = null;
            }
        }

        // מסמן תרגיל כבוצע אצל מתאמן ומעדכן את זמן הביצוע
        private void MarkExerciseAsDone(TraineeDTO trainee, int exerciseId, DateTime performedAt)
        {
            if (!traineesExerciseStatus.TryGetValue(trainee.TraineeId, out TraineeExerciseStatus status))
                return;

            var ex = status.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
            if (ex != null)
            {
                ex.IsDone = true;
                ex.PerformedAt = performedAt;
            }
        }

        // מחזיר את משך הזמן של תרגיל לפי מזהה
        private TimeSpan GetExerciseDuration(ExercisePlanDTO exercisePlan)
        {
            //var exerciseDuration = exercisePlan.TimesMax;
            return TimeSpan.FromMinutes(exercisePlan.TimesMax);
            // var exercise = exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);

            //return exercise?.Duration ?? TimeSpan.FromMinutes(15); 
        }

        // יוצר אובייקטים של ExerciseEntry עבור כל תרגיל במסלול הנבחר
        private Dictionary<int, ExerciseEntry> CreateExerciseEntries(List<int> exercisePath)
        {
            var entries = new Dictionary<int, ExerciseEntry>();

            for (int i = 0; i < exercisePath.Count; i++)
            {
                int exerciseId = exercisePath[i];
                int exerciseIndex = GetExerciseIndex(exerciseId);

                entries[i] = new ExerciseEntry
                {
                    ExerciseId = exerciseId,
                    OrderInList = i,
                    Slot = exerciseIndex >= 0 ? queueSlots[exerciseIndex] : null
                };
            }

            return entries;
        }

        #endregion
    }
}











//if (memo.TryGetValue(memoKey, out var cachedResult))
//{
//    endTime = cachedResult.EndTime;
//    return (cachedResult.Found, cachedResult.NumAlternatives,
//           cachedResult.BestPath, cachedResult.EndTime);
//}



//private bool TryRescheduleOtherTrainee(TraineeDTO otherTrainee, int exerciseId, DateTime desiredTime)
//{
//    // נסה למצוא זמן חלופי למתאמן האחר
//    int exerciseIndex = GetExerciseIndex(exerciseId);
//    var queueSlot = queueSlots[exerciseIndex];

//    // חפש סלוטים פנויים בסביבה
//    var possibleTimes = FindAlternativeSlots(exerciseId, desiredTime);

//    foreach (var alternativeTime in possibleTimes)
//    {
//        if (IsSlotAvailable(exerciseId, alternativeTime))
//        {
//            // הזז את המתאמן האחר
//            var duration = GetExerciseDuration(exerciseId);
//            RemoveTraineeFromSpecificSlot(otherTrainee, exerciseId, desiredTime);
//            AddTraineeToSlot(exerciseId, alternativeTime, duration, otherTrainee);
//            return true;
//        }
//    }

//    return false;
//}
//private bool TryRescheduleOtherTraineeByReordering(TraineeDTO otherTrainee, int blockingExerciseId, DateTime blockingTime)
//{
//    // שלב 1: שלוף את רשימת התרגילים שטרם בוצעו עבורו
//    var remainingExercises = GetRemainingExercisesForTrainee(otherTrainee, blockingTime);

//    // שלב 2: בנה סדרות אפשריות של ביצוע תרגילים (אפשר עם BacktrackWithPriority או TryRegularExercises)
//    // תנאי: שלא יבצע את blockingExerciseId ב-blockingTime
//    var result = TryFindPathWithoutBlockingExerciseAtTime(
//        otherTrainee,
//        remainingExercises,
//        blockingExerciseId,
//        blockingTime
//    );

//    // שלב 3: אם מצאנו מסלול, נבצע אותו בפועל (נעדכן את הסלוטים, הסדר וכו')
//    if (result.found)
//    {
//        ApplyNewExerciseOrderToTrainee(otherTrainee, result.path, blockingTime);
//        return true;
//    }

//    return false;
//}


//private void RestoreTraineeToSlot(TraineeDTO trainee, int exerciseId, DateTime originalTime)
//{
//    var duration = GetExerciseDuration(exerciseId);
//    AddTraineeToSlot(exerciseId, originalTime, duration, trainee);
//}

//private void RemoveTraineeFromSpecificSlot(TraineeDTO trainee, int exerciseId, DateTime startTime)
//{
//    int exerciseIndex = GetExerciseIndex(exerciseId);
//    var queueSlot = queueSlots[exerciseIndex];

//    if (queueSlot.SlotsByStartTime.TryGetValue(startTime, out var slot))
//    {
//        slot.ExercisesByTrainee.Remove(trainee);
//    }
//}

//private List<DateTime> FindAlternativeSlots(int exerciseId, DateTime originalTime)
//{
//    var alternatives = new List<DateTime>();
//    int exerciseIndex = GetExerciseIndex(exerciseId);
//    var queueSlot = queueSlots[exerciseIndex];

//    // חפש בטווח של ±30 דקות
//    var searchStart = originalTime.AddMinutes(-30);
//    var searchEnd = originalTime.AddMinutes(30);

//    foreach (var kvp in queueSlot.SlotsByStartTime)
//    {
//        if (kvp.Key >= searchStart && kvp.Key <= searchEnd && kvp.Key != originalTime)
//        {
//            alternatives.Add(kvp.Key);
//        }
//    }

//    // מיין לפי קרבה לזמן המקורי
//    return alternatives.OrderBy(t => Math.Abs((t - originalTime).TotalMinutes)).ToList();
//}




//private List<int> GetAlternativeExercises(int originalExerciseId)
//{
//    // פונקציה זו צריכה להחזיר רשימה של תרגילים חילופיים
//    // בהתבסס על קבוצות שרירים או סוג התרגיל
//    var alternatives = new List<int>();

//    // כאן תוכל להוסיף לוגיקה מתוחכמת יותר
//    // לדוגמה: תרגילים שמאמנים את אותם השרירים
//    var originalExercise = exercises.FirstOrDefault(e => e.ExerciseId == originalExerciseId);
//    if (originalExercise != null)
//    {
//        // מצא תרגילים דומים (זו דוגמה פשוטה)
//        alternatives.AddRange(exercises
//            .Where(e => e.ExerciseId != originalExerciseId &&
//                       e.MuscleGroupId == originalExercise.MuscleGroupId)
//            .Select(e => e.ExerciseId));
//    }

//    return alternatives;
//}
















// הרחבות למחלקות קיימות
public static class Extensions
{
    public static void RemoveTraineeFromSlot(this QueueSlot queueSlot, DateTime startTime, int slotsCount, TraineeDTO trainee)
    {
        var currentTime = startTime;
        for (int i = 0; i < slotsCount; i++)
        {
            if (queueSlot.SlotsByStartTime.TryGetValue(currentTime, out var slot))
            {
                slot.ExercisesByTrainee.Remove(trainee);
                currentTime = slot.EndTime;
            }
        }
    }
}



//private (bool found, int numAlternatives, List<int> bestPath, DateTime endTime)
//    TryReschedulingOthers(List<int> exerciseOrder, int mask, List<int> currentPath,
//                        int currentAlternatives, DateTime currentTime, int lastNodeId)
//{
//    for (int i = 0; i < exerciseOrder.Count; i++)
//    {
//        if (IsExerciseDone(mask, i)) continue;

//        int nodeId = exerciseOrder[i];

//        if (!IsLegalTransition(lastNodeId, nodeId, currentTime)) continue;
//        if (!CanReachAllRemainingExercises(nodeId, exerciseOrder, mask)) continue;

//        var occupyingTrainee = GetOccupyingTraineeInSlot(nodeId, currentTime);
//        //if (occupyingTrainee != null && occupyingTrainee.TraineeId != currentTrainee.TraineeId)
//        //{
//        //    if (TryRescheduleOtherTrainee(occupyingTrainee, nodeId, currentTime))
//        //    {
//        //        var duration = GetExerciseDuration(nodeId);
//        //        AddTraineeToSlot(nodeId, currentTime, duration, currentTrainee);
//        //        currentPath.Add(nodeId);

//        //        var result = BacktrackWithPriority(exerciseOrder, MarkExerciseDone(mask, i),
//        //                                         currentPath, currentAlternatives + 2,
//        //                                         currentTime.Add(duration),
//        //                                         out DateTime candidateEndTime);

//        //        if (result.found)
//        //        {
//        //            return (true, result.numAlternatives, result.bestPath, candidateEndTime);
//        //        }

//        //        // החזר את המצב הקודם
//        //        RemoveTraineeFromSlot(nodeId, currentTime, duration);
//        //        currentPath.RemoveAt(currentPath.Count - 1);
//        //        RestoreTraineeToSlot(occupyingTrainee, nodeId, currentTime);
//        //    }
//        //}
//        // עבור כל מתאמן שתופס את הסלוט
//        if (occupyingTrainee != null && occupyingTrainee.TraineeId != currentTrainee.TraineeId)
//        {
//            // רשימת התרגילים שנותרו לאותו מתאמן
//            var remainingExercises = GetRemainingExercisesForTrainee(occupyingTrainee, currentTime);

//            // נסה להריץ אלגוריתם מלא עבורו, אבל עם דגל שמונע ממנו לשנות אחרים
//            var result = BacktrackWithPriority(
//                remainingExercises,
//                /*mask*/ 0,
//                /*currentPath*/ new List<int>(),
//                /*currentAlternatives*/ 0,
//                /*currentTime*/ currentTime,
//                out DateTime _,
//                isReschedulingAnotherTrainee: true // חשוב!
//            );

//            if (result.found)
//            {
//                // החלף לו את הסדר בפועל, עדכן סלוטים וכו'
//                ApplyNewExerciseOrderToTrainee(occupyingTrainee, result.bestPath, currentTime);
//                // עכשיו אפשר להכניס את המתאמן שלך לתרגיל
//                // ...
//            }
//        }
//    }

//    return (false, int.MaxValue, null, DateTime.MinValue);
//}
//private (bool found, int numAlternatives, List<int> bestPath, DateTime endTime)
//    TryReschedulingOthers(List<int> exerciseOrder, int mask, List<int> currentPath,
//                 int currentAlternatives, DateTime currentTime, int lastNodeId)
//{
//    for (int i = 0; i < exerciseOrder.Count; i++)
//    {
//        if (IsExerciseDone(mask, i)) continue;

//        int nodeId = exerciseOrder[i];

//        // גם תרגילים חילופיים
//        var alternatives = new List<int> { nodeId };
//        int nextNodeId = GetNextExerciseId(exerciseOrder, mask, i);
//        alternatives.AddRange(GetAlternativeExercises(lastNodeId, nodeId, nextNodeId));

//        foreach (var alt in alternatives)
//        {
//            var occupyingTrainee = GetOccupyingTraineeInSlot(alt, currentTime);
//            if (occupyingTrainee != null && occupyingTrainee.TraineeId != currentTrainee.TraineeId)
//            {
//                // שלוף את רשימת התרגילים שנותרו למתאמן זה
//                var othersRemainingExercises = GetRemainingExercisesForTrainee(occupyingTrainee, currentTime);

//                // נסה להריץ אלגוריתם מלא עבורו (אבל לא יוכל להזיז אחרים)
//                var result = BacktrackWithPriority(
//                    othersRemainingExercises,
//                    0,
//                    new List<int>(),
//                    0,
//                    currentTime,
//                    out DateTime _,
//                    isReschedulingAnotherTrainee: true // מונע רקורסיה
//                );

//                if (result.found)
//                {
//                    // עדכן בפועל את סדר התרגילים של המתאמן ואת הסלוטים
//                    ApplyNewExerciseOrderToTrainee(occupyingTrainee, result.bestPath, currentTime);

//                    // עכשיו ניתן להכניס את המתאמן הנוכחי לתרגיל שהתפנה
//                    var duration = GetExerciseDuration(alt);
//                    AddTraineeToSlot(alt, currentTime, duration, currentTrainee);
//                    currentPath.Add(alt);

//                    var res = BacktrackWithPriority(exerciseOrder, MarkExerciseDone(mask, i),
//                                                 currentPath, currentAlternatives + 2,
//                                                 currentTime.Add(duration),
//                                                 out DateTime candidateEndTime);

//                    if (res.found)
//                        return (true, res.numAlternatives, res.bestPath, candidateEndTime);

//                    // החזר את המצב לקדמותו
//                    RemoveTraineeFromSlot(alt, currentTime, duration);
//                    currentPath.RemoveAt(currentPath.Count - 1);
//                    UndoApplyNewExerciseOrderToTrainee(occupyingTrainee, currentTime); // פונקציה שתשחזר את הסדר והסלוטים
//                }
//            }
//        }
//    }

//    return (false, int.MaxValue, null, DateTime.MinValue);
//}






//// DTOs נוספים שעשויים להידרש
//public class ExerciseDTO
//{
//    public int ExerciseId { get; set; }
//    public string Name { get; set; }
//    public int MuscleGroupId { get; set; }
//    public TimeSpan Duration { get; set; }
//}

//public class TraineeDTO
//{
//    public int TraineeId { get; set; }
//    public string Name { get; set; }
//    public List<int> ExerciseProgram { get; set; }
//}

//public class GraphEdgeDTO
//{
//    public int Device1Id { get; set; }
//    public int Device2Id { get; set; }
//}

//public class DeviceMuscleEdgeDTO
//{
//    public int DeviceId { get; set; }
//    public int MuscleId { get; set; }
//}

//public class MuscleEdgeDTO
//{
//    public int MuscleId1 { get; set; }
//    public int MuscleId2 { get; set; }
//}

