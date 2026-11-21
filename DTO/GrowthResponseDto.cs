namespace Kid_Guard_part1.DTO
{
    public class GrowthResponseDto
    {
        public int Id { get; set; }
        public string ChildName { get; set; }
        public int AgeInMonths { get; set; }
        public double Weight { get; set; }
        public double Height { get; set; }
        public string WeightStatus { get; set; }
        public string HeightStatus { get; set; }
        public string CreatedAt { get; set; }
    }
}
