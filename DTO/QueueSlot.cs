using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    //מחלקה ליצוג תור
    public class QueueSlot
    {
        public int EquipmentCount { get; set; }// כמות המכשירים במכון עבור התור הזה
        public Dictionary<DateTime, Slot> SlotsByStartTime { get; set; } = new();// רשימה של סלוטים (בכל סלוט יש תרגילים/מתאמנים וכו')

        public QueueSlot(int equipmentCount, DateTime firstSlotStart, int slotMinutes, int slotCount)
        {
            EquipmentCount = equipmentCount;
            GenerateSlots(firstSlotStart, slotMinutes, slotCount);
        }

        public void GenerateSlots(DateTime firstSlotStart, int slotMinutes, int slotCount)
        {
            SlotsByStartTime.Clear();
            for (int i = 0; i < slotCount; i++)
            {
                var start = firstSlotStart.AddMinutes(i * slotMinutes);
                var end = start.AddMinutes(slotMinutes);

                SlotsByStartTime[start] = new Slot(start, end);
            }
        }

        // פונקציית עזר להוספת מתאמן לסלוט זמן מסוים
        public void AddTraineeToSlot(DateTime desiredStartTime,int sumOfSlots,TraineeDTO trainee)
        {
            var startTime = desiredStartTime;
           for (int i = 0; i < sumOfSlots; i++)
           {
                SlotsByStartTime[startTime].AddTranee(trainee,null);
                startTime = SlotsByStartTime[startTime].EndTime;
           }
        }
    }
}
