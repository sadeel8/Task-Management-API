using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Task_Management_API.DbContexts;
using Task_Management_API.DTO.Auth;
using Task_Management_API.Models;

namespace Task_Management_API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TaskManagementContext _dbContext;
        private readonly IConfiguration _config;

        public AuthController(TaskManagementContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        [HttpPost("Signup")]
        public IActionResult Signup([FromBody] SignupDto signupDto)
        {
            try
            {
                if (_dbContext.Users.Any(x => x.Email.ToLower() == signupDto.Email.ToLower()))
                {
                    return Conflict(new Exception("Email already exists.")); //409
                }
                if (_dbContext.Users.Any(x => x.Username.ToLower() == signupDto.Username.ToLower()))
                {
                    return Conflict(new Exception("Username already exists.")); //409
                }

                var user = new User()
                {
                    Id = 0,
                    Username = signupDto.Username,
                    Email = signupDto.Email,
                    HashedPassword = BCrypt.Net.BCrypt.HashPassword(signupDto.Password)
                };
                _dbContext.Users.Add(user);
                _dbContext.SaveChanges(); //Go's to database

                return CreatedAtAction(nameof(Signup), new { id = user.Id }, new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Exception(ex.Message));
            }
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(x => x.Email.ToLower() == loginDto.Email.ToLower());
                if (user == null)
                {
                    return Unauthorized(new Exception("Invalid email or password.")); //401
                }
                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.HashedPassword))
                {
                    return Unauthorized(new Exception("Invalid email or password.")); //401
                }
                //generate token
                string token = GenerateToken(user);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Exception(ex.Message));
            }
        }

        private string GenerateToken(User user)
        {
            // Claims ===> User Info
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var claims = new List<Claim>();
            claims.Add(new Claim("UserId", user.Id.ToString()));
            claims.Add(new Claim("Username", user.Username));
            claims.Add(new Claim("Email", user.Email));

            var tokenSettings = new JwtSecurityToken(
                 issuer: jwt["Issuer"],
                 audience: jwt["Audience"],
                 claims: claims,
                 expires: DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpiryMinutes"] ?? "60")),
                 signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.WriteToken(tokenSettings);

            return token;
        }
    }
}
