//using IBLL;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BLL
//{
    

//    public class newALgorithem
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

//        // יצירת טבלה תלת-ממדית
//        public void InitTable(List<ExerciseNode> exercises, int slotsCount)
//        {
//            exerciseCount = exercises.Count;
//            timeSlotsCount = slotsCount;
//            Table3D = new TableCell[exerciseCount, exerciseCount, timeSlotsCount];

//            // יוצר הטבלה ביעילות - מחשב את ערך החוקיות פעם אחת לכל זוג תרגילים
//            for (int to = 0; to < exerciseCount; to++)
//                for (int from = 0; from < exerciseCount; from++)
//                {
//                    // מחשב פעם אחת עבור כל (to, from)
//                    int legality;
//                    if (to == from)
//                        legality = 0; // אותו תרגיל
//                    else if (HasPathToNextExercise(exercises[from], exercises[to]))
//                        legality = (int)Math.Pow(2, exercises[to].MuscleGroup); // מעבר חוקי - שומר את קבוצת השריר
//                    else
//                        legality = -1; // מעבר לא חוקי

//                    // לכל סלוט מעתיק את אותו ערך LegalityValue, אבל Queue נפרד
//                    for (int slot = 0; slot < timeSlotsCount; slot++)
//                    {
//                        Table3D[to, from, slot] = new TableCell();
//                        Table3D[to, from, slot].LegalityValue = legality;
//                        // Queue כבר מאותחל ריק בכל איטרציה
//                    }
//                }
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

//        // בודק אם יש מסלול בין שני תרגילים (כמו שמימשת)
//        private bool HasPathToNextExercise(Node currentNode, Node nextNode)
//        {
//            // בדוק אם יש קשת ישירה
//            if (currentNode.Neighbors.Contains(nextNode))
//                return true;

//            // מצא את הצומת מסוג MuscleGroup שמחובר לתרגיל הנוכחי
//            var currentMuscleNode = currentNode.Neighbors.FirstOrDefault(n => n.Type == NodeType.MuscleGroup);
//            var nextMuscleNode = nextNode.Neighbors.FirstOrDefault(n => n.Type == NodeType.MuscleGroup);

//            // אם אין צמתים מסוג MuscleGroup מחוברים, אין מסלול
//            if (currentMuscleNode == null || nextMuscleNode == null)
//                return false;

//            // אם הצומת הנוכחי של MuscleGroup הוא גם הצומת של היעד, החזר false
//            if (currentMuscleNode == nextMuscleNode)
//                return false;

//            // חיפוש מסלול בין צמתים מסוג MuscleGroup באמצעות BFS
//            var visited = new HashSet<Node>();
//            var queue = new Queue<Node>();

//            queue.Enqueue(currentMuscleNode);
//            visited.Add(currentMuscleNode);

//            while (queue.Count > 0)
//            {
//                var current = queue.Dequeue();

//                // עבור כל שכן מסוג MuscleGroup
//                foreach (var neighbor in current.Neighbors.Where(n => n.Type == NodeType.MuscleGroup))
//                {
//                    if (visited.Contains(neighbor))
//                        continue;

//                    // אם הגענו ליעד, החזר true
//                    if (neighbor == nextMuscleNode)
//                        return true;

//                    queue.Enqueue(neighbor);
//                    visited.Add(neighbor);
//                }
//            }

//            return false;
//        }

//        // בונה מטריצת נגישות - שמייצגת אילו תרגילים יכולים להגיע לאילו תרגילים
//        public Dictionary<int, HashSet<int>> BuildReachabilityMatrix(IEnumerable<Node> nodes)
//        {
//            var nodesList = nodes.ToList();
//            var result = new Dictionary<int, HashSet<int>>();

//            foreach (var nodeA in nodesList)
//            {
//                var reachable = new HashSet<int>();
//                foreach (var nodeB in nodesList)
//                {
//                    if (nodeA.Id == nodeB.Id)
//                        continue;
//                    if (HasPathToNextExercise(nodeA, nodeB))
//                        reachable.Add(nodeB.Id);
//                }
//                result[nodeA.Id] = reachable;
//            }
//            return result;
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

//        // האלגוריתם הרקורסיבי העיקרי עם עדיפויות
//        private (bool found, List<int> bestPath, DateTime endTime) BacktrackWithPriority(
//            SubGraph graph,                                   // הגרף
//            Dictionary<int, Node> nodeDict,                  // מילון צמתים לגישה מהירה
//            Dictionary<int, HashSet<int>> reachableFromNode, // מטריצת נגישות
//            List<int> exerciseOrder,                         // סדר התרגילים
//            Dictionary<int, List<int>> alternativeDevices,   // תרגילים חילופיים (אם יש)
//            int mask,                                       // אילו תרגילים בוצעו (bitmask)
//            List<int> currentPath,                           // המסלול שנבנה עד כה
//            int currentAlternatives,                         // מספר חילופים עד כה
//            DateTime currentTime,                            // הזמן הנוכחי
//            out DateTime endTime                             // הזמן שבו הסתיים המסלול
//        )
//        {
//            int lastNodeId = currentPath.Count > 0 ? currentPath.Last() : -1; // מזהה התרגיל האחרון
//            long timeMinutes = (long)(currentTime - DateTime.MinValue).TotalMinutes; // ייצוג הזמן במספר שלם
//            long roundedTimeMinutes = timeMinutes - (timeMinutes % slotDurationMinutes); // עיגול לזמן סלוט
//            var memoKey = (mask, lastNodeId, roundedTimeMinutes); // מפתח ייחודי למצב

//            // בדיקה האם כבר חישבנו תוצאה עבור המצב הזה (memoization)
//            if (memo.TryGetValue(memoKey, out var memoResult))
//            {
//                endTime = memoResult.endTime;
//                return (memoResult.found, memoResult.bestPath, memoResult.endTime);
//            }

//            // תנאי עצירה: אם כל התרגילים בוצעו
//            if (CountBits(mask) == exerciseOrder.Count)
//            {
//                endTime = currentTime;
//                return (true, new List<int>(currentPath), currentTime);
//            }

//            bool foundAny = false;
//            int minAlternatives = int.MaxValue;
//            List<int> bestPath = null;
//            DateTime bestEndTime = DateTime.MinValue;

//            // אסטרטגיה 1: נסה לשנות את סדר האימון של המתאמן הנוכחי
//            // (כלומר, נסה תרגילים בסדר אחר לפני שמנסים להחליף מכשירים או לשנות זמנים)
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                int nodeId = exerciseOrder[i];

//                // אם התרגיל כבר בוצע, דלג
//                if ((mask & (1 << i)) != 0)
//                    continue;

//                // שליפת הצומת מהמילון
//                if (!nodeDict.TryGetValue(nodeId, out var node))
//                    continue;

//                // 1.1: בדוק אם המעבר לתרגיל זה חוקי (מבחינת גרף השרירים)
//                bool isLegalMove = lastNodeId == -1 || // אם זה התרגיל הראשון, תמיד חוקי
//                                   (reachableFromNode.ContainsKey(lastNodeId) &&
//                                    reachableFromNode[lastNodeId].Contains(nodeId));

//                if (!isLegalMove)
//                    continue;

//                // 1.2: בדוק זמינות המכשיר בזמן הנוכחי
//                int currentSlot = TimeToSlot(graph.StartTime, currentTime);
//                if (currentSlot >= 0 && currentSlot < timeSlotsCount)
//                {
//                    bool isAvailable = true;

//                    // בדוק אם אחד המכשירים של התרגיל הזה זמין כרגע
//                    // (אם אין בכלל מכשירים פנויים לתרגיל זה, נסה תרגיל אחר)

//                    if (isAvailable)
//                    {
//                        // נסה להמשיך במסלול עם התרגיל הזה
//                        currentPath.Add(nodeId);

//                        var nextTime = currentTime.AddMinutes(node.UsageDurationMinutes);

//                        var result = BacktrackWithPriority(
//                            graph, nodeDict, reachableFromNode, exerciseOrder, alternativeDevices,
//                            mask | (1 << i), currentPath, currentAlternatives,
//                            nextTime, out DateTime candidateEndTime
//                        );

//                        if (result.found)
//                        {
//                            // אם מצאנו מסלול טוב והוא משתמש בפחות חילופים מהמינימום הנוכחי
//                            // או שזה המסלול הראשון שמצאנו, עדכן את המסלול הטוב ביותר
//                            if (!foundAny || currentAlternatives < minAlternatives)
//                            {
//                                foundAny = true;
//                                minAlternatives = currentAlternatives;
//                                bestPath = result.bestPath;
//                                bestEndTime = candidateEndTime;
//                            }
//                        }

//                        // חזרה אחורה בעץ החיפוש
//                        currentPath.RemoveAt(currentPath.Count - 1);
//                    }
//                }
//            }

//            // אם מצאנו מסלול תקף בשלב הראשון, החזר אותו מיד
//            if (foundAny)
//            {
//                endTime = bestEndTime;

//                // שמור את התוצאה למקרה שנגיע לאותו מצב בעתיד (memoization)
//                memo[memoKey] = (true, minAlternatives, bestPath, bestEndTime);

//                return (true, bestPath, bestEndTime);
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

//            // אסטרטגיה 3: נסה לדחות את התרגילים בזמן (אם אפשר)
//            // במקום להוציא מתאמנים אחרים מהמכשיר, פשוט דחה את הזמן שלך עד שהמכשיר מתפנה
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                int nodeId = exerciseOrder[i];

//                // דלג על תרגילים שכבר בוצעו
//                if ((mask & (1 << i)) != 0)
//                    continue;

//                if (!nodeDict.TryGetValue(nodeId, out var node))
//                    continue;

//                // בדוק האם המעבר לתרגיל זה חוקי בכלל
//                bool isLegalMove = lastNodeId == -1 ||
//                                  (reachableFromNode.ContainsKey(lastNodeId) &&
//                                   reachableFromNode[lastNodeId].Contains(nodeId));

//                if (!isLegalMove)
//                    continue;

//                // בדוק אם אפשר למצוא סלוט זמן פנוי בהמשך
//                int currentSlot = TimeToSlot(graph.StartTime, currentTime);
//                int maxSearchSlot = Math.Min(currentSlot + 12, timeSlotsCount - 1); // חפש עד שעה קדימה (12 סלוטים של 5 דקות)

//                for (int slot = currentSlot + 1; slot <= maxSearchSlot; slot++)
//                {
//                    bool isAvailable = true;

//                    // בדוק זמינות בסלוט העתידי

//                    if (isAvailable)
//                    {
//                        // עדכן את זמן התחלת התרגיל לסלוט הזמין
//                        DateTime delayedTime = SlotToTime(graph.StartTime, slot);

//                        currentPath.Add(nodeId);

//                        var nextTime = delayedTime.AddMinutes(node.UsageDurationMinutes);

//                        var result = BacktrackWithPriority(
//                            graph, nodeDict, reachableFromNode, exerciseOrder, alternativeDevices,
//                            mask | (1 << i), currentPath, currentAlternatives,
//                            nextTime, out DateTime candidateEndTime
//                        );

//                        if (result.found)
//                        {
//                            if (!foundAny || currentAlternatives < minAlternatives)
//                            {
//                                foundAny = true;
//                                minAlternatives = currentAlternatives;
//                                bestPath = result.bestPath;
//                                bestEndTime = candidateEndTime;
//                            }
//                        }

//                        currentPath.RemoveAt(currentPath.Count - 1);

//                        // אם מצאנו פתרון, אין צורך לבדוק זמנים מאוחרים יותר
//                        if (foundAny)
//                            break;
//                    }
//                }
//            }

//            // אסטרטגיה 4: נסה לשנות את הסדר אצל מתאמנים אחרים
//            // זו האסטרטגיה הכי אגרסיבית - להזיז תרגילים אצל מתאמנים אחרים
//            // מימוש חסר - צריך להכניס פה את הקוד שמנסה להזיז מתאמנים אחרים

//            // שמירת התוצאה (אם לא מצאנו כלום, אז found=false)
//            endTime = foundAny ? bestEndTime : DateTime.MinValue;
//            memo[memoKey] = (foundAny, minAlternatives, bestPath, endTime);

//            return (foundAny, bestPath, endTime);
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
//    }
//}