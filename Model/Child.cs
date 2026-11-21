namespace Kid_Guard_part1.Model
{
    public class Child
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int ParentId { get; set; }
        public Parent Parent { get; set; }
        // الربط مع الجداول التانية لضمان الحذف التلقائي
        public ICollection<Medication> Medications { get; set; }
        public ICollection<ChildVaccine> ChildVaccines { get; set; }
        // داخل كلاس Child.cs
        public virtual ICollection<GrowthMeasurement> GrowthMeasurements { get; set; } = new List<GrowthMeasurement>();
    }
}
