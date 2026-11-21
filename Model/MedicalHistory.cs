using Kid_Guard_part1.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kid_Guard_part1.Model
{
    public class MedicalHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ChildId { get; set; }

        [Required]
        [StringLength(200)]
        public string MedicationName { get; set; }

        [StringLength(100)]
        public string Dosage { get; set; }

        public int Frequency { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public int ActualUsageDays { get; set; }

        [StringLength(200)]
        public string DaysPattern { get; set; } // سيخزن مثل: "Sat, Sun, Mon"

        [StringLength(500)]
        public string TimesPattern { get; set; } // سيخزن مثل: "08:00, 14:00"

        [StringLength(100)]
        public string ActionTaken { get; set; } // (Update, Delete, Deactivate)

        // العلاقة مع جدول الطفل
        [ForeignKey("ChildId")]
        public virtual Child Child { get; set; }
    }
}