using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    // מעבר בין תרגילים: מכיל את הערך החוקי של המעבר ותור הזמנים
    public class ExerciseTransition
    {
        public int LegalityValue { get; set; } // לדוג' 2^שריר, -1 (לא חוקי), 0 (אותו תרגיל)
        public QueueSlot QueueSlots { get; set; } // מצביע לתור של סלוטים (QueueSlot)

        // בנאי
        public ExerciseTransition(QueueSlot queueSlot) 
        {
            QueueSlots = queueSlot;
        }

        //פונקצית בדיקת זמינות במקום מסוים

    }
}




//// זה כשמהתחלים את התורים 
//public ExerciseTransition(int EquipmentCount, DateTime firstSlotStart, int slotMinutes, int slotCount)
//{
//    QueueSlots = new QueueSlot(EquipmentCount); //יצירת תור חדש 
//    QueueSlots.GenerateSlots(firstSlotStart,slotCount,slotMinutes); //יציר סלוטים לתור
//}







//// בנאי
//public ExerciseTransition(int numSlots, int slotCapacity)
//{
//    QueueSlots = new QueueSlot[numSlots];
//    for (int i = 0; i < numSlots; i++)
//    {
//        QueueSlots[i] = new QueueSlot(slotCapacity);
//    }
//}
//// מעבר בין תרגילים: מכיל את הערך החוקי של המעבר ותור הזמנים
//public class ExerciseTransition
//{
//    public int LegalityValue { get; set; } // לדוג' 2^שריר, -1 (לא חוקי), 0 (אותו תרגיל)
//    public Queue<Trainee>[] TimeSlots { get; set; } // מערך של תורים - אחד לכל סלוט זמן

//    // בנאי
//    public ExerciseTransition(int totalTimeSlots)
//    {
//        TimeSlots = new Queue<Trainee>[totalTimeSlots];
//        for (int i = 0; i < totalTimeSlots; i++)
//        {
//            TimeSlots[i] = new Queue<Trainee>();
//        }
//    }

//    // פונקציית עזר לבדיקה האם סלוט זמן מסוים פנוי
//    public bool IsSlotAvailable(int timeSlot, int maxInQueue = 1)
//    {
//        if (timeSlot < 0 || timeSlot >= TimeSlots.Length)
//            return false;

//        return TimeSlots[timeSlot].Count < maxInQueue;
//    }

//    // פונקציית עזר להוספת מתאמן לסלוט זמן מסוים
//    public void AddTraineeToSlot(int timeSlot, Trainee trainee)
//    {
//        if (timeSlot >= 0 && timeSlot < TimeSlots.Length)
//        {
//            TimeSlots[timeSlot].Enqueue(trainee);
//        }
//    }

//    // פונקציית עזר להסרת מתאמן מסלוט זמן מסוים
//    public Trainee RemoveTraineeFromSlot(int timeSlot)
//    {
//        if (timeSlot >= 0 && timeSlot < TimeSlots.Length && TimeSlots[timeSlot].Count > 0)
//        {
//            return TimeSlots[timeSlot].Dequeue();
//        }
//        return null;
//    }

//    // פונקציית עזר לבדיקה מי המתאמן הנוכחי בסלוט
//    public Trainee GetCurrentTraineeInSlot(int timeSlot)
//    {
//        if (timeSlot >= 0 && timeSlot < TimeSlots.Length && TimeSlots[timeSlot].Count > 0)
//        {
//            return TimeSlots[timeSlot].Peek();
//        }
//        return null;
//    }
//}
