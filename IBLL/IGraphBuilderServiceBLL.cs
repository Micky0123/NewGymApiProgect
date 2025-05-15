using DBEntities.Models;
using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    //הגרף
    public class Graph
    {
        public List<Node> Nodes { get; set; } = new();
        public List<Edge> Edges { get; set; } = new();
    }

    //כל צומת
    public class Node
    {
        //כל צומת
        public int Id { get; set; }

        public string Name { get; set; }//לא יודעת

        //סוג הצומת האם היא תרגיל או שריר
        public NodeType Type { get; set; }

        //רשימת הצמתים השכנים המקושרים לצומת זו 
        public List<Node> Neighbors { get; set; } = new List<Node>();


        // רק עבור מכשירים:
        public List<DateTime> UsageQueue { get; set; } = new List<DateTime>(); // תור של התחלת שימושים
        public int UsageDurationMinutes { get; set; } = 10; // זמן שימוש ברירת מחדל לדוגמה

        // תור מתאמנים לפי זמן התחלה
        public List<(DateTime StartTime, TimeSpan Duration)> Schedule { get; set; } = new List<(DateTime, TimeSpan)>();



        public bool IsAvailableAt(DateTime time)
        {
            return !UsageQueue.Any(t => time >= t && time < t.AddMinutes(UsageDurationMinutes));
        }

        public void AddToQueue(DateTime time)
        {
            UsageQueue.Add(time);
        }
    }


    public class SubGraph : Graph
    {
        // אפשר להוסיף כאן לוגיקה נוספת אם תת-גרף שונה מהותית
    }

    public enum NodeType
    {
        MuscleGroup,
        Exsercise
    }

    public class Edge
    {
        public int FromNodeId { get; set; }
        public int ToNodeId { get; set; }
        //public double Weight { get; set; } = 1.0;
    }



    public interface IGraphBuilderServiceBLL
    {
        Graph BuildGymGraph( List<GraphEdgeDTO> exerciseEdges, List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges, List<MuscleEdgeDTO> muscleEdges, List<ExerciseDTO> exercises, List<MuscleDTO> muscles );
        SubGraph BuildUserSubGraph(Graph graph, List<int> userExerciseIds);
    }
}
