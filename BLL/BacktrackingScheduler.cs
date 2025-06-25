using DBEntities.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Vml.Office;
using DTO;
using IBLL;
using IDAL;
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
        public static int SlotMinutes { get; private set; }
        private readonly ITraineeBLL traineeBLL;

        public BacktrackingScheduler(ITraineeBLL traineeBLL)
        {
            this.traineeBLL = traineeBLL;
        }

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
        private void InitQueueSlots(Dictionary<int, int> equipmentCountByExercise, DateTime firstSlotStart,
                                   int slotMinutes, int slotCount)
        {
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
                    if (i != j && transitionMatrix[j,i].LegalityValue > 0)
                    {
                        reachable.Add(exercises[j].ExerciseId);
                    }
                }
                reachabilityCache[exercises[i].ExerciseId] = reachable;
            }
        }

        //הדפסת המטריצה
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

            string header = "".PadLeft(cellWidth); // רווח ראשון לשם שורה
            for (int col = 0; col < exercises.Count; col++)
            {
                header += AlignText(names[col], cellWidth);
            }
            Console.WriteLine(header);

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

        #endregion

        #region האלגוריתם הראשי

        // מחשב את המסלול האופטימלי עבור מתאמן, כולל סדר התרגילים וזמני התחלה וסיום.
        public async Task<PathResult> FindOptimalPath(TraineeDTO trainee, List<ExercisePlanDTO> exerciseOrder, DateTime startTime)
        {
            //בדיקה אם יש זמן התחלה בסולט
            if (!queueSlots.Any() || !queueSlots.Values.Any(q => q.SlotsByStartTime.ContainsKey(startTime)))
            {
                //תעגל את הזמן התחלה של המתאמןלה למעלה לזמן זמין הקרוב ביותר
                startTime = queueSlots.Values
                    .SelectMany(q => q.SlotsByStartTime.Keys)
                    .Where(t => t > startTime)
                    .OrderBy(t => t)
                    .FirstOrDefault();
            }
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
                        PerformedAt = null,

                    }).ToList()
            };

            currentTrainee = trainee;
            memo.Clear();

            var result = await BacktrackWithPriority(exerciseOrder, 0, new List<int>(),
                                             0, startTime);

            if (result.found)
            {
                // נקה קודם כל שיבוצים ישנים
                RemoveTraineeFromAllSlots(trainee);

                // עדכן את השיבוץ הסופי
                return await AssignTraineeToFinalSlots(trainee, exerciseOrder, result.bestPath, startTime, result.numAlternatives);
            }
            return null;
        }

        // אלגוריתם רקורסיבי עיקרי למציאת מסלול אופטימלי, תוך שמירה על חוקיות וזמינות.
        // משתמש בזיכרון (memoization) להאצת חישוב.
        private async Task<(bool found, int numAlternatives, List<int> bestPath, DateTime endTime)>
        BacktrackWithPriority(List<ExercisePlanDTO> exerciseOrder, int mask, List<int> currentPath,
                            int currentAlternatives, DateTime currentTime, bool isReschedulingAnotherTrainee = false)
        {
            // בדיקת מטמון
            int lastNodeId = currentPath.Count > 0 ? currentPath.Last() : -1;
            var memoKey = (mask, lastNodeId, currentTime.Ticks / TimeSpan.TicksPerMinute);

            if (memo.TryGetValue(memoKey, out var cachedResult))
            {
                if (cachedResult.Found && cachedResult.NumAlternatives <= currentAlternatives)
                {
                    return (cachedResult.Found, cachedResult.NumAlternatives, cachedResult.BestPath, cachedResult.EndTime);
                }
                // אחרת תמשיך לבדוק, אולי תמצא מסלול עם פחות חלופות
            }

            // תנאי סיום - כל התרגילים בוצעו
            if (mask == (1 << exerciseOrder.Count) - 1)
            {
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
                        EndTime = result.Item4
                    };
                }

                return result;
            }


            // נבדוק את כל האסטרטגיות ונבחר את הטובה ביותר
            (bool found, int numAlternatives, List<int> bestPath, DateTime endTime) bestResult = (false, int.MaxValue, null, DateTime.MinValue);

            // אסטרטגיה 1: נסה תרגילים רגילים
            var strategy1Result = await TryRegularExercises(exerciseOrder, mask, currentPath, currentAlternatives, currentTime, lastNodeId);
            if (strategy1Result.found && strategy1Result.numAlternatives == 0)
            {
                memo[memoKey] = new CachedResult
                {
                    Found = true,
                    NumAlternatives = 0,
                    BestPath = new List<int>(strategy1Result.bestPath),
                    EndTime = strategy1Result.endTime
                };
                return strategy1Result;
            }
            if (strategy1Result.found && strategy1Result.numAlternatives < bestResult.numAlternatives)
                bestResult = strategy1Result;

            // אסטרטגיה 2: תרגיל חילופי
            var strategy2Result = await TryAlternativeExercises(exerciseOrder, mask, currentPath, currentAlternatives, currentTime, lastNodeId);
            if (strategy2Result.found && strategy2Result.numAlternatives < bestResult.numAlternatives)
                bestResult = strategy2Result;

            // אסטרטגיה 3: סידור מתאמנים אחרים (רק אם לא ברקורסיה)
            if (!isReschedulingAnotherTrainee)
            {
                var strategy3Result = await TryReschedulingOthers(exerciseOrder, mask, currentPath, currentAlternatives, currentTime, lastNodeId);
                if (strategy3Result.found && strategy3Result.numAlternatives < bestResult.numAlternatives)
                    bestResult = strategy3Result;
            }
            // אם מצאנו מסלול, נעדכן את ה־memo ונחזיר אותו
            if (bestResult.found)
            {
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
            return (false, int.MaxValue, null, DateTime.MinValue);
        }

        #endregion

        #region אסטרטגיות חיפוש
        // מנסה לבצע את התרגילים המקוריים לפי הסדר, כל עוד אפשרי וזמין
        private async Task<(bool found, int numAlternatives, List<int> bestPath, DateTime endTime)>
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
                    var duration = TimeSpan.FromMinutes(exercisePlan.TimesMax);
                    currentPath.Add(nodeId);

                    var nextTime = currentTime.Add(duration);

                    var result = await BacktrackWithPriority(exerciseOrder, MarkExerciseDone(mask, i),
                                                     currentPath, currentAlternatives, nextTime);
                    if (result.found)
                    {

                        foundAny = true;
                        if (result.numAlternatives < minAlternatives)
                        {
                            minAlternatives = result.numAlternatives;
                            bestPath = new List<int>(result.bestPath);
                            bestEndTime = result.endTime;
                        }
                        if (result.numAlternatives == 0)
                        {
                            // החזר מיידית את התוצאה האופטימלית
                            return (foundAny, minAlternatives, bestPath, bestEndTime);
                        }
                    }
                    // מנקה אחרי כשלון
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
            }
            return (foundAny, minAlternatives, bestPath, bestEndTime);
        }

        // מנסה לבצע תרגילים חילופיים במקום תרגילים שלא ניתן לבצע
        private async Task<(bool found, int numAlternatives, List<int> bestPath, DateTime endTime)>
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
                        var duration = TimeSpan.FromMinutes(during);
                        currentPath.Add(altNodeId);
                        var nextTime = currentTime.Add(duration);

                        var result = await BacktrackWithPriority(exerciseOrder, MarkExerciseDone(mask, i),
                                                         currentPath, currentAlternatives + 1, nextTime);

                        if (result.found)
                        {
                            foundAny = true;
                            if (result.numAlternatives < minAlternatives)
                            {
                                minAlternatives = result.numAlternatives;
                                bestPath = new List<int>(result.bestPath);
                                bestEndTime = result.endTime;
                            }
                        }

                        // נקה אחרי כשלון
                        currentPath.RemoveAt(currentPath.Count - 1);
                    }
                }
            }
            return (foundAny, minAlternatives, bestPath, bestEndTime);
        }

        // מנסה לסדר מחדש מתאמנים אחרים כדי לפנות תרגיל למתאמן הנוכחי, כולל טיפול באלטרנטיבות 
        private async Task<(bool found, int numAlternatives, List<int> bestPath, DateTime endTime)>
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
                    foreach (var ocTrainee1 in occupyingTrainee)
                    {
                        var ocTrainee = await traineeBLL.GetTraineeByIdAsync(ocTrainee1);
                        if (ocTrainee != null && ocTrainee.TraineeId != currentTrainee.TraineeId)
                        {
                            var othersRemainingExercisesID = GetRemainingExercisesForTrainee(ocTrainee, currentTime);
                            List<ExercisePlanDTO> othersRemainingExercises = exerciseOrder
                                                .Where(e => othersRemainingExercisesID.Contains(e.ExerciseId))
                                                .ToList();
                            // קריאה לאלגוריתם הראשי עבור המתאמן השני, עם דגל שמונע ממנו להזיז אחרים
                            var result = await BacktrackWithPriority(
                                othersRemainingExercises,
                                0,
                                new List<int>(),
                                0,
                                currentTime,
                                isReschedulingAnotherTrainee: true
                            );

                            if (result.found)
                            {
                                ApplyNewExerciseOrderToTrainee(ocTrainee, result.bestPath, currentTime);
                                var duration = TimeSpan.FromMinutes(during);
                                currentPath.Add(alt);

                                var res = await BacktrackWithPriority(exerciseOrder, MarkExerciseDone(mask, i),
                                                             currentPath, currentAlternatives + 2,
                                                             currentTime.Add(duration));
                                if (res.found)
                                {
                                    foundAny = true;
                                    if (result.numAlternatives < minAlternatives)
                                    {
                                        minAlternatives = result.numAlternatives;
                                        bestPath = new List<int>(result.bestPath);
                                        bestEndTime = result.endTime;
                                    }
                                    break;
                                }
                                currentPath.RemoveAt(currentPath.Count - 1);
                                UndoApplyNewExerciseOrderToTrainee(ocTrainee, currentTime);

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

        // בודק אם מותר לעבור בין שני תרגילים בזמן נתון 
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
                    var traineeToRemove = slot.ExercisesByTrainee.Keys.FirstOrDefault(t => t == currentTrainee.TraineeId);
                    if (traineeToRemove != null)
                    {
                        slot.ExercisesByTrainee.Remove(traineeToRemove);
                    }
                }
                currentSlotTime = slot?.EndTime ?? currentSlotTime.AddMinutes(duration.TotalMinutes);
            }
        }

        // מחזיר את המתאמן שתופס סלוט מסוים, אם קיים
        private List<int> GetOccupyingTraineeInSlot(int exerciseId, DateTime startTime)
        {
            int exerciseIndex = GetExerciseIndex(exerciseId);
            var queueSlot = queueSlots[exerciseIndex];
            if (queueSlot.SlotsByStartTime.TryGetValue(startTime, out var slot))
            {
                return slot.ExercisesByTrainee.Keys.ToList();
            }

            return new List<int>(); // מחזיר רשימה ריקה אם אין סלוט כזה
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
            if (currIdx < 0 || nextIdx < 0)
                return alternatives;
            if (prevIdx > 0)
            {
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
            }
            return alternatives;
        }

        #endregion

        #region פונקציות עזר - סידור מחדש של מתאמנים
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

        #region פונקציות נוספות

        // משלים את המסלול הסופי עבור המתאמן, כולל הוספת תרגילים לסלוטים
        // בסיום האלגוריתם, עבור המסלול הסופי בלבד:
        private async Task<PathResult> AssignTraineeToFinalSlots(
            TraineeDTO trainee,
            List<ExercisePlanDTO> exerciseOrder,
            List<int> bestPath,
            DateTime startTime,
            int alternativesCount)
        {
            var currentTime = startTime;
            int orderInList = 1;
            var exerciseEntries = new Dictionary<int, ExerciseEntry>();
            var allUsedSlots = new List<Slot>();

            foreach (var exerciseId in bestPath)
            {
                // חפש את פרטי התרגיל לתזמון ומשך
                var plan = exerciseOrder.FirstOrDefault(x => x.ExerciseId == exerciseId);

                if (plan == null) continue;

                var duration = TimeSpan.FromMinutes(plan.TimesMax);
                int exerciseIndex = GetExerciseIndex(exerciseId);
                var queueSlot = queueSlots[exerciseIndex];
                int slotsNeeded = GetSlotsNeeded(duration);

                // חפש את הסלוטים המתאימים לתרגיל הזה
                var slotsForThisExercise = new List<Slot>();
                var slotTime = currentTime;
                for (int i = 0; i < slotsNeeded; i++)
                {
                    if (queueSlot.SlotsByStartTime.TryGetValue(slotTime, out var slot))
                    {
                        slotsForThisExercise.Add(slot);
                        slotTime = slot.EndTime;
                    }
                }
                // הגדר את זמן סיום התרגיל (התבסס על זמן סיום הסלוט האחרון)
                var exerciseStartTime = currentTime;
                var exerciseEndTime = slotsForThisExercise.Count > 0
                    ? slotsForThisExercise.Last().EndTime
                    : currentTime.Add(duration);

                // צור אובייקט תרגיל למסלול
                var entry = new ExerciseEntry
                {
                    ExerciseId = exerciseId,
                    OrderInList = orderInList,
                    StartTime = exerciseStartTime,
                    EndTime = exerciseEndTime,
                    Slots = slotsForThisExercise,
                    ExerciseDetails = plan,
                };

                exerciseEntries[exerciseId] = entry;
                allUsedSlots.AddRange(slotsForThisExercise);

                // עדכן זמן נוכחי לתרגיל הבא
                currentTime = exerciseEndTime;
                orderInList++;
            }

            // בנה את המסלול המלא להחזרה
            var pathResult = new PathResult
            {
                Trainee = trainee,
                ExerciseIdsInPath = exerciseEntries,
                StartTime = startTime,
                EndTime = currentTime,
                AlternativesUsed = alternativesCount
            };
            // קשר דו-כיווני מתאמן-סלוט
            foreach (var slot in allUsedSlots)
            {
                slot.AddTranee(trainee, pathResult);
            }
            return pathResult;
        }


        // מנקה שיבוצים ישנים לפני הרצה חדשה
        private void RemoveTraineeFromAllSlots(TraineeDTO trainee)
        {
            foreach (var q in queueSlots.Values)
            {
                foreach (var slot in q.SlotsByStartTime.Values)
                {
                    slot.ExercisesByTrainee.Remove(trainee.TraineeId);
                }
            }
        }

        //לקרא פעם ביום לפונקציה הזו
        //פונקציה שמנקה את כל הסלוטים מכל המתאמנים
        private void ClearAllSlots()
        {
            foreach (var q in queueSlots.Values)
            {
                foreach (var slot in q.SlotsByStartTime.Values)
                {
                    slot.ExercisesByTrainee.Clear();
                }
            }
        }

        #endregion
    }
}



