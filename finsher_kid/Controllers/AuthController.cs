using Kid_Guard_part1.Db_context;
using Kid_Guard_part1.DTO;
using Kid_Guard_part1.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Kid_Guard_part1.Controllers
{ 

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _context.Parents.AnyAsync(p => p.Email == dto.Email))
            return BadRequest("الإيميل مسجل مسبقاً.");

        string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, salt);

        var parent = new Parent
        {
            FullName = dto.FullName,
            Age = dto.Age,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        _context.Parents.Add(parent);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "تم إنشاء الحساب بنجاح." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var parent = await _context.Parents
            .FirstOrDefaultAsync(p => p.Email == dto.EmailOrPhone || p.PhoneNumber == dto.EmailOrPhone);

        if (parent == null || !BCrypt.Net.BCrypt.Verify(dto.Password, parent.PasswordHash))
            return Unauthorized("بيانات الدخول غير صحيحة.");

        var token = CreateToken(parent);

        return Ok(new { Token = token, Message = "تم تسجيل الدخول بنجاح" });
    }

    private string CreateToken(Parent parent)
    {
        var claims = new List<Claim>
        {
           
            new Claim(ClaimTypes.NameIdentifier, parent.Id.ToString()),
            new Claim(ClaimTypes.Email, parent.Email ?? ""),
            new Claim(ClaimTypes.Name, parent.FullName)
        };

     
        var keyString = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(keyString))
            throw new Exception("JWT Token key is missing in appsettings.json");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

     
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = creds,
            Issuer = issuer,   
            Audience = audience 
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
}