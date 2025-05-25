//using DBEntities.Models;
//using IBLL;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BLL
//{
//    public class PathFinderServiceBLLNEW

//    {
//        //3D
//        public class TableCell
//        {
//            public int LegalityValue { get; set; } // לדוג' 2^שריר, -1, 0
//            public Queue<Trainee> Queue { get; set; } = new Queue<Trainee>();
//        }
//        // גודל: [תרגיל יעד, תרגיל מקור, זמן]
//        //TableCell[,,] Table3D = new TableCell[N, N, M];
//        //// נניח ש-N = מספר תרגילים, M = מספר תורים (או זמנים אפשריים)
//        //public void InitializeTable3D() {
//        //    for (int to = 0; to < N; to++)
//        //        for (int from = 0; from < N; from++)
//        //            for (int slot = 0; slot < M; slot++)
//        //            {
//        //                Table3D[to, from, slot] = new TableCell
//        //                {
//        //                    LegalityValue = /* לחשב לפי חוקיות הגרף והמעברים */,
//        //                    Queue = new Queue<Trainee>()
//        //                };
//        //            }
//            //דוגמה לשימוש בפונקציה
//    //        int from = ...; // תרגיל קודם
//    //        int to = ...;   // תרגיל נוכחי
//    //        int slot = ...; // אינדקס זמן

//    //        var cell = Table3D[to, from, slot];

//    //        // בדיקת חוקיות מעבר
//    //        if (cell.LegalityValue == -1) return false; // אסור
//    //        if (cell.Queue.Count < maxTraineesPerSlot)
//    //        {
//    //            cell.Queue.Enqueue(currentTrainee); // שיבוץ מתאמן בתור
//    //                                                // המשך אלגוריתם (רקורסיה/לולאה)
//    //            ...
//    //cell.Queue.Dequeue(); // Backtrack - הסרה מהתור
//    //        }
//    //        else
//    //        {
//    //            // חפש תרגיל חלופי, דחה בזמן, או נסה חילוף עם מתאמן אחר
//    //        }
//        }

//        public class Node
//        {
//            public int Id { get; set; }
//            public int UsageDurationMinutes { get; set; }
//            public List<Node> Neighbors { get; set; } = new List<Node>();

//            // מילון: זמן -> תור של מתאמנים (אפשר גם HashSet אם כל מכשיר מקבל רק אחד בכל זמן)
//            public Dictionary<DateTime, Queue<Trainee>> TimeQueues { get; set; } = new();

//            public bool IsAvailableAt(DateTime time, int maxInQueue = 1)
//            {
//                if (!TimeQueues.ContainsKey(time))
//                    return true;
//                return TimeQueues[time].Count < maxInQueue;
//            }

//            public void AddToQueue(DateTime time, Trainee trainee)
//            {
//                if (!TimeQueues.ContainsKey(time))
//                    TimeQueues[time] = new Queue<Trainee>();
//                TimeQueues[time].Enqueue(trainee);
//            }

//            public void RemoveFromQueue(DateTime time, Trainee trainee)
//            {
//                if (TimeQueues.ContainsKey(time))
//                {
//                    // מחיקה מתור – כאן אפשר לשפר ליעילות אם צריך
//                    var q = TimeQueues[time];
//                    var newQ = new Queue<Trainee>(q.Where(t => t != trainee));
//                    TimeQueues[time] = newQ;
//                }
//            }
//        }

//        public class Trainee
//        {
//            public int Id { get; set; }
//            public string Name { get; set; }
//            // אפשר להוסיף כאן נתונים רלוונטיים
//        }
//        public class SubGraph
//        {
//            public List<Node> Nodes { get; set; } = new List<Node>();
//        }
//        // תוצאה של מסלול
//        public class PathResult
//        {
//            public List<int> ExerciseIdsInPath { get; set; } = new();
//            public DateTime StartTime { get; set; }
//            public DateTime EndTime { get; set; }
//        }

//        // מילון memoization: (mask, lastNodeId, roundedTime) -> תוצאה
//        private Dictionary<(int, int, long), (bool, int, List<int>, DateTime)> memo = new();

//        // טבלה תלת-ממדית: [exercise, time] => Queue<Trainee> (כל תרגיל בכל סלוט זמן, מי בתור)
//        // נניח שה-Node עצמו מחזיק Queue, או שנבנה מבנה עזר

//        private bool IsAvailable(Node node, DateTime time)
//        {
//            // נניח Queue של תורים לפי זמנים במבנה הנתונים
//            return node.IsAvailableAt(time);
//        }
//        private (bool found, int numAlternatives, List<int> bestPath, DateTime endTime) Backtrack(
//            SubGraph graph,
//            Dictionary<int, Node> nodeDict,
//            Dictionary<int, HashSet<int>> reachableFromNode,
//            List<int> exerciseOrder,
//            Dictionary<int, List<int>> alternativeDevices,
//            int mask,
//            List<int> currentPath,
//            int currentAlternatives,
//            DateTime currentTime,
//            out DateTime endTime,
//            List<Trainee> allTrainees,
//            Trainee currentTrainee)
//        {
//            // תנאי עצירה
//            if (CountBits(mask) == exerciseOrder.Count)
//            {
//                endTime = currentTime;
//                return (true, currentAlternatives, new List<int>(currentPath), currentTime);
//            }

//            // Memoization
//            int lastNodeId = currentPath.Count > 0 ? currentPath.Last() : -1;
//            long timeMinutes = (long)(currentTime - DateTime.MinValue).TotalMinutes;
//            long roundedTimeMinutes = timeMinutes - (timeMinutes % 5);
//            var memoKey = (mask, lastNodeId, roundedTimeMinutes);
//            if (memo.TryGetValue(memoKey, out var memoResult))
//            {
//                endTime = memoResult.Item4;
//                return memoResult;
//            }

//            bool foundAny = false;
//            int minAlternatives = int.MaxValue;
//            List<int> bestPath = null;
//            DateTime bestEndTime = DateTime.MinValue;

//            // שלב 1: מנסים את כל האפשרויות אצל המתאמן הנוכחי (כולל תרגילים חילופיים ודחייה)
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                int nodeId = exerciseOrder[i];
//                if ((mask & (1 << nodeId)) != 0)
//                    continue;

//                var node = nodeDict[nodeId];

//                // האם חוקי לעבור אליו (כללי הגרף/שרירים, וכו')
//                if (!reachableFromNode[currentPath.LastOrDefault()].Contains(nodeId) && currentPath.Count > 0)
//                    continue;

//                // 1. נסה את התרגיל המקורי אם פנוי
//                if (IsAvailable(node, currentTime))
//                {
//                    node.AddToQueue(currentTime, currentTrainee); // תוסיף לתור
//                    currentPath.Add(nodeId);

//                    var (found, alternatives, pathCandidate, candidateEndTime) = Backtrack(
//                        graph, nodeDict, reachableFromNode, exerciseOrder, alternativeDevices,
//                        mask | (1 << nodeId), currentPath, currentAlternatives,
//                        currentTime.AddMinutes(node.UsageDurationMinutes),
//                        out DateTime candEndTime, allTrainees, currentTrainee
//                    );

//                    if (found && alternatives < minAlternatives)
//                    {
//                        foundAny = true;
//                        minAlternatives = alternatives;
//                        bestPath = new List<int>(pathCandidate);
//                        bestEndTime = candidateEndTime;
//                        if (alternatives == 0) break;
//                    }

//                    node.RemoveFromQueue(currentTime, currentTrainee); // החזר מצב
//                    currentPath.RemoveAt(currentPath.Count - 1);
//                }

//                // 2. נסה תרגילים חילופיים (רק אצל עצמו)
//                if (alternativeDevices.TryGetValue(nodeId, out var alternativesList))
//                {
//                    foreach (var altId in alternativesList)
//                    {
//                        if ((mask & (1 << altId)) != 0)
//                            continue;
//                        var altNode = nodeDict[altId];
//                        if (!IsAvailable(altNode, currentTime))
//                            continue;

//                        altNode.AddToQueue(currentTime, currentTrainee);
//                        currentPath.Add(altId);

//                        var (found, alternatives, pathCandidate, candidateEndTime) = Backtrack(
//                            graph, nodeDict, reachableFromNode, exerciseOrder, alternativeDevices,
//                            mask | (1 << nodeId) | (1 << altId), currentPath, currentAlternatives + 1,
//                            currentTime.AddMinutes(altNode.UsageDurationMinutes),
//                            out DateTime candEndTime, allTrainees, currentTrainee
//                        );

//                        if (found && alternatives < minAlternatives)
//                        {
//                            foundAny = true;
//                            minAlternatives = alternatives;
//                            bestPath = new List<int>(pathCandidate);
//                            bestEndTime = candidateEndTime;
//                            if (alternatives == 0) break;
//                        }

//                        altNode.RemoveFromQueue(currentTime, currentTrainee);
//                        currentPath.RemoveAt(currentPath.Count - 1);
//                    }
//                }

//                // 3. אפשרות לדחות תרגיל לזמן פנוי (רשות, תוסיף אם תרצה)
//                // כאן אפשר להוסיף לולאת דחייה קדימה בזמן (לדוג' 5-10 דקות), רק אם זה חוקי לכללי המתאמן
//            }

//            // שלב 2: אם כל האפשרויות אצל עצמי כשלו, בדוק אפשרות להזיז למתאמנים אחרים
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                int nodeId = exerciseOrder[i];
//                if ((mask & (1 << nodeId)) != 0)
//                    continue;
//                var node = nodeDict[nodeId];

//                Trainee otherTrainee = node.GetOccupyingTraineeAt(currentTime);
//                if (otherTrainee != null && otherTrainee != currentTrainee)
//                {
//                    if (otherTrainee.CanRescheduleExercise(nodeId, currentTime))
//                    {
//                        otherTrainee.RescheduleExercise(nodeId, currentTime);
//                        node.AddToQueue(currentTime, currentTrainee);
//                        currentPath.Add(nodeId);

//                        var (found, alternatives, pathCandidate, candidateEndTime) = Backtrack(
//                            graph, nodeDict, reachableFromNode, exerciseOrder, alternativeDevices,
//                            mask | (1 << nodeId), currentPath, currentAlternatives,
//                            currentTime.AddMinutes(node.UsageDurationMinutes),
//                            out DateTime candEndTime, allTrainees, currentTrainee
//                        );

//                        if (found && alternatives < minAlternatives)
//                        {
//                            foundAny = true;
//                            minAlternatives = alternatives;
//                            bestPath = new List<int>(pathCandidate);
//                            bestEndTime = candidateEndTime;
//                        }

//                        node.RemoveFromQueue(currentTime, currentTrainee);
//                        currentPath.RemoveAt(currentPath.Count - 1);
//                        otherTrainee.UndoReschedule(nodeId, currentTime);
//                    }
//                }
//            }

//            endTime = bestEndTime;
//            var result = foundAny ? (true, minAlternatives, bestPath, bestEndTime) : (false, 0, null, DateTime.MinValue);
//            memo[memoKey] = result;
//            return result;
//        }


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



//    }




//}
