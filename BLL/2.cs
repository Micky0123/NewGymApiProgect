//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//namespace BLL
//{
//    public class _2
//    {
//        // מחלקות עזר שחסרות בקוד המקורי
//        public class Trainee
//        {
//            public int Id { get; set; }
//            public string Name { get; set; }
//            public bool HasStartedTraining { get; set; }
//            public DateTime StartTime { get; set; }
//            public List<int> PlannedExercises { get; set; } = new List<int>();
//        }

//        public class ExerciseNode : Node
//        {
//            public MuscleGroup MuscleGroup { get; set; }
//            public int UsageDurationMinutes { get; set; }
//            public List<ExerciseNode> Neighbors { get; set; } = new List<ExerciseNode>();
//        }

//        public class Node
//        {
//            public int Id { get; set; }
//            public string Name { get; set; }
//            public int UsageDurationMinutes { get; set; } = 15; // ברירת מחדל
//        }

//        public class SubGraph
//        {
//            public List<Node> Nodes { get; set; } = new List<Node>();
//            public DateTime StartTime { get; set; }
//            public DateTime EndTime { get; set; }
//        }

//        public enum MuscleGroup
//        {
//            Chest = 0,    // חזה
//            Back = 1,     // גב
//            Shoulders = 2, // כתפיים
//            Arms = 3,     // זרועות
//            Legs = 4,     // רגליים
//            Core = 5,     // ליבה
//            Cardio = 6    // אירובי
//        }
//        // טבלת תלת-ממד: [תרגיל יעד, תרגיל מקור, סלוט זמן]
//        private TableCell[,,] Table3D;
//        private int exerciseCount;
//        private int timeSlotsCount;
//        private int slotDurationMinutes = 5; // ברירת מחדל: כל סלוט הוא 5 דקות
//        private List<ExerciseNode> exercises; // שמירת רשימת התרגילים
//        private Trainee currentTrainee; // המתאמן הנוכחי



//        // מחלקה פנימית לתוצאה - מייצגת מסלול תרגילים עם זמן התחלה וסיום
//        public class PathResult
//        {
//            public List<int> ExerciseIdsInPath { get; set; } = new();
//            public DateTime StartTime { get; set; }
//            public DateTime EndTime { get; set; }
//            public int AlternativesUsed { get; set; } = 0; // מספר החילופים שנעשו
//        }

//        // מילון לתכנון דינאמי - שומר תוצאות של מצבים שכבר חושבו (עבור memoization)
//        private Dictionary<(int mask, int lastNodeId, long timeMinutes), (bool found, int numAlternatives, List<int> bestPath, DateTime endTime)> memo = new();

//        // תא טבלה: מייצג מעבר מתרגיל לתרגיל בשעת סלוט מסוימת
//        public class TableCell
//        {
//            public int LegalityValue { get; set; } // לדוג' 2^שריר, -1, 0
//            public Queue<Trainee> Queue { get; set; } = new Queue<Trainee>();

//            // פונקציית עזר לבדיקה האם המשבצת פנויה בזמן מסוים
//            public bool IsAvailable(int maxInQueue = 1)
//            {
//                return Queue.Count < maxInQueue;
//            }

//            // פונקציית עזר להוספת מתאמן לתור
//            public void AddTrainee(Trainee trainee)
//            {
//                Queue.Enqueue(trainee);
//            }

//            // פונקציית עזר להסרת מתאמן (למשל לצורך ביטול שינוי)
//            public Trainee RemoveTrainee()
//            {
//                if (Queue.Count > 0)
//                    return Queue.Dequeue();
//                return null;
//            }

//            // פונקציית עזר לבדיקה מי המתאמן הנוכחי בתור
//            public Trainee GetCurrentTrainee()
//            {
//                if (Queue.Count > 0)
//                    return Queue.Peek();
//                return null;
//            }
//        }

//        // במקום לקרוא לפונקציה HasPathToNextExercise בכל פעם
//        private bool IsLegalTransition(int fromExerciseId, int toExerciseId, int timeSlot)
//        {
//            // אם זה אותו תרגיל, החזר false
//            if (fromExerciseId == toExerciseId)
//                return false;

//            int fromIdx = GetExerciseIndex(fromExerciseId);
//            int toIdx = GetExerciseIndex(toExerciseId);

//            // בדוק תקינות האינדקסים
//            if (fromIdx < 0 || toIdx < 0 || timeSlot < 0 || timeSlot >= timeSlotsCount)
//                return false;

//            // בדוק ישירות מהטבלה התלת-ממדית
//            return Table3D[toIdx, fromIdx, timeSlot].LegalityValue > 0;
//        }

//        // אופטימיזציה של אתחול הטבלה - יצירה יעילה יותר
//        public void InitTable(List<ExerciseNode> exerciseList, int slotsCount)
//        {
//            exercises = exerciseList ?? throw new ArgumentNullException(nameof(exerciseList));
//            exerciseCount = exercises.Count;
//            timeSlotsCount = slotsCount;
//            Table3D = new TableCell[exerciseCount, exerciseCount, timeSlotsCount];

//            // אתחול מילון מיידי מזהה -> אינדקס
//            exerciseIdToIndex = new Dictionary<int, int>();
//            for (int i = 0; i < exercises.Count; i++)
//            {
//                exerciseIdToIndex[exercises[i].Id] = i;
//            }

//            // חישוב מטריצת החוקיות פעם אחת בלבד
//            int[,] legalityMatrix = new int[exerciseCount, exerciseCount];

//            for (int to = 0; to < exerciseCount; to++)
//            {
//                for (int from = 0; from < exerciseCount; from++)
//                {
//                    if (to == from)
//                        legalityMatrix[to, from] = 0; // אותו תרגיל
//                    else
//                    {
//                        // בדוק אם יש קשר ישיר בין התרגילים
//                        bool hasDirectPath = exercises[from].Neighbors?.Contains(exercises[to]) ?? false;

//                        // אם יש קשר ישיר, נשמור את קבוצת השריר
//                        legalityMatrix[to, from] = hasDirectPath ?
//                            (int)Math.Pow(2, (int)exercises[to].MuscleGroup) : -1;
//                    }
//                }
//            }

//            // יצירת המשבצות בטבלה התלת-ממדית
//            for (int to = 0; to < exerciseCount; to++)
//            {
//                for (int from = 0; from < exerciseCount; from++)
//                {
//                    int legality = legalityMatrix[to, from];

//                    // אתחול לכל סלוט זמן
//                    for (int slot = 0; slot < timeSlotsCount; slot++)
//                    {
//                        Table3D[to, from, slot] = new TableCell { LegalityValue = legality };
//                    }
//                }
//            }
//        }

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

//                    // בדוק ישירות מהטבלה אם המעבר חוקי
//                    int fromIdx = GetExerciseIndex(nodeA.Id);
//                    int toIdx = GetExerciseIndex(nodeB.Id);

//                    if (fromIdx >= 0 && toIdx >= 0 && fromIdx < exerciseCount && toIdx < exerciseCount &&
//                        Table3D[toIdx, fromIdx, 0].LegalityValue > 0)
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

//        // אלגוריתם Backtracking מתוקן עם כל החלקים החסרים
//        private (bool found, List<int> bestPath, DateTime endTime) BacktrackWithPriority(
//            SubGraph graph,
//            Dictionary<int, Node> nodeDict,
//            Dictionary<int, HashSet<int>> reachableFromNode,
//            List<int> exerciseOrder,
//            Dictionary<int, List<int>> alternativeDevices,
//            int mask,
//            List<int> currentPath,
//            int currentAlternatives,
//            DateTime currentTime,
//            out DateTime endTime
//            )
//        {
//            int lastNodeId = currentPath.Count > 0 ? currentPath.Last() : -1;
//            int currentSlot = TimeToSlot(graph.StartTime, currentTime);

//            // מפתח למטמון
//            var memoKey = (mask, lastNodeId, currentTime.Ticks / TimeSpan.TicksPerMinute);

//            // בדיקה במטמון
//            if (memo.TryGetValue(memoKey, out var cachedResult))
//            {
//                endTime = cachedResult.endTime;
//                return (cachedResult.found, cachedResult.bestPath, cachedResult.endTime);
//            }

//            // בדיקת תנאי סיום: כל התרגילים בוצעו
//            if (mask == (1 << exerciseOrder.Count) - 1)
//            {
//                endTime = currentTime;
//                return (true, new List<int>(currentPath), currentTime);
//            }

//            bool foundAny = false;
//            List<int> bestPath = null;
//            DateTime bestEndTime = DateTime.MinValue;
//            int minAlternatives = int.MaxValue;

//            // אסטרטגיה 1: נסה לבצע תרגילים לפי סדר רגיל
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                // אם התרגיל כבר בוצע, דלג
//                if (IsExerciseDone(mask, i))
//                    continue;

//                int nodeId = exerciseOrder[i];
//                if (!nodeDict.TryGetValue(nodeId, out var node))
//                    continue;

//                // בדוק האם המעבר חוקי
//                if (lastNodeId != -1 && !IsLegalTransition(lastNodeId, nodeId, currentSlot))
//                    continue;

//                // בדוק זמינות
//                if (currentSlot < timeSlotsCount && IsAvailable(nodeId, lastNodeId, currentSlot))
//                {
//                    // "תפוס" את המכשיר זמנית
//                    AddTrainee(nodeId, lastNodeId, currentSlot, currentTrainee);

//                    // נסה להמשיך את המסלול
//                    currentPath.Add(nodeId);

//                    var result = BacktrackWithPriority(
//                        graph, nodeDict, reachableFromNode, exerciseOrder, alternativeDevices,
//                        MarkExerciseDone(mask, i), currentPath,
//                        currentAlternatives, currentTime.AddMinutes(node.UsageDurationMinutes),
//                        out DateTime candidateEndTime
//                    );

//                    if (result.found)
//                    {
//                        foundAny = true;
//                        bestPath = result.bestPath;
//                        bestEndTime = candidateEndTime;
//                        minAlternatives = currentAlternatives;

//                        // שחרר את המכשיר
//                        RemoveTrainee(nodeId, lastNodeId, currentSlot);

//                        // שמור את התוצאה ב-cache
//                        memo[memoKey] = (true, minAlternatives, bestPath, bestEndTime);

//                        endTime = bestEndTime;
//                        return (true, bestPath, bestEndTime);
//                    }

//                    // שחרר את המכשיר (ניסיון לא צלח)
//                    RemoveTrainee(nodeId, lastNodeId, currentSlot);
//                    currentPath.RemoveAt(currentPath.Count - 1);
//                }
//            }

//            // אסטרטגיה 2: נסה למצוא תרגילים חלופיים
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                int originalNodeId = exerciseOrder[i];

//                // דלג על תרגילים שכבר בוצעו
//                if (IsExerciseDone(mask, i))
//                    continue;

//                // אם יש מכשירים חלופיים לתרגיל זה
//                if (alternativeDevices.ContainsKey(originalNodeId))
//                {
//                    foreach (int alternativeNodeId in alternativeDevices[originalNodeId])
//                    {
//                        // וודא שהמכשיר החלופי קיים בגרף ושונה מהמקורי
//                        if (alternativeNodeId == originalNodeId || !nodeDict.ContainsKey(alternativeNodeId))
//                            continue;

//                        var alternativeNode = nodeDict[alternativeNodeId];

//                        // בדוק האם המעבר לתרגיל החלופי חוקי
//                        bool isLegalMove = lastNodeId == -1 || // אם זה התרגיל הראשון, תמיד חוקי
//                                           (reachableFromNode.ContainsKey(lastNodeId) &&
//                                            reachableFromNode[lastNodeId].Contains(alternativeNodeId));

//                        if (!isLegalMove)
//                            continue;

//                        if (currentSlot >= 0 && currentSlot < timeSlotsCount &&
//                            IsAvailable(alternativeNodeId, lastNodeId, currentSlot))
//                        {
//                            // תפוס את המכשיר החלופי
//                            AddTrainee(alternativeNodeId, lastNodeId, currentSlot, currentTrainee);

//                            // נסה להמשיך במסלול עם התרגיל החלופי
//                            currentPath.Add(alternativeNodeId);

//                            var nextTime = currentTime.AddMinutes(alternativeNode.UsageDurationMinutes);

//                            // שים לב: הגדלנו את מספר החילופים ב-1
//                            var result = BacktrackWithPriority(
//                                graph, nodeDict, reachableFromNode, exerciseOrder, alternativeDevices,
//                                MarkExerciseDone(mask, i), currentPath, currentAlternatives + 1,
//                                nextTime, out DateTime candidateEndTime
//                            );

//                            if (result.found)
//                            {
//                                if (!foundAny || currentAlternatives + 1 < minAlternatives)
//                                {
//                                    foundAny = true;
//                                    minAlternatives = currentAlternatives + 1;
//                                    bestPath = result.bestPath;
//                                    bestEndTime = candidateEndTime;
//                                }
//                            }

//                            // שחרר את המכשיר החלופי
//                            RemoveTrainee(alternativeNodeId, lastNodeId, currentSlot);
//                            currentPath.RemoveAt(currentPath.Count - 1);
//                        }
//                    }
//                }
//            }

//            // אסטרטגיה 3: נסה לשנות את הסדר של מתאמנים אחרים
//            if (!foundAny)
//            {
//                for (int i = 0; i < exerciseOrder.Count; i++)
//                {
//                    if (IsExerciseDone(mask, i))
//                        continue;

//                    int nodeId = exerciseOrder[i];
//                    if (!nodeDict.TryGetValue(nodeId, out var node))
//                        continue;

//                    // בדוק אם יש מתאמן אחר שתופס את המכשיר
//                    if (currentSlot < timeSlotsCount && !IsAvailable(nodeId, lastNodeId, currentSlot))
//                    {
//                        // נסה להזיז את המתאמן האחר
//                        var occupyingTrainee = GetOccupyingTrainee(nodeId, lastNodeId, currentSlot);
//                        if (occupyingTrainee != null && occupyingTrainee != currentTrainee)
//                        {
//                            if (TryRescheduleOtherTrainee(occupyingTrainee, nodeId, currentTime))
//                            {
//                                // אם הצלחנו להזיז את המתאמן האחר, נסה שוב
//                                AddTrainee(nodeId, lastNodeId, currentSlot, currentTrainee);
//                                currentPath.Add(nodeId);

//                                var result = BacktrackWithPriority(
//                                    graph, nodeDict, reachableFromNode, exerciseOrder, alternativeDevices,
//                                    MarkExerciseDone(mask, i), currentPath,
//                                    currentAlternatives + 2, // עלות גבוהה יותר להזזת מתאמן אחר
//                                    currentTime.AddMinutes(node.UsageDurationMinutes),
//                                    out DateTime candidateEndTime
//                                );

//                                if (result.found)
//                                {
//                                    if (!foundAny || currentAlternatives + 2 < minAlternatives)
//                                    {
//                                        foundAny = true;
//                                        minAlternatives = currentAlternatives + 2;
//                                        bestPath = result.bestPath;
//                                        bestEndTime = candidateEndTime;
//                                    }
//                                }

//                                RemoveTrainee(nodeId, lastNodeId, currentSlot);
//                                currentPath.RemoveAt(currentPath.Count - 1);
//                            }
//                        }
//                    }
//                }
//            }

//            // שמירת התוצאה ב-cache
//            endTime = foundAny ? bestEndTime : DateTime.MinValue;
//            memo[memoKey] = (foundAny, foundAny ? minAlternatives : int.MaxValue, bestPath, endTime);

//            return (foundAny, bestPath, endTime);
//        }

//        // אופטימיזציה: מציאת האינדקס של תרגיל במערך על ידי Dictionary
//        private Dictionary<int, int> exerciseIdToIndex;

//        // פונקציית עזר למציאת אינדקס של תרגיל
//        private int GetExerciseIndex(int exerciseId)
//        {
//            if (exerciseIdToIndex.TryGetValue(exerciseId, out int index))
//                return index;
//            return -1;
//        }

//        // בדיקת זמינות - גישה יעילה יותר לטבלה התלת-ממדית
//        private bool IsAvailable(int toExerciseId, int fromExerciseId, int timeSlot, int maxInQueue = 1)
//        {
//            int toIndex = GetExerciseIndex(toExerciseId);
//            int fromIndex = GetExerciseIndex(fromExerciseId);

//            // אם אינדקס לא נמצא או חורג מהתחום, לא זמין
//            if (toIndex < 0 || toIndex >= exerciseCount ||
//                fromIndex < 0 || fromIndex >= exerciseCount ||
//                timeSlot < 0 || timeSlot >= timeSlotsCount)
//            {
//                return false;
//            }

//            return Table3D[toIndex, fromIndex, timeSlot].Queue.Count < maxInQueue;
//        }

//        // הוספת מתאמן לתור - גישה יעילה יותר
//        private void AddTrainee(int toExerciseId, int fromExerciseId, int timeSlot, Trainee trainee)
//        {
//            int toIndex = GetExerciseIndex(toExerciseId);
//            int fromIndex = GetExerciseIndex(fromExerciseId);

//            // אם האינדקסים תקינים, הוסף את המתאמן לתור
//            if (toIndex >= 0 && toIndex < exerciseCount &&
//                fromIndex >= 0 && fromIndex < exerciseCount &&
//                timeSlot >= 0 && timeSlot < timeSlotsCount)
//            {
//                Table3D[toIndex, fromIndex, timeSlot].Queue.Enqueue(trainee);
//            }
//        }

//        // הסרת מתאמן מהתור
//        private Trainee RemoveTrainee(int toExerciseId, int fromExerciseId, int timeSlot)
//        {
//            int toIndex = GetExerciseIndex(toExerciseId);
//            int fromIndex = GetExerciseIndex(fromExerciseId);

//            if (toIndex >= 0 && toIndex < exerciseCount &&
//                fromIndex >= 0 && fromIndex < exerciseCount &&
//                timeSlot >= 0 && timeSlot < timeSlotsCount &&
//                Table3D[toIndex, fromIndex, timeSlot].Queue.Count > 0)
//            {
//                return Table3D[toIndex, fromIndex, timeSlot].Queue.Dequeue();
//            }

//            return null;
//        }

//        // פונקציה חדשה - מוצאת את המתאמן שתופס מכשיר מסוים
//        private Trainee GetOccupyingTrainee(int toExerciseId, int fromExerciseId, int timeSlot)
//        {
//            int toIndex = GetExerciseIndex(toExerciseId);
//            int fromIndex = GetExerciseIndex(fromExerciseId);

//            if (toIndex >= 0 && toIndex < exerciseCount &&
//                fromIndex >= 0 && fromIndex < exerciseCount &&
//                timeSlot >= 0 && timeSlot < timeSlotsCount)
//            {
//                return Table3D[toIndex, fromIndex, timeSlot].GetCurrentTrainee();
//            }

//            return null;
//        }

//        // בדיקה האם תרגיל כבר בוצע
//        private bool IsExerciseDone(int mask, int exerciseIndex)
//        {
//            return (mask & (1 << exerciseIndex)) != 0;
//        }

//        // הוספת תרגיל לרשימת התרגילים שבוצעו
//        private int MarkExerciseDone(int mask, int exerciseIndex)
//        {
//            return mask | (1 << exerciseIndex);
//        }

//        // קבלת אינדקס התרגיל הבא שטרם בוצע
//        private int GetNextUncompletedExercise(int mask, List<int> exerciseOrder)
//        {
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                if (!IsExerciseDone(mask, i))
//                    return i;
//            }
//            return -1;
//        }

//        // ספירת מספר התרגילים שכבר בוצעו
//        private int CountCompletedExercises(int mask)
//        {
//            int count = 0;
//            while (mask > 0)
//            {
//                count += mask & 1;
//                mask >>= 1;
//            }
//            return count;
//        }

//        // פונקציה שבודקת אם אפשר להזיז מתאמן אחר באופן רקורסיבי
//        private bool TryRescheduleOtherTrainee(
//            Trainee otherTrainee,
//            int exerciseToReschedule,
//            DateTime desiredTime)
//        {
//            // מימוש בסיסי - יש לפתח לפי הצורך
//            try
//            {
//                // בדוק אם המתאמן כבר התחיל את האימון
//                if (otherTrainee.HasStartedTraining)
//                    return false; // אם כבר התחיל, לא מזיזים אותו

//                // נסה למצוא מסלול חוקי חדש עבור המתאמן השני
//                // שמוציא את התרגיל הבעייתי מהזמן המבוקש
//                return TryFindAlternativePathForTrainee(otherTrainee, exerciseToReschedule, desiredTime);
//            }
//            catch
//            {
//                return false; // במקרה של שגיאה, לא מזיזים
//            }
//        }

//        // פונקציית עזר להזזת מתאמן אחר
//        private bool TryFindAlternativePathForTrainee(Trainee trainee, int conflictExercise, DateTime conflictTime)
//        {
//            // כאן צריך לממש את הלוגיקה שמנסה למצוא מסלול חלופי למתאמן
//            // זה יכול להיות רקורסיה לאלגוריתם הראשי או אלגוריתם פשוט יותר

//            // מימוש בסיסי - מחזיר true בהסתברות של 30% (לדוגמה)
//            Random rand = new Random();
//            return rand.NextDouble() < 0.3;
//        }

//        // פונקציית עזר: המרת DateTime לאינדקス סלוט
//        private int TimeToSlot(DateTime startTime, DateTime slotTime)
//        {
//            return (int)((slotTime - startTime).TotalMinutes / slotDurationMinutes);
//        }

//        // פונקציית עזר: המרת אינדקס סלוט ל-DateTime 
//        private DateTime SlotToTime(DateTime startTime, int slotIndex)
//        {
//            return startTime.AddMinutes(slotIndex * slotDurationMinutes);
//        }

//        // ספירת ביטים דלוקים ב-mask (כמה תרגילים כבר בוצעו)
//        private int CountBits(int mask)
//        {
//            int count = 0;
//            while (mask > 0)
//            {
//                count += mask & 1;
//                mask >>= 1;
//            }
//            return count;
//        }

//        // מתודת העטיפה הראשית - מוצאת מסלול תרגילים תקף למתאמן
//        public PathResult FindValidPath(
//            SubGraph subGraph,
//            List<int> exerciseOrder,
//            DateTime startTime,
//            Trainee trainee,
//            Dictionary<int, List<int>> alternativeDevices = null)
//        {
//            if (subGraph == null)
//                throw new ArgumentNullException(nameof(subGraph));
//            if (exerciseOrder == null || exerciseOrder.Count == 0)
//                throw new ArgumentException("רשימת התרגילים ריקה", nameof(exerciseOrder));

//            currentTrainee = trainee ?? throw new ArgumentNullException(nameof(trainee));

//            if (alternativeDevices == null)
//                alternativeDevices = new Dictionary<int, List<int>>();

//            // שלב 1: לבנות מילון צמתים לגישה מהירה
//            var nodeDict = subGraph.Nodes.ToDictionary(n => n.Id);

//            // שלב 2: לבנות מטריצת נגישות
//            var reachableFromNode = BuildReachabilityMatrix(nodeDict.Values);

//            int mask = 0; // ייצוג בינארי של אילו תרגילים כבר בוצעו
//            var path = new List<int>(); // המסלול הנבחר

//            // מחיקת המילון הקודם לפני חיפוש חדש
//            memo.Clear();

//            // שלב 3: קריאה לאלגוריתם הרקורסיבי
//            var (found, bestPath, finalTime) = BacktrackWithPriority(
//                subGraph,
//                nodeDict,
//                reachableFromNode,
//                exerciseOrder,
//                alternativeDevices,
//                mask,
//                path,
//                0,
//                startTime,
//                out DateTime endTime
//            );

//            // אם לא נמצא מסלול, נזרוק חריגה
//            if (!found || bestPath == null)
//                throw new InvalidOperationException("לא נמצא מסלול חוקי לפי ההגבלות.");

//            // בניית תוצאת המסלול המלא
//            return new PathResult
//            {
//                ExerciseIdsInPath = bestPath,
//                StartTime = startTime,
//                EndTime = finalTime,
//                AlternativesUsed = CalculateAlternativesUsed(bestPath, exerciseOrder)
//            };
//        }

//        // פונקציית עזר לחישוב כמה חילופים נעשו
//        private int CalculateAlternativesUsed(List<int> actualPath, List<int> originalOrder)
//        {
//            int alternatives = 0;
//            for (int i = 0; i < Math.Min(actualPath.Count, originalOrder.Count); i++)
//            {
//                if (actualPath[i] != originalOrder[i])
//                    alternatives++;
//            }
//            return alternatives;
//        }

//        // פונקציה לאיפוס המטמון (שימושי לביצועים)
//        public void ClearCache()
//        {
//            memo.Clear();
//            reachabilityCache = null;
//        }

//        // פונקציה לקבלת סטטיסטיקות על הביצועים
//        public (int CacheHits, int TotalSearches, double HitRatio) GetPerformanceStats()
//        {
//            int totalSearches = memo.Count;
//            // כאן ניתן להוסיף מעקב על cache hits אם נרצה
//            return (0, totalSearches, 0.0);
//        }
//    }

//}