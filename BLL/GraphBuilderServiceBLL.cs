using DTO;
using IBLL;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core.DAG;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{

    public class GraphBuilderServiceBLL : IGraphBuilderServiceBLL
    {
        public Graph BuildGymGraph(
            List<GraphEdgeDTO> exerciseEdges, // טבלה המכילה קשתות בין תרגילים
            List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges, // טבלה המכילה קשתות בין תרגילים לשרירים
            List<MuscleEdgeDTO> muscleEdges, // טבלה המכילה קשתות בין שרירים
            List<ExerciseDTO> exercises, // רשימת התרגילים
            List<MuscleDTO> muscles // רשימת השרירים
            )
        {
            var graph = new Graph();
            var nodeIdCounter = 1;

            // מילון לצמתים של השרירים
            var muscleNodes = new Dictionary<int, Node>();
            // מילון לצמתים של התרגילים
            var exerciseNodes = new Dictionary<int, Node>();

            // צור צמתים לשרירים
            foreach (var muscle in muscles)
            {
                var muscleNode = new Node
                {
                    Id = nodeIdCounter++,
                    Name = muscle.MuscleName,
                    Type = NodeType.MuscleGroup
                };
                muscleNodes[muscle.MuscleId] = muscleNode;
                graph.Nodes.Add(muscleNode);
            }

            // צור צמתים לתרגילים
            foreach (var exercise in exercises)
            {
                var exerciseNode = new Node
                {
                    Id = nodeIdCounter++,
                    Name = exercise.ExerciseName,
                    Type = NodeType.Exsercise
                };
                exerciseNodes[exercise.ExerciseId] = exerciseNode;
                graph.Nodes.Add(exerciseNode);
            }

            // צור קשתות בין תרגילים לשרירים
            //וקשתות בין השרירים לתרגילים
            foreach (var edge in exerciseToMuscleEdges)
            {
                if (exerciseNodes.ContainsKey(edge.DeviceId) && muscleNodes.ContainsKey(edge.MuscleId))
                {
                    graph.Edges.Add(new Edge
                    {
                        FromNodeId = exerciseNodes[edge.DeviceId].Id,
                        ToNodeId = muscleNodes[edge.MuscleId].Id,
                        //Weight = edge.Weight
                    });
                    graph.Edges.Add(new Edge
                    {
                        FromNodeId = muscleNodes[edge.MuscleId].Id,
                        ToNodeId = exerciseNodes[edge.DeviceId].Id,
                        //Weight = edge.Weight
                    });
                }
            }

            // צור קשתות בין תרגילים
            foreach (var edge in exerciseEdges)
            {
                if (exerciseNodes.ContainsKey(edge.Device1Id) && exerciseNodes.ContainsKey(edge.Device2Id))
                {
                    graph.Edges.Add(new Edge
                    {
                        FromNodeId = exerciseNodes[edge.Device1Id].Id,
                        ToNodeId = exerciseNodes[edge.Device2Id].Id,
                       // Weight = edge.Weight
                    });
                }
            }

            // צור קשתות בין שרירים
            foreach (var edge in muscleEdges)
            {
                if (muscleNodes.ContainsKey(edge.MuscleId1) && muscleNodes.ContainsKey(edge.MuscleId2))
                {
                    graph.Edges.Add(new Edge
                    {
                        FromNodeId = muscleNodes[edge.MuscleId1].Id,
                        ToNodeId = muscleNodes[edge.MuscleId2].Id,
                        //Weight = edge.Weight
                    });
                }
            }

            return graph;
        }

        public SubGraph BuildUserSubGraph(Graph graph, List<int> userExerciseIds)
        {
            if (graph == null || userExerciseIds == null || !userExerciseIds.Any())
            {
                throw new ArgumentException("Graph or userExerciseIds is invalid.");
            }

            // צור תת-גרף רלוונטי לפי התוכנית של המשתמש
            var subGraph = new SubGraph();

            // שלב 1: צור את הצמתים מתוך הגרף לפי המכשירים של המשתמש
            // וגם צמתים שהם מסוג NodeType.MuscleGroup
            var userNodes = graph.Nodes
                .Where(n => userExerciseIds.Contains(n.Id) || n.Type == NodeType.MuscleGroup)
                .ToDictionary(n => n.Id, n => new Node
                {
                    Id = n.Id,
                    Name = n.Name,
                    Type = n.Type,
                    Neighbors = new List<Node>() // ודא שזו רשימה ריקה
                });


            // שלב 2: צור קשרים בין הצמתים אם קיימות קשתות תואמות בגרף הראשי
            foreach (var edge in graph.Edges)
            {
                if (userNodes.ContainsKey(edge.FromNodeId) && userNodes.ContainsKey(edge.ToNodeId))
                {
                    var fromNode = userNodes[edge.FromNodeId];
                    var toNode = userNodes[edge.ToNodeId];

                    //יוצר רשימת שכנים- רק הצמתים שמחוברים אליו עם קשת מכוונת
                    fromNode.Neighbors.Add(toNode);
                    subGraph.Edges.Add(new Edge
                    {
                        FromNodeId = fromNode.Id,
                        ToNodeId = toNode.Id,
                       // Weight = edge.Weight
                    });
                }
            }

            subGraph.Nodes = userNodes.Values.ToList();

            return subGraph;
        }

    }
}



//public Graph BuildGymGraph(List<ExerciseDTO> machines)
//{
//    // צור גרף מכשירים עם צמתים וקשתות לפי הכללים
//    var graph = new Graph();
//    var nodeIdCounter = 1;

//    var muscleGroupNodes = new Dictionary<string, Node>();
//    var machineNodes = new List<Node>();

//    // צור צמתים לקבוצות שרירים
//    foreach (var machine in machines)
//    {
//        if (!muscleGroupNodes.ContainsKey(machine MuscleGroup))
//        {
//            var muscleNode = new Node
//            {
//                Id = nodeIdCounter++,
//                Name = machine.MuscleGroup,
//                Type = NodeType.MuscleGroup
//            };
//            muscleGroupNodes[machine.MuscleGroup] = muscleNode;
//            graph.Nodes.Add(muscleNode);
//        }
//    }

//    // צור צמתים למכשירים וקשת מהמכשיר לקבוצת השריר שלו
//    foreach (var machine in machines)
//    {
//        var machineNode = new Node
//        {
//            Id = nodeIdCounter++,
//            Name = machine.Name,
//            Type = NodeType.Machine
//        };
//        machineNodes.Add(machineNode);
//        graph.Nodes.Add(machineNode);

//        var muscleNode = muscleGroupNodes[machine.MuscleGroup];
//        graph.Edges.Add(new Edge
//        {
//            FromNodeId = machineNode.Id,
//            ToNodeId = muscleNode.Id,
//            Weight = 1
//        });
//    }

//    // צור קשתות בין קבוצות שרירים - לפי הגיון איזון (לדוגמה: רגליים ↔ גב, חזה ↔ כתפיים)
//    var connections = new List<(string From, string To)>
//{
//    ("רגליים", "גב"),
//    ("חזה", "כתפיים"),
//    ("גב", "יד קדמית"),
//    ("חזה", "יד אחורית")
//};

//    foreach (var (from, to) in connections)
//    {
//        if (muscleGroupNodes.ContainsKey(from) && muscleGroupNodes.ContainsKey(to))
//        {
//            var fromNode = muscleGroupNodes[from];
//            var toNode = muscleGroupNodes[to];

//            graph.Edges.Add(new Edge
//            {
//                FromNodeId = fromNode.Id,
//                ToNodeId = toNode.Id,
//                Weight = 1
//            });

//            graph.Edges.Add(new Edge
//            {
//                FromNodeId = toNode.Id,
//                ToNodeId = fromNode.Id,
//                Weight = 1
//            });
//        }
//    }

//    return graph;
//}

//    var exerciseDtos = dbContext.Exercises
//.Where(e => e.Active == true)
//.Select(e => new ExerciseDTO
//{
//    Name = e.ExerciseName,
//    MuscleGroups = e.MuscleGroups.Select(mg => mg.Name).ToList()
//})
//.ToList();

//    public Graph BuildGymGraph(List<ExerciseDTO> machines, ExerciseWithMuscleInfo exerciseInfo)
//    {
//        var graph = new Graph();
//        var nodeIdCounter = 1;

//        var muscleGroupNodes = new Dictionary<string, Node>();
//        var machineNodes = new List<Node>();

//        // צור צמתים לקבוצות שרירים
//        foreach (var machine in machines)
//        {
//            foreach (var muscleGroup in machine.MuscleGroups)
//            {
//                if (!muscleGroupNodes.ContainsKey(muscleGroup))
//                {
//                    var muscleNode = new Node
//                    {
//                        Id = nodeIdCounter++,
//                        Name = muscleGroup,
//                        Type = NodeType.MuscleGroup
//                    };
//                    muscleGroupNodes[muscleGroup] = muscleNode;
//                    graph.Nodes.Add(muscleNode);
//                }
//            }
//        }

//        // צור צמתים למכשירים וקשת מהמכשיר לקבוצות השרירים שלו
//        foreach (var machine in machines)
//        {
//            var machineNode = new Node
//            {
//                Id = nodeIdCounter++,
//                Name = machine.Name,
//                Type = NodeType.Machine
//            };
//            machineNodes.Add(machineNode);
//            graph.Nodes.Add(machineNode);

//            foreach (var muscleGroup in machine.MuscleGroups)
//            {
//                var muscleNode = muscleGroupNodes[muscleGroup];
//                graph.Edges.Add(new Edge
//                {
//                    FromNodeId = machineNode.Id,
//                    ToNodeId = muscleNode.Id,
//                    Weight = 1
//                });
//            }
//        }

//        // חיבורים בין קבוצות שרירים לפי איזון
//        var connections = new List<(string From, string To)>
//{
//    ("רגליים", "גב"),
//    ("חזה", "כתפיים"),
//    ("גב", "יד קדמית"),
//    ("חזה", "יד אחורית")
//};

//        foreach (var (from, to) in connections)
//        {
//            if (muscleGroupNodes.ContainsKey(from) && muscleGroupNodes.ContainsKey(to))
//            {
//                var fromNode = muscleGroupNodes[from];
//                var toNode = muscleGroupNodes[to];

//                graph.Edges.Add(new Edge
//                {
//                    FromNodeId = fromNode.Id,
//                    ToNodeId = toNode.Id,
//                    Weight = 1
//                });

//                graph.Edges.Add(new Edge
//                {
//                    FromNodeId = toNode.Id,
//                    ToNodeId = fromNode.Id,
//                    Weight = 1
//                });
//            }
//        }

//        return graph;
//    }

//    public Graph BuildGymGraph(List<ExerciseDTO> exercises, ExerciseWithMuscleInfo exerciseInfo, List<MuscleDTO> muscles)
//    {
//        var graph = new Graph();
//        var nodeIdCounter = 1;

//        var muscleGroupNodes = new Dictionary<string, Node>();
//        var exercisesNodes = new List<Node>();

//        // צור צמתים לקבוצות שרירים
//        foreach (var muscleGroup in muscles)
//        {
//            if (!muscleGroupNodes.ContainsKey(muscleGroup.MuscleName))
//            {
//                //יצרנו צומת לכל שריר
//                var muscleNode = new Node
//                {
//                    Id = nodeIdCounter++,
//                    Name = muscleGroup.MuscleName,
//                    Type = NodeType.MuscleGroup
//                };
//                muscleGroupNodes[muscleGroup.MuscleName] = muscleNode;
//                graph.Nodes.Add(muscleNode);
//            }
//        }

//        // צור צמתים למכשירים וקשת מהמכשיר לקבוצות השרירים שלו
//        foreach (var exercise in exercises)
//        {
//            //יצרנו צומת לכל מכשיר
//            var exerciseNode = new Node
//            {
//                Id = nodeIdCounter++,
//                Name = exercise.ExerciseName,
//                Type = NodeType.Exsercise
//            };
//            exercisesNodes.Add(exerciseNode);
//            graph.Nodes.Add(exerciseNode);

//            foreach (var muscleGroup in muscles)
//            {
//                var muscleNode = muscleGroupNodes[muscleGroup.MuscleName];
//                graph.Edges.Add(new Edge
//                {
//                    FromNodeId = exerciseNode.Id,
//                    ToNodeId = muscleNode.Id,
//                  //  Weight = 1
//                });
//            }
//        }

//        // חיבורים בין קבוצות שרירים לפי איזון
//        var connections = new List<(string From, string To)>
//{
//    ("רגליים", "גב"),
//    ("חזה", "כתפיים"),
//    ("גב", "יד קדמית"),
//    ("חזה", "יד אחורית")
//};

//        foreach (var (from, to) in connections)
//        {
//            if (muscleGroupNodes.ContainsKey(from) && muscleGroupNodes.ContainsKey(to))
//            {
//                var fromNode = muscleGroupNodes[from];
//                var toNode = muscleGroupNodes[to];

//                graph.Edges.Add(new Edge
//                {
//                    FromNodeId = fromNode.Id,
//                    ToNodeId = toNode.Id,
//                    Weight = 1
//                });

//                graph.Edges.Add(new Edge
//                {
//                    FromNodeId = toNode.Id,
//                    ToNodeId = fromNode.Id,
//                    Weight = 1
//                });
//            }
//        }

//        return graph;
//    }

//public SubGraph BuildUserSubGraph(Graph graph, List<int> userMachineIds)
//{
//    // צור תת-גרף רלוונטי לפי התוכנית של המשתמש
//    var subGraph = new SubGraph();

//    // שלב 1: צור את הצמתים בתת-הגרף רק מתוך ה־IDs של המתאמן
//    var userNodes = fullGraph.Nodes
//        .Where(n => userMachineIds.Contains(n.Id))
//        .ToDictionary(n => n.Id, n => new Node
//        {
//            Id = n.Id,
//            Name = n.Name
//        });

//    // שלב 2: צור קשתות בתת-הגרף, רק אם הן קיימות בגרף הראשי
//    foreach (var originalNode in fullGraph.Nodes)
//    {
//        if (!userNodes.ContainsKey(originalNode.Id))
//            continue;

//        var subNode = userNodes[originalNode.Id];

//        foreach (var neighbor in originalNode.Neighbors)
//        {
//            if (userNodes.ContainsKey(neighbor.Id))
//            {
//                subNode.Neighbors.Add(userNodes[neighbor.Id]);
//            }
//        }
//    }

//    // שלב 3: הוסף את הצמתים לתוך תת-הגרף
//    subGraph.Nodes = userNodes.Values.ToList();

//    return subGraph;
//}
