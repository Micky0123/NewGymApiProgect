//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using DBEntities.Models;
//using IBLL;
//using Microsoft.Azure.Management.ResourceManager.Fluent.Core.DAG;

//namespace BLL
//{
//    public class PathFinderServiceBLL  /*IPathFinderService*/
//    {
//        // מחלקה פנימית לתוצאה - מייצגת מסלול תרגילים עם זמן התחלה וסיום
//        public class PathResult
//        {
//            public List<int> ExerciseIdsInPath { get; set; } = new();
//            public DateTime StartTime { get; set; }
//            public DateTime EndTime { get; set; }
//        }

//        // מילון לתכנון דינאמי - שומר תוצאות של מצבים שכבר חושבו (עבור memoization)
//        private Dictionary<(int mask, int lastNodeId, long timeMinutes), (bool found, int numAlternatives, List<int> bestPath, DateTime endTime)> memo = new();

//        // פונקציה שמוצאת מסלול חוקי עבור המתאמן, בהתחשב בתוכנית התרגילים והזמנים
//        public PathResult FindValidPath(SubGraph subGraph,//הגרף של המתאמן 
//                                        List<int> exerciseOrder,//רשימת התרגילים מהתוכנית הדיפולטיבית
//                                        Dictionary<int, List<int>> alternativeDevices,//מילון של תרגילים חילופיים
//                                        DateTime startTime)//תאריך התחלה
//        {
//            // שלב 1: לבנות מילון צמתים לגישה מהירה
//            var nodeDict = subGraph.Nodes.ToDictionary(n => n.Id);

//            // שלב 2: לבנות מטריצת נגישות פעם אחת בלבד!
//            Dictionary<int, HashSet<int>> reachableFromNode = BuildReachabilityMatrix(nodeDict.Values);

//            int mask = 0; // ייצוג בינארי של אילו תרגילים כבר בוצעו
//            var path = new List<int>(); // המסלול הנבחר
//            DateTime endTime = DateTime.MinValue; // ישמר זמן הסיום של המסלול

//            // שלב 3: קריאה לאלגוריתם הרקורסיבי שמחפש מסלול חוקי
//            var (found, numAlt, bestPath, finalTime) = Backtrack(
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

//        /// האלגוריתם: Backtracking + Memoization
//        // אלגוריתם רקורסיבי למציאת מסלול חוקי(עם memoization)
//        private (bool found, int numAlternatives, List<int> bestPath, DateTime endTime) Backtrack(
//            SubGraph graph,                                  // הגרף
//            Dictionary<int, Node> nodeDict,                  // מילון צמתים לגישה מהירה
//            Dictionary<int, HashSet<int>> reachableFromNode,// מילון צמתים לגישה מהירה
//            List<int> exerciseOrder,                         // סדר התרגילים
//            Dictionary<int, List<int>> alternativeDevices,   // תרגילים חילופיים (אם יש)
//            int mask,                                       // אילו תרגילים בוצעו (bitmask)
//            List<int> currentPath,                           // המסלול שנבנה עד כה
//            int currentAlternatives,                         // מספר חילופים עד כה
//            DateTime currentTime,                            // הזמן הנוכחי
//            out DateTime endTime)                            // הזמן שבו הסתיים המסלול
//        {

//            int lastNodeId = currentPath.Count > 0 ? currentPath.Last() : -1; // מזהה התרגיל האחרון
//            long timeMinutes = (long)(currentTime - DateTime.MinValue).TotalMinutes; // ייצוג הזמן במספר שלם
//            long roundedTimeMinutes = timeMinutes - (timeMinutes % 5); // עיגול ל-5 דקות*********************************
//            var memoKey = (mask, lastNodeId, roundedTimeMinutes); // מפתח ייחודי למצב


//            // בדיקה האם כבר חישבנו תוצאה עבור המצב הזה (memoization)
//            if (memo.TryGetValue(memoKey, out var memoResult))
//            {
//                endTime = memoResult.endTime;
//                return (memoResult.found, memoResult.numAlternatives, memoResult.bestPath, memoResult.endTime);
//            }

//            // תנאי עצירה: אם כל התרגילים בוצעו
//            if (CountBits(mask) == exerciseOrder.Count)
//            {
//                endTime = currentTime;
//                return (true, currentAlternatives, new List<int>(currentPath), currentTime);
//            }

//            bool foundAny = false;
//            int minAlternatives = int.MaxValue;
//            List<int> bestPath = null;
//            DateTime bestEndTime = DateTime.MinValue;

//            // מעבר על כל התרגילים שעדיין לא בוצעו
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                int nodeId = exerciseOrder[i];
//                if ((mask & (1 << nodeId)) != 0)// אם התרגיל בוצע כבר - דלג
//                    continue;

//                // שליפה מהירה של הצומת מהמילון
//                if (!nodeDict.TryGetValue(nodeId, out var node))
//                    continue;

//                // בדיקה האם ניתן להגיע לכל שאר התרגילים שטרם בוצעו מהצומת הנוכחי
//                bool isAvailableNode = exerciseOrder
//                    .Where(e => (mask & (1 << e)) == 0 && e != nodeId)
//                    .All(nextNode => reachableFromNode[node.Id].Contains(nextNode));
//                if (!isAvailableNode)
//                    continue;

//                // אם התרגיל זמין כעת - ננסה להוסיף אותו למסלול
//                if (node.IsAvailableAt(currentTime))
//                {
//                    currentPath.Add(nodeId);
//                    var (found, alternatives, pathCandidate, candidateEndTime) = Backtrack(
//                        graph, nodeDict, reachableFromNode, exerciseOrder, alternativeDevices,
//                        mask | (1 << nodeId), currentPath, currentAlternatives,
//                        currentTime.AddMinutes(node.UsageDurationMinutes),
//                        out DateTime candEndTime
//                    );
//                    if (found && alternatives < minAlternatives)
//                    {
//                        foundAny = true;
//                        minAlternatives = alternatives;
//                        bestPath = new List<int>(pathCandidate);
//                        bestEndTime = candEndTime;

//                        // אם מצאנו מסלול בלי אף תרגיל חילופי - אין צורך להמשיך, זה הכי טוב שאפשר
//                        if (alternatives == 0)
//                        {
//                            break; // יוצאים מהלולאה כי זה מסלול אופטימלי
//                        }
//                    }
//                    currentPath.RemoveAt(currentPath.Count - 1);
//                }
//                // אם לא זמין - נבדוק תרגילים חילופיים
//                else if (alternativeDevices.TryGetValue(nodeId, out var alternativesList))
//                {
//                    foreach (var altId in alternativesList)
//                    {
//                        if ((mask & (1 << altId)) != 0)
//                            continue;
//                        if (!nodeDict.TryGetValue(altId, out var altNode) || !altNode.IsAvailableAt(currentTime))
//                            continue;
//                        currentPath.Add(altId);
//                        var (found, alternatives, pathCandidate, candidateEndTime) = Backtrack(
//                            graph, nodeDict, reachableFromNode, exerciseOrder, alternativeDevices,
//                            mask | (1 << nodeId) | (1 << altId), currentPath, currentAlternatives + 1,
//                            currentTime.AddMinutes(altNode.UsageDurationMinutes),
//                            out DateTime candEndTime
//                        );
//                        if (found && alternatives < minAlternatives)
//                        {
//                            foundAny = true;
//                            minAlternatives = alternatives;
//                            bestPath = new List<int>(pathCandidate);
//                            bestEndTime = candEndTime;

//                            // אם מצאנו מסלול בלי אף תרגיל חילופי - אין צורך להמשיך, זה הכי טוב שאפשר
//                            if (alternatives == 0)
//                            {
//                                break; // יוצאים מהלולאה כי זה מסלול אופטימלי
//                            }
//                        }
//                        currentPath.RemoveAt(currentPath.Count - 1);
//                    }
//                }
//            }

//            // שמירת התוצאה במילון לשימוש עתידי (memoization)
//            endTime = bestEndTime;
//            var result = foundAny ? (true, minAlternatives, bestPath, bestEndTime) : (false, 0, null, DateTime.MinValue);
//            memo[memoKey] = result;
//            return result;
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

//        Dictionary<int, HashSet<int>> BuildReachabilityMatrix(IEnumerable<Node> nodes)
//        {
//            var nodesList = nodes.ToList();
//            var result = new Dictionary<int, HashSet<int>>();
//            //foreach (var node in nodes)
//            //{
//            //    var reachable = new HashSet<int>();
//            //    // BFS או DFS מהצומת הזה
//            //    var queue = new Queue<Node>();
//            //    var visited = new HashSet<int>();
//            //    queue.Enqueue(node);
//            //    while (queue.Count > 0)
//            //    {
//            //        var curr = queue.Dequeue();
//            //        foreach (var neighbor in curr.Neighbors) // נניח שיש רשימת שכנים לכל Node
//            //        {
//            //            if (visited.Add(neighbor.Id))
//            //            {
//            //                reachable.Add(neighbor.Id);
//            //                queue.Enqueue(neighbor);
//            //            }
//            //        }
//            //    }
//            //    result[node.Id] = reachable;
//            //}
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

//        private Node CanReachNode(SubGraph graph, Node targetNodeId, List<int> remainingNodes)
//        {
//            // רשימת צמתים לביקור
//            var toVisit = new Queue<int>(remainingNodes);
//            var visited = new HashSet<int>();

//            while (toVisit.Count > 0)
//            {
//                var currentNodeId = toVisit.Dequeue();

//                // הוסף את הצומת הנוכחי לרשימת ביקור
//                visited.Add(currentNodeId);

//                // מצא את הצומת הנוכחי בתוך הגרף
//                var currentNode = graph.Nodes.FirstOrDefault(n => n.Id == currentNodeId);

//                if (currentNode != null)
//                {
//                    //אני צריכה לבדוק כאן על התרגיל המבוקש האם ניתן לגשת אליו באיזו שהיא דרך מהצומת הנוכחי
//                    var validPath = HasPathToNextExercise(targetNodeId, currentNode);
//                    if (validPath != null)
//                    {
//                        return currentNode; // מחזירים את הצומת הנוכחי
//                    }

//                }
//            }

//            // אם לא מצאנו דרך לצומת היעד
//            return null; // מחזירים null אם לא מצאנו
//        }
//        private int? FindNodeWithNeighborToTarget(SubGraph graph, int targetNodeId, List<int> remainingNodes)
//        {
//            foreach (var nodeId in remainingNodes)
//            {
//                var currentNode = graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
//                if (currentNode != null)
//                {
//                    foreach (var neighbor in currentNode.Neighbors)
//                    {
//                        if (neighbor.Id == targetNodeId)
//                        {
//                            return currentNode.Id;
//                        }
//                    }
//                }
//            }
//            // אם לא נמצא כזה צומת
//            return null;
//        }

//        private List<Node> GetMuscleGroupPath(Node currentNode, Node nextExerciseNode)
//        {
//            // מצא את הצומת מסוג MuscleGroup שמחובר לתרגיל הנוכחי
//            var currentMuscleNode = currentNode.Neighbors.FirstOrDefault(n => n.Type == NodeType.MuscleGroup);
//            var nextMuscleNode = nextExerciseNode.Neighbors.FirstOrDefault(n => n.Type == NodeType.MuscleGroup);

//            // אם אין MuscleGroup שמחוברים לתרגיל הנוכחי או לתרגיל הבא, אין מסלול
//            if (currentMuscleNode == null || nextMuscleNode == null)
//                return null;

//            // אם הצומת הנוכחי של MuscleGroup זהה לצומת של היעד, החזר אותו בלבד
//            if (currentMuscleNode == nextMuscleNode)
//                return new List<Node> { currentMuscleNode };

//            // חיפוש מסלול הקצר ביותר באמצעות BFS
//            var visited = new HashSet<Node>();
//            var queue = new Queue<(Node Node, List<Node> Path)>();

//            // הוספת הצומת ההתחלתי לתור
//            queue.Enqueue((currentMuscleNode, new List<Node> { currentMuscleNode }));
//            visited.Add(currentMuscleNode);

//            while (queue.Count > 0)
//            {
//                var (current, path) = queue.Dequeue();

//                // עבור כל שכן מסוג MuscleGroup
//                foreach (var neighbor in current.Neighbors.Where(n => n.Type == NodeType.MuscleGroup))
//                {
//                    // דלג אם כבר ביקרנו בצומת
//                    if (visited.Contains(neighbor))
//                        continue;

//                    // יצירת מסלול חדש עם השכן הנוכחי
//                    var newPath = new List<Node>(path) { neighbor };

//                    // אם הגענו ליעד, החזר את המסלול
//                    if (neighbor == nextMuscleNode)
//                        return newPath;

//                    // הוספת השכן לתור ולסט הביקור
//                    queue.Enqueue((neighbor, newPath));
//                    visited.Add(neighbor);
//                }
//            }

//            // אם לא נמצא מסלול, החזר null
//            return null;
//        }

//        private (bool found, int numAlternatives, List<int> bestPath, DateTime endTime) BacktrackWithPriority(
//        SubGraph graph,
//        Dictionary<int, Node> nodeDict,
//        Dictionary<int, HashSet<int>> reachableFromNode,
//        List<int> exerciseOrder,
//        Dictionary<int, List<int>> alternativeDevices,
//        int mask,
//        List<int> currentPath,
//        int currentAlternatives,
//        DateTime currentTime,
//        out DateTime endTime,
//        List<Trainee> allTrainees, // כל המתאמנים במערכת
//        Trainee currentTrainee     // המתאמן הנוכחי
//)
//        {
//            // ... תנאי עצירה וכו' כמו אצלך

//            // 1. קודם כל – ננסה להזיז/לשנות אצל המתאמן הנוכחי בלבד
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                int nodeId = exerciseOrder[i];
//                if ((mask & (1 << nodeId)) != 0)
//                    continue;
//                var node = nodeDict[nodeId];

//                // בדוק זמינות אצל המתאמן הנוכחי בלבד
//                if (node.IsAvailableAt(currentTime))
//                {
//                    // נסה להמשיך במסלול
//                    currentPath.Add(nodeId);
//                    var result = BacktrackWithPriority(
//                        graph, nodeDict, reachableFromNode, exerciseOrder,
//                        alternativeDevices, mask | (1 << nodeId), currentPath,
//                        currentAlternatives, currentTime.AddMinutes(node.UsageDurationMinutes),
//                        out DateTime candEndTime, allTrainees, currentTrainee
//                    );
//                    if (result.found)
//                    {
//                        endTime = candEndTime;
//                        return result;
//                    }
//                    currentPath.RemoveAt(currentPath.Count - 1);
//                }
//                // ...בדוק חילופים אצל המתאמן הנוכחי עצמו (alternativeDevices)
//            }

//            // 2. רק אם לא הצלחנו – ננסה להזיז למתאמנים אחרים
//            // (למשל, נניח שתרגיל מסוים תפוס ע"י מתאמן אחר)
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                int nodeId = exerciseOrder[i];
//                if ((mask & (1 << nodeId)) != 0)
//                    continue;
//                var node = nodeDict[nodeId];

//                // נניח שהמכשיר תפוס ע"י מתאמן אחר
//                Trainee otherTrainee = node.GetOccupyingTraineeAt(currentTime);
//                if (otherTrainee != null && otherTrainee != currentTrainee)
//                {
//                    // נסה להזיז את התרגיל אצל המתאמן השני (רק אם זה חוקי)
//                    if (otherTrainee.CanRescheduleExercise(nodeId, currentTime))
//                    {
//                        // הזז זמנית את התרגיל אצל המתאמן השני
//                        otherTrainee.RescheduleExercise(nodeId, currentTime);

//                        // נסה שוב להמשיך במסלול
//                        currentPath.Add(nodeId);
//                        var result = BacktrackWithPriority(
//                            graph, nodeDict, reachableFromNode, exerciseOrder,
//                            alternativeDevices, mask | (1 << nodeId), currentPath,
//                            currentAlternatives, currentTime.AddMinutes(node.UsageDurationMinutes),
//                            out DateTime candEndTime, allTrainees, currentTrainee
//                        );
//                        if (result.found)
//                        {
//                            endTime = candEndTime;
//                            return result;
//                        }
//                        currentPath.RemoveAt(currentPath.Count - 1);

//                        // החזר את השינוי במתאמן השני (rollback)
//                        otherTrainee.UndoReschedule(nodeId, currentTime);
//                    }
//                }
//            }

//            // אם לא הצלחנו בשום דרך, נחזיר כישלון
//            endTime = DateTime.MinValue;
//            return (false, 0, null, DateTime.MinValue);
//        }


//        //private (bool found, int numAlternatives, List<int> bestPath) Backtrack(
//        //    SubGraph graph,
//        //    List<int> exerciseOrder,
//        //    Dictionary<int, List<int>> alternativeDevices,
//        //    HashSet<int> visitedExercises,
//        //    List<int> currentPath,
//        //    int currentAlternatives,
//        //    DateTime currentTime
//        //)
//        //{
//        //    if (visitedExercises.Count == exerciseOrder.Count)
//        //        return (true, currentAlternatives, new List<int>(currentPath));

//        //    bool foundAny = false;
//        //    int minAlternatives = int.MaxValue;
//        //    List<int> bestPath = null;

//        //    for (int i = 0; i < exerciseOrder.Count; i++)
//        //    {
//        //        int nodeId = exerciseOrder[i];
//        //        if (visitedExercises.Contains(nodeId))
//        //            continue;

//        //        var node = graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
//        //        if (node == null)
//        //            continue;

//        //        // ננסה קודם את התרגיל הרגיל
//        //        if (node.IsAvailableAt(currentTime))
//        //        {
//        //            visitedExercises.Add(nodeId);
//        //            currentPath.Add(nodeId);

//        //            var nextTime = currentTime.AddMinutes(node.UsageDurationMinutes);
//        //            var (found, alternatives, pathCandidate) = Backtrack(
//        //                graph, exerciseOrder, alternativeDevices, visitedExercises, currentPath, currentAlternatives, nextTime);

//        //            if (found && alternatives < minAlternatives)
//        //            {
//        //                foundAny = true;
//        //                minAlternatives = alternatives;
//        //                bestPath = new List<int>(pathCandidate);
//        //            }

//        //            visitedExercises.Remove(nodeId);
//        //            currentPath.RemoveAt(currentPath.Count - 1);
//        //        }
//        //        // ננסה תרגיל חילופי
//        //        else if (alternativeDevices.TryGetValue(nodeId, out var alternativesList))
//        //        {
//        //            foreach (var altId in alternativesList)
//        //            {
//        //                if (visitedExercises.Contains(altId))
//        //                    continue;
//        //                var altNode = graph.Nodes.FirstOrDefault(n => n.Id == altId);
//        //                if (altNode == null || !altNode.IsAvailableAt(currentTime))
//        //                    continue;

//        //                // שים לב - מחליפים ברשימה את המקורי בחילופי!
//        //                var oldValue = exerciseOrder[i];
//        //                exerciseOrder[i] = altId;


//        //                visitedExercises.Add(altId);
//        //                currentPath.Add(altId);

//        //                var nextTimeAlt = currentTime.AddMinutes(altNode.UsageDurationMinutes);
//        //                var (found, alternatives, pathCandidate) = Backtrack(
//        //                    graph, exerciseOrder, alternativeDevices, visitedExercises, currentPath, currentAlternatives + 1, nextTimeAlt);

//        //                if (found && alternatives < minAlternatives)
//        //                {
//        //                    foundAny = true;
//        //                    minAlternatives = alternatives;
//        //                    bestPath = new List<int>(pathCandidate);
//        //                }

//        //                // חזרה למצב קודם
//        //                exerciseOrder[i] = oldValue;
//        //                visitedExercises.Remove(altId);
//        //                currentPath.RemoveAt(currentPath.Count - 1);
//        //            }
//        //        }
//        //    }

//        //    if (foundAny)
//        //        return (true, minAlternatives, bestPath);
//        //    return (false, 0, null);
//        //}

//        //private Dictionary<(int mask, int lastNodeId, long timeMinutes), (bool found, int numAlternatives, List<int> bestPath)> memo = new();



//        //private (bool found, int numAlternatives, List<int> bestPath) Backtrack(
//        //    SubGraph graph,
//        //    List<int> exerciseOrder,
//        //    Dictionary<int, List<int>> alternativeDevices,
//        //    int mask,
//        //    List<int> currentPath,
//        //    int currentAlternatives,
//        //    DateTime currentTime
//        //)
//        //{
//        //    int lastNodeId = currentPath.Count > 0 ? currentPath.Last() : -1;
//        //    long timeMinutes = (long)(currentTime - DateTime.MinValue).TotalMinutes;
//        //    var memoKey = (mask, lastNodeId, timeMinutes);

//        //    if (memo.TryGetValue(memoKey, out var memoResult))
//        //        return memoResult;

//        //    if (CountBits(mask) == exerciseOrder.Count)
//        //        return (true, currentAlternatives, new List<int>(currentPath));

//        //    bool foundAny = false;
//        //    int minAlternatives = int.MaxValue;
//        //    List<int> bestPath = null;

//        //    for (int i = 0; i < exerciseOrder.Count; i++)
//        //    {
//        //        int nodeId = exerciseOrder[i];
//        //        if ((mask & (1 << nodeId)) != 0)
//        //            continue;

//        //        var node = graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
//        //        if (node == null)
//        //            continue;

//        //        bool isAvilableNode = true;
//        //        foreach (var nextNode in exerciseOrder.Where(e => (mask & (1 << e)) == 0 && e != nodeId))
//        //        {
//        //            var nextExerciseNode = graph.Nodes.FirstOrDefault(n => n.Id == nextNode);
//        //            if (nextExerciseNode == null)
//        //                continue;
//        //            if (!HasPathToNextExercise(node, nextExerciseNode))
//        //            {
//        //                isAvilableNode = false;
//        //                break;
//        //            }
//        //        }
//        //        if (!isAvilableNode)
//        //            continue;

//        //        // תרגיל רגיל
//        //        if (node.IsAvailableAt(currentTime))
//        //        {
//        //            currentPath.Add(nodeId);
//        //            var (found, alternatives, pathCandidate) = Backtrack(
//        //                graph, exerciseOrder, alternativeDevices, mask | (1 << nodeId), currentPath, currentAlternatives, currentTime.AddMinutes(node.UsageDurationMinutes)
//        //            );
//        //            if (found && alternatives < minAlternatives)
//        //            {
//        //                foundAny = true;
//        //                minAlternatives = alternatives;
//        //                bestPath = new List<int>(pathCandidate);
//        //            }
//        //            currentPath.RemoveAt(currentPath.Count - 1);
//        //        }
//        //        // תרגיל חילופי
//        //        else if (alternativeDevices.TryGetValue(nodeId, out var alternativesList))
//        //        {
//        //            foreach (var altId in alternativesList)
//        //            {
//        //                if ((mask & (1 << altId)) != 0)
//        //                    continue;
//        //                var altNode = graph.Nodes.FirstOrDefault(n => n.Id == altId);
//        //                if (altNode == null || !altNode.IsAvailableAt(currentTime))
//        //                    continue;
//        //                currentPath.Add(altId);
//        //                var (found, alternatives, pathCandidate) = Backtrack(
//        //                    graph, exerciseOrder, alternativeDevices, mask | (1 << nodeId) | (1 << altId), currentPath, currentAlternatives + 1, currentTime.AddMinutes(altNode.UsageDurationMinutes)
//        //                );
//        //                if (found && alternatives < minAlternatives)
//        //                {
//        //                    foundAny = true;
//        //                    minAlternatives = alternatives;
//        //                    bestPath = new List<int>(pathCandidate);
//        //                }
//        //                currentPath.RemoveAt(currentPath.Count - 1);
//        //            }
//        //        }
//        //    }

//        //    var result = foundAny ? (true, minAlternatives, bestPath) : (false, 0, null);
//        //    memo[memoKey] = result;
//        //    return result;
//        //}

//        // עזר לספירת ביטים דולקים במסיכה

//        //// מחוץ לפונקציה (ברמת הקלאס)
//        //private Dictionary<string, (bool found, int numAlternatives, List<int> bestPath)> memo = new Dictionary<string, (bool, int, List<int>)>();

//        //// פונקציה ליצירת מפתח ייחודי למצב
//        //private string GetMemoKey(HashSet<int> visitedExercises, int lastNodeId, long timeMinutes)
//        //{
//        //    // מיון התרגילים שבוצעו כדי שהמפתח יהיה אחיד
//        //    var visitedArr = visitedExercises.OrderBy(x => x);
//        //    string visitedStr = string.Join(",", visitedArr);
//        //    return $"{visitedStr}|{lastNodeId}|{timeMinutes}";
//        //}

//        //// ברמת הקלאס:
//        //private Dictionary<(int mask, int lastNodeId, long timeMinutes), (bool found, int numAlternatives, List<int> bestPath)> memo1 = new();

//        //// פונקציה עוזרת: מחשבת את ה־mask הנוכחי מתוך visitedExercises
//        //private int GetMask(HashSet<int> visitedExercises)
//        //{
//        //    int mask = 0;
//        //    foreach (var ex in visitedExercises)
//        //        mask |= (1 << ex);
//        //    return mask;
//        //}


//        //private (bool found, int numAlternatives, List<int> bestPath) Backtrack(
//        //        SubGraph graph,
//        //        List<int> exerciseOrder,
//        //        Dictionary<int, List<int>> alternativeDevices,
//        //        HashSet<int> visitedExercises,
//        //        List<int> currentPath,
//        //        int currentAlternatives,
//        //        DateTime currentTime
//        //    )
//        //{
//        //    // קביעת מזהה ייחודי למצב הנוכחי
//        //    int lastNodeId = currentPath.Count > 0 ? currentPath.Last() : -1;
//        //    long timeMinutes = (long)(currentTime - DateTime.MinValue).TotalMinutes;
//        //    string memoKey = GetMemoKey(visitedExercises, lastNodeId, timeMinutes);

//        //    // בדיקה אם כבר חישבנו תוצאה עבור המצב הזה
//        //    if (memo.TryGetValue(memoKey, out var memoResult))
//        //        return memoResult;

//        //    //int lastNodeId = currentPath.Count > 0 ? currentPath.Last() : -1;
//        //    //int mask = GetMask(visitedExercises);
//        //    //long timeMinutes = (long)(currentTime - DateTime.MinValue).TotalMinutes;

//        //    //var memoKey = (mask, lastNodeId, timeMinutes);

//        //    //// בדיקת זיכרון
//        //    //if (memo.TryGetValue(memoKey, out var memoResult))
//        //    //    return memoResult;





//        //    // תנאי עצירה: אם כל התרגילים בוצעו (במקור או בחילופי)
//        //    if (visitedExercises.Count == exerciseOrder.Count)
//        //        return (true, currentAlternatives, new List<int>(currentPath));

//        //    bool foundAny = false;
//        //    int minAlternatives = int.MaxValue;
//        //    List<int> bestPath = null;

//        //    for (int i = 0; i < exerciseOrder.Count; i++)
//        //    {
//        //        int nodeId = exerciseOrder[i];
//        //        if (visitedExercises.Contains(nodeId))
//        //            continue;

//        //        var node = graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
//        //        if (node == null)
//        //            continue;

//        //        bool isAvilableNode = true;
//        //        //מעבר על כל התרגילים שלא עברו עליהם ובדיקה האם יש תרגיל שלא ניתן להגיע אליו מהצומת הנוכחית
//        //        //כאן צריך להוציא לפונקציה חיצונית ששומרת את האפשרויות בתכנון דינאמי
//        //        foreach(var nextNode in exerciseOrder.Where(e => !visitedExercises.Contains(e) && e != nodeId))
//        //        {
//        //            var nextExerciseNode = graph.Nodes.FirstOrDefault(n => n.Id == nextNode);
//        //            if (nextExerciseNode == null)
//        //                continue;
//        //            var hasPath = HasPathToNextExercise(node, nextExerciseNode);
//        //            if (!hasPath)
//        //            {
//        //                isAvilableNode = false;
//        //                break;
//        //            }
//        //        }
//        //        if (!isAvilableNode)
//        //        {
//        //            continue;
//        //        }

//        //        // ננסה קודם את התרגיל הרגיל
//        //        if (node.IsAvailableAt(currentTime))
//        //        {
//        //            visitedExercises.Add(nodeId);
//        //            currentPath.Add(nodeId);

//        //            DateTime nextTime = currentTime.AddMinutes(node.UsageDurationMinutes);
//        //            var (found, alternatives, pathCandidate) = Backtrack(
//        //                graph, exerciseOrder, alternativeDevices, visitedExercises, currentPath, currentAlternatives, nextTime);

//        //            if (found && alternatives < minAlternatives)
//        //            {
//        //                foundAny = true;
//        //                minAlternatives = alternatives;
//        //                bestPath = new List<int>(pathCandidate);
//        //            }

//        //            visitedExercises.Remove(nodeId);
//        //            currentPath.RemoveAt(currentPath.Count - 1);
//        //        }
//        //        // אם אינו זמין ננסה תרגיל חילופי
//        //        else if (alternativeDevices.TryGetValue(nodeId, out var alternativesList))
//        //        {
//        //            foreach (var altId in alternativesList)
//        //            {
//        //                if (visitedExercises.Contains(altId))
//        //                    continue;
//        //                var altNode = graph.Nodes.FirstOrDefault(n => n.Id == altId);
//        //                if (altNode == null || !altNode.IsAvailableAt(currentTime))
//        //                    continue;
//        //                visitedExercises.Add(nodeId); // מסמן שביצענו את התרגיל הזה ע"י חילופי
//        //                visitedExercises.Add(altId);
//        //                currentPath.Add(altId);

//        //                DateTime nextTimeAlt = currentTime.AddMinutes(altNode.UsageDurationMinutes);
//        //                var (found, alternatives, pathCandidate) = Backtrack(
//        //                    graph, exerciseOrder, alternativeDevices, visitedExercises, currentPath, currentAlternatives + 1, nextTimeAlt);

//        //                if (found && alternatives < minAlternatives)
//        //                {
//        //                    foundAny = true;
//        //                    minAlternatives = alternatives;
//        //                    bestPath = new List<int>(pathCandidate);
//        //                }

//        //                visitedExercises.Remove(nodeId);
//        //                visitedExercises.Remove(altId);
//        //                currentPath.RemoveAt(currentPath.Count - 1);
//        //            }
//        //        }
//        //    }

//        //    // שמירת התוצאה ב-memo כדי למנוע חישוב חוזר
//        //    var result = foundAny ? (true, minAlternatives, bestPath) : (false, 0, null);
//        //    memo[memoKey] = result;
//        //    return result;

//        //    //var result = foundAny ? (true, minAlternatives, bestPath) : (false, 0, null);
//        //    //memo[memoKey] = result;
//        //    //return result;



//        //    //if (foundAny)
//        //    //    return (true, minAlternatives, bestPath);
//        //    //return (false, 0, null);
//        //}

//        // בנה מטריצת נגישות מראש (פעם אחת בתחילת האלגוריתם)


//        // פונקציה עזר:
//        // פונקציה שמוצאת מסלול חוקי עבור המתאמן, בהתחשב בתוכנית התרגילים והזמנים
//        //public PathResult FindValidPath(SubGraph subGraph,//הגרף של המתאמן 
//        //                                List<int> exerciseOrder,//רשימת התרגילים מהתוכנית הדיפולטיבית
//        //                                DateTime startTime)//תאריך התחלה
//        //{
//        //    //var visited = new HashSet<int>();
//        //    //var path = new List<int>();
//        //    //var currentTime = startTime;
//        //    // בניית מילון צמתים לגישה מהירה

//        //    ////שליחה לאלגוריתם שמוצא מסלול חוקי
//        //    //var success = Backtrack(
//        //    //    subGraph,
//        //    //    exerciseOrder,
//        //    //    new Dictionary<int, List<int>>(), // מכשירים חילופיים **********************************************
//        //    //    visited,
//        //    //    path,
//        //    //    currentTime,
//        //    //    out var finalTime
//        //    //);
//        //    // שלב 1: לבנות מילון צמתים לגישה מהירה
//        //    var nodeDict = subGraph.Nodes.ToDictionary(n => n.Id);

//        //    // שלב 2: לבנות מטריצת נגישות פעם אחת בלבד!
//        //    Dictionary<int, HashSet<int>> reachableFromNode = BuildReachabilityMatrix(nodeDict.Values);

//        //    int mask = 0; // ייצוג בינארי של אילו תרגילים כבר בוצעו
//        //    var path = new List<int>(); // המסלול הנבחר
//        //    DateTime endTime = DateTime.MinValue; // ישמר זמן הסיום של המסלול


//        //    // שלב 3: להעביר את reachableFromNode לכל מקום שצריך (למשל לפונקציית Backtrack)
//        //    // קריאה לאלגוריתם הרקורסיבי שמחפש מסלול חוקי
//        //    var (found, numAlt, bestPath, finalTime) = Backtrack(
//        //        subGraph,
//        //        nodeDict,
//        //        reachableFromNode,
//        //        exerciseOrder,
//        //        new Dictionary<int, List<int>>(), // אם יש לך תרגילים חילופיים, תעביר כאן
//        //        mask,
//        //        path,
//        //        0,           // מספר חילופים בשלב התחלתי
//        //        startTime,
//        //        out endTime
//        //    );

//        //    // אם לא נמצא מסלול, נזרוק חריגה
//        //    if (!found || bestPath == null)
//        //        throw new Exception("לא נמצא מסלול חוקי לפי ההגבלות.");

//        //    // עדכון תורי זמינות (הוספת המתאמן לכל מכשיר במסלול בזמנים המתאימים)
//        //    DateTime currentTime = startTime;
//        //    foreach (var machineId in bestPath)
//        //    {
//        //        var machineNode = nodeDict[machineId];
//        //        machineNode.AddToQueue(currentTime);
//        //        currentTime = currentTime.AddMinutes(machineNode.UsageDurationMinutes);
//        //    }

//        //    // בניית תוצאת המסלול המלא
//        //    return new PathResult
//        //    {
//        //        ExerciseIdsInPath = bestPath,
//        //        StartTime = startTime,
//        //        EndTime = finalTime
//        //    };


//        //    //if (!success)
//        //    //    throw new Exception("לא נמצא מסלול חוקי לפי ההגבלות.");
//        //    //// אם לא נמצא מסלול חוקי, נכניס אופציה של תרגיל חילופי
//        //    //// איך נדע איזה תרגיל להחליף
//        //    //// עדכון תורי זמינות
//        //    //foreach (var machineId in path)
//        //    //{
//        //    //    var machineNode = subGraph.Nodes[machineId];
//        //    //    machineNode.AddToQueue(currentTime); // מוסיף את המתאמן לתור בשעה הזו
//        //    //    currentTime = currentTime.AddMinutes(machineNode.UsageDurationMinutes);
//        //    //}

//        //    //return new PathResult
//        //    //{
//        //    //    ExerciseIdsInPath = path,
//        //    //    StartTime = startTime,
//        //    //    EndTime = finalTime
//        //    //};
//        //}



//        /// האלגוריתם: Backtracking + Memoization
//        //private bool Backtrack(
//        //    SubGraph graph, // הגרף של המתאמן
//        //    List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
//        //    Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
//        //    HashSet<int> visitedExercises, // התרגילים שביקרנו (רק מסוג Exercise)
//        //    List<int> path, // מסלול
//        //    DateTime currentTime, // תאריך התחלה
//        //    out DateTime finishTime // תאריך סיום
//        //)
//        //{
//        //    // אם כל התרגילים מסוג Exercise בוצעו, נעדכן את זמן הסיום
//        //    if (visitedExercises.Count == exerciseOrder.Count)
//        //    {
//        //        finishTime = currentTime;
//        //        return true;
//        //    }

//        //    // ננסה כל תרגיל שנשאר ברשימה
//        //    // ננסה כל תרגיל שנשאר ברשימה, בתנאי שיש מעבר לגיטימי
//        //    for (int i = 0; i < exerciseOrder.Count; i++)
//        //    {
//        //        // בדיקה האם הצומת נמצא כבר במסלול 
//        //        if (visitedExercises.Contains(exerciseOrder[i]))
//        //            continue;

//        //        var nodeId = exerciseOrder[i];
//        //        var node = graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
//        //        if (node == null)
//        //            continue;

//        //        // בדיקה: האם יש מעבר לגיטימי לצמתים הבאים ברשימה?
//        //        bool hasValidPathToNext = false; // דגל שמוודא אם יש מעבר

//        //        //נראה לי שצריך לבדוק על הכל ולא רק מה שבהמשך*******************************
//        //        //לא בטוח כי אני החלפתי את הסדר של התרגילים במידה וניתן לשנות סדר

//        //        //נראלי כן צריך לבדוק את כל מי שזה לא הוא ולא עברנו בו עדיין
//        //        for (int j = i + 1; j < exerciseOrder.Count; j++)
//        //        {
//        //            var nextNodeId = exerciseOrder[j];
//        //            var nextNode = graph.Nodes.FirstOrDefault(n => n.Id == nextNodeId);

//        //            if (nextNode != null && HasPathToNextExercise(node, nextNode))
//        //            {
//        //                hasValidPathToNext = true;
//        //                break;
//        //            }
//        //        }

//        //        // אם אין מעבר לגיטימי לצמתים הבאים, דלג על הצומת הנוכחי
//        //        if (!hasValidPathToNext && i + 1 < exerciseOrder.Count)
//        //        {
//        //            finishTime = currentTime;
//        //            return false;
//        //        }

//        //        // אם הצומת מסוג Exercise, נוודא שהוא זמין
//        //        //לא בטוח שצריך בדיקה אם זה תרגיל כייש לי רק תרגילים
//        //        if (!node.IsAvailableAt(currentTime))
//        //        {
//        //            // בדוק אם ניתן להגיע אליו מאוחר יותר
//        //            var reachableNode = CanReachNode(graph, node, exerciseOrder.Where(e => !visitedExercises.Contains(e) && e != nodeId).ToList());
//        //            if (reachableNode == null)
//        //            {
//        //                // אם אי אפשר להגיע אליו ואין מכשיר חילופי, החזר FALSE
//        //                if (!alternativeDevices.ContainsKey(nodeId) || alternativeDevices[nodeId].Count == 0)
//        //                {
//        //                    finishTime = currentTime;
//        //                    return false;
//        //                }

//        //                // נסה מכשיר חילופי
//        //                foreach (var alternativeId in alternativeDevices[nodeId])
//        //                {
//        //                    var alternativeNode = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
//        //                    if (alternativeNode != null && !visitedExercises.Contains(alternativeId) && alternativeNode.IsAvailableAt(currentTime))
//        //                    {
//        //                        // נסה תרגיל חילופי
//        //                        visitedExercises.Add(alternativeId);
//        //                        path.Add(alternativeId);

//        //                        var nextTime = currentTime.AddMinutes(alternativeNode.UsageDurationMinutes);

//        //                        if (i + 1 < exerciseOrder.Count)
//        //                        {
//        //                            var nextExerciseId = exerciseOrder[i + 1];
//        //                            var nextExerciseNode = graph.Nodes.FirstOrDefault(n => n.Id == nextExerciseId);

//        //                            // בדיקה למעבר דרך MuscleGroup אם אין קשת ישירה
//        //                            if (nextExerciseNode != null && !node.Neighbors.Contains(nextExerciseNode))
//        //                            {
//        //                                // ננסה מעבר דרך MuscleGroup
//        //                                var muscleGroupsPath = GetMuscleGroupPath(node, nextExerciseNode);
//        //                                if (muscleGroupsPath != null)
//        //                                {
//        //                                    foreach (var muscleGroupNode in muscleGroupsPath)
//        //                                    {
//        //                                        path.Add(muscleGroupNode.Id);
//        //                                    }
//        //                                }
//        //                                else
//        //                                {
//        //                                    // אם לא מצאנו מעבר, דלג לתרגיל הבא
//        //                                    continue;
//        //                                }
//        //                            }
//        //                        }


//        //                        //exerciseOrder.Where(e => e != nodeId).ToList()????????????????????
//        //                        if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
//        //                            return true;

//        //                        // אם תרגיל חילופי לא הצליח, נחזור אחורה
//        //                        visitedExercises.Remove(alternativeId);
//        //                        path.RemoveAt(path.Count - 1);
//        //                    }
//        //                }

//        //                // אם אין תרגיל חילופי זמין, החזר FALSE
//        //                finishTime = currentTime;
//        //                return false;
//        //            }
//        //            else
//        //            {
//        //                // נניח ש-reachableNode.Id הוא מזהה הצומת
//        //                int reachableNodeId = reachableNode.Id; // או reachableNode אם הוא לא אובייקט
//        //                // מצא את אינדקס הצומת ברשימה
//        //                int reachableNodeIndex = exerciseOrder.IndexOf(reachableNodeId);
//        //                // החלף בין המיקומים
//        //                // שומר את הצומת הנוכחי במיקום i
//        //                int temp = exerciseOrder[i];
//        //                exerciseOrder[i] = exerciseOrder[reachableNodeIndex];
//        //                exerciseOrder[reachableNodeIndex] = temp;


//        //                // ננסה מעבר דרך MuscleGroup
//        //                var muscleGroupsPath = GetMuscleGroupPath(node, reachableNode);
//        //                if (muscleGroupsPath != null)
//        //                {
//        //                    foreach (var muscleGroupNode in muscleGroupsPath)
//        //                    {
//        //                        path.Add(muscleGroupNode.Id);
//        //                    }
//        //                }
//        //                else
//        //                {
//        //                    //לא יודעת  מה לעשות כאן 
//        //                }
//        //                if (Backtrack(graph, exerciseOrder, alternativeDevices, visitedExercises, path, currentTime, out finishTime))//תריך לתת את כל הרשימה
//        //                    return true;
//        //            }
//        //        }
//        //        else //המכשיר זמין
//        //        {
//        //            //לא לנקסט אלא הבא שלא נמצא ברשימה #############


//        //            // אם הצומת מסוג MuscleGroup, אין צורך לבדוק זמינות ואין צורך להכניס לסט visited
//        //            visitedExercises.Add(nodeId); // הוסף רק צמתים מסוג Exercise לסט visited
//        //            path.Add(nodeId);

//        //            // **מעבר ישיר בין תרגילים**
//        //            // בדוק אם ניתן לעבור ישירות לתרגיל הבא ברשימה
//        //            if (i + 1 < exerciseOrder.Count)
//        //            {
//        //                var nextExerciseId = exerciseOrder[i + 1];
//        //                var nextExerciseNode = graph.Nodes.FirstOrDefault(n => n.Id == nextExerciseId);

//        //                // בדיקה למעבר דרך MuscleGroup אם אין קשת ישירה
//        //                if (nextExerciseNode != null && !node.Neighbors.Contains(nextExerciseNode))
//        //                {
//        //                    // ננסה מעבר דרך MuscleGroup
//        //                    var muscleGroupsPath = GetMuscleGroupPath(node, nextExerciseNode);
//        //                    if (muscleGroupsPath != null)
//        //                    {
//        //                        foreach (var muscleGroupNode in muscleGroupsPath)
//        //                        {
//        //                            path.Add(muscleGroupNode.Id);
//        //                        }
//        //                    }
//        //                    else
//        //                    {
//        //                        finishTime = currentTime;
//        //                        return false;
//        //                    }
//        //                }
//        //            }
//        //            // עדכון הזמן לתרגיל הבא
//        //            var nextTime = currentTime.AddMinutes(node.UsageDurationMinutes);

//        //            //אם יש תרגיל שלא ניתן לגשת אליו משום מקום################

//        //            // חפש את המסלול הבא
//        //            if (Backtrack(graph, exerciseOrder, alternativeDevices, visitedExercises, path, nextTime, out finishTime))
//        //                return true;



//        //            // אם לא הצלחנו, נחזור אחורה
//        //            if (node.Type == NodeType.Exsercise)
//        //                visitedExercises.Remove(nodeId); // הסר רק צמתים מסוג Exercise מ-visited

//        //            path.RemoveAt(path.Count - 1);

//        //            //נראלי פה צריך לשים תחליף
//        //            //בתנאי שיש לי גישה אליו מהמקום הקודם

//        //        }
//        //    }

//        //    finishTime = currentTime;
//        //    return false;
//        //}
//        ////private bool Backtrack(
//        ////    SubGraph graph, // הגרף של המתאמן
//        ////    List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
//        ////    Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
//        ////    HashSet<int> visitedExercises, // התרגילים שביקרנו (רק מסוג Exercise)
//        ////    List<int> path, // מסלול
//        ////    DateTime currentTime, // תאריך התחלה
//        ////    out DateTime finishTime // תאריך סיום
//        ////)
//        ////{
//        ////    // אם כל התרגילים מסוג Exercise בוצעו, נעדכן את זמן הסיום
//        ////    if (visitedExercises.Count == exerciseOrder.Count)
//        ////    {
//        ////        finishTime = currentTime;
//        ////        return true;
//        ////    }

//        ////    // ננסה כל תרגיל שנשאר ברשימה
//        ////    // ננסה כל תרגיל שנשאר ברשימה, בתנאי שיש מעבר לגיטימי
//        ////    for (int i = 0; i < exerciseOrder.Count; i++)
//        ////    {
//        ////        // בדיקה האם הצומת נמצא כבר במסלול 
//        ////        if (visitedExercises.Contains(exerciseOrder[i]))
//        ////            continue;

//        ////        var nodeId = exerciseOrder[i];
//        ////        var node = graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
//        ////        if (node == null)
//        ////            continue;

//        ////        // בדיקה: האם יש מעבר לגיטימי לצמתים הבאים ברשימה?
//        ////        bool hasValidPathToNext = false; // דגל שמוודא אם יש מעבר


//        ////        //נראה לי שצריך לבדוק על הכל ולא רק מה שבהמשך*******************************
//        ////        //לא בטוח כי אני החלפתי את הסדר של התרגילים במידה וניתן לשנות סדר

//        ////        for (int j = i + 1; j < exerciseOrder.Count; j++)
//        ////        {
//        ////            var nextNodeId = exerciseOrder[j];
//        ////            var nextNode = graph.Nodes.FirstOrDefault(n => n.Id == nextNodeId);

//        ////            if (nextNode != null && HasPathToNextExercise(node, nextNode))
//        ////            {
//        ////                hasValidPathToNext = true;
//        ////                break;
//        ////            }
//        ////        }

//        ////        ///*****************************************************************
//        ////        // אם אין מעבר לגיטימי לצמתים הבאים, דלג על הצומת הנוכחי
//        ////        if (!hasValidPathToNext && i+1< exerciseOrder.Count)
//        ////            continue;

//        ////        // אם הצומת מסוג Exercise, נוודא שהוא זמין
//        ////        //לא בטוח שצריך בדיקה אם זה תרגיל כייש לי רק תרגילים
//        ////        if (node.Type == NodeType.Exsercise && !node.IsAvailableAt(currentTime))
//        ////        {
//        ////            // בדוק אם ניתן להגיע אליו מאוחר יותר
//        ////            var reachableNode = CanReachNode(graph, node, exerciseOrder.Where(e => !visitedExercises.Contains(e) && e != nodeId).ToList());
//        ////            //if (!CanReachNode(graph, nodeId, exerciseOrder.Where(e => !visitedExercises.Contains(e) && e != nodeId).ToList()))
//        ////            if (reachableNode == null)
//        ////            {
//        ////                // אם אי אפשר להגיע אליו ואין מכשיר חילופי, החזר FALSE
//        ////                if (!alternativeDevices.ContainsKey(nodeId) || alternativeDevices[nodeId].Count == 0)
//        ////                {
//        ////                    finishTime = currentTime;
//        ////                    return false;
//        ////                }

//        ////                // נסה מכשיר חילופי
//        ////                foreach (var alternativeId in alternativeDevices[nodeId])
//        ////                {
//        ////                    var alternativeNode = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
//        ////                    if (alternativeNode != null && !visitedExercises.Contains(alternativeId) && alternativeNode.IsAvailableAt(currentTime))
//        ////                    {
//        ////                        // נסה תרגיל חילופי
//        ////                        visitedExercises.Add(alternativeId);
//        ////                        path.Add(alternativeId);

//        ////                        var nextTime = currentTime.AddMinutes(alternativeNode.UsageDurationMinutes);

//        ////                        if (i + 1 < exerciseOrder.Count)
//        ////                        {
//        ////                            var nextExerciseId = exerciseOrder[i + 1];
//        ////                            var nextExerciseNode = graph.Nodes.FirstOrDefault(n => n.Id == nextExerciseId);

//        ////                            // בדיקה למעבר דרך MuscleGroup אם אין קשת ישירה
//        ////                            if (nextExerciseNode != null && !node.Neighbors.Contains(nextExerciseNode))
//        ////                            {
//        ////                                // ננסה מעבר דרך MuscleGroup
//        ////                                var muscleGroupsPath = GetMuscleGroupPath(node, nextExerciseNode);
//        ////                                if (muscleGroupsPath != null)
//        ////                                {
//        ////                                    foreach (var muscleGroupNode in muscleGroupsPath)
//        ////                                    {
//        ////                                        path.Add(muscleGroupNode.Id);
//        ////                                    }
//        ////                                }
//        ////                                else
//        ////                                {
//        ////                                    // אם לא מצאנו מעבר, דלג לתרגיל הבא
//        ////                                    continue;
//        ////                                }
//        ////                            }
//        ////                        }


//        ////                        //exerciseOrder.Where(e => e != nodeId).ToList()????????????????????
//        ////                        if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
//        ////                            return true;

//        ////                        // אם תרגיל חילופי לא הצליח, נחזור אחורה
//        ////                        visitedExercises.Remove(alternativeId);
//        ////                        path.RemoveAt(path.Count - 1);
//        ////                    }
//        ////                }

//        ////                // אם אין תרגיל חילופי זמין, החזר FALSE
//        ////                finishTime = currentTime;
//        ////                return false;
//        ////            }
//        ////            else
//        ////            {
//        ////                // נניח ש-reachableNode הוא מספר של הצומת שברצונך להעביר
//        ////                // נניח ש-reachableNode.Id הוא מזהה הצומת
//        ////                int reachableNodeId = reachableNode.Id; // או reachableNode אם הוא לא אובייקט
//        ////                // מצא את אינדקס הצומת ברשימה
//        ////                int reachableNodeIndex = exerciseOrder.IndexOf(reachableNodeId);
//        ////                // החלף בין המיקומים
//        ////                // שומר את הצומת הנוכחי במיקום i
//        ////                int temp = exerciseOrder[i];
//        ////                exerciseOrder[i] = exerciseOrder[reachableNodeIndex];
//        ////                exerciseOrder[reachableNodeIndex] = temp;


//        ////                // ננסה מעבר דרך MuscleGroup
//        ////                var muscleGroupsPath = GetMuscleGroupPath(node, reachableNode);
//        ////                if (muscleGroupsPath != null)
//        ////                {
//        ////                    foreach (var muscleGroupNode in muscleGroupsPath)
//        ////                    {
//        ////                        path.Add(muscleGroupNode.Id);
//        ////                    }
//        ////                }
//        ////                else
//        ////                {
//        ////                   //לא יודעת  מה לעשות כאן 
//        ////                }

//        ////                // אם ברצונך להדפיס את הרשימה אחרי ההחלפה
//        ////                // Console.WriteLine("Updated exerciseOrder: " + string.Join(", ", exerciseOrder));

//        ////                // אם אפשר להגיע אליו, נסה לשנות את הסדר ולהמשיך
//        ////                //var remainingNodes = exerciseOrder.Where(e => !visitedExercises.Contains(e)).ToList();
//        ////                //אולי נחליף את המיקום עם התרגיל שיכול לגשת אליו*****************************************
//        ////                if (Backtrack(graph, exerciseOrder, alternativeDevices, visitedExercises, path, currentTime, out finishTime))//תריך לתת את כל הרשימה
//        ////                    return true;
//        ////            }
//        ////        }
//        ////        else
//        ////        {
//        ////            // אם הצומת מסוג MuscleGroup, אין צורך לבדוק זמינות ואין צורך להכניס לסט visited
//        ////            if (node.Type == NodeType.MuscleGroup || (node.Type == NodeType.Exsercise && node.IsAvailableAt(currentTime)))
//        ////            {
//        ////                // אם הצומת מסוג MuscleGroup, אין צורך לבדוק זמינות ואין צורך להכניס לסט visited
//        ////                if (node.Type == NodeType.Exsercise)
//        ////                    visitedExercises.Add(nodeId); // הוסף רק צמתים מסוג Exercise לסט visited

//        ////                path.Add(nodeId);

//        ////                // **מעבר ישיר בין תרגילים**
//        ////                // בדוק אם ניתן לעבור ישירות לתרגיל הבא ברשימה
//        ////                if (i + 1 < exerciseOrder.Count)
//        ////                {
//        ////                    var nextExerciseId = exerciseOrder[i + 1];
//        ////                    var nextExerciseNode = graph.Nodes.FirstOrDefault(n => n.Id == nextExerciseId);

//        ////                    // בדיקה למעבר דרך MuscleGroup אם אין קשת ישירה
//        ////                    if (nextExerciseNode != null && !node.Neighbors.Contains(nextExerciseNode))
//        ////                    {
//        ////                        // ננסה מעבר דרך MuscleGroup
//        ////                        var muscleGroupsPath = GetMuscleGroupPath(node, nextExerciseNode);
//        ////                        if (muscleGroupsPath != null)
//        ////                        {
//        ////                            foreach (var muscleGroupNode in muscleGroupsPath)
//        ////                            {
//        ////                                path.Add(muscleGroupNode.Id);
//        ////                            }
//        ////                        }
//        ////                        else
//        ////                        {
//        ////                            // אם לא מצאנו מעבר, דלג לתרגיל הבא
//        ////                            continue;
//        ////                        }
//        ////                    }
//        ////                }
//        ////            }
//        ////            // עדכון הזמן לתרגיל הבא
//        ////            var nextTime = currentTime.AddMinutes(node.UsageDurationMinutes);

//        ////            // חפש את המסלול הבא
//        ////            //לא יודעת למה exerciseOrder.Where(e => e != nodeId).ToList()?????????
//        ////            if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
//        ////                return true;

//        ////            // אם לא הצלחנו, נחזור אחורה
//        ////            if (node.Type == NodeType.Exsercise)
//        ////                visitedExercises.Remove(nodeId); // הסר רק צמתים מסוג Exercise מ-visited

//        ////            path.RemoveAt(path.Count - 1);
//        ////        }
//        ////    }

//        ////    finishTime = currentTime;
//        ////    return false;
//        ////}

//        ////private bool HasPathToNextExercise(Node currentNode, Node nextNode)
//        ////{
//        ////    // בדוק אם יש קשת ישירה
//        ////    if (currentNode.Neighbors.Contains(nextNode))
//        ////        return true;

//        ////    // בדוק אם יש מסלול דרך MuscleGroup
//        ////    foreach (var neighbor in currentNode.Neighbors)
//        ////    {
//        ////        if (neighbor.Type == NodeType.MuscleGroup && neighbor.Neighbors.Contains(nextNode))
//        ////        {
//        ////            return true;
//        ////        }
//        ////    }

//        ////    // אם לא נמצא מסלול
//        ////    return false;
//        ////}

//        ////צריך לבדוק רק אם יש תרגיל באותה קבוצת שרירים שיכול לגשת אליו 
//        ////private bool CanReachNode(SubGraph graph, int targetNodeId, List<int> remainingNodes)
//        ////{
//        ////     רשימת צמתים לביקור
//        ////    var toVisit = new Queue<int>(remainingNodes);
//        ////    var visited = new HashSet<int>();

//        ////    while (toVisit.Count > 0)
//        ////    {
//        ////        var currentNodeId = toVisit.Dequeue();

//        ////         אם הגענו לצומת היעד
//        ////        if (currentNodeId == targetNodeId)
//        ////            return true;

//        ////         הוסף את הצומת הנוכחי לרשימת ביקור
//        ////        visited.Add(currentNodeId);

//        ////         הוסף את כל השכנים של הצומת הנוכחי לתור, אם הם לא ביקרו בהם
//        ////        var currentNode = graph.Nodes.FirstOrDefault(n => n.Id == currentNodeId);
//        ////        if (currentNode != null)
//        ////        {
//        ////            foreach (var neighbor in currentNode.Neighbors)
//        ////            {
//        ////                if (!visited.Contains(neighbor.Id) && remainingNodes.Contains(neighbor.Id))
//        ////                {
//        ////                    toVisit.Enqueue(neighbor.Id);
//        ////                }
//        ////            }
//        ////        }
//        ////    }

//        ////     אם לא מצאנו דרך לצומת היעד
//        ////    return false;
//        ////}

//        ////דחווווווווווווווף לסדר את הפונקציה***********

//        //// בדוק את כל השכנים של הצומת הנוכחי
//        ////foreach (var neighbor in currentNode.Neighbors)
//        ////{
//        ////    // אם מדובר בשכן הרצוי, החזר את הצומת הנוכחי
//        ////    if (neighbor.Id == targetNodeId)
//        ////    {
//        ////        return currentNode; // מחזירים את הצומת הנוכחי
//        ////    }

//        ////    // הוסף את השכן לתור אם לא ביקרנו בו עדיין
//        ////    if (!visited.Contains(neighbor.Id) && remainingNodes.Contains(neighbor.Id))
//        ////    {
//        ////        toVisit.Enqueue(neighbor.Id);
//        ////    }
//        ////}



//        //// פונקציה שמוצאת את המסלול דרך MuscleGroup
//        ////private List<Node> GetMuscleGroupPath(Node currentNode, Node nextExerciseNode)
//        ////{
//        ////    var path = new List<Node>();
//        ////    foreach (var neighbor in currentNode.Neighbors)
//        ////    {
//        ////        if (neighbor.Type == NodeType.MuscleGroup && neighbor.Neighbors.Contains(nextExerciseNode))
//        ////        {
//        ////            path.Add(neighbor);
//        ////        }
//        ////    }

//        ////    return path.Count > 0 ? path : null;
//        ////}

//        ////        private bool Backtrack(
//        ////    SubGraph graph, // הגרף של המתאמן
//        ////    List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
//        ////    Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
//        ////    HashSet<int> visitedExercises, // התרגילים שביקרנו (רק מסוג Exercise)
//        ////    List<int> path, // מסלול
//        ////    DateTime currentTime, // תאריך התחלה
//        ////    out DateTime finishTime // תאריך סיום
//        ////)
//        ////        {
//        ////            // אם כל התרגילים מסוג Exercise בוצעו, נעדכן את זמן הסיום
//        ////            if (visitedExercises.Count == exerciseOrder.Count)
//        ////            {
//        ////                finishTime = currentTime;
//        ////                return true;
//        ////            }

//        ////            // ננסה כל תרגיל שנשאר ברשימה
//        ////            for (int i = 0; i < exerciseOrder.Count; i++)
//        ////            {
//        ////                var nodeId = exerciseOrder[i];
//        ////                var node = graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
//        ////                if (node == null)
//        ////                    continue;

//        ////                // אם זה צומת מסוג Exercise וביקרנו בו כבר, נדלג עליו
//        ////                if (node.Type == NodeType.Exsercise && visitedExercises.Contains(nodeId))
//        ////                    continue;

//        ////                // אם הצומת מסוג Exercise, נוודא שהוא זמין
//        ////                if (node.Type == NodeType.Exsercise && !node.IsAvailableAt(currentTime))
//        ////                {
//        ////                    // בדוק אם ניתן להגיע אליו מאוחר יותר
//        ////                    if (!CanReachNode(graph, nodeId, exerciseOrder.Where(e => !visitedExercises.Contains(e) && e != nodeId).ToList()))
//        ////                    {
//        ////                        // אם אי אפשר להגיע אליו ואין מכשיר חילופי, החזר FALSE
//        ////                        if (!alternativeDevices.ContainsKey(nodeId) || alternativeDevices[nodeId].Count == 0)
//        ////                        {
//        ////                            finishTime = currentTime;
//        ////                            return false;
//        ////                        }

//        ////                        // נסה מכשיר חילופי
//        ////                        foreach (var alternativeId in alternativeDevices[nodeId])
//        ////                        {
//        ////                            var alternativeNode = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
//        ////                            if (alternativeNode != null && !visitedExercises.Contains(alternativeId) && alternativeNode.IsAvailableAt(currentTime))
//        ////                            {
//        ////                                // נסה תרגיל חילופי
//        ////                                visitedExercises.Add(alternativeId);
//        ////                                path.Add(alternativeId);

//        ////                                var nextTime = currentTime.AddMinutes(alternativeNode.UsageDurationMinutes);

//        ////                                if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
//        ////                                    return true;

//        ////                                // אם תרגיל חילופי לא הצליח, נחזור אחורה
//        ////                                visitedExercises.Remove(alternativeId);
//        ////                                path.RemoveAt(path.Count - 1);
//        ////                            }
//        ////                        }

//        ////                        // אם אין תרגיל חילופי זמין, החזר FALSE
//        ////                        finishTime = currentTime;
//        ////                        return false;
//        ////                    }
//        ////                    else
//        ////                    {
//        ////                        // אם אפשר להגיע אליו, נסה לשנות את הסדר ולהמשיך
//        ////                        var remainingNodes = exerciseOrder.Where(e => !visitedExercises.Contains(e)).ToList();
//        ////                        if (Backtrack(graph, remainingNodes, alternativeDevices, visitedExercises, path, currentTime, out finishTime))
//        ////                            return true;
//        ////                    }
//        ////                }
//        ////                else
//        ////                {
//        ////                    // בדוק אם יש מסלול חוקי לתרגיל הבא
//        ////                    if (i + 1 < exerciseOrder.Count)
//        ////                    {
//        ////                        var nextExerciseId = exerciseOrder[i + 1];
//        ////                        var nextExerciseNode = graph.Nodes.FirstOrDefault(n => n.Id == nextExerciseId);

//        ////                        // בדיקה אם יש מסלול ישיר או עקיף דרך MuscleGroup
//        ////                        if (nextExerciseNode != null && !HasPathToNextExercise(node, nextExerciseNode))
//        ////                        {
//        ////                            // אם אין מסלול, לא ניתן לעבור לתרגיל הבא
//        ////                            finishTime = currentTime;
//        ////                            return false;
//        ////                        }
//        ////                    }

//        ////                    // אם הצומת מסוג MuscleGroup, אין צורך לבדוק זמינות ואין צורך להכניס לסט visited
//        ////                    if (node.Type == NodeType.MuscleGroup || (node.Type == NodeType.Exsercise && node.IsAvailableAt(currentTime)))
//        ////                    {
//        ////                        if (node.Type == NodeType.Exsercise)
//        ////                            visitedExercises.Add(nodeId); // הוסף רק צמתים מסוג Exercise לסט visited

//        ////                        path.Add(nodeId);

//        ////                        // עדכון הזמן לתרגיל הבא
//        ////                        var nextTime = currentTime.AddMinutes(node.UsageDurationMinutes);

//        ////                        // חפש את המסלול הבא
//        ////                        if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
//        ////                            return true;

//        ////                        // אם לא הצלחנו, נחזור אחורה
//        ////                        if (node.Type == NodeType.Exsercise)
//        ////                            visitedExercises.Remove(nodeId); // הסר רק צמתים מסוג Exercise מ-visited

//        ////                        path.RemoveAt(path.Count - 1);
//        ////                    }
//        ////                }
//        ////            }

//        ////            finishTime = currentTime;
//        ////            return false;
//        ////        }

//        ////        // פונקציה שמוודאת אם יש מסלול לתרגיל הבא
//        ////        private bool HasPathToNextExercise(Node currentNode, Node nextExerciseNode)
//        ////        {
//        ////            // בדוק אם יש קשת ישירה
//        ////            if (currentNode.Neighbors.Contains(nextExerciseNode))
//        ////                return true;

//        ////            // בדוק אם יש מסלול דרך MuscleGroup
//        ////            foreach (var neighbor in currentNode.Neighbors)
//        ////            {
//        ////                if (neighbor.Type == NodeType.MuscleGroup && neighbor.Neighbors.Contains(nextExerciseNode))
//        ////                {
//        ////                    return true;
//        ////                }
//        ////            }

//        ////            // אם לא נמצא מסלול
//        ////            return false;
//        ////        }
//        ///

//        //        bool Backtrack(
//        //    List<int> exerciseOrder,
//        //    int currentExerciseIndex,
//        //    Trainee currentTrainee,
//        //    DateTime currentTime,
//        //    /* ... פרמטרים נוספים ... */
//        //)
//        //        {
//        //            // תנאי עצירה
//        //            if (currentExerciseIndex == exerciseOrder.Count)
//        //                return true; // כל התרגילים שובצו

//        //            int exerciseId = exerciseOrder[currentExerciseIndex];
//        //            Node exerciseNode = nodeDict[exerciseId];

//        //            // 1. קודם כל: ממצים כל אפשרות אצל המתאמן הנוכחי
//        //            // -------------------------------------------------
//        //            // א. האם התרגיל פנוי בשעה הזו?
//        //            if (exerciseNode.IsAvailableAt(currentTime, currentTrainee))
//        //            {
//        //                // משבצים וממשיכים רק אצל המתאמן עצמו
//        //                exerciseNode.Assign(currentTime, currentTrainee);
//        //                if (Backtrack(exerciseOrder, currentExerciseIndex + 1, currentTrainee, currentTime.AddMinutes(exerciseNode.Duration)))
//        //                    return true;
//        //                exerciseNode.Unassign(currentTime, currentTrainee); // חזרה לאחור
//        //            }

//        //            // ב. האם יש תרגיל חלופי חוקי לעצמי?
//        //            foreach (int altId in GetMyAlternativeExercises(exerciseId, currentTrainee)) // פונקציה שמחזירה תרגילים חלופיים חוקיים למתאמן
//        //            {
//        //                Node altNode = nodeDict[altId];
//        //                if (altNode.IsAvailableAt(currentTime, currentTrainee))
//        //                {
//        //                    altNode.Assign(currentTime, currentTrainee);
//        //                    if (Backtrack(exerciseOrder, currentExerciseIndex + 1, currentTrainee, currentTime.AddMinutes(altNode.Duration)))
//        //                        return true;
//        //                    altNode.Unassign(currentTime, currentTrainee);
//        //                }
//        //            }

//        //            // ג. אפשרות לדחות את התרגיל לזמן פנוי (המתנה במידה וזה חוקי לפי הלוגיקה שלך)
//        //            DateTime? nextFree = exerciseNode.GetNextAvailableTime(currentTime, currentTrainee);
//        //            if (nextFree != null)
//        //            {
//        //                exerciseNode.Assign(nextFree.Value, currentTrainee);
//        //                if (Backtrack(exerciseOrder, currentExerciseIndex + 1, currentTrainee, nextFree.Value.AddMinutes(exerciseNode.Duration)))
//        //                    return true;
//        //                exerciseNode.Unassign(nextFree.Value, currentTrainee);
//        //            }

//        //            // 2. רק אם כל האפשרויות אצל עצמי נכשלו — בודקים שינוי אצל מתאמנים אחרים
//        //            // ---------------------------------------------------------------------
//        //            Trainee occupyingTrainee = exerciseNode.GetOccupyingTraineeAt(currentTime);
//        //            if (occupyingTrainee != null && occupyingTrainee != currentTrainee)
//        //            {
//        //                if (occupyingTrainee.CanRescheduleExercise(exerciseId, currentTime))
//        //                {
//        //                    occupyingTrainee.RescheduleExercise(exerciseId, currentTime);
//        //                    exerciseNode.Assign(currentTime, currentTrainee);
//        //                    if (Backtrack(exerciseOrder, currentExerciseIndex + 1, currentTrainee, currentTime.AddMinutes(exerciseNode.Duration)))
//        //                        return true;
//        //                    exerciseNode.Unassign(currentTime, currentTrainee);
//        //                    occupyingTrainee.UndoReschedule(exerciseId, currentTime);
//        //                }
//        //            }

//        //            return false; // לא נמצא מסלול
//        //        }
//        //    }

//    }
//}



////private bool Backtrack(
////    SubGraph graph, // הגרף של המתאמן
////    List<int> exerciseOrder,//רשימת התרגילים מהתוכנית הדיפולטיבית
////    HashSet<int> visited, //התרגילים שביקרנו
////    List<int> path, //מסלול
////    DateTime currentTime,//תאריך התחלה
////    out DateTime finishTime //תאריך סיום
////)
////{
////    // אם כל התרגילים ביקרנו בהם, נעדכן את זמן הסיום
////    if (visited.Count == exerciseOrder.Count)
////    {
////        finishTime = currentTime;
////        return true;
////    }

////    // ננסה לפי הסדר, אבל נבחר רק צמתים שלא ביקרנו ושהם זמינים בזמן הזה
////    foreach (var ExerciseId in exerciseOrder)
////    {
////        if (visited.Contains(ExerciseId))
////            continue;

////        // נוודא שהתרגיל קיים בגרף והוא זמין בזמן הזה
////        var exercise = graph.Nodes.FirstOrDefault(n => n.Id == ExerciseId);
////        if (exercise == null || !exercise.IsAvailableAt(currentTime))
////            continue;

////        //אם התרגיל קיים וזמין, נוסיף אותו למסלול
////        visited.Add(ExerciseId);
////        path.Add(ExerciseId);

////        //צריך לשהות לפי משך הזמן של התרגיל********************************************
////        var nextTime = currentTime.AddMinutes(exercise.UsageDurationMinutes);

////        //נחפש את המסלול הבא
////        if (Backtrack(graph, exerciseOrder, visited, path, nextTime, out finishTime))
////            return true;
////        //במידה ולא נמצא מסלול חוקי, נחזור לתרגיל הקודם

////        visited.Remove(ExerciseId);
////        path.RemoveAt(path.Count - 1);
////    }

////    finishTime = currentTime;
////    return false;
////}

////        private bool Backtrack(
////            SubGraph graph, // הגרף של המתאמן
////            List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
////            Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
////            HashSet<int> visited, // התרגילים שביקרנו
////            List<int> path, // מסלול
////            DateTime currentTime, // תאריך התחלה
////            out DateTime finishTime // תאריך סיום
////)
////        {
////            // אם כל התרגילים בוצעו, נעדכן את זמן הסיום
////            if (visited.Count == exerciseOrder.Count)
////            {
////                finishTime = currentTime;
////                return true;
////            }

////            // ננסה לפי הסדר, אבל נבחר רק צמתים שלא ביקרנו ושהם זמינים בזמן הזה
////            foreach (var exerciseId in exerciseOrder)
////            {
////                if (visited.Contains(exerciseId))
////                    continue;

////                // נוודא שהתרגיל קיים בגרף והוא זמין בזמן הזה
////                var exercise = graph.Nodes.FirstOrDefault(n => n.Id == exerciseId);
////                if (exercise != null && exercise.IsAvailableAt(currentTime))
////                {
////                    // אם התרגיל זמין, נוסיף אותו למסלול
////                    visited.Add(exerciseId);
////                    path.Add(exerciseId);

////                    // עדכון הזמן לתרגיל הבא
////                    var nextTime = currentTime.AddMinutes(exercise.UsageDurationMinutes);

////                    // חפש את המסלול הבא
////                    if (Backtrack(graph, exerciseOrder, alternativeDevices, visited, path, nextTime, out finishTime))
////                        return true;

////                    // אם לא הצלחנו, נחזור אחורה
////                    visited.Remove(exerciseId);
////                    path.RemoveAt(path.Count - 1);
////                }
////                else
////                {
////                    // אם התרגיל לא זמין, ננסה מכשירים חילופיים
////                    if (alternativeDevices.ContainsKey(exerciseId))
////                    {
////                        foreach (var alternativeId in alternativeDevices[exerciseId])
////                        {
////                            var alternativeExercise = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
////                            if (alternativeExercise != null && !visited.Contains(alternativeId) && alternativeExercise.IsAvailableAt(currentTime))
////                            {
////                                // נסה תרגיל חילופי
////                                visited.Add(alternativeId);
////                                path.Add(alternativeId);

////                                // עדכון הזמן לתרגיל הבא
////                                var nextTime = currentTime.AddMinutes(alternativeExercise.UsageDurationMinutes);

////                                // חפש את המסלול הבא
////                                if (Backtrack(graph, exerciseOrder, alternativeDevices, visited, path, nextTime, out finishTime))
////                                    return true;

////                                // אם התרגיל החילופי לא הצליח, נחזור אחורה
////                                visited.Remove(alternativeId);
////                                path.RemoveAt(path.Count - 1);
////                            }
////                        }
////                    }
////                }
////            }

////            finishTime = currentTime;
////            return false;
////        }


////private bool Backtrack(
////    SubGraph graph, // הגרף של המתאמן
////    List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
////    Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
////    HashSet<int> visited, // התרגילים שביקרנו
////    List<int> path, // מסלול
////    DateTime currentTime, // תאריך התחלה
////    out DateTime finishTime) // תאריך סיום
////{
////    // אם כל התרגילים בוצעו, נעדכן את זמן הסיום
////    if (visited.Count == exerciseOrder.Count)
////    {
////        finishTime = currentTime;
////        return true;
////    }

////    // ננסה כל תרגיל שנשאר ברשימה
////    for (int i = 0; i < exerciseOrder.Count; i++)
////    {
////        var exerciseId = exerciseOrder[i];

////        if (visited.Contains(exerciseId))
////            continue;

////        // נוודא שהתרגיל קיים בגרף
////        var exercise = graph.Nodes.FirstOrDefault(n => n.Id == exerciseId);
////        if (exercise != null && exercise.IsAvailableAt(currentTime))
////        {
////            // אם התרגיל זמין, נוסיף אותו למסלול
////            visited.Add(exerciseId);
////            path.Add(exerciseId);

////            // עדכון הזמן לתרגיל הבא
////            var nextTime = currentTime.AddMinutes(exercise.UsageDurationMinutes);

////            // חפש את המסלול הבא
////            if (Backtrack(graph, exerciseOrder.Where(e => e != exerciseId).ToList(), alternativeDevices, visited, path, nextTime, out finishTime))
////                return true;

////            // אם לא הצלחנו, נחזור אחורה
////            visited.Remove(exerciseId);
////            path.RemoveAt(path.Count - 1);
////        }
////        else
////        {
////            // אם התרגיל לא זמין, ננסה מכשירים חילופיים
////            if (alternativeDevices.ContainsKey(exerciseId))
////            {
////                foreach (var alternativeId in alternativeDevices[exerciseId])
////                {
////                    var alternativeExercise = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
////                    if (alternativeExercise != null && !visited.Contains(alternativeId) && alternativeExercise.IsAvailableAt(currentTime))
////                    {
////                        // נסה תרגיל חילופי
////                        visited.Add(alternativeId);
////                        path.Add(alternativeId);

////                        // עדכון הזמן לתרגיל הבא
////                        var nextTime = currentTime.AddMinutes(alternativeExercise.UsageDurationMinutes);

////                        // חפש את המסלול הבא
////                        if (Backtrack(graph, exerciseOrder.Where(e => e != exerciseId).ToList(), alternativeDevices, visited, path, nextTime, out finishTime))
////                            return true;

////                        // אם תרגיל חילופי לא הצליח, נחזור אחורה
////                        visited.Remove(alternativeId);
////                        path.RemoveAt(path.Count - 1);
////                    }
////                }
////            }
////        }
////    }

////    finishTime = currentTime;
////    return false;
////}



////מעולה רק בלי ENUM של סוגי הצמתים
////private bool Backtrack(
////    SubGraph graph, // הגרף של המתאמן
////    List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
////    Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
////    HashSet<int> visited, // התרגילים שביקרנו
////    List<int> path, // מסלול
////    DateTime currentTime, // תאריך התחלה
////    out DateTime finishTime // תאריך סיום
////)
////{
////    // אם כל התרגילים בוצעו, נעדכן את זמן הסיום
////    if (visited.Count == exerciseOrder.Count)
////    {
////        finishTime = currentTime;
////        return true;
////    }

////    // ננסה כל תרגיל שנשאר ברשימה
////    for (int i = 0; i < exerciseOrder.Count; i++)
////    {
////        var exerciseId = exerciseOrder[i];

////        if (visited.Contains(exerciseId))
////            continue;

////        // נוודא שהתרגיל קיים בגרף
////        var exercise = graph.Nodes.FirstOrDefault(n => n.Id == exerciseId);
////        if (exercise != null && exercise.IsAvailableAt(currentTime))
////        {
////            // אם התרגיל זמין, נוסיף אותו למסלול
////            visited.Add(exerciseId);
////            path.Add(exerciseId);

////            // עדכון הזמן לתרגיל הבא
////            var nextTime = currentTime.AddMinutes(exercise.UsageDurationMinutes);

////            // חפש את המסלול הבא
////            if (Backtrack(graph, exerciseOrder.Where(e => e != exerciseId).ToList(), alternativeDevices, visited, path, nextTime, out finishTime))
////                return true;

////            // אם לא הצלחנו, נחזור אחורה
////            visited.Remove(exerciseId);
////            path.RemoveAt(path.Count - 1);
////        }
////        else
////        {
////            // בדוק אם ניתן להגיע לתרגיל הזה מאוחר יותר
////            if (!CanReachNode(graph, exerciseId, exerciseOrder.Where(e => !visited.Contains(e) && e != exerciseId).ToList()))
////            {
////                // אם אי אפשר להגיע אליו ואין מכשיר חילופי, החזר FALSE
////                if (!alternativeDevices.ContainsKey(exerciseId) || alternativeDevices[exerciseId].Count == 0)
////                {
////                    finishTime = currentTime;
////                    return false;
////                }
////                // אם אי אפשר להגיע אליו, ננסה מכשיר חילופי
////                foreach (var alternativeId in alternativeDevices[exerciseId])
////                {
////                    var alternativeExercise = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
////                    if (alternativeExercise != null && !visited.Contains(alternativeId) && alternativeExercise.IsAvailableAt(currentTime))
////                    {
////                        // נסה תרגיל חילופי
////                        visited.Add(alternativeId);
////                        path.Add(alternativeId);

////                        // עדכון הזמן לתרגיל הבא
////                        var nextTime = currentTime.AddMinutes(alternativeExercise.UsageDurationMinutes);

////                        // חפש את המסלול הבא
////                        if (Backtrack(graph, exerciseOrder.Where(e => e != exerciseId).ToList(), alternativeDevices, visited, path, nextTime, out finishTime))
////                            return true;

////                        // אם תרגיל חילופי לא הצליח, נחזור אחורה
////                        visited.Remove(alternativeId);
////                        path.RemoveAt(path.Count - 1);
////                    }
////                }

////                //// אם אין תרגיל חילופי זמין, החזר FALSE
////                //finishTime = currentTime;
////                //return false;
////            }
////            else
////            {
////                // אם אפשר להגיע אליו, נסה לשנות את הסדר ולהמשיך
////                var remainingExercises = exerciseOrder.Where(e => !visited.Contains(e)).ToList();
////                if (Backtrack(graph, remainingExercises, alternativeDevices, visited, path, currentTime, out finishTime))
////                    return true;
////            }
////        }
////    }

////    finishTime = currentTime;
////    return false;
////}


////private bool Backtrack(
////    SubGraph graph, // הגרף של המתאמן
////    List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
////    Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
////    HashSet<int> visitedExercises, // התרגילים שביקרנו (רק מסוג Exercise)
////    List<int> path, // מסלול
////    DateTime currentTime, // תאריך התחלה
////    out DateTime finishTime // תאריך סיום
////)
////{
////    // אם כל התרגילים מסוג Exercise בוצעו, נעדכן את זמן הסיום
////    if (visitedExercises.Count == exerciseOrder.Count)
////    {
////        finishTime = currentTime;
////        return true;
////    }

////    // ננסה כל תרגיל שנשאר ברשימה
////    for (int i = 0; i < exerciseOrder.Count; i++)
////    {
////        var nodeId = exerciseOrder[i];

////        var node = graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
////        if (node == null)
////            continue;

////        // אם זה צומת מסוג Exercise וביקרנו בו כבר, נדלג עליו
////        if (node.Type == NodeType.Exsercise && visitedExercises.Contains(nodeId))
////            continue;

////        // אם הצומת מסוג Exercise, נוודא שהוא זמין
////        if (node.Type == NodeType.Exsercise && !node.IsAvailableAt(currentTime))
////        {
////            // בדוק אם ניתן להגיע אליו מאוחר יותר
////            if (!CanReachNode(graph, nodeId, exerciseOrder.Where(e => !visitedExercises.Contains(e) && e != nodeId).ToList()))
////            {
////                // אם אי אפשר להגיע אליו ואין מכשיר חילופי, החזר FALSE
////                if (!alternativeDevices.ContainsKey(nodeId) || alternativeDevices[nodeId].Count == 0)
////                {
////                    finishTime = currentTime;
////                    return false;
////                }

////                // נסה מכשיר חילופי
////                foreach (var alternativeId in alternativeDevices[nodeId])
////                {
////                    var alternativeNode = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
////                    if (alternativeNode != null && !visitedExercises.Contains(alternativeId) && alternativeNode.IsAvailableAt(currentTime))
////                    {
////                        // נסה תרגיל חילופי
////                        visitedExercises.Add(alternativeId);
////                        path.Add(alternativeId);

////                        var nextTime = currentTime.AddMinutes(alternativeNode.UsageDurationMinutes);

////                        if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
////                            return true;

////                        // אם תרגיל חילופי לא הצליח, נחזור אחורה
////                        visitedExercises.Remove(alternativeId);
////                        path.RemoveAt(path.Count - 1);
////                    }
////                }

////                // אם אין תרגיל חילופי זמין, החזר FALSE
////                finishTime = currentTime;
////                return false;
////            }
////            else
////            {
////                // אם אפשר להגיע אליו, נסה לשנות את הסדר ולהמשיך
////                var remainingNodes = exerciseOrder.Where(e => !visitedExercises.Contains(e)).ToList();
////                if (Backtrack(graph, remainingNodes, alternativeDevices, visitedExercises, path, currentTime, out finishTime))
////                    return true;
////            }
////        }
////        else
////        {
////            // אם הצומת מסוג MuscleGroup, אין צורך לבדוק זמינות ואין צורך להכניס לסט visited
////            if (node.Type == NodeType.MuscleGroup || (node.Type == NodeType.Exsercise && node.IsAvailableAt(currentTime)))
////            {
////                if (node.Type == NodeType.Exsercise)
////                    visitedExercises.Add(nodeId); // הוסף רק צמתים מסוג Exercise לסט visited

////                path.Add(nodeId);

////                // עדכון הזמן לתרגיל הבא
////                var nextTime = currentTime.AddMinutes(node.UsageDurationMinutes);

////                // חפש את המסלול הבא
////                if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
////                    return true;

////                // אם לא הצלחנו, נחזור אחורה
////                if (node.Type == NodeType.Exsercise)
////                    visitedExercises.Remove(nodeId); // הסר רק צמתים מסוג Exercise מ-visited

////                path.RemoveAt(path.Count - 1);
////            }
////        }
////    }

////    finishTime = currentTime;
////    return false;
////}


//////הוספה של הצמתים השרירים למסלול
////private bool Backtrack(
////    SubGraph graph, // הגרף של המתאמן
////    List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
////    Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
////    HashSet<int> visitedExercises, // התרגילים שביקרנו (רק מסוג Exercise)
////    List<int> path, // מסלול
////    DateTime currentTime, // תאריך התחלה
////    out DateTime finishTime // תאריך סיום
////)
////{
////    // אם כל התרגילים מסוג Exercise בוצעו, נעדכן את זמן הסיום
////    if (visitedExercises.Count == exerciseOrder.Count)
////    {
////        finishTime = currentTime;
////        return true;
////    }

////    // ננסה כל תרגיל שנשאר ברשימה
////    for (int i = 0; i < exerciseOrder.Count; i++)
////    {
////        var nodeId = exerciseOrder[i];

////        var node = graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
////        if (node == null)
////            continue;

////        // אם זה צומת מסוג Exercise וביקרנו בו כבר, נדלג עליו
////        if (node.Type == NodeType.Exsercise && visitedExercises.Contains(nodeId))
////            continue;

////        // אם הצומת מסוג Exercise, נוודא שהוא זמין
////        if (node.Type == NodeType.Exsercise && !node.IsAvailableAt(currentTime))
////        {
////            // בדוק אם ניתן להגיע אליו מאוחר יותר
////            if (!CanReachNode(graph, nodeId, exerciseOrder.Where(e => !visitedExercises.Contains(e) && e != nodeId).ToList()))
////            {
////                // אם אי אפשר להגיע אליו ואין מכשיר חילופי, החזר FALSE
////                if (!alternativeDevices.ContainsKey(nodeId) || alternativeDevices[nodeId].Count == 0)
////                {
////                    finishTime = currentTime;
////                    return false;
////                }

////                // נסה מכשיר חילופי
////                foreach (var alternativeId in alternativeDevices[nodeId])
////                {
////                    var alternativeNode = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
////                    if (alternativeNode != null && !visitedExercises.Contains(alternativeId) && alternativeNode.IsAvailableAt(currentTime))
////                    {
////                        // נסה תרגיל חילופי
////                        visitedExercises.Add(alternativeId);
////                        path.Add(alternativeId);

////                        var nextTime = currentTime.AddMinutes(alternativeNode.UsageDurationMinutes);

////                        if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
////                            return true;

////                        // אם תרגיל חילופי לא הצליח, נחזור אחורה
////                        visitedExercises.Remove(alternativeId);
////                        path.RemoveAt(path.Count - 1);
////                    }
////                }

////                // אם אין תרגיל חילופי זמין, החזר FALSE
////                finishTime = currentTime;
////                return false;
////            }
////            else
////            {
////                // אם אפשר להגיע אליו, נסה לשנות את הסדר ולהמשיך
////                var remainingNodes = exerciseOrder.Where(e => !visitedExercises.Contains(e)).ToList();
////                if (Backtrack(graph, remainingNodes, alternativeDevices, visitedExercises, path, currentTime, out finishTime))
////                    return true;
////            }
////        }
////        else
////        {
////            // אם הצומת מסוג MuscleGroup, אין צורך לבדוק זמינות ואין צורך להכניס לסט visited
////            if (node.Type == NodeType.MuscleGroup || (node.Type == NodeType.Exsercise && node.IsAvailableAt(currentTime)))
////            {
////                if (node.Type == NodeType.Exsercise)
////                    visitedExercises.Add(nodeId); // הוסף רק צמתים מסוג Exercise לסט visited

////                path.Add(nodeId);

////                // **מעבר בצמתים מסוג MuscleGroup**
////                // נבדוק את כל השכנים של הצומת הנוכחי
////                foreach (var neighbor in node.Neighbors)
////                {
////                    if (neighbor.Type == NodeType.MuscleGroup && !path.Contains(neighbor.Id))
////                    {
////                        path.Add(neighbor.Id); // הוסף את צומת השריר למסלול
////                    }
////                }

////                // עדכון הזמן לתרגיל הבא
////                var nextTime = currentTime.AddMinutes(node.UsageDurationMinutes);

////                // חפש את המסלול הבא
////                if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
////                    return true;

////                // אם לא הצלחנו, נחזור אחורה
////                if (node.Type == NodeType.Exsercise)
////                    visitedExercises.Remove(nodeId); // הסר רק צמתים מסוג Exercise מ-visited

////                path.RemoveAt(path.Count - 1);
////            }
////        }
////    }

////    finishTime = currentTime;
////    return false;
////}

////שילוב של בדיקה אם לעבור לצומת של השריר
////private bool Backtrack(
////    SubGraph graph, // הגרף של המתאמן
////    List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
////    Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
////    HashSet<int> visitedExercises, // התרגילים שביקרנו (רק מסוג Exercise)
////    List<int> path, // מסלול
////    DateTime currentTime, // תאריך התחלה
////    out DateTime finishTime // תאריך סיום
////)
////{
////    // אם כל התרגילים מסוג Exercise בוצעו, נעדכן את זמן הסיום
////    if (visitedExercises.Count == exerciseOrder.Count)
////    {
////        finishTime = currentTime;
////        return true;
////    }

////    // ננסה כל תרגיל שנשאר ברשימה
////    for (int i = 0; i < exerciseOrder.Count; i++)
////    {
////        var nodeId = exerciseOrder[i];
////        var node = graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
////        if (node == null)
////            continue;

////        // אם זה צומת מסוג Exercise וביקרנו בו כבר, נדלג עליו
////        if (node.Type == NodeType.Exsercise && visitedExercises.Contains(nodeId))
////            continue;

////        // אם הצומת מסוג Exercise, נוודא שהוא זמין
////        if (node.Type == NodeType.Exsercise && !node.IsAvailableAt(currentTime))
////        {
////            // בדוק אם ניתן להגיע אליו מאוחר יותר
////            if (!CanReachNode(graph, nodeId, exerciseOrder.Where(e => !visitedExercises.Contains(e) && e != nodeId).ToList()))
////            {
////                // אם אי אפשר להגיע אליו ואין מכשיר חילופי, החזר FALSE
////                if (!alternativeDevices.ContainsKey(nodeId) || alternativeDevices[nodeId].Count == 0)
////                {
////                    finishTime = currentTime;
////                    return false;
////                }

////                // נסה מכשיר חילופי
////                foreach (var alternativeId in alternativeDevices[nodeId])
////                {
////                    var alternativeNode = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
////                    if (alternativeNode != null && !visitedExercises.Contains(alternativeId) && alternativeNode.IsAvailableAt(currentTime))
////                    {
////                        // נסה תרגיל חילופי
////                        visitedExercises.Add(alternativeId);
////                        path.Add(alternativeId);

////                        var nextTime = currentTime.AddMinutes(alternativeNode.UsageDurationMinutes);

////                        if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
////                            return true;

////                        // אם תרגיל חילופי לא הצליח, נחזור אחורה
////                        visitedExercises.Remove(alternativeId);
////                        path.RemoveAt(path.Count - 1);
////                    }
////                }

////                // אם אין תרגיל חילופי זמין, החזר FALSE
////                finishTime = currentTime;
////                return false;
////            }
////            else
////            {
////                // אם אפשר להגיע אליו, נסה לשנות את הסדר ולהמשיך
////                var remainingNodes = exerciseOrder.Where(e => !visitedExercises.Contains(e)).ToList();
////                if (Backtrack(graph, remainingNodes, alternativeDevices, visitedExercises, path, currentTime, out finishTime))
////                    return true;
////            }
////        }
////        else
////        {
////            // אם הצומת מסוג MuscleGroup, אין צורך לבדוק זמינות ואין צורך להכניס לסט visited
////            if (node.Type == NodeType.MuscleGroup || (node.Type == NodeType.Exsercise && node.IsAvailableAt(currentTime)))
////            {
////                //if (node.Type == NodeType.Exsercise)
////                //    visitedExercises.Add(nodeId); // הוסף רק צמתים מסוג Exercise לסט visited

////                //path.Add(nodeId);

////                //// **מעבר ישיר בין תרגילים**
////                //// בדוק אם ניתן לעבור ישירות לתרגיל הבא ברשימה
////                //if (i + 1 < exerciseOrder.Count)
////                //{
////                //    var nextExerciseId = exerciseOrder[i + 1];
////                //    var nextExerciseNode = graph.Nodes.FirstOrDefault(n => n.Id == nextExerciseId);
////                //    if (nextExerciseNode != null && node.Neighbors.Contains(nextExerciseNode))
////                //    {
////                //        // אם יש מעבר ישיר, המשך לתרגיל הבא
////                //        continue;
////                //    }
////                //}

////                /////******************
////                //// **מעבר בצמתים מסוג MuscleGroup**
////                //// נבדוק את כל השכנים של הצומת הנוכחי
////                //foreach (var neighbor in node.Neighbors)
////                //{
////                //    if (neighbor.Type == NodeType.MuscleGroup && !path.Contains(neighbor.Id))
////                //    {
////                //        path.Add(neighbor.Id); // הוסף את צומת השריר למסלול
////                //    }
////                //}
////                // אם הצומת מסוג MuscleGroup, אין צורך לבדוק זמינות ואין צורך להכניס לסט visited
////                if (node.Type == NodeType.Exsercise)
////                    visitedExercises.Add(nodeId); // הוסף רק צמתים מסוג Exercise לסט visited

////                path.Add(nodeId);

////                // **מעבר ישיר בין תרגילים**
////                // בדוק אם ניתן לעבור ישירות לתרגיל הבא ברשימה
////                if (i + 1 < exerciseOrder.Count)
////                {
////                    var nextExerciseId = exerciseOrder[i + 1];
////                    var nextExerciseNode = graph.Nodes.FirstOrDefault(n => n.Id == nextExerciseId);

////                    // בדיקה למעבר דרך MuscleGroup אם אין קשת ישירה
////                    if (nextExerciseNode != null && !node.Neighbors.Contains(nextExerciseNode))
////                    {
////                        // ננסה מעבר דרך MuscleGroup
////                        var muscleGroupsPath = GetMuscleGroupPath(node, nextExerciseNode);
////                        if (muscleGroupsPath != null)
////                        {
////                            foreach (var muscleGroupNode in muscleGroupsPath)
////                            {
////                                path.Add(muscleGroupNode.Id);
////                            }
////                        }
////                        else
////                        {
////                            // אם לא מצאנו מעבר, דלג לתרגיל הבא
////                            continue;
////                        }
////                    }
////                }
////            }
////            // עדכון הזמן לתרגיל הבא
////            var nextTime = currentTime.AddMinutes(node.UsageDurationMinutes);

////            // חפש את המסלול הבא
////            if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
////                return true;

////            // אם לא הצלחנו, נחזור אחורה
////            if (node.Type == NodeType.Exsercise)
////                visitedExercises.Remove(nodeId); // הסר רק צמתים מסוג Exercise מ-visited

////            path.RemoveAt(path.Count - 1);
////        }
////    }

////    finishTime = currentTime;
////    return false;
////}


////*****************


////private bool Backtrack(
////    SubGraph graph,
////    List<int> exerciseOrder,
////    HashSet<int> visited,
////    List<int> path,
////    DateTime currentTime,
////    out DateTime finishTime
////)
////{
////    if (visited.Count == exerciseOrder.Count)
////    {
////        finishTime = currentTime;
////        return true;
////    }

////    foreach (var machineId in exerciseOrder)
////    {
////        if (visited.Contains(machineId))
////            continue;

////        var machine = graph.Nodes[machineId];

////        if (!machine.IsAvailableAt(currentTime))
////            continue;

////        visited.Add(machineId);
////        path.Add(machineId);
////        var newTime = currentTime.AddMinutes(machine.UsageDurationMinutes);

////        if (Backtrack(graph, exerciseOrder, visited, path, newTime, out finishTime))
////            return true;

////        // Backtrack
////        visited.Remove(machineId);
////        path.RemoveAt(path.Count - 1);
////    }

////    finishTime = currentTime;
////    return false;
////}



////******************
////private (bool found, int numAlternatives, List<int> bestPath) Backtrack(
////        SubGraph graph,
////        List<int> exerciseOrder,
////        Dictionary<int, List<int>> alternativeDevices,
////        HashSet<int> visitedExercises,
////        List<int> currentPath,
////        int currentAlternatives,
////        DateTime currentTime
////    )
////{
////    // תנאי עצירה: אם כל התרגילים בוצעו (במקור או בחילופי)
////    if (visitedExercises.Count == exerciseOrder.Count)
////        return (true, currentAlternatives, new List<int>(currentPath));

////    bool foundAny = false;
////    int minAlternatives = int.MaxValue;
////    List<int> bestPath = null;

////    for (int i = 0; i < exerciseOrder.Count; i++)
////    {
////        int nodeId = exerciseOrder[i];
////        if (visitedExercises.Contains(nodeId))
////            continue;

////        var node = graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
////        if (node == null)
////            continue;

////        bool isAvilableNode = true;
////        //מעבר על כל התרגילים שלא עברו עליהם ובדיקה האם יש תרגיל שלא ניתן להגיע אליו מהצומת הנוכחית
////        //כאן צריך להוציא לפונקציה חיצונית ששומרת את האפשרויות בתכנון דינאמי
////        foreach (var nextNode in exerciseOrder.Where(e => !visitedExercises.Contains(e) && e != nodeId))
////        {
////            var nextExerciseNode = graph.Nodes.FirstOrDefault(n => n.Id == nextNode);
////            if (nextExerciseNode == null)
////                continue;
////            var hasPath = HasPathToNextExercise(node, nextExerciseNode);
////            if (!hasPath)
////            {
////                isAvilableNode = false;
////                break;
////            }
////        }
////        if (!isAvilableNode)
////        {
////            continue;
////        }

////        //// אם זה הצעד הראשון במסלול, לא צריך בדיקת קשת
////        //bool canMove = currentPath.Count == 0;

////        //// אם זה לא הראשון, בדוק האם יש קשת חוקית מהאחרון למסומן
////        //if (!canMove && currentPath.Count > 0)
////        //{
////        //    int lastNodeId = currentPath.Last();
////        //    var lastNode = graph.Nodes.FirstOrDefault(n => n.Id == lastNodeId);
////        //    // בדיקה: קשת ישירה או דרך MuscleGroup
////        //    canMove =  GetMuscleGroupPath(lastNode, node) != null;
////        //}

////        //if (!canMove)
////        //    continue;

////        // נבדוק האם אפשר לעבור בדרך חוקית לצומת הבא שטרם בוצע
////        //bool hasValidNext = true; // ברירת מחדל - אם אנחנו בתרגיל האחרון
////        //for (int j = 0; j < exerciseOrder.Count; j++)
////        //{
////        //    if (i == j || visitedExercises.Contains(exerciseOrder[j]))
////        //        continue;
////        //    var nextNode = graph.Nodes.FirstOrDefault(n => n.Id == exerciseOrder[j]);
////        //    if (nextNode != null && HasPathToNextExercise(node, nextNode))
////        //    {
////        //        hasValidNext = true;
////        //        break;
////        //    }
////        //    else
////        //    {
////        //        hasValidNext = false;
////        //    }
////        //}

////        //if (!hasValidNext && visitedExercises.Count + 1 < exerciseOrder.Count)
////        //    continue;

////        // ננסה קודם את התרגיל הרגיל
////        if (node.IsAvailableAt(currentTime))
////        {
////            visitedExercises.Add(nodeId);
////            currentPath.Add(nodeId);

////            DateTime nextTime = currentTime.AddMinutes(node.UsageDurationMinutes);
////            var (found, alternatives, pathCandidate) = Backtrack(
////                graph, exerciseOrder, alternativeDevices, visitedExercises, currentPath, currentAlternatives, nextTime);

////            if (found && alternatives < minAlternatives)
////            {
////                foundAny = true;
////                minAlternatives = alternatives;
////                bestPath = new List<int>(pathCandidate);
////            }

////            visitedExercises.Remove(nodeId);
////            currentPath.RemoveAt(currentPath.Count - 1);
////        }
////        // אם אינו זמין ננסה תרגיל חילופי
////        else if (alternativeDevices.TryGetValue(nodeId, out var alternativesList))
////        {
////            foreach (var altId in alternativesList)
////            {
////                if (visitedExercises.Contains(altId))
////                    continue;
////                var altNode = graph.Nodes.FirstOrDefault(n => n.Id == altId);
////                if (altNode == null || !altNode.IsAvailableAt(currentTime))
////                    continue;

////                // בדוק מעבר חוקי בין node (או בין תרגיל קודם) ל-altNode
////                //bool hasValidAltNext = true;
////                //for (int j = 0; j < exerciseOrder.Count; j++)
////                //{
////                //    if (i == j || visitedExercises.Contains(exerciseOrder[j]))
////                //        continue;
////                //    var nextNode = graph.Nodes.FirstOrDefault(n => n.Id == exerciseOrder[j]);
////                //    if (nextNode != null && HasPathToNextExercise(altNode, nextNode))
////                //    {
////                //        hasValidAltNext = true;
////                //        break;
////                //    }
////                //    else
////                //    {
////                //        hasValidAltNext = false;
////                //    }
////                //}
////                //if (!hasValidAltNext && visitedExercises.Count + 1 < exerciseOrder.Count)
////                //    continue;

////                visitedExercises.Add(nodeId); // מסמן שביצענו את התרגיל הזה ע"י חילופי
////                visitedExercises.Add(altId);
////                currentPath.Add(altId);

////                DateTime nextTimeAlt = currentTime.AddMinutes(altNode.UsageDurationMinutes);
////                var (found, alternatives, pathCandidate) = Backtrack(
////                    graph, exerciseOrder, alternativeDevices, visitedExercises, currentPath, currentAlternatives + 1, nextTimeAlt);

////                if (found && alternatives < minAlternatives)
////                {
////                    foundAny = true;
////                    minAlternatives = alternatives;
////                    bestPath = new List<int>(pathCandidate);
////                }

////                visitedExercises.Remove(nodeId);
////                visitedExercises.Remove(altId);
////                currentPath.RemoveAt(currentPath.Count - 1);
////            }
////        }
////    }

////    if (foundAny)
////        return (true, minAlternatives, bestPath);
////    return (false, 0, null);
////}
