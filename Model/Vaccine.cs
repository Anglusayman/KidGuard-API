namespace Kid_Guard_part1.Model
{
    public class Vaccine
    {
        public int Id { get; set; }
        public string VaccineNameAr { get; set; } = string.Empty;
        public string VaccineNameEn { get; set; } = string.Empty;
        public int AgeInMonths { get; set; }
        public string AgeDescription { get; set; } = string.Empty;
    }
}
