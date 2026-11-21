using System.Reflection.PortableExecutable;

namespace Kid_Guard_part1.Model
{
    public class ChildVaccine
    {
        public int Id { get; set; }

        public int ChildId { get; set; }
        public Child Child { get; set; } // Navigation Property

        public int VaccineId { get; set; }
        public Vaccine Vaccine { get; set; } // Navigation Property

        public bool IsUsed { get; set; } = false;
        public bool IsOverdue { get; set; } = false;
    }
}
