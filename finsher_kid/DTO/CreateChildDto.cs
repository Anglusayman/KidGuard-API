namespace Kid_Guard_part1.DTO
{
    public class CreateChildDto
    {
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
    }

    public class ChildDisplayDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
