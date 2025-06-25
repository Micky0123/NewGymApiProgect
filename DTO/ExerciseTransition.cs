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

    }
}

