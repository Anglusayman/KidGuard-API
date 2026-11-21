namespace Kid_Guard_part1.Model
{
    public class Parent
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // العلاقة مع الأبناء

        public ICollection<Child> Children { get; set; } = new List<Child>();
    }
}
