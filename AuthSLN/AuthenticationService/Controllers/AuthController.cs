using AuthenticationService.Data;
using AuthenticationService.Models;
using AuthenticationService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthenticationService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AuthDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Invalid sign up request.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return Conflict("User with this email already exists.");
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = HashPassword(request.Password) // Note: Use a stronger hashing algorithm in production
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Sign up successful");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Invalid login request.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || user.Password != HashPassword(request.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = GenerateJwtToken(user);

            return Ok(new { access_token = token });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Placeholder for logout logic
            return Ok("Logout successful");
        }

        [HttpGet("getappname")]
        public IActionResult GetAppName()
        {
            return Ok("nLogistic");
        }

        private string HashPassword(string password)
        {
            // Note: This is a simple hashing example. Use a library like BCrypt.Net for better security.
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private string GenerateJwtToken(User user)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(System.IO.File.ReadAllText("/app/jwt_private.pem"));

            var securityKey = new RsaSecurityKey(rsa);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.IdUser.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrEmpty(user.Email))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: null, // Not specifying audience
                claims: claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
