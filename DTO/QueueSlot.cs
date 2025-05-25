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


        //  public List<Slot> Slots { get; set; } = new List<Slot>();

        // קונסטרקטור
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
        // פעולה שמייצרת סלוטים לפי פרמטר של אורך במינוטות וכמות
        //public void GenerateSlots(DateTime firstSlotStart, int slotMinutes, int slotCount)
        //{
        //    Slots.Clear();
        //    for (int i = 0; i < slotCount; i++)
        //    {
        //        var start = firstSlotStart.AddMinutes(i * slotMinutes);
        //        var end = start.AddMinutes(slotMinutes);
        //        Slots.Add(new Slot(start, end));
        //    }
        //}


        // פונקציית עזר לבדיקה האם סלוט זמן מסוים פנוי
        public bool IsSlotAvailable(DateTime desiredStartTime)
        {
           if(SlotsByStartTime.TryGetValue(desiredStartTime, out Slot slot))
            {
                return true;
            }
           return false;
        }

        // פונקציית עזר להוספת מתאמן לסלוט זמן מסוים
        public void AddTraineeToSlot(DateTime desiredStartTime,int sumOfSlots,TraineeDTO trainee)
        {
            var startTime = desiredStartTime;
           for (int i = 0; i < sumOfSlots; i++)
           {
                SlotsByStartTime[startTime].AddTranee(trainee);
                startTime = SlotsByStartTime[startTime].EndTime;
           }
        }

        // פונקציית עזר להסרת מתאמן מסלוט זמן מסוים
        //public Trainee RemoveTraineeFromSlot(int timeSlot)
        //{
        //    if (timeSlot >= 0 && timeSlot < TimeSlots.Length && TimeSlots[timeSlot].Count > 0)
        //    {
        //        return TimeSlots[timeSlot].Dequeue();
        //    }
        //    return null;
        //}

        //// פונקציית עזר לבדיקה מי המתאמן הנוכחי בסלוט
        //public Trainee GetCurrentTraineeInSlot(int timeSlot)
        //{
        //    if (timeSlot >= 0 && timeSlot < TimeSlots.Length && TimeSlots[timeSlot].Count > 0)
        //    {
        //        return TimeSlots[timeSlot].Peek();
        //    }
        //    return null;
        //}
    }
}




//public class QueueSlot<T>
//{
//    // מערך של סלוטים, כל סלוט מצביע לרשימה
//    public List<T>[] Slots { get; private set; }

//    // קונסטרקטור שמקבל את מספר הסלוטים ומאתחל כל סלוט לרשימה ריקה
//    public QueueSlot(int slotCount)
//    {
//        if (slotCount <= 0)
//            throw new ArgumentException("slotCount must be positive");

//        Slots = new List<T>[slotCount];
//        for (int i = 0; i < slotCount; i++)
//        {
//            Slots[i] = new List<T>();
//        }
//    }
//}
