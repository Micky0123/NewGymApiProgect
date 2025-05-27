//using DBEntities.Models;
//using DTO;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BLL
//{
//    public class BacktrackingScheduler2
//    {
//        private TraineeDTO currentTrainee;
//        private ExerciseTransition[,] transitionMatrix;
//        private int exerciseCount;
//        private List<ExerciseDTO> exercises;
//        private Dictionary<int, QueueSlot> queueSlots;
//        private Dictionary<int, int> exerciseIdToIndex;
//        private Dictionary<int, HashSet<int>> reachabilityCache;
//        Dictionary<int, TraineeExerciseStatus> traineesExerciseStatus = new();

//        // מטמון לתוצאות חישוב
//        private Dictionary<(int mask, int lastNodeId, long timeMinutes), CachedResult> memo = new();

//        // מבנה לשמירת תוצאות במטמון
//        private struct CachedResult
//        {
//            public bool Found;
//            public int NumAlternatives;
//            public List<int> BestPath;
//            public DateTime EndTime;
//        }


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
//                return 0;

//            if (directEdges.Contains((fromId, toId)))
//                return toMuscle >= 0 ? (1 << toMuscle) : 0;

//            if (toMuscle >= 0 && fromMuscle >= 0 &&
//                muscleConnections.TryGetValue(fromMuscle, out var neighbors) &&
//                neighbors.Contains(toMuscle))
//                return 1 << toMuscle;

//            return -1;
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

//        #region האלגוריתם הראשי האופטימלי
//        public PathResult FindOptimalPath(TraineeDTO trainee, List<int> exerciseOrder, DateTime startTime)
//        {
//            // איתחול מצב המתאמן
//            traineesExerciseStatus[trainee.TraineeId] = new TraineeExerciseStatus
//            {
//                Trainee = trainee,
//                Exercises = exerciseOrder
//                    .Select((exId, idx) => new ExerciseStatusEntry
//                    {
//                        ExerciseId = exId,
//                        OrderInList = idx,
//                        IsDone = false,
//                        PerformedAt = null
//                    }).ToList()
//            };

//            currentTrainee = trainee;
//            memo.Clear();

//            // חיפוש עם הערכת עלות
//            var bestSolution = FindBestSolution(exerciseOrder, startTime);

//            if (bestSolution != null)
//            {
//                return new PathResult
//                {
//                    Trainee = trainee,
//                    ExerciseIdsInPath = CreateExerciseEntries(bestSolution.path),
//                    StartTime = startTime,
//                    EndTime = bestSolution.endTime,
//                    AlternativesUsed = bestSolution.alternativesCount,
//                    ReschedulingsUsed = bestSolution.reschedulingsCount
//                };
//            }

//            return null;
//        }

//        private OptimalSolution FindBestSolution(List<int> exerciseOrder, DateTime startTime)
//        {
//            var initialState = new SearchState
//            {
//                mask = 0,
//                currentPath = new List<int>(),
//                currentTime = startTime,
//                lastExerciseId = -1,
//                alternativesCount = 0,
//                reschedulingsCount = 0
//            };

//            return OptimalBacktrack(exerciseOrder, initialState);
//        }

//        private OptimalSolution OptimalBacktrack(List<int> exerciseOrder, SearchState state)
//        {
//            // בדיקת מטמון מתקדם
//            var memoKey = CreateMemoKey(state);
//            if (memo.TryGetValue(memoKey, out var cachedResult))
//            {
//                return cachedResult;
//            }

//            // תנאי סיום - כל התרגילים בוצעו
//            if (state.mask == (1 << exerciseOrder.Count) - 1)
//            {
//                var solution = new OptimalSolution
//                {
//                    found = true,
//                    path = new List<int>(state.currentPath),
//                    endTime = state.currentTime,
//                    alternativesCount = state.alternativesCount,
//                    reschedulingsCount = state.reschedulingsCount,
//                    totalCost = CalculateSolutionCost(state)
//                };

//                memo[memoKey] = solution;
//                return solution;
//            }

//            OptimalSolution bestSolution = null;
//            double bestCost = double.MaxValue;

//            // *** הוסף כאן בדיקת קיצור דרך ***
//            var directOption = TryFindDirectPath(exerciseOrder, state);
//            if (directOption != null)
//            {
//                return directOption; // מצאנו נתיב ישיר - סיימנו!
//            }


//            // יצירת כל האפשרויות האפשריות
//            var allOptions = GenerateAllOptions(exerciseOrder, state);

//            foreach (var option in allOptions)
//            {
//                if (!IsValidOption(option, exerciseOrder, state)) continue;

//                // ביצוע המהלך
//                var newState = ApplyOption(option, state);
//                ExecuteStateChange(option, newState);

//                try
//                {
//                    // חיפוש רקורסיבי
//                    var result = OptimalBacktrack(exerciseOrder, newState);

//                    if (result != null && result.found && result.totalCost < bestCost)
//                    {
//                        bestCost = result.totalCost;
//                        bestSolution = result;
//                    }
//                }
//                finally
//                {
//                    // ניקוי (חשוב לעשות ב-finally)
//                    UndoStateChange(option, state);
//                }
//            }

//            // שמירה במטמון
//            memo[memoKey] = bestSolution;
//            return bestSolution;
//        }
//        private OptimalSolution TryFindDirectPath(List<int> exerciseOrder, SearchState state)
//        {
//            // נסה לבנות נתיב שלם עם התרגילים המקוריים בלבד
//            var testState = new SearchState { /* העתק מצב נוכחי */ };

//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                if (IsExerciseDone(state.mask, i)) continue;

//                int exerciseId = exerciseOrder[i];

//                // בדוק אם אפשר לבצע ישירות
//                if (!CanExecuteDirectly(exerciseId, testState))
//                {
//                    return null; // לא אפשר נתיב ישיר
//                }

//                // עדכן מצב בדיקה
//                testState = ApplyExercise(exerciseId, testState);
//            }

//            // הצלחנו! יש נתיב ישיר
//            return new OptimalSolution
//            {
//                found = true,
//                path = testState.currentPath,
//                alternativesCount = 0,
//                reschedulingsCount = 0,
//                totalCost = CalculateDirectPathCost(testState)
//            };
//        }
//        private List<ExerciseOption> GenerateAllOptions(List<int> exerciseOrder, SearchState state)
//        {
//            var options = new List<ExerciseOption>();

//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                if (IsExerciseDone(state.mask, i)) continue;

//                int originalExerciseId = exerciseOrder[i];

//                // אופציה 1: תרגיל מקורי
//                if (CanExecuteDirectly(originalExerciseId, state))
//                {
//                    options.Add(new ExerciseOption
//                    {
//                        exerciseId = originalExerciseId,
//                        exerciseIndex = i,
//                        type = OptionType.Direct,
//                        estimatedCost = CalculateDirectCost(originalExerciseId, state)
//                    });
//                }

//                // אופציה 2: תרגילים חילופיים
//                var alternatives = GetAlternativeExercises(state.lastExerciseId, originalExerciseId, GetNextExerciseId(exerciseOrder, state.mask, i));
//                foreach (var altId in alternatives)
//                {
//                    if (CanExecuteDirectly(altId, state))
//                    {
//                        options.Add(new ExerciseOption
//                        {
//                            exerciseId = altId,
//                            exerciseIndex = i,
//                            type = OptionType.Alternative,
//                            estimatedCost = CalculateAlternativeCost(altId, state)
//                        });
//                    }
//                }

//                // אופציה 3: סידור מחדש של מתאמנים אחרים
//                var reschedulingOptions = GenerateReschedulingOptions(originalExerciseId, state);
//                options.AddRange(reschedulingOptions);
//            }

//            // מיון לפי עלות משוערת (הטובים ביותר ראשונים)
//            return options.OrderBy(o => o.estimatedCost).ToList();
//        }

//        private double CalculateSolutionCost(SearchState state)
//        {
//            // משקלים לחישוב עלות
//            const double ALTERNATIVE_WEIGHT = 1.0;
//            const double RESCHEDULING_WEIGHT = 2.0;
//            const double TIME_WEIGHT = 0.001; // עלות זמן נמוכה יחסית

//            double cost = 0;
//            cost += state.alternativesCount * ALTERNATIVE_WEIGHT;
//            cost += state.reschedulingsCount * RESCHEDULING_WEIGHT;
//            cost += (state.currentTime.Ticks / TimeSpan.TicksPerMinute) * TIME_WEIGHT;

//            return cost;
//        }

//        private double CalculateDirectCost(int exerciseId, SearchState state)
//        {
//            // עלות נמוכה לתרגיל ישיר
//            return GetExerciseDuration(exerciseId).TotalMinutes * 0.001;
//        }

//        private double CalculateAlternativeCost(int exerciseId, SearchState state)
//        {
//            // עלות בינונית לתרגיל חילופי
//            return 1.0 + GetExerciseDuration(exerciseId).TotalMinutes * 0.001;
//        }

//        private bool CanExecuteDirectly(int exerciseId, SearchState state)
//        {
//            return IsLegalTransition(state.lastExerciseId, exerciseId, state.currentTime) &&
//                   CanReachAllRemainingExercisesFromState(exerciseId, state) &&
//                   IsSlotAvailable(exerciseId, state.currentTime);
//        }

//        private bool CanReachAllRemainingExercisesFromState(int currentNodeId, SearchState state)
//        {
//            if (!reachabilityCache.TryGetValue(currentNodeId, out var reachable))
//                return false;

//            // בדוק את כל התרגילים שטרם בוצעו
//            for (int i = 0; i < exerciseCount; i++)
//            {
//                if (IsExerciseDone(state.mask, i)) continue;

//                int exerciseId = exercises[i].ExerciseId;
//                if (exerciseId != currentNodeId && !reachable.Contains(exerciseId))
//                    return false;
//            }

//            return true;
//        }
//        #endregion

//        #region ניהול מצב ואופציות
//        private class SearchState
//        {
//            public int mask;
//            public List<int> currentPath;
//            public DateTime currentTime;
//            public int lastExerciseId;
//            public int alternativesCount;
//            public int reschedulingsCount;
//        }

//        private class ExerciseOption
//        {
//            public int exerciseId;
//            public int exerciseIndex;
//            public OptionType type;
//            public double estimatedCost;
//            public TraineeDTO traineeToReschedule; // עבור rescheduling
//            public List<int> newScheduleForTrainee; // עבור rescheduling
//        }

//        private enum OptionType
//        {
//            Direct,
//            Alternative,
//            Rescheduling
//        }

//        private class OptimalSolution
//        {
//            public bool found;
//            public List<int> path;
//            public DateTime endTime;
//            public int alternativesCount;
//            public int reschedulingsCount;
//            public double totalCost;
//        }

//        private SearchState ApplyOption(ExerciseOption option, SearchState currentState)
//        {
//            var newState = new SearchState
//            {
//                mask = MarkExerciseDone(currentState.mask, option.exerciseIndex),
//                currentPath = new List<int>(currentState.currentPath),
//                currentTime = currentState.currentTime.Add(GetExerciseDuration(option.exerciseId)),
//                lastExerciseId = option.exerciseId,
//                alternativesCount = currentState.alternativesCount + (option.type == OptionType.Alternative ? 1 : 0),
//                reschedulingsCount = currentState.reschedulingsCount + (option.type == OptionType.Rescheduling ? 1 : 0)
//            };

//            newState.currentPath.Add(option.exerciseId);
//            return newState;
//        }

//        private void ExecuteStateChange(ExerciseOption option, SearchState newState)
//        {
//            // הוסף מתאמן לסלוט
//            var duration = GetExerciseDuration(option.exerciseId);
//            AddTraineeToSlot(option.exerciseId, newState.currentTime.Subtract(duration), duration, currentTrainee);

//            // עדכן מצב תרגיל
//            MarkExerciseAsDone(currentTrainee, option.exerciseId, newState.currentTime.Subtract(duration));

//            // אם זה rescheduling, בצע את השינוי
//            if (option.type == OptionType.Rescheduling && option.traineeToReschedule != null)
//            {
//                ApplyNewExerciseOrderToTrainee(option.traineeToReschedule, option.newScheduleForTrainee, newState.currentTime);
//            }
//        }

//        private void UndoStateChange(ExerciseOption option, SearchState originalState)
//        {
//            // הסר מתאמן מסלוט
//            var duration = GetExerciseDuration(option.exerciseId);
//            RemoveTraineeFromSlot(option.exerciseId, originalState.currentTime, duration);

//            // בטל עדכון מצב תרגיל
//            UndoMarkExerciseAsDone(currentTrainee, option.exerciseId);

//            // אם זה rescheduling, בטל את השינוי
//            if (option.type == OptionType.Rescheduling && option.traineeToReschedule != null)
//            {
//                UndoApplyNewExerciseOrderToTrainee(option.traineeToReschedule, originalState.currentTime);
//            }
//        }

//        private string CreateMemoKey(SearchState state)
//        {
//            // מפתח מטמון שלוקח בחשבון את כל המשתנים הרלוונטיים
//            return $"{state.mask}_{state.lastExerciseId}_{state.currentTime.Ticks / TimeSpan.TicksPerMinute}_{GetSlotStateHash()}";
//        }

//        private int GetSlotStateHash()
//        {
//            // יצירת hash של מצב הסלוטים הנוכחי
//            int hash = 0;
//            foreach (var kvp in queueSlots)
//            {
//                foreach (var slot in kvp.Value.SlotsByStartTime.Values)
//                {
//                    hash ^= slot.ExercisesByTrainee.Count.GetHashCode();
//                }
//            }
//            return hash;
//        }
//        #endregion

//        #region פונקציות עזר משופרות
//        private List<ExerciseOption> GenerateReschedulingOptions(int desiredExerciseId, SearchState state)
//        {
//            var options = new List<ExerciseOption>();

//            var occupyingTrainee = GetOccupyingTraineeInSlot(desiredExerciseId, state.currentTime);
//            if (occupyingTrainee != null && occupyingTrainee.TraineeId != currentTrainee.TraineeId)
//            {
//                var remainingExercises = GetRemainingExercisesForTrainee(occupyingTrainee, state.currentTime);

//                // נסה למצוא סדר חדש עבור המתאמן החוסם
//                var alternativeSchedule = FindAlternativeScheduleForTrainee(occupyingTrainee, remainingExercises, desiredExerciseId, state.currentTime);

//                if (alternativeSchedule != null)
//                {
//                    options.Add(new ExerciseOption
//                    {
//                        exerciseId = desiredExerciseId,
//                        exerciseIndex = GetOriginalExerciseIndex(desiredExerciseId, state),
//                        type = OptionType.Rescheduling,
//                        traineeToReschedule = occupyingTrainee,
//                        newScheduleForTrainee = alternativeSchedule,
//                        estimatedCost = 2.0 + GetExerciseDuration(desiredExerciseId).TotalMinutes * 0.001
//                    });
//                }
//            }

//            return options;
//        }

//        private List<int> FindAlternativeScheduleForTrainee(TraineeDTO trainee, List<int> remainingExercises, int excludeExerciseId, DateTime excludeTime)
//        {
//            // אלגוריתם פשוט למציאת סדר חלופי
//            // כאן ניתן להשתמש באלגוריתם greedy או backtracking מקוצר

//            var schedule = new List<int>();
//            var availableExercises = new List<int>(remainingExercises);
//            var currentTime = excludeTime;

//            while (availableExercises.Count > 0)
//            {
//                int bestExercise = -1;
//                DateTime bestTime = DateTime.MaxValue;

//                foreach (var exId in availableExercises)
//                {
//                    if (exId == excludeExerciseId && currentTime == excludeTime)
//                        continue; // דלג על התרגיל החוסם בזמן החסום

//                    var nextAvailableTime = FindNextAvailableSlot(exId, currentTime);
//                    if (nextAvailableTime < bestTime)
//                    {
//                        bestTime = nextAvailableTime;
//                        bestExercise = exId;
//                    }
//                }

//                if (bestExercise == -1) break; // לא מצאנו פתרון

//                schedule.Add(bestExercise);
//                availableExercises.Remove(bestExercise);
//                currentTime = bestTime.Add(GetExerciseDuration(bestExercise));
//            }

//            return availableExercises.Count == 0 ? schedule : null; // החזר null אם לא הצלחנו לתזמן הכל
//        }

//        private DateTime FindNextAvailableSlot(int exerciseId, DateTime fromTime)
//        {
//            int exerciseIndex = GetExerciseIndex(exerciseId);
//            if (exerciseIndex < 0) return DateTime.MaxValue;

//            var queueSlot = queueSlots[exerciseIndex];
//            var duration = GetExerciseDuration(exerciseId);
//            var slotsNeeded = GetSlotsNeeded(duration);

//            foreach (var kvp in queueSlot.SlotsByStartTime.Where(s => s.Key >= fromTime).OrderBy(s => s.Key))
//            {
//                if (CanFitInSlots(exerciseId, kvp.Key, slotsNeeded))
//                {
//                    return kvp.Key;
//                }
//            }

//            return DateTime.MaxValue;
//        }

//        private bool CanFitInSlots(int exerciseId, DateTime startTime, int slotsNeeded)
//        {
//            int exerciseIndex = GetExerciseIndex(exerciseId);
//            var queueSlot = queueSlots[exerciseIndex];

//            var currentSlotTime = startTime;
//            for (int i = 0; i < slotsNeeded; i++)
//            {
//                if (!queueSlot.SlotsByStartTime.TryGetValue(currentSlotTime, out var slot) ||
//                    slot.ExercisesByTrainee.Count >= queueSlot.EquipmentCount)
//                {
//                    return false;
//                }
//                currentSlotTime = slot.EndTime;
//            }

//            return true;
//        }

//        private int GetOriginalExerciseIndex(int exerciseId, SearchState state)
//        {
//            // מצא את האינדקס המקורי של התרגיל ברשימת התרגילים
//            for (int i = 0; i < exercises.Count; i++)
//            {
//                if (exercises[i].ExerciseId == exerciseId)
//                    return i;
//            }
//            return -1;
//        }

//        private bool IsValidOption(ExerciseOption option, List<int> exerciseOrder, SearchState state)
//        {
//            return IsLegalTransition(state.lastExerciseId, option.exerciseId, state.currentTime) &&
//                   CanReachAllRemainingExercisesFromState(option.exerciseId, state);
//        }
//        #endregion

//        #region פונקציות עזר קיימות (נשמרות כמו שהן)
//        private bool IsExerciseDone(int mask, int exerciseIndex)
//        {
//            return (mask & (1 << exerciseIndex)) != 0;
//        }

//        private int MarkExerciseDone(int mask, int exerciseIndex)
//        {
//            return mask | (1 << exerciseIndex);
//        }

//        private bool IsLegalTransition(int fromNodeId, int toNodeId, DateTime currentTime)
//        {
//            if (fromNodeId == -1) return true;

//            int fromIdx = GetExerciseIndex(fromNodeId);
//            int toIdx = GetExerciseIndex(toNodeId);

//            if (fromIdx < 0 || toIdx < 0) return false;

//            return transitionMatrix[toIdx, fromIdx].LegalityValue > 0;
//        }

//        private int GetExerciseIndex(int exerciseId)
//        {
//            return exerciseIdToIndex.TryGetValue(exerciseId, out int index) ? index : -1;
//        }

//        private TimeSpan GetExerciseDuration(int exerciseId)
//        {
//            var exercise = exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
//            return exercise?.Duration ?? TimeSpan.FromMinutes(15);
//        }

//        // שאר הפונקציות נשארות כמו בקוד המקורי...
//        #endregion

//        #region אסטרטגיות חיפוש

//        private (bool found, int numAlternatives, List<int> bestPath, DateTime endTime)
//            TryRegularExercises(List<int> exerciseOrder, int mask, List<int> currentPath,
//                              int currentAlternatives, DateTime currentTime, int lastNodeId)
//        {
//            bool foundAny = false;
//            int minAlternatives = int.MaxValue;
//            List<int> bestPath = null;
//            DateTime bestEndTime = DateTime.MinValue;

//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                if (IsExerciseDone(mask, i)) continue;

//                int nodeId = exerciseOrder[i];

//                // בדיקות חוקיות
//                if (!IsLegalTransition(lastNodeId, nodeId, currentTime)) continue;
//                if (!CanReachAllRemainingExercises(nodeId, exerciseOrder, mask)) continue;

//                // בדיקת זמינות
//                if (IsSlotAvailable(nodeId, currentTime))
//                {
//                    var duration = GetExerciseDuration(nodeId);
//                    AddTraineeToSlot(nodeId, currentTime, duration, currentTrainee);

//                    currentPath.Add(nodeId);
//                    // לפני/אחרי הוספת התרגיל ל־currentPath
//                    MarkExerciseAsDone(currentTrainee, nodeId, currentTime);

//                    var nextTime = currentTime.Add(duration);

//                    var result = BacktrackWithPriority(exerciseOrder, MarkExerciseDone(mask, i),
//                                                     currentPath, currentAlternatives, nextTime,
//                                                     out DateTime candidateEndTime);

//                    //if (result.found)
//                    //{
//                    //    return (true, result.numAlternatives, result.bestPath, candidateEndTime);
//                    //}
//                    if (result.found)
//                    {
//                        foundAny = true;
//                        if (result.numAlternatives < minAlternatives)
//                        {
//                            minAlternatives = result.numAlternatives;
//                            bestPath = new List<int>(result.bestPath);
//                            bestEndTime = candidateEndTime;
//                        }
//                    }

//                    // נקה אחרי כשלון
//                    RemoveTraineeFromSlot(nodeId, currentTime, duration);
//                    currentPath.RemoveAt(currentPath.Count - 1);
//                    UndoMarkExerciseAsDone(currentTrainee, nodeId); // <--- הוסף שורה זו!
//                }
//            }

//            return (foundAny, minAlternatives, bestPath, bestEndTime);
//            //return (false, int.MaxValue, null, DateTime.MinValue);
//        }

//        private (bool found, int numAlternatives, List<int> bestPath, DateTime endTime)
//            TryAlternativeExercises(List<int> exerciseOrder, int mask, List<int> currentPath,
//                                  int currentAlternatives, DateTime currentTime, int lastNodeId)
//        {
//            bool foundAny = false;
//            int minAlternatives = int.MaxValue;
//            List<int> bestPath = null;
//            DateTime bestEndTime = DateTime.MinValue;

//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                if (IsExerciseDone(mask, i)) continue;

//                int originalNodeId = exerciseOrder[i];
//                // נזהה את התרגיל הבא (אם יש)
//                int nextIdx = -1;
//                for (int j = i + 1; j < exerciseOrder.Count; j++)
//                {
//                    if (!IsExerciseDone(mask, j))
//                    {
//                        nextIdx = exerciseOrder[j];
//                        break;
//                    }
//                }
//                // אלטרנטיבות לפי הפונקציה החדשה:
//                var alternatives = GetAlternativeExercises(lastNodeId, originalNodeId, nextIdx);
//                //var alternatives = GetAlternativeExercises(originalNodeId);//לא בטוח

//                foreach (int altNodeId in alternatives)
//                {
//                    if (!IsLegalTransition(lastNodeId, altNodeId, currentTime)) continue;
//                    if (!CanReachAllRemainingExercises(altNodeId, exerciseOrder, mask)) continue;

//                    if (IsSlotAvailable(altNodeId, currentTime))
//                    {
//                        var duration = GetExerciseDuration(altNodeId);
//                        AddTraineeToSlot(altNodeId, currentTime, duration, currentTrainee);

//                        currentPath.Add(altNodeId);
//                        var nextTime = currentTime.Add(duration);
//                        // לפני/אחרי הוספת התרגיל ל־currentPath
//                        MarkExerciseAsDone(currentTrainee, altNodeId, currentTime);

//                        var result = BacktrackWithPriority(exerciseOrder, MarkExerciseDone(mask, i),
//                                                         currentPath, currentAlternatives + 1, nextTime,
//                                                         out DateTime candidateEndTime);

//                        //if (result.found)
//                        //{
//                        //    return (true, result.numAlternatives, result.bestPath, candidateEndTime);
//                        //}
//                        if (result.found)
//                        {
//                            foundAny = true;
//                            if (result.numAlternatives < minAlternatives)
//                            {
//                                minAlternatives = result.numAlternatives;
//                                bestPath = new List<int>(result.bestPath);
//                                bestEndTime = candidateEndTime;
//                            }
//                        }


//                        // נקה אחרי כשלון
//                        RemoveTraineeFromSlot(altNodeId, currentTime, duration);
//                        currentPath.RemoveAt(currentPath.Count - 1);
//                        UndoMarkExerciseAsDone(currentTrainee, altNodeId); // <--- הוסף שורה זו!
//                    }
//                }
//            }
//            return (foundAny, minAlternatives, bestPath, bestEndTime);
//            // return (false, int.MaxValue, null, DateTime.MinValue);
//        }

//        private (bool found, int numAlternatives, List<int> bestPath, DateTime endTime)
//            TryReschedulingOthers(List<int> exerciseOrder, int mask, List<int> currentPath,
//                                 int currentAlternatives, DateTime currentTime, int lastNodeId)
//        {
//            bool foundAny = false;
//            int minAlternatives = int.MaxValue;
//            List<int> bestPath = null;
//            DateTime bestEndTime = DateTime.MinValue;

//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                if (IsExerciseDone(mask, i)) continue;

//                int nodeId = exerciseOrder[i];

//                var alternatives = new List<int> { nodeId };
//                int nextNodeId = GetNextExerciseId(exerciseOrder, mask, i);
//                alternatives.AddRange(GetAlternativeExercises(lastNodeId, nodeId, nextNodeId));

//                foreach (var alt in alternatives)
//                {
//                    var occupyingTrainee = GetOccupyingTraineeInSlot(alt, currentTime);
//                    if (occupyingTrainee != null && occupyingTrainee.TraineeId != currentTrainee.TraineeId)
//                    {
//                        var othersRemainingExercises = GetRemainingExercisesForTrainee(occupyingTrainee, currentTime);

//                        // קריאה לאלגוריתם הראשי עבור המתאמן השני, עם דגל שמונע ממנו להזיז אחרים
//                        var result = BacktrackWithPriority(
//                            othersRemainingExercises,
//                            0,
//                            new List<int>(),
//                            0,
//                            currentTime,
//                            out DateTime _,
//                            isReschedulingAnotherTrainee: true
//                        );

//                        if (result.found)
//                        {
//                            ApplyNewExerciseOrderToTrainee(occupyingTrainee, result.bestPath, currentTime);

//                            var duration = GetExerciseDuration(alt);
//                            AddTraineeToSlot(alt, currentTime, duration, currentTrainee);
//                            currentPath.Add(alt);
//                            // לפני/אחרי הוספת התרגיל ל־currentPath
//                            MarkExerciseAsDone(currentTrainee, nodeId, currentTime);

//                            var res = BacktrackWithPriority(exerciseOrder, MarkExerciseDone(mask, i),
//                                                         currentPath, currentAlternatives + 2,
//                                                         currentTime.Add(duration),
//                                                         out DateTime candidateEndTime);

//                            //if (res.found)
//                            //    return (true, res.numAlternatives, res.bestPath, candidateEndTime);
//                            if (res.found)
//                            {
//                                foundAny = true;
//                                if (result.numAlternatives < minAlternatives)
//                                {
//                                    minAlternatives = result.numAlternatives;
//                                    bestPath = new List<int>(result.bestPath);
//                                    bestEndTime = candidateEndTime;
//                                }
//                            }

//                            RemoveTraineeFromSlot(alt, currentTime, duration);
//                            currentPath.RemoveAt(currentPath.Count - 1);
//                            UndoApplyNewExerciseOrderToTrainee(occupyingTrainee, currentTime);
//                            UndoMarkExerciseAsDone(currentTrainee, nodeId); // <--- הוסף שורה זו!
//                        }
//                    }
//                }
//            }
//            return (foundAny, minAlternatives, bestPath, bestEndTime);
//            //return (false, int.MaxValue, null, DateTime.MinValue);
//        }

//        #endregion

//        #region פונקציות עזר - בדיקות חוקיות

//        private bool CanReachAllRemainingExercises(int currentNodeId, List<int> exerciseOrder, int mask)
//        {
//            if (!reachabilityCache.TryGetValue(currentNodeId, out var reachable))
//                return false;

//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                if (IsExerciseDone(mask, i)) continue;

//                int exerciseId = exerciseOrder[i];
//                if (exerciseId != currentNodeId && !reachable.Contains(exerciseId))
//                    return false;
//            }

//            return true;
//        }

//        #endregion

//        #region פונקציות עזר - ניהול סלוטים

//        private bool IsSlotAvailable(int exerciseId, DateTime startTime)
//        {
//            int exerciseIndex = GetExerciseIndex(exerciseId);
//            if (exerciseIndex < 0) return false;

//            var queueSlot = queueSlots[exerciseIndex];
//            if (!queueSlot.SlotsByStartTime.TryGetValue(startTime, out var slot))
//                return false;

//            var duration = GetExerciseDuration(exerciseId);
//            var slotsNeeded = GetSlotsNeeded(duration);

//            // בדוק זמינות של כל הסלוטים הנדרשים
//            var currentSlotTime = startTime;
//            for (int i = 0; i < slotsNeeded; i++)
//            {
//                if (!queueSlot.SlotsByStartTime.TryGetValue(currentSlotTime, out var checkSlot) ||
//                    checkSlot.ExercisesByTrainee.Count >= queueSlot.EquipmentCount)
//                {
//                    return false;
//                }
//                currentSlotTime = checkSlot.EndTime;
//            }

//            return true;
//        }

//        private void AddTraineeToSlot(int exerciseId, DateTime startTime, TimeSpan duration, TraineeDTO trainee)
//        {
//            int exerciseIndex = GetExerciseIndex(exerciseId);
//            var queueSlot = queueSlots[exerciseIndex];
//            var slotsNeeded = GetSlotsNeeded(duration);

//            queueSlot.AddTraineeToSlot(startTime, slotsNeeded, trainee);
//        }

//        private void RemoveTraineeFromSlot(int exerciseId, DateTime startTime, TimeSpan duration)
//        {
//            int exerciseIndex = GetExerciseIndex(exerciseId);
//            var queueSlot = queueSlots[exerciseIndex];
//            var slotsNeeded = GetSlotsNeeded(duration);

//            var currentSlotTime = startTime;
//            for (int i = 0; i < slotsNeeded; i++)
//            {
//                if (queueSlot.SlotsByStartTime.TryGetValue(currentSlotTime, out var slot))
//                {
//                    var traineeToRemove = slot.ExercisesByTrainee.Keys.FirstOrDefault(t => t.TraineeId == currentTrainee.TraineeId);
//                    if (traineeToRemove != null)
//                    {
//                        slot.ExercisesByTrainee.Remove(traineeToRemove);
//                    }
//                }
//                currentSlotTime = slot?.EndTime ?? currentSlotTime.AddMinutes(5); // ברירת מחדל
//            }
//        }

//        private TraineeDTO GetOccupyingTraineeInSlot(int exerciseId, DateTime startTime)
//        {
//            int exerciseIndex = GetExerciseIndex(exerciseId);
//            var queueSlot = queueSlots[exerciseIndex];

//            if (queueSlot.SlotsByStartTime.TryGetValue(startTime, out var slot))
//            {
//                return slot.ExercisesByTrainee.Keys.FirstOrDefault();
//            }

//            return null;
//        }

//        private int GetSlotsNeeded(TimeSpan duration)
//        {
//            return (int)Math.Ceiling(duration.TotalMinutes / 5.0); // נניח שכל סלוט הוא 5 דקות
//        }

//        #endregion

//        #region פונקציות עזר - תרגילים חילופיים

//        private List<int> GetAlternativeExercises(int prevExerciseId, int currentExerciseId, int nextExerciseId)
//        {
//            var alternatives = new List<int>();

//            int prevIdx = GetExerciseIndex(prevExerciseId);
//            int currIdx = GetExerciseIndex(currentExerciseId);
//            int nextIdx = GetExerciseIndex(nextExerciseId);

//            if (prevIdx < 0 || currIdx < 0 || nextIdx < 0)
//                return alternatives;

//            // הערך שצריך לחפש
//            int referenceValue = transitionMatrix[currIdx, prevIdx].LegalityValue;

//            var possibleAlternatives = new List<int>();
//            // שלב א: מציאת כל האלטרנטיביים בשורה של התרגיל הקודם שיש להם את אותו ערך כמו המשבצת המקורית
//            for (int rowIdx = 0; rowIdx < exerciseCount; rowIdx++)
//            {
//                if (rowIdx == currIdx) continue; // לא התרגיל הנוכחי
//                if (transitionMatrix[rowIdx, prevIdx].LegalityValue == referenceValue)
//                {
//                    possibleAlternatives.Add(rowIdx);
//                }
//            }

//            // שלב ב: עבור כל האפשריים, בדוק אם בתרגיל הבא (בשורה שלו) יש ערך חוקי (>0 או שונה מ-(-1) ו-0)
//            foreach (int altIdx in possibleAlternatives)
//            {
//                int transitionValue = transitionMatrix[nextIdx, altIdx].LegalityValue;
//                if (transitionValue != 0 && transitionValue != -1)
//                {
//                    alternatives.Add(exercises[altIdx].ExerciseId);
//                }
//            }

//            return alternatives;
//        }

//        #endregion

//        #region פונקציות עזר - סידור מחדש של מתאמנים

//        private int GetNextExerciseId(List<int> exerciseOrder, int mask, int currentIdx)
//        {
//            for (int j = currentIdx + 1; j < exerciseOrder.Count; j++)
//            {
//                if (!IsExerciseDone(mask, j))
//                    return exerciseOrder[j];
//            }
//            return -1;
//        }
//        private List<int> GetRemainingExercisesForTrainee(TraineeDTO trainee, DateTime currentTime)
//        {
//            if (!traineesExerciseStatus.TryGetValue(trainee.TraineeId, out TraineeExerciseStatus status))
//                return new List<int>();
//            // בחר רק תרגילים שלא בוצעו
//            return status.Exercises
//                .Where(e => !e.IsDone)
//                .OrderBy(e => e.OrderInList)
//                .Select(e => e.ExerciseId)
//                .ToList();
//        }
//        private void ApplyNewExerciseOrderToTrainee(TraineeDTO trainee, List<int> newOrder, DateTime currentTime)
//        {
//            if (!traineesExerciseStatus.TryGetValue(trainee.TraineeId, out TraineeExerciseStatus status))
//                return;

//            // שמור מצב קודם ל-Undo
//            status.History.Push(status.Exercises.Select(e => new ExerciseStatusEntry
//            {
//                ExerciseId = e.ExerciseId,
//                OrderInList = e.OrderInList,
//                IsDone = e.IsDone,
//                PerformedAt = e.PerformedAt
//            }).ToList());

//            // צור סדר חדש - שומר על אותם IsDone/PerformedAt לפי ExerciseId
//            var dictById = status.Exercises.ToDictionary(e => e.ExerciseId);
//            status.Exercises = newOrder
//                .Select((exId, idx) =>
//                    dictById.ContainsKey(exId)
//                        ? new ExerciseStatusEntry
//                        {
//                            ExerciseId = exId,
//                            OrderInList = idx,
//                            IsDone = dictById[exId].IsDone,
//                            PerformedAt = dictById[exId].PerformedAt
//                        }
//                        : new ExerciseStatusEntry
//                        {
//                            ExerciseId = exId,
//                            OrderInList = idx,
//                            IsDone = false,
//                            PerformedAt = null
//                        }
//                ).ToList();
//        }
//        private void UndoApplyNewExerciseOrderToTrainee(TraineeDTO trainee, DateTime currentTime)
//        {
//            if (!traineesExerciseStatus.TryGetValue(trainee.TraineeId, out TraineeExerciseStatus status))
//                return;

//            if (status.History.Count > 0)
//                status.Exercises = status.History.Pop();
//        }
//        #endregion

//        #region פונקציות עזר - כלליות
//        private void UndoMarkExerciseAsDone(TraineeDTO trainee, int exerciseId)
//        {
//            if (!traineesExerciseStatus.TryGetValue(trainee.TraineeId, out TraineeExerciseStatus status))
//                return;

//            var ex = status.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
//            if (ex != null)
//            {
//                ex.IsDone = false;
//                ex.PerformedAt = null;
//            }
//        }
//        private void MarkExerciseAsDone(TraineeDTO trainee, int exerciseId, DateTime performedAt)
//        {
//            if (!traineesExerciseStatus.TryGetValue(trainee.TraineeId, out TraineeExerciseStatus status))
//                return;

//            var ex = status.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
//            if (ex != null)
//            {
//                ex.IsDone = true;
//                ex.PerformedAt = performedAt;
//            }
//        }
//        private Dictionary<int, ExerciseEntry> CreateExerciseEntries(List<int> exercisePath)
//        {
//            var entries = new Dictionary<int, ExerciseEntry>();

//            for (int i = 0; i < exercisePath.Count; i++)
//            {
//                int exerciseId = exercisePath[i];
//                int exerciseIndex = GetExerciseIndex(exerciseId);

//                entries[i] = new ExerciseEntry
//                {
//                    ExerciseId = exerciseId,
//                    OrderInList = i,
//                    Slot = exerciseIndex >= 0 ? queueSlots[exerciseIndex] : null
//                };
//            }

//            return entries;
//        }

//        #endregion

//    }
//}
