//using Castle.Core.Internal;
//using Microsoft.Azure.Management.ResourceManager.Fluent.Core.DAG;
//using Microsoft.Graph;
//using Microsoft.Graph.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;

//namespace BLL
//{
//    public class TimeSlot
//    {
//        public int UserId { get; set; }
//        public DateTime StartTime { get; set; }
//        public DateTime EndTime { get; set; }
//    }

//    public class GraphNode
//    {
//        public List<TimeSlot> TimeQueue { get; set; } = new List<TimeSlot>();
//        public SemaphoreSlim Semaphore { get; set; } = new SemaphoreSlim(1, 1);

//        public bool IsAvailableAt(DateTime startTime, DateTime endTime)
//        {
//            return TimeQueue.All(slot => slot.EndTime <= startTime || slot.StartTime >= endTime);
//        }
//    }

//    public class Graph<TNodeData, TNode>
//    {
//        public Dictionary<string, TNode> Nodes { get; set; } = new Dictionary<string, TNode>();
//        public Dictionary<string, List<string>> Edges { get; set; } = new Dictionary<string, List<string>>();

//        public void AddNode(string key, TNode node)
//        {
//            Nodes[key] = node;
//        }

//        public void AddEdge(string from, string to)
//        {
//            if (!Edges.ContainsKey(from))
//                Edges[from] = new List<string>();

//            Edges[from].Add(to);
//        }
//    }

//    public class Trainee
//    {
//        public int Id { get; set; }
//        public List<string> DefaultProgram { get; set; } = new List<string>();
//        public int ExerciseDuration { get; set; } // משך כל תרגיל בדקות
//    }


//    //public class GymManagementSystem
//    //{
//    //    private Graph<string, GraphNode> exerciseGraph;

//    //    public GymManagementSystem(Graph<string, GraphNode> graph)
//    //    {
//    //        this.exerciseGraph = graph;
//    //    }

//    //    //public List<string> ManageWorkout(TraineeBLL user, DateTime startTime)
//    //    //{
//    //    //    var path = FindOptimalPath(user, startTime);
//    //    //    if (path == null || path.Count == 0)
//    //    //        throw new Exception("No valid path found for the user's workout program.");

//    //    //    DateTime currentTime = startTime;
//    //    //    foreach (var exercise in path)
//    //    //    {
//    //    //        var machine = exerciseGraph.Nodes[exercise];

//    //    //        if (machine.Semaphore.Wait(0))
//    //    //        {
//    //    //            try
//    //    //            {
//    //    //                if (machine.IsAvailableAt(currentTime, currentTime.AddMinutes(user.ExerciseDuration)))
//    //    //                {
//    //    //                    machine.TimeQueue.Add(new TimeSlot
//    //    //                    {
//    //    //                        UserId = user.Id,
//    //    //                        StartTime = currentTime,
//    //    //                        EndTime = currentTime.AddMinutes(user.ExerciseDuration)
//    //    //                    });
//    //    //                    currentTime = currentTime.AddMinutes(user.ExerciseDuration);
//    //    //                }
//    //    //                else
//    //    //                {
//    //    //                    throw new Exception($"No available machines for exercise {exercise}");
//    //    //                }
//    //    //            }
//    //    //            finally
//    //    //            {
//    //    //                machine.Semaphore.Release();
//    //    //            }
//    //    //        }
//    //    //    }
//    //    //    return path;
//    //    //}
//    //    public List<string> ManageWorkout(Trainee user, DateTime startTime)
//    //    {
//    //        var completedExercises = new List<string>();
//    //        var visitedEdges = new HashSet<(string, string)>();
//    //        var remainingExercises = new HashSet<string>(user.DefaultProgram);

//    //        DateTime currentTime = startTime;

//    //        // מפעיל DFS כדי למצוא מסלול חוקי
//    //        if (!FindPath(user, currentTime, user.DefaultProgram.First(), completedExercises, visitedEdges, remainingExercises))
//    //        {
//    //            throw new Exception("No valid workout path found.");
//    //        }

//    //        return completedExercises;
//    //    }

//    //    private bool FindPath(Trainee user, DateTime currentTime, string currentExercise, List<string> completedExercises, HashSet<(string, string)> visitedEdges, HashSet<string> remainingExercises)
//    //    {
//    //        // אם ביצענו את כל התרגילים, סיימנו
//    //        if (remainingExercises.Count == 0)
//    //        {
//    //            return true;
//    //        }

//    //        // אם המכשיר הנוכחי תפוס, ננסה לבדוק תרגילים חלופיים
//    //        var machine = exerciseGraph.Nodes[currentExercise];
//    //        if (!machine.Semaphore.Wait(0))
//    //        {
//    //            // בדיקת תרגילים חלופיים
//    //            var alternativeExercise = FindAlternativeExercise(currentExercise, remainingExercises);
//    //            if (alternativeExercise != null)
//    //            {
//    //                return FindPath(user, currentTime, alternativeExercise, completedExercises, visitedEdges, remainingExercises);
//    //            }

//    //            // אם אין תרגיל חלופי, ננסה לשנות את תוכניות האימון של המתאמנים בתור
//    //            if (TryReorderOtherTrainees(machine, currentTime, user))
//    //            {
//    //                return FindPath(user, currentTime, currentExercise, completedExercises, visitedEdges, remainingExercises);
//    //            }

//    //            // אם לא הצלחנו לשנות את תוכניות המתאמנים, מחזירים את המתאמן לתור
//    //            machine.TimeQueue.Add(new TimeSlot
//    //            {
//    //                UserId = user.Id,
//    //                StartTime = currentTime,
//    //                EndTime = DateTime.MaxValue // מסמן שאין זמן סיום מוגדר
//    //            });

//    //            return false;
//    //        }

//    //        try
//    //        {
//    //            // בדיקת זמינות המכשיר
//    //            if (machine.IsAvailableAt(currentTime, currentTime.AddMinutes(user.ExerciseDuration)))
//    //            {
//    //                // מוסיפים את המתאמן לתור
//    //                machine.TimeQueue.Add(new TimeSlot
//    //                {
//    //                    UserId = user.Id,
//    //                    StartTime = currentTime,
//    //                    EndTime = currentTime.AddMinutes(user.ExerciseDuration)
//    //                });

//    //                // עדכון השעה הנוכחית והתרגילים שהושלמו
//    //                currentTime = currentTime.AddMinutes(user.ExerciseDuration);
//    //                completedExercises.Add(currentExercise);
//    //                remainingExercises.Remove(currentExercise);
//    //            }
//    //            else
//    //            {
//    //                return false; // אם המכשיר תפוס ואין אפשרות להשתמש בו
//    //            }
//    //        }
//    //        finally
//    //        {
//    //            machine.Semaphore.Release();
//    //        }

//    //        // מעבר לצמתים הבאים בתוכנית
//    //        foreach (var nextExercise in user.DefaultProgram)
//    //        {
//    //            if (remainingExercises.Contains(nextExercise) &&
//    //                !visitedEdges.Contains((currentExercise, nextExercise)) &&
//    //                exerciseGraph.Edges[currentExercise].Contains(nextExercise))
//    //            {
//    //                // מסמנים את הקשת כנמצאת בשימוש כדי למנוע חזרתיות
//    //                visitedEdges.Add((currentExercise, nextExercise));

//    //                // ננסה לעבור לצומת הבא
//    //                if (FindPath(user, currentTime, nextExercise, completedExercises, visitedEdges, remainingExercises))
//    //                {
//    //                    return true; // אם המסלול הצליח, מחזירים הצלחה
//    //                }

//    //                // אם המסלול נכשל, מסירים את הקשת מהסט
//    //                visitedEdges.Remove((currentExercise, nextExercise));
//    //            }
//    //        }

//    //        return false; // אם לא נמצאה דרך חוקית להמשיך, מחזירים כשלון
//    //    }

//    //    private string FindAlternativeExercise(string currentExercise, HashSet<string> remainingExercises)
//    //    {
//    //        // חיפוש תרגיל חלופי שעובד על אותם שרירים ומפרקים
//    //        var exerciseData = GetExerciseData(currentExercise);
//    //        foreach (var alternative in exerciseData.AlternativeExercises)
//    //        {
//    //            if (remainingExercises.Contains(alternative))
//    //            {
//    //                return alternative; // מחזיר תרגיל חלופי זמין
//    //            }
//    //        }
//    //        return null; // אין תרגיל חלופי זמין
//    //    }

//    //    private bool TryReorderOtherTrainees(GraphNode machine, DateTime currentTime, Trainee currentUser)
//    //    {
//    //        // ניסיון לשנות את תוכניות האימון של מתאמנים אחרים בתור
//    //        foreach (var timeSlot in machine.TimeQueue)
//    //        {
//    //            var otherUser = GetUserById(timeSlot.UserId);
//    //            if (TryReorderUserExercises(otherUser, machine, currentTime))
//    //            {
//    //                return true;
//    //            }
//    //        }
//    //        return false;
//    //    }

//    //    private bool TryReorderUserExercises(Trainee otherUser, GraphNode machine, DateTime currentTime)
//    //    {
//    //        // בדיקת אפשרות לשנות את תוכנית האימון של מתאמן אחר
//    //        foreach (var exercise in otherUser.DefaultProgram)
//    //        {
//    //            if (exerciseGraph.Nodes[exercise].IsAvailableAt(currentTime, currentTime.AddMinutes(otherUser.ExerciseDuration)))
//    //            {
//    //                return true; // ניתן לשנות את התוכנית
//    //            }
//    //        }

//    //        return false; // לא ניתן לשנות את התוכנית
//    //    }

//    //    private List<string> FindOptimalPath(Trainee user, DateTime currentTime)
//    //    {
//    //        var openSet = new PriorityQueue<string, double>();
//    //        var cameFrom = new Dictionary<string, string>();
//    //        var gScore = new Dictionary<string, double>();
//    //        var fScore = new Dictionary<string, double>();

//    //        foreach (var node in exerciseGraph.Nodes.Keys)
//    //        {
//    //            gScore[node] = double.MaxValue;
//    //            fScore[node] = double.MaxValue;
//    //        }

//    //        // מתחילים מהצומת הראשון בתוכנית האימון
//    //        string startNode = user.DefaultProgram.First();
//    //        gScore[startNode] = 0;
//    //        fScore[startNode] = Heuristic(startNode, user.DefaultProgram.Last(), currentTime);

//    //        openSet.Enqueue(startNode, fScore[startNode]);

//    //        while (openSet.Count > 0)
//    //        {
//    //            string current = openSet.Dequeue();

//    //            // אם הגענו לצומת האחרון בתוכנית, סיימנו
//    //            if (current == user.DefaultProgram.Last())
//    //            {
//    //                return ReconstructPath(cameFrom, current);
//    //            }

//    //            // מוצאים את הצומת הבא בתוכנית האימון
//    //            int currentIndex = user.DefaultProgram.IndexOf(current);
//    //            if (currentIndex == -1 || currentIndex == user.DefaultProgram.Count - 1)
//    //            {
//    //                continue; // אין צומת הבא
//    //            }

//    //            string nextExercise = user.DefaultProgram[currentIndex + 1];

//    //            // בודקים אם הצומת הבא חוקי
//    //            if (exerciseGraph.Edges.ContainsKey(current) && exerciseGraph.Edges[current].Contains(nextExercise))
//    //            {
//    //                double tentativeGScore = gScore[current] + GetEdgeCost(current, nextExercise, user, currentTime);

//    //                if (tentativeGScore < gScore[nextExercise])
//    //                {
//    //                    cameFrom[nextExercise] = current;
//    //                    gScore[nextExercise] = tentativeGScore;
//    //                    fScore[nextExercise] = gScore[nextExercise] + Heuristic(nextExercise, user.DefaultProgram.Last(), currentTime);

//    //                    if (!openSet.Any(x => x == nextExercise))
//    //                    {
//    //                        openSet.Enqueue(nextExercise, fScore[nextExercise]);
//    //                    }
//    //                }
//    //            }
//    //        }

//    //        return null;
//    //    }

//    //    private double Heuristic(string current, string target, DateTime currentTime)
//    //    {
//    //        var currentNode = exerciseGraph.Nodes[current];
//    //        var targetNode = exerciseGraph.Nodes[target];

//    //        double queueFactor = currentNode.TimeQueue.Count;
//    //        double availabilityFactor = currentNode.IsAvailableAt(currentTime, currentTime.AddMinutes(10)) ? 0 : 100;

//    //        return queueFactor + availabilityFactor;
//    //    }

//    //    private double GetEdgeCost(string from, string to, User user, DateTime currentTime)
//    //    {
//    //        var fromNode = exerciseGraph.Nodes[from];
//    //        var toNode = exerciseGraph.Nodes[to];

//    //        double timeCost = toNode.IsAvailableAt(currentTime, currentTime.AddMinutes(user.ExerciseDuration)) ? 0 : 50;
//    //        double queueCost = toNode.TimeQueue.Count;

//    //        return timeCost + queueCost;
//    //    }

//    //    private List<string> ReconstructPath(Dictionary<string, string> cameFrom, string current)
//    //    {
//    //        var path = new List<string> { current };
//    //        while (cameFrom.ContainsKey(current))
//    //        {
//    //            current = cameFrom[current];
//    //            path.Insert(0, current);
//    //        }
//    //        return path;
//    //    }

//    //    private bool ReorderExercises(TraineeBLL user, GraphNode machine, DateTime currentTime, HashSet<(string, string)> visitedEdges)
//    //    {
//    //        foreach (var timeSlot in machine.TimeQueue)
//    //        {
//    //            var otherUser = GetUserById(timeSlot.UserId);
//    //            if (TryReorderUserExercises(otherUser, machine, currentTime, visitedEdges))
//    //            {
//    //                return true;
//    //            }
//    //        }
//    //        return false;
//    //    }

//    //    private bool TryReorderUserExercises(TraineeBLL otherUser, GraphNode machine, DateTime currentTime, HashSet<(string, string)> visitedEdges)
//    //    {
//    //        foreach (var exercise in otherUser.DefaultProgram)
//    //        {
//    //            if (exerciseGraph.Nodes[exercise].IsAvailableAt(currentTime, currentTime.AddMinutes(otherUser.ExerciseDuration)))
//    //            {
//    //                return true;
//    //            }
//    //        }

//    //        return false;
//    //    }
//    //}

//    //public class GymManagementSystem
//    //{
//    //    private Graph<string, GraphNode> exerciseGraph;
//    //    private Dictionary<string, ExerciseData> exerciseDataMap;

//    //    public GymManagementSystem(Graph<string, GraphNode> graph, Dictionary<string, ExerciseData> exerciseData)
//    //    {
//    //        this.exerciseGraph = graph;
//    //        this.exerciseDataMap = exerciseData;
//    //    }

//    //    public List<string> ManageWorkout(Trainee user, DateTime startTime)
//    //    {
//    //        var completedExercises = new List<string>();
//    //        var remainingExercises = new HashSet<string>(user.DefaultProgram);

//    //        DateTime currentTime = startTime;

//    //        // מפעיל BFS כדי למצוא מסלול חוקי
//    //        if (!FindPathBFS(user, currentTime, completedExercises, remainingExercises))
//    //        {
//    //            throw new Exception("No valid workout path found.");
//    //        }

//    //        return completedExercises;
//    //    }

//    //    private bool FindPathBFS(Trainee user, DateTime startTime, List<string> completedExercises, HashSet<string> remainingExercises)
//    //    {
//    //        var queue = new Queue<(string, DateTime, List<string>)>();
//    //        queue.Enqueue((user.DefaultProgram.First(), startTime, new List<string>()));

//    //        while (queue.Count > 0)
//    //        {
//    //            var (currentExercise, currentTime, path) = queue.Dequeue();

//    //            if (remainingExercises.Count == 0)
//    //            {
//    //                completedExercises.AddRange(path);
//    //                return true;
//    //            }

//    //            // בדיקת זמינות התרגיל הנוכחי
//    //            var machine = exerciseGraph.Nodes[currentExercise];
//    //            if (machine.IsAvailableAt(currentTime, currentTime.AddMinutes(user.ExerciseDuration)))
//    //            {
//    //                machine.TimeQueue.Add(new TimeSlot
//    //                {
//    //                    UserId = user.Id,
//    //                    StartTime = currentTime,
//    //                    EndTime = currentTime.AddMinutes(user.ExerciseDuration)
//    //                });

//    //                path.Add(currentExercise);
//    //                remainingExercises.Remove(currentExercise);
//    //                currentTime = currentTime.AddMinutes(user.ExerciseDuration);
//    //            }
//    //            else
//    //            {
//    //                // חיפוש תרגיל חלופי
//    //                var alternativeExercise = FindAlternativeExercise(currentExercise, remainingExercises);
//    //                if (alternativeExercise != null)
//    //                {
//    //                    queue.Enqueue((alternativeExercise, currentTime, new List<string>(path)));
//    //                    continue;
//    //                }

//    //                // ניסיון לשנות תוכניות למתאמנים בתור
//    //                if (TryReorderOtherTrainees(machine, currentTime, user))
//    //                {
//    //                    queue.Enqueue((currentExercise, currentTime, new List<string>(path)));
//    //                    continue;
//    //                }

//    //                // אם אין פתרון, מוסיפים את המתאמן לתור
//    //                machine.TimeQueue.Add(new TimeSlot
//    //                {
//    //                    UserId = user.Id,
//    //                    StartTime = currentTime,
//    //                    EndTime = DateTime.MaxValue
//    //                });
//    //                return false;
//    //            }

//    //            foreach (var nextExercise in exerciseGraph.Edges[currentExercise])
//    //            {
//    //                if (remainingExercises.Contains(nextExercise))
//    //                {
//    //                    queue.Enqueue((nextExercise, currentTime, new List<string>(path)));
//    //                }
//    //            }
//    //        }

//    //        return false;
//    //    }

//    //    private string FindAlternativeExercise(string currentExercise, HashSet<string> remainingExercises)
//    //    {
//    //        if (!exerciseDataMap.ContainsKey(currentExercise))
//    //            return null;

//    //        foreach (var alternative in exerciseDataMap[currentExercise].AlternativeExercises)
//    //        {
//    //            if (remainingExercises.Contains(alternative))
//    //            {
//    //                return alternative; // מחזיר תרגיל חלופי זמין
//    //            }
//    //        }

//    //        return null; // אין תרגיל חלופי זמין
//    //    }

//    //    private bool TryReorderOtherTrainees(GraphNode machine, DateTime currentTime, Trainee currentUser)
//    //    {
//    //        foreach (var timeSlot in machine.TimeQueue)
//    //        {
//    //            var otherUser = GetUserById(timeSlot.UserId);
//    //            if (TryReorderUserExercises(otherUser, machine, currentTime))
//    //            {
//    //                return true;
//    //            }
//    //        }
//    //        return false;
//    //    }

//    //    private bool TryReorderUserExercises(Trainee otherUser, GraphNode machine, DateTime currentTime)
//    //    {
//    //        foreach (var exercise in otherUser.DefaultProgram)
//    //        {
//    //            if (exerciseGraph.Nodes[exercise].IsAvailableAt(currentTime, currentTime.AddMinutes(otherUser.ExerciseDuration)))
//    //            {
//    //                return true; // ניתן לשנות את התוכנית
//    //            }
//    //        }

//    //        return false;
//    //    }
//    //}

//    //public class GymManagementSystem
//    //{
//    //    private Graph<string, GraphNode> exerciseGraph;
//    //    private Dictionary<string, ExerciseData> exerciseDataMap;

//    //    public GymManagementSystem(Graph<string, GraphNode> graph, Dictionary<string, ExerciseData> exerciseData)
//    //    {
//    //        this.exerciseGraph = graph;
//    //        this.exerciseDataMap = exerciseData;
//    //    }

//    //    public List<string> ManageWorkout(Trainee user, DateTime startTime)
//    //    {
//    //        var completedExercises = new List<string>();
//    //        var remainingExercises = new HashSet<string>(user.DefaultProgram);
//    //        DateTime currentTime = startTime;

//    //        // הפעלת האלגוריתם עם הסדר המעודכן
//    //        if (!FindPath(user, currentTime, completedExercises, remainingExercises))
//    //        {
//    //            throw new Exception("No valid workout path found.");
//    //        }

//    //        return completedExercises;
//    //    }

//    //    private bool FindPath(Trainee user, DateTime currentTime, List<string> completedExercises, HashSet<string> remainingExercises)
//    //    {
//    //        var queue = new Queue<(string, DateTime, List<string>)>();
//    //        queue.Enqueue((user.DefaultProgram.First(), currentTime, new List<string>()));

//    //        while (queue.Count > 0)
//    //        {
//    //            var (currentExercise, currentTime, path) = queue.Dequeue();

//    //            if (remainingExercises.Count == 0)
//    //            {
//    //                completedExercises.AddRange(path);
//    //                return true;
//    //            }

//    //            var machine = exerciseGraph.Nodes[currentExercise];

//    //            // שלב 1: בדיקת תרגיל חלופי
//    //            if (!machine.IsAvailableAt(currentTime, currentTime.AddMinutes(user.ExerciseDuration)))
//    //            {
//    //                var alternativeExercise = FindAlternativeExercise(currentExercise, remainingExercises);
//    //                if (alternativeExercise != null)
//    //                {
//    //                    queue.Enqueue((alternativeExercise, currentTime, new List<string>(path)));
//    //                    continue;
//    //                }

//    //                // שלב 2: ניסיון לשנות את סדר התרגילים של המתאמן
//    //                if (TryReorderUserExercises(user, currentExercise, currentTime, remainingExercises))
//    //                {
//    //                    queue.Enqueue((currentExercise, currentTime, new List<string>(path)));
//    //                    continue;
//    //                }

//    //                // שלב 3: ניסיון לשנות את סדר התרגילים של מתאמנים אחרים
//    //                if (TryReorderOtherTrainees(machine, currentTime))
//    //                {
//    //                    queue.Enqueue((currentExercise, currentTime, new List<string>(path)));
//    //                    continue;
//    //                }

//    //                // שלב 4: הוספה לתור
//    //                machine.TimeQueue.Add(new TimeSlot
//    //                {
//    //                    UserId = user.Id,
//    //                    StartTime = currentTime,
//    //                    EndTime = DateTime.MaxValue
//    //                });
//    //                return false;
//    //            }

//    //            // עדכון הזמן והתרגילים
//    //            path.Add(currentExercise);
//    //            remainingExercises.Remove(currentExercise);
//    //            currentTime = currentTime.AddMinutes(user.ExerciseDuration);

//    //            foreach (var nextExercise in exerciseGraph.Edges[currentExercise])
//    //            {
//    //                if (remainingExercises.Contains(nextExercise))
//    //                {
//    //                    queue.Enqueue((nextExercise, currentTime, new List<string>(path)));
//    //                }
//    //            }
//    //        }

//    //        return false;
//    //    }

//    //    private string FindAlternativeExercise(string currentExercise, HashSet<string> remainingExercises)
//    //    {
//    //        if (!exerciseDataMap.ContainsKey(currentExercise))
//    //            return null;

//    //        foreach (var alternative in exerciseDataMap[currentExercise].AlternativeExercises)
//    //        {
//    //            if (remainingExercises.Contains(alternative))
//    //            {
//    //                return alternative; // מחזיר תרגיל חלופי זמין
//    //            }
//    //        }

//    //        return null; // אין תרגיל חלופי זמין
//    //    }

//    //    private bool TryReorderUserExercises(Trainee user, string currentExercise, DateTime currentTime, HashSet<string> remainingExercises)
//    //    {
//    //        foreach (var exercise in user.DefaultProgram)
//    //        {
//    //            if (exercise != currentExercise && remainingExercises.Contains(exercise))
//    //            {
//    //                var machine = exerciseGraph.Nodes[exercise];
//    //                if (machine.IsAvailableAt(currentTime, currentTime.AddMinutes(user.ExerciseDuration)))
//    //                {
//    //                    return true; // ניתן לשנות את סדר התרגילים
//    //                }
//    //            }
//    //        }

//    //        return false;
//    //    }

//    //    private bool TryReorderOtherTrainees(GraphNode machine, DateTime currentTime)
//    //    {
//    //        foreach (var timeSlot in machine.TimeQueue)
//    //        {
//    //            var otherUser = GetUserById(timeSlot.UserId);
//    //            if (TryReorderUserExercises(otherUser, null, currentTime, new HashSet<string>(otherUser.DefaultProgram)))
//    //            {
//    //                return true; // ניתן לשנות את סדר התרגילים של מתאמנים אחרים
//    //            }
//    //        }

//    //        return false;
//    //    }
//    //}

//    public class OptimizedGymManagementSystem
//    {
//        private Graph<string, GraphNode> exerciseGraph;
//        private Dictionary<string, ExerciseData> exerciseDataMap;

//        public OptimizedGymManagementSystem(Graph<string, GraphNode> graph, Dictionary<string, ExerciseData> exerciseData)
//        {
//            this.exerciseGraph = graph;
//            this.exerciseDataMap = exerciseData;
//        }

//        public List<string> ManageWorkout(Trainee user, DateTime startTime)
//        {
//            var completedExercises = new List<string>();
//            var remainingExercises = new HashSet<string>(user.DefaultProgram);

//            DateTime currentTime = startTime;

//            // Optimize workout schedule
//            if (!OptimizeWorkout(user, currentTime, completedExercises, remainingExercises))
//            {
//                throw new Exception("No valid workout path found.");
//            }

//            return completedExercises;
//        }

//        private bool OptimizeWorkout(Trainee user, DateTime currentTime, List<string> completedExercises, HashSet<string> remainingExercises)
//        {
//            var priorityQueue = new SortedSet<(double Fitness, string Exercise, DateTime Time, List<string> Path)>(Comparer<(double, string, DateTime, List<string>)>.Create((x, y) => x.Fitness.CompareTo(y.Fitness)));
//            priorityQueue.Add((CalculateFitness(null, null, null), user.DefaultProgram.First(), currentTime, new List<string>()));

//            while (priorityQueue.Count > 0)
//            {
//                var (fitness, currentExercise, currentTime, path) = priorityQueue.Min;
//                priorityQueue.Remove(priorityQueue.Min);

//                if (remainingExercises.Count == 0)
//                {
//                    completedExercises.AddRange(path);
//                    return true;
//                }

//                // Check availability of the current exercise
//                var machine = exerciseGraph.Nodes[currentExercise];
//                if (machine.IsAvailableAt(currentTime, currentTime.AddMinutes(user.ExerciseDuration)))
//                {
//                    machine.TimeQueue.Add(new TimeSlot
//                    {
//                        UserId = user.Id,
//                        StartTime = currentTime,
//                        EndTime = currentTime.AddMinutes(user.ExerciseDuration)
//                    });

//                    path.Add(currentExercise);
//                    remainingExercises.Remove(currentExercise);
//                    currentTime = currentTime.AddMinutes(user.ExerciseDuration);
//                }
//                else
//                {
//                    // Search for an alternative exercise
//                    var alternativeExercise = FindAlternativeExercise(currentExercise, remainingExercises);
//                    if (alternativeExercise != null)
//                    {
//                        priorityQueue.Add((CalculateFitness(alternativeExercise, currentTime, path), alternativeExercise, currentTime, new List<string>(path)));
//                        continue;
//                    }

//                    // Try to reorder other trainees
//                    if (TryReorderOtherTrainees(machine, currentTime, user))
//                    {
//                        priorityQueue.Add((CalculateFitness(currentExercise, currentTime, path), currentExercise, currentTime, new List<string>(path)));
//                        continue;
//                    }

//                    // Add the trainee to the queue if no solution is found
//                    machine.TimeQueue.Add(new TimeSlot
//                    {
//                        UserId = user.Id,
//                        StartTime = currentTime,
//                        EndTime = DateTime.MaxValue
//                    });
//                    return false;
//                }

//                // Add next exercises to the priority queue
//                foreach (var nextExercise in exerciseGraph.Edges[currentExercise])
//                {
//                    if (remainingExercises.Contains(nextExercise))
//                    {
//                        priorityQueue.Add((CalculateFitness(nextExercise, currentTime, path), nextExercise, currentTime, new List<string>(path)));
//                    }
//                }
//            }

//            return false;
//        }

//        private double CalculateFitness(string exercise, DateTime? currentTime, List<string> path)
//        {
//            // Fitness combines three goals:
//            // 1. Minimize total wait time
//            // 2. Maximize device utilization
//            // 3. Optimize program flow
//            double waitTimeScore = currentTime.HasValue ? 1.0 / (1 + (DateTime.Now - currentTime.Value).TotalMinutes) : 0.0;
//            double utilizationScore = exercise != null && exerciseGraph.Nodes.ContainsKey(exercise)
//                ? exerciseGraph.Nodes[exercise].UtilizationRate
//                : 0.0;
//            double programOptimalityScore = path != null ? path.Count / (double)(path.Count + 1) : 0.0;

//            // Weighted combination
//            return 0.5 * waitTimeScore + 0.3 * utilizationScore + 0.2 * programOptimalityScore;
//        }

//        private string FindAlternativeExercise(string currentExercise, HashSet<string> remainingExercises)
//        {
//            if (!exerciseDataMap.ContainsKey(currentExercise))
//                return null;

//            foreach (var alternative in exerciseDataMap[currentExercise].AlternativeExercises)
//            {
//                if (remainingExercises.Contains(alternative))
//                {
//                    return alternative; // Return available alternative exercise
//                }
//            }

//            return null; // No alternative exercise available
//        }

//        private bool TryReorderOtherTrainees(GraphNode machine, DateTime currentTime, Trainee currentUser)
//        {
//            foreach (var timeSlot in machine.TimeQueue)
//            {
//                var otherUser = GetUserById(timeSlot.UserId);
//                if (TryReorderUserExercises(otherUser, machine, currentTime))
//                {
//                    return true;
//                }
//            }
//            return false;
//        }

//        private bool TryReorderUserExercises(Trainee otherUser, GraphNode machine, DateTime currentTime)
//        {
//            foreach (var exercise in otherUser.DefaultProgram)
//            {
//                if (exerciseGraph.Nodes[exercise].IsAvailableAt(currentTime, currentTime.AddMinutes(otherUser.ExerciseDuration)))
//                {
//                    return true; // Reordering is possible
//                }
//            }

//            return false;
//        }
//    }
//}


