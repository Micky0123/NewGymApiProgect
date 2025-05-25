//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using IBLL;
//using DTO;
//using DocumentFormat.OpenXml.Drawing;

//namespace BLL
//{
//    public class Michal
//    {
//        // טבלת תלת-ממד: [תרגיל יעד, תרגיל מקור, סלוט זמן]
//        private TableCell[,,] Table3D;
//        private int exerciseCount;
//        private int timeSlotsCount;

//        // מחלקה פנימית לתוצאה - מייצגת מסלול תרגילים עם זמן התחלה וסיום
//        public class PathResult
//        {
//            public List<int> ExerciseIdsInPath { get; set; } = new();
//            public DateTime StartTime { get; set; }
//            public DateTime EndTime { get; set; }
//        }
//        // מילון לתכנון דינאמי - שומר תוצאות של מצבים שכבר חושבו (עבור memoization)
//        private Dictionary<(int mask, int lastNodeId, long timeMinutes), (bool found, int numAlternatives, List<int> bestPath, DateTime endTime)> memo = new();

//        Dictionary<int, HashSet<int>> BuildReachabilityMatrix(IEnumerable<Node> nodes)
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
//            //כי בעצם אם אין קשת ישירה והם באותה קבוצת שרירים זה אומר שהפונקציה בעצם עוברת בקשת של התרגיל חזרה ושריר ומהשריר לתרגיל השני ואסור לעשות את זה
//            if (currentMuscleNode == nextMuscleNode)
//                return false;

//            // חיפוש מסלול בין צמתים מסוג MuscleGroup באמצעות BFS
//            var visited = new HashSet<Node>();
//            var queue = new Queue<Node>();

//            // התחל עם הצומת הנוכחי
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

//                    // הוסף לתור ולסט הביקור
//                    queue.Enqueue(neighbor);
//                    visited.Add(neighbor);
//                }
//            }

//            // אם לא נמצא מסלול, החזר false
//            return false;
//        }

//        //טבלהההההההה
//        // תא טבלה: מייצג מעבר מתרגיל לתרגיל בשעת סלוט מסוימת
//        public class TableCell
//        {
//            public int LegalityValue { get; set; } // לדוג' 2^שריר, -1, 0
//            public Queue<Trainee> Queue { get; set; } = new Queue<Trainee>();
//        }

//        //פונקציות של הטבלה התלת מימדית

//        // יצירת טבלה תלת-ממדית
//        public void InitTable(List<ExerciseNode> exercises, int slotsCount)
//        {
//            exerciseCount = exercises.Count;
//            timeSlotsCount = slotsCount;
//            Table3D = new TableCell[exerciseCount, exerciseCount, timeSlotsCount];

//            //for (int to = 0; to < exerciseCount; to++)
//            //    for (int from = 0; from < exerciseCount; from++)
//            //        for (int slot = 0; slot < timeSlotsCount; slot++)
//            //        {
//            //            Table3D[to, from, slot] = new TableCell();

//            //            // קביעת ערך החוקיות
//            //            if (to == from)
//            //                Table3D[to, from, slot].LegalityValue = 0;
//            //            else if (exercises[from].Neighbors.Contains(exercises[to]))
//            //                Table3D[to, from, slot].LegalityValue = (int)Math.Pow(2, exercises[to].MuscleGroup);
//            //            else
//            //                Table3D[to, from, slot].LegalityValue = -1;
//            //        }
//            for (int to = 0; to < exerciseCount; to++)
//                for (int from = 0; from < exerciseCount; from++)
//                {
//                    // מחשב פעם אחת עבור כל (to, from)
//                    int legality;
//                    if (to == from)
//                        legality = 0;
//                    else if (exercises[from].Neighbors.Contains(exercises[to]))
//                        legality = (int)Math.Pow(2, exercises[to].MuscleGroup);
//                    else
//                        legality = -1;

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
//        private int TimeToSlot(DateTime startTime, DateTime slotTime, int slotMinutes)
//        {
//            return (int)((slotTime - startTime).TotalMinutes / slotMinutes);
//        }

//        // בדיקת זמינות
//        private bool IsAvailable(int to, int from, int slot, int maxInQueue = 1)
//        {
//            return Table3D[to, from, slot].Queue.Count < maxInQueue;
//        }

//        // שיבוץ מתאמן
//        private void AddTrainee(int to, int from, int slot, Trainee trainee)
//        {
//            Table3D[to, from, slot].Queue.Enqueue(trainee);
//        }

//        // הסרת מתאמן (Backtrack)
//        private void RemoveTrainee(int to, int from, int slot)
//        {
//            if (Table3D[to, from, slot].Queue.Count > 0)
//                Table3D[to, from, slot].Queue.Dequeue();
//        }

//        // בודק האם המעבר מתרגיל from ל־to בסלוט מסוים חוקי (לפי ערך חוקיות בטבלה)
//        private bool IsLegalMove(int to, int from, int slot)
//        {
//            return Table3D[to, from, slot].LegalityValue != -1;
//        }

//        // בודק מי המתאמן בתור של תא [to, from, slot] (נניח שיש רק אחד בתור)
//        private Trainee GetOccupyingTrainee(int to, int from, int slot)
//        {
//            if (Table3D[to, from, slot].Queue.Count > 0)
//                return Table3D[to, from, slot].Queue.Peek();
//            return null;
//        }

//        // בודק אם אפשר לדחות את התרגיל לסלוט הבא
//        private int? FindNextAvailableSlot(int to, int from, DateTime currentTime, DateTime startTime, int slotMinutes, int maxInQueue)
//        {
//            int currentSlot = TimeToSlot(startTime, currentTime, slotMinutes);
//            for (int slot = currentSlot + 1; slot < timeSlotsCount; slot++)
//            {
//                if (IsLegalMove(to, from, slot) && IsAvailable(to, from, slot, maxInQueue))
//                    return slot;
//            }
//            return null;
//        }
//        //צריך לקבל:
//        //3D
//        //במקום רשימה של חילופים

//        public PathResult FindValidPath(SubGraph subGraph,//הגרף של המתאמן 
//                                  List<int> exerciseOrder,//רשימת התרגילים מהתוכנית הדיפולטיבית
//                                                          //Dictionary<int, List<int>> alternativeDevices,//מילון של תרגילים חילופיים
//                                                          //List<ExerciseDTO> exercises,//רשימה של כל התרגילים
//                                  TableCell tableCell, //3D
//                                  DateTime startTime)//תאריך התחלה
//        {
//            //// שלב 1: לבנות מילון צמתים לגישה מהירה
//            var nodeDict = subGraph.Nodes.ToDictionary(n => n.Id);
//            //לא צריך כי רשום בטבלה********   :)
//            //// שלב 2: לבנות מטריצת נגישות פעם אחת בלבד!
//            //Dictionary<int, HashSet<int>> reachableFromNode = BuildReachabilityMatrix(nodeDict.Values);







//            int mask = 0; // ייצוג בינארי של אילו תרגילים כבר בוצעו
//            var path = new List<int>(); // המסלול הנבחר
//            DateTime endTime = DateTime.MinValue; // ישמר זמן הסיום של המסלול

//            // שלב 3: קריאה לאלגוריתם הרקורסיבי שמחפש מסלול חוקי
//            var (found, bestPath, finalTime) = BacktrackWithPriority(
//                subGraph,
//                nodeDict,
//                reachableFromNode,
//                exerciseOrder,
//                new Dictionary<int, List<int>>(), // אם יש לך תרגילים חילופיים, תעביר כאן
//                mask,
//                path,
//                0,           // מספר חילופים בשלב התחלתי
//                startTime,
//                out endTime
//            );

//            // אם לא נמצא מסלול, נזרוק חריגה
//            if (!found || bestPath == null)
//                throw new Exception("לא נמצא מסלול חוקי לפי ההגבלות.");

//            // עדכון תורי זמינות (הוספת המתאמן לכל מכשיר במסלול בזמנים המתאימים)
//            DateTime currentTime = startTime;
//            foreach (var machineId in bestPath)
//            {
//                var machineNode = nodeDict[machineId];
//                machineNode.AddToQueue(currentTime);
//                currentTime = currentTime.AddMinutes(machineNode.UsageDurationMinutes);
//            }

//            // בניית תוצאת המסלול המלא
//            return new PathResult
//            {
//                ExerciseIdsInPath = bestPath,
//                StartTime = startTime,
//                EndTime = finalTime
//            };
//        }


//        private (bool found, List<int> bestPath, DateTime endTime) BacktrackWithPriority(
//                //SubGraph graph,                                  // הגרף
//                Dictionary<int, Node> nodeDict,                  // מילון צמתים לגישה מהירה
//                                                                 //Dictionary<int, HashSet<int>> reachableFromNode,// מילון צמתים לגישה מהירה
//                List<int> exerciseOrder,                         // סדר התרגילים
//                                                                 //Dictionary<int, List<int>> alternativeDevices,   // תרגילים חילופיים (אם יש)
//                TableCell tableCell, //3D יש כאן איזשהיא בעיה
//                int slotsCount,
//                int mask,                                       // אילו תרגילים בוצעו (bitmask)
//                List<int> currentPath,                           // המסלול שנבנה עד כה
//                int currentAlternatives,                         // מספר חילופים עד כה
//                DateTime currentTime,                            // הזמן הנוכחי
//                out DateTime endTime,                            // הזמן שבו הסתיים המסלול
//                List<Trainee> allTrainees, // כל המתאמנים במערכת
//                Trainee currentTrainee     // המתאמן הנוכחי
//)
//        {
//            Table3D = new TableCell[exerciseCount, exerciseCount, timeSlotsCount];

//            int lastNodeId = currentPath.Count > 0 ? currentPath.Last() : -1; // מזהה התרגיל האחרון
//            long timeMinutes = (long)(currentTime - DateTime.MinValue).TotalMinutes; // ייצוג הזמן במספר שלם
//            long roundedTimeMinutes = timeMinutes - (timeMinutes % slotsCount); // עיגול ל-5 דקות*********************************
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


//            //#################דבר ראשון הוא בודק אם הוא יכול לשונת את סדר האימון שלו אם כן מצוין פשוט תשנה#########################


//            // 1. קודם כל – ננסה להזיז/לשנות אצל המתאמן הנוכחי בלבד
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                int nodeId = exerciseOrder[i];
//                if ((mask & (1 << nodeId)) != 0)// אם התרגיל בוצע כבר - דלג
//                    continue;

//                // שליפה מהירה של הצומת מהמילון
//                if (!nodeDict.TryGetValue(nodeId, out var node))
//                    continue;

//                ///במקום זה מעבר על כל הטור של האינדקס של התרגיל הנוכחי
//                ///במידה והאינדקס שבטבלה הוא התרגיל....
//                bool isAvailableNode = true;
//                for (int j = 0; j < exerciseOrder.Count; j++)
//                {
//                    // דילוג על תרגילים שכבר נבחרו או על הצומת הנוכחי
//                    if ((mask & (1 << j)) != 0 || j == nodeId)
//                        continue;

//                    if (Table3D[i, j, 0].LegalityValue <= 0)
//                        isAvailableNode = false;
//                }
//                if (!isAvailableNode)
//                    continue;//זה אומר שהתרגיל הזה בכלל לא יכול להיות כאן


//                // בדוק זמינות אצל המתאמן הנוכחי בלבד
//                if (node.IsAvailableAt(currentTime))//כאן שליחה לפונקציה שבודקת אם זמין בתור*****************
//                {
//                    // נסה להמשיך במסלול
//                    currentPath.Add(nodeId);
//                    var result = BacktrackWithPriority(
//                         nodeDict, exerciseOrder, Table3D, slotsCount,
//                         mask | (1 << nodeId), currentPath,
//                        currentAlternatives, currentTime.AddMinutes(node.UsageDurationMinutes),
//                        out DateTime candEndTime, allTrainees, currentTrainee
//                    );
//                    if (result.found)
//                    {
//                        //הייייי לא צריך כאן חילופי כי הוא לא מחליף הוא רק מוצא מסלול שונה
//                        endTime = candEndTime;
//                        return result;
//                    }
//                    currentPath.RemoveAt(currentPath.Count - 1);
//                }
//            }

//            //############### אחרי שהוא בדק אם הוא יכול לשנות סדר והוא לא הצליח  #############################################
//            //############### הוא עובר על כל התרגילים האפשריים במקום התרגיל שלא זמיו #########################################
//            //############### אם הוא מוצא תרגיל פנוי הוא לוקח אותו ############################################################
//            //############### אם הוא לא מוצא תרגיל פנוי הוא בודק האם המתאמן שתופס את התרגיל יכול להחליף את הסדר של התרגילים שלו ####
//            //###############  אם הוא יכול להחליף אז מחליפים עם המתאמן השני וממשיכים לבדוק מסלול חוקי ########################
//            //############### במידה ולא נמצא מסלול חוקי האלגוריתם מנסה תרגיל אחר אם הוא זמין הוא לוקח וחוזר על עצמו ##########

//            //@@@@@@@@ איך המתאמן השני יודעה אם יש לו מסלול? @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
//            //@@@@@@@@ הוא שולח את הרשימה של התרגילים הבאה שלו ל backtracking @@@@@@@@@@@@@@@@@
//            //@@@@@@@@ שבודק רק אם ניתן להחליף את הסדר של האימון שלו @@@@@@@@@@@@@@@@@@@@@@@@@@



//            // 2. רק אם לא הצלחנו – ננסה להזיז למתאמנים אחרים
//            // (למשל, נניח שתרגיל מסוים תפוס ע"י מתאמן אחר)
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                int nodeId = exerciseOrder[i];
//                if ((mask & (1 << nodeId)) != 0)
//                    continue;
//                //  var node = nodeDict[nodeId];
//                // שליפה מהירה של הצומת מהמילון
//                if (!nodeDict.TryGetValue(nodeId, out var node))
//                    continue;


//                if (node.IsAvailableAt(currentTime))
//                {
//                    // נסה להמשיך במסלול
//                    currentPath.Add(nodeId);
//                    var result = BacktrackWithPriority(
//                         nodeDict, exerciseOrder, Table3D, slotsCount,
//                         mask | (1 << nodeId), currentPath,
//                        currentAlternatives, currentTime.AddMinutes(node.UsageDurationMinutes),
//                        out DateTime candEndTime, allTrainees, currentTrainee
//                    );
//                    if (result.found)
//                    {
//                        //הייייי לא צריך כאן חילופי כי הוא לא מחליף הוא רק מוצא מסלול שונה
//                        endTime = candEndTime;
//                        return result;
//                    }
//                    currentPath.RemoveAt(currentPath.Count - 1);
//                }
//                else
//                {
//                    // נניח שהמכשיר תפוס ע"י מתאמן אחר
//                    Trainee otherTrainee = GetOccupyingTraineeAt(lastNodeId, exerciseOrder[i + 1] , slotsCount);
//                    if (otherTrainee != null && otherTrainee != currentTrainee)
//                    {
//                        // נסה להזיז את התרגיל אצל המתאמן השני (רק אם זה חוקי)
//                        if (otherTrainee.CanRescheduleExercise(nodeId, currentTime))
//                        {
//                            // הזז זמנית את התרגיל אצל המתאמן השני
//                            otherTrainee.RescheduleExercise(nodeId, currentTime);

//                            // נסה שוב להמשיך במסלול
//                            currentPath.Add(nodeId);
//                            var result = BacktrackWithPriority(
//                                 nodeDict, exerciseOrder, Table3D, slotsCount,
//                                 mask | (1 << nodeId), currentPath,
//                                currentAlternatives, currentTime.AddMinutes(node.UsageDurationMinutes),
//                                out DateTime candEndTime, allTrainees, currentTrainee
//                            );
//                            if (result.found)
//                            {
//                                endTime = candEndTime;
//                                return result;
//                            }
//                            currentPath.RemoveAt(currentPath.Count - 1);

//                            // החזר את השינוי במתאמן השני (rollback)
//                            otherTrainee.UndoReschedule(nodeId, currentTime);
//                        }
//                    }

//                }
//            }

//            // אם לא הצלחנו בשום דרך, נחזיר כישלון
//            endTime = DateTime.MinValue;
//            return (false, null, DateTime.MinValue);

//        }

//        // בגרסה של טבלה תלת-ממדית
//        // מחזיר את המתאמן הראשון בתור עבור תא מסויים (בדרך כלל זה המתאמן שמבצע עכשיו)
//        public Trainee GetOccupyingTraineeAt(int to, int from, int slot)
//        {
//            var queue = Table3D[to, from, slot].Queue;
//            if (queue.Count > 0)
//                return queue.Peek(); // הראשון בתור
//            return null;
//        }

//        // בודק האם ניתן לדחות מתאמן מסויים לשעה/סלוט אחר (למשל, אם יש מקום פנוי)
//        // כאן: בודק אם אפשר למצוא סלוט פנוי עבור אותו מעבר
//        public bool CanRescheduleExercise(int to, int from, int currentSlot, int maxInQueue)
//        {
//            //כאן הוא בודק שינוי בתוכנית אימון של המתאמנים האחרים*****************************
//            // נניח שמותר רק לדחות לסלוט הבא/הבא אחריו
//            //for (int slot = currentSlot + 1; slot < timeSlotsCount; slot++)
//            //{
//            //    if (Table3D[to, from, slot].Queue.Count < maxInQueue)
//            //        return true;
//            //}
//            return false;
//        }


//        //אלגוריתם למציאת מסלול תקין 











//        // מזיז את המתאמן מהתור של סלוט אחד לסלוט אחר
//        public bool RescheduleExercise(int to, int from, int oldSlot, int maxInQueue, Trainee trainee)
//        {
//            // נסה למצוא סלוט פנוי
//            for (int newSlot = oldSlot + 1; newSlot < timeSlotsCount; newSlot++)
//            {
//                var newQueue = Table3D[to, from, newSlot].Queue;
//                if (newQueue.Count < maxInQueue)
//                {
//                    // מסיר מהתור הישן
//                    var oldQueue = Table3D[to, from, oldSlot].Queue;
//                    var tempList = oldQueue.ToList();
//                    tempList.Remove(trainee);
//                    Table3D[to, from, oldSlot].Queue = new Queue<Trainee>(tempList);

//                    // מוסיף לתור החדש
//                    newQueue.Enqueue(trainee);
//                    return true;
//                }
//            }
//            return false;
//        }

//        //מעולההההההההה
//        // מחזיר את המתאמן לסלוט/תור הקודם (למשל ב-backtrack)
//        public void UndoReschedule(int to, int from, int newSlot, int oldSlot, Trainee trainee)
//        {
//            // מסיר מהתור החדש
//            var newQueue = Table3D[to, from, newSlot].Queue;
//            var tempListNew = newQueue.ToList();
//            tempListNew.Remove(trainee);
//            Table3D[to, from, newSlot].Queue = new Queue<Trainee>(tempListNew);

//            // מוסיף חזרה לתור הישן
//            Table3D[to, from, oldSlot].Queue.Enqueue(trainee);
//        }


//        // פונקציית עזר - סופרת כמה תרגילים בוצעו ב-mask
//        private int CountBits(int mask)
//        {
//            int count = 0;
//            while (mask != 0)
//            {
//                count += mask & 1;
//                mask >>= 1;
//            }
//            return count;
//        }




//    }
//}
