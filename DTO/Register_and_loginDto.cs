namespace Kid_Guard_part1.DTO
{
    public class RegisterDto
    {
        public string FullName { get; set; } = null!;
        public int Age { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Password { get; set; } = null!;
    }
    public class LoginDto
    {
        public string EmailOrPhone { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
