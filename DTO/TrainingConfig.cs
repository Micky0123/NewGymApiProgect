using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace DTO
{
    public class TrainingConfig
    {
        public string DaysInWeekSheet { get; set; }
        public string MuscleSheet { get; set; }
        public string SmallMuscleSheet { get; set; }
        public string RepitByGoalSheet { get; set; }
        public string CategorySheet { get; set; }
        public string SumOfLargeByTimeSheet { get; set; }
        public string SumOfSmallByTimeSheet { get; set; }
        public string TypMuscleSheet { get; set; }
        public string EquipmentSheet { get; set; }
        public string NeedSubMuscleSheet { get; set; }
        public string OrderListSheet { get; set; }
        public string ExcelFilePath { get; set; }

        public static TrainingConfig Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<TrainingConfig>(json);
        }
    }
}
