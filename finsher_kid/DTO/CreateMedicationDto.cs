namespace Kid_Guard_part1.DTO
{
    public class CreateMedicationDto
    {
        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public int Frequency { get; set; }
        public string Description { get; set; }
        public int ChildId { get; set; }
        public bool IsActive { get; set; } // إضافة هذا السطر ضروري

        public bool IsMonday { get; set; }
        public bool IsTuesday { get; set; }
        public bool IsWednesday { get; set; }
        public bool IsThursday { get; set; }
        public bool IsFriday { get; set; }
        public bool IsSaturday { get; set; }
        public bool IsSunday { get; set; }

        public List<TimeSpan> Times { get; set; }
    }
}