namespace Kid_Guard_part1.Model
{
    public class Medication
    {
        public int Id { get; set; }
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public int Frequency { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public int ChildId { get; set; }
        public bool IsActive { get; set; } // تأكد من وجود هذا السطر

        public ICollection<MedicationSchedule> Schedules { get; set; }
        public Child Child { get; set; } // ضروري جداً لحل خطأ الـ Navigation
    }

    // ملف MedicationSchedule.cs
    public class MedicationSchedule
    {
        public int Id { get; set; }
        public int MedicationId { get; set; }
        public TimeSpan ScheduledTime { get; set; }
        public bool IsActive { get; set; } // أعد هذا السطر ليطابق قاعدة بياناتك
        public bool IsMonday { get; set; }
        public bool IsTuesday { get; set; }
        public bool IsWednesday { get; set; }
        public bool IsThursday { get; set; }
        public bool IsFriday { get; set; }
        public bool IsSaturday { get; set; }
        public bool IsSunday { get; set; }

        public Medication Medication { get; set; }
    }
}