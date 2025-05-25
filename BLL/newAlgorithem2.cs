//using IBLL;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BLL
//{
//    public class newAlgorithem2
//    {
//        // טבלת תלת-ממד: [תרגיל יעד, תרגיל מקור, סלוט זמן]
//        private TableCell[,,] Table3D;
//        private int exerciseCount;
//        private int timeSlotsCount;
//        private int slotDurationMinutes = slotDurationMinutes; // ברירת מחדל: כל סלוט הוא 5 דקות



//        // מחלקה פנימית לתוצאה - מייצגת מסלול תרגילים עם זמן התחלה וסיום
//        public class PathResult
//        {
//            public List<int> ExerciseIdsInPath { get; set; } = new();
//            public DateTime StartTime { get; set; }
//            public DateTime EndTime { get; set; }
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
//            // אם זה אותו תרגיל, החזר true (או false, לפי הלוגיקה שלך)
//            if (fromExerciseId == toExerciseId)
//                return false; // בדרך כלל לא רוצים לחזור לאותו תרגיל

//            // אחרת, בדוק ישירות מהטבלה התלת-ממדית
//            return Table3D[toExerciseId, fromExerciseId, timeSlot].LegalityValue > 0;
//        }


//        // אופטימיזציה של אתחול הטבלה - יצירה יעילה יותר
//        public void InitTable(List<ExerciseNode> exercises, int slotsCount)
//        {
//            exerciseCount = exercises.Count;
//            timeSlotsCount = slotsCount;
//            Table3D = new TableCell[exerciseCount, exerciseCount, timeSlotsCount];

//            // אתחול מילון מיידי מזהה -> אינדקס
//            exerciseIdToIndex = new Dictionary<int, int>();
//            for (int i = 0; i < exercises.Count; i++)
//            {
//                exerciseIdToIndex[exercises[i].Id] = i;
//            }

//            // שאר הקוד כרגיל...
//            // חשב מראש את כל ערכי החוקיות ללא תלות בזמן
//            int[,] legalityMatrix = new int[exerciseCount, exerciseCount];

//            // חישוב מטריצת החוקיות פעם אחת בלבד
//            for (int to = 0; to < exerciseCount; to++)
//            {
//                for (int from = 0; from < exerciseCount; from++)
//                {
//                    if (to == from)
//                        legalityMatrix[to, from] = 0; // אותו תרגיל
//                    else
//                    {
//                        // בדוק אם יש קשר ישיר בין התרגילים (שימוש בשכנים ישירים)
//                        bool hasDirectPath = exercises[from].Neighbors.Contains(exercises[to]);

//                        // אם יש קשר ישיר, נשמור את קבוצת השריר
//                        legalityMatrix[to, from] = hasDirectPath ?
//                            (int)Math.Pow(2, exercises[to].MuscleGroup) : -1;
//                    }
//                }
//            }

//            // כעת נשתמש במטריצה המוכנה ליצירת המשבצות בטבלה התלת-ממדית
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

//        //2. שימוש במכניזם קשירות (Caching) מתקדם עבור BuildReachabilityMatrix
//        public Dictionary<int, HashSet<int>> BuildReachabilityMatrix(IEnumerable<Node> nodes)
//        {
//            // בדוק אם כבר חישבנו את המטריצה בעבר
//            if (reachabilityCache != null)
//                return reachabilityCache;

//            var nodesList = nodes.ToList();
//            var result = new Dictionary<int, HashSet<int>>();

//            // במקום לבצע BFS בכל פעם, השתמש בערכים שכבר מחושבים בטבלה התלת-ממדית
//            foreach (var nodeA in nodesList)
//            {
//                var reachable = new HashSet<int>();
//                foreach (var nodeB in nodesList)
//                {
//                    if (nodeA.Id == nodeB.Id)
//                        continue;

//                    // בדוק ישירות מהטבלה אם המעבר חוקי (עבור סלוט זמן 0 למשל, כי הערך לא תלוי בזמן)
//                    int fromIdx = nodesList.IndexOf(nodeA);
//                    int toIdx = nodesList.IndexOf(nodeB);

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


//        //3. שיפור BacktrackWithPriority עם חיפוש לפי בינארי (Bitset) במקום רשימה
//        //הנה גרסה משופרת של אלגוריתם הBitset לחיפוש מהיר יותר של צירופים:
//        private (bool found, List<int> bestPath, DateTime endTime) BacktrackWithPriority(
//            SubGraph graph,
//            Dictionary<int, Node> nodeDict,
//            int mask,                   // ביטסט - אילו תרגילים בוצעו
//            List<int> currentPath,      // המסלול שנבנה עד כה
//            int currentAlternatives,    // מספר חילופים עד כה
//            DateTime currentTime,       // הזמן הנוכחי
//            List<int> exerciseOrder,    // סדר התרגילים המקורי
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

//            // אסטרטגיה 1: נסה לבצע תרגילים לפי סדר רגיל, אבל בדוק קודם את כל האפשרויות של המתאמן עצמו

//            // מבנה נתונים למעקב אחר איזה תרגילים כבר ניסינו להזזות ב-bitset
//            int triedMask = 0;

//            // נסה תחילה את התרגיל הבא בתור לפי הסדר המקורי
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                // אם התרגיל כבר בוצע, דלג
//                if ((mask & (1 << i)) != 0)
//                    continue;

//                // סמן שניסינו את התרגיל הזה
//                triedMask |= (1 << i);

//                int nodeId = exerciseOrder[i];
//                if (!nodeDict.TryGetValue(nodeId, out var node))
//                    continue;

//                // בדוק האם המעבר חוקי מהטבלה התלת-ממדית
//                if (lastNodeId != -1)
//                {
//                    // מצא את האינדקסים בטבלה
//                    int fromIdx = -1, toIdx = -1;

//                    // מציאת האינדקסים בדרך יעילה יותר
//                    for (int idx = 0; idx < exerciseCount; idx++)
//                    {
//                        if (exercises[idx].Id == lastNodeId)
//                            fromIdx = idx;
//                        if (exercises[idx].Id == nodeId)
//                            toIdx = idx;

//                        // אם מצאנו את שניהם, אפשר לצאת מהלולאה
//                        if (fromIdx != -1 && toIdx != -1)
//                            break;
//                    }

//                    // אם אחד מהאינדקסים לא נמצא, או שהמעבר לא חוקי, דלג
//                    if (fromIdx == -1 || toIdx == -1 ||
//                        currentSlot >= timeSlotsCount ||
//                        Table3D[toIdx, fromIdx, currentSlot].LegalityValue <= 0)
//                    {
//                        continue;
//                    }
//                }

//                // בדוק זמינות
//                if (currentSlot < timeSlotsCount && IsAvailable(nodeId, lastNodeId, currentSlot))
//                {
//                    // "תפוס" את המכשיר זמנית
//                    AddTrainee(nodeId, lastNodeId, currentSlot, currentTrainee);

//                    // נסה להמשיך את המסלול
//                    currentPath.Add(nodeId);

//                    var result = BacktrackWithPriority(
//                        graph, nodeDict, mask | (1 << i), currentPath,
//                        currentAlternatives, currentTime.AddMinutes(node.UsageDurationMinutes),
//                        exerciseOrder, out DateTime candidateEndTime
//                    );

//                    if (result.found)
//                    {
//                        foundAny = true;
//                        bestPath = result.bestPath;
//                        bestEndTime = candidateEndTime;

//                        // שחרר את המכשיר (כי מצאנו פתרון)
//                        RemoveTrainee(nodeId, lastNodeId, currentSlot);

//                        // שמור את התוצאה ב-cache
//                        memo[memoKey] = (true, currentAlternatives, bestPath, bestEndTime);

//                        endTime = bestEndTime;
//                        return (true, bestPath, bestEndTime);
//                    }

//                    // שחרר את המכשיר (ניסיון לא צלח)
//                    RemoveTrainee(nodeId, lastNodeId, currentSlot);
//                    currentPath.RemoveAt(currentPath.Count - 1);
//                }
//            }

//            // אסטרטגיה 2: נסה למצוא תרגילים חלופיים (אם יש)
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                int originalNodeId = exerciseOrder[i];

//                // דלג על תרגילים שכבר בוצעו
//                if ((mask & (1 << i)) != 0)
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

//                        int currentSlot = TimeToSlot(graph.StartTime, currentTime);
//                        if (currentSlot >= 0 && currentSlot < timeSlotsCount)
//                        {
//                            bool isAvailable = true;

//                            // בדוק זמינות של המכשיר החלופי

//                            if (isAvailable)
//                            {
//                                // נסה להמשיך במסלול עם התרגיל החלופי
//                                currentPath.Add(alternativeNodeId);

//                                var nextTime = currentTime.AddMinutes(alternativeNode.UsageDurationMinutes);

//                                // שים לב: הגדלנו את מספר החילופים ב-1
//                                var result = BacktrackWithPriority(
//                                    graph, nodeDict, reachableFromNode, exerciseOrder, alternativeDevices,
//                                    mask | (1 << i), currentPath, currentAlternatives + 1,
//                                    nextTime, out DateTime candidateEndTime
//                                );

//                                if (result.found)
//                                {
//                                    if (!foundAny || currentAlternatives + 1 < minAlternatives)
//                                    {
//                                        foundAny = true;
//                                        minAlternatives = currentAlternatives + 1;
//                                        bestPath = result.bestPath;
//                                        bestEndTime = candidateEndTime;
//                                    }
//                                }

//                                currentPath.RemoveAt(currentPath.Count - 1);
//                            }
//                        }
//                    }
//                }
//            }

//            // אם מצאנו מסלול תקף בשלב השני, החזר אותו
//            if (foundAny)
//            {
//                endTime = bestEndTime;
//                memo[memoKey] = (true, minAlternatives, bestPath, bestEndTime);
//                return (true, bestPath, bestEndTime);
//            }


//            /// אסטרטגיה 3: נסה לשנות את הסדר אצל מתאמנים אחרים
//            /// זו האסטרטגיה הכי אגרסיבית - להזיז תרגילים אצל מתאמנים אחרים
//            /// מימוש חסר - צריך להכניס פה את הקוד שמנסה להזיז מתאמנים אחרים
//            // אסטרטגיה 3: נסה לשנות את הסדר של מתאמנים אחרים (כאן מקום ליישום שלך)


//            // שמירת התוצאה ב-cache
//            endTime = foundAny ? bestEndTime : DateTime.MinValue;
//            memo[memoKey] = (foundAny, currentAlternatives, bestPath, endTime);

//            return (foundAny, bestPath, endTime);
//        }

//        //4. אופטימיזציה: מציאת האינדקס של תרגיל במערך על ידי Dictionary
//        // במקום לחפש כל פעם את האינדקס של תרגיל, אפשר לשמור מילון מיידי מזהה -> אינדקס:
//        private Dictionary<int, int> exerciseIdToIndex;

//        // פונקציית עזר למציאת אינדקס של תרגיל
//        private int GetExerciseIndex(int exerciseId)
//        {
//            if (exerciseIdToIndex.TryGetValue(exerciseId, out int index))
//                return index;
//            return -1;
//        }

//        //5. שיפור ניהול התור בטבלה התלת-ממדית
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

//        //6. אופטימיזציית Bitset עבור בדיקת תרגילים שבוצעו
//        //  במקום לבצע חיפוש לינארי כדי לדעת אילו תרגילים כבר בוצעו, נשתמש ב-bitset עם פעולות ביטוויות מהירות:
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
//            // כאן יש לממש את הלוגיקה שמנסה לשנות את הסדר של מתאמן אחר
//            // הפונקציה צריכה לבדוק אם אפשר לשנות את סדר התרגילים של המתאמן השני
//            // כך שהתרגיל exerciseToReschedule לא יתבצע בזמן desiredTime

//            // מימוש בסיסי לדוגמה:
//            /*
//            // בדוק אם המתאמן כבר התחיל את האימון
//            if (otherTrainee.HasStartedTraining)
//                return false; // אם כבר התחיל, לא מזיזים אותו

//            // נסה למצוא מסלול חוקי חדש עבור המתאמן השני
//            // שמוציא את התרגיל הבעייתי מהזמן המבוקש
//            return otherTrainee.TryFindAlternativePath(exerciseToReschedule, desiredTime);
//            */

//            // כרגע מחזיר false כי זה תלוי במימוש של Trainee
//            return false;
//        }

//        // פונקציית עזר: המרת DateTime לאינדקס סלוט
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
//            SubGraph subGraph,              // הגרף של המתאמן
//            List<int> exerciseOrder,        // רשימת התרגילים מהתוכנית הדיפולטיבית
//            DateTime startTime,             // זמן התחלה
//            Dictionary<int, List<int>> alternativeDevices = null) // אופציונלי: מילון תרגילים חילופיים
//        {
//            if (alternativeDevices == null)
//                alternativeDevices = new Dictionary<int, List<int>>();

//            // שלב 1: לבנות מילון צמתים לגישה מהירה
//            var nodeDict = subGraph.Nodes.ToDictionary(n => n.Id);

//            // שלב 2: לבנות מטריצת נגישות (אילו תרגילים יכולים להגיע לאילו)
//            var reachableFromNode = BuildReachabilityMatrix(nodeDict.Values);

//            int mask = 0; // ייצוג בינארי של אילו תרגילים כבר בוצעו
//            var path = new List<int>(); // המסלול הנבחר
//            DateTime endTime = DateTime.MinValue; // ישמר זמן הסיום של המסלול

//            // מחיקת המילון הקודם לפני חיפוש חדש
//            memo.Clear();

//            // שלב 3: קריאה לאלגוריתם הרקורסיבי שמחפש מסלול חוקי
//            var (found, bestPath, finalTime) = BacktrackWithPriority(
//                subGraph,
//                nodeDict,
//                reachableFromNode,
//                exerciseOrder,
//                alternativeDevices,
//                mask,
//                path,
//                0,           // מספר חילופים בשלב התחלתי
//                startTime,
//                out endTime
//            );

//            // אם לא נמצא מסלול, נזרוק חריגה
//            if (!found || bestPath == null)
//                throw new Exception("לא נמצא מסלול חוקי לפי ההגבלות.");

//            // בניית תוצאת המסלול המלא
//            return new PathResult
//            {
//                ExerciseIdsInPath = bestPath,
//                StartTime = startTime,
//                EndTime = finalTime
//            };
//        }

//    }
//}




























////// אסטרטגיה 2: נסה לדחות בזמן (אם אפשר)
////// במקום לנסות כל תרגיל בכל זמן, נסה לדחות רק עד שהמכשיר מתפנה

//////לא בטוח שצריך*****************
////// עבור כל תרגיל שעדיין לא בוצע
////for (int i = 0; i < exerciseOrder.Count; i++)
////{
////    // אם התרגיל כבר בוצע, דלג
////    if ((mask & (1 << i)) != 0)
////        continue;

////    int nodeId = exerciseOrder[i];
////    if (!nodeDict.TryGetValue(nodeId, out var node))
////        continue;

////    // מצא את הסלוט הזמין הקרוב ביותר
////    for (int futureSlot = currentSlot + 1; futureSlot < Math.Min(currentSlot + 12, timeSlotsCount); futureSlot++)
////    {
////        if (lastNodeId != -1)
////        {
////            // מציאת האינדקסים בטבלה
////            int fromIdx = -1, toIdx = -1;
////            for (int idx = 0; idx < exerciseCount; idx++)
////            {
////                if (exercises[idx].Id == lastNodeId)
////                    fromIdx = idx;
////                if (exercises[idx].Id == nodeId)
////                    toIdx = idx;

////                if (fromIdx != -1 && toIdx != -1)
////                    break;
////            }

////            // אם המעבר לא חוקי, דלג
////            if (fromIdx == -1 || toIdx == -1 ||
////                Table3D[toIdx, fromIdx, futureSlot].LegalityValue <= 0)
////            {
////                continue;
////            }
////        }

////        // בדוק אם המכשיר פנוי בזמן העתידי
////        if (IsAvailable(nodeId, lastNodeId, futureSlot))
////        {
////            // חשב את הזמן החדש
////            DateTime delayedTime = SlotToTime(graph.StartTime, futureSlot);

////            // תפוס את המכשיר
////            AddTrainee(nodeId, lastNodeId, futureSlot, currentTrainee);

////            // נסה להמשיך את המסלול
////            currentPath.Add(nodeId);

////            var result = BacktrackWithPriority(
////                graph, nodeDict, mask | (1 << i), currentPath,
////                currentAlternatives, delayedTime.AddMinutes(node.UsageDurationMinutes),
////                exerciseOrder, out DateTime candidateEndTime
////            );

////            if (result.found)
////            {
////                foundAny = true;
////                bestPath = result.bestPath;
////                bestEndTime = candidateEndTime;

////                // שחרר את המכשיר
////                RemoveTrainee(nodeId, lastNodeId, futureSlot);

////                // שמור את התוצאה ב-cache
////                memo[memoKey] = (true, currentAlternatives, bestPath, bestEndTime);

////                endTime = bestEndTime;
////                return (true, bestPath, bestEndTime);
////            }

////            // שחרר את המכשיר
////            RemoveTrainee(nodeId, lastNodeId, futureSlot);
////            currentPath.RemoveAt(currentPath.Count - 1);
////        }
////    }
////}

