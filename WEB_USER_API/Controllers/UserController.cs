using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_USER_API.DATA;
using WEB_USER_API.Models;
using WEB_USER_API.Models.Entities;
using BCrypt.Net;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace WEB_USER_API.Controllers
{
    //local
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly API_DBContext _context;

        public UserController(API_DBContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: api/user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    Licenses = u.Licenses.Select(l => new LicenseDto
                    {
                        Id = l.Id,
                        Name = l.Name,
                        ExpirationDate = l.ExpirationDate
                    }).ToList()
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET api/user/{id}
        [HttpGet]
        [Route("{id:guid}")]
        public async Task<ActionResult<UserDto>> GetUsers(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Licenses)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
               return NotFound();

            var userDto = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                Licenses = user.Licenses.Select(l => new LicenseDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    ExpirationDate = l.ExpirationDate
                }).ToList()
            };
            

            return Ok(userDto);
        }

        // POST api/user
        [HttpPost]
        public async Task<ActionResult<UserDto>> AddUser(AddUserDto addUserDto)
        {
            if (string.IsNullOrWhiteSpace(addUserDto.Email) || string.IsNullOrWhiteSpace(addUserDto.Password)) 
                return BadRequest("Email and password are required");

            var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == addUserDto.Email); 
            if (existingUser != null) 
                return Conflict("Email already exists");

            // Explicitly set Id so the new user has a stable key immediately
            var user = new User()
            {
                Id = Guid.NewGuid(),
                Name = addUserDto.Name,
                Email = addUserDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(addUserDto.Password),
                Phone = addUserDto.Phone,
                Role = addUserDto.Role ?? "User",
                Licenses = new List<License>()
            };

            if (addUserDto.Licenses != null)
            {
                foreach (var l in addUserDto.Licenses)
                {
                    user.Licenses.Add(new License
                    {
                        Id = Guid.NewGuid(),
                        Name = l.Name,
                        ExpirationDate = l.ExpirationDate,
                        
                    });
                }
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userResponse = new UserDto
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                Licenses = user.Licenses.Select(l => new LicenseDto
                {
                    Name = l.Name,
                    ExpirationDate = l.ExpirationDate
                }).ToList()
            };

            return Ok(userResponse);
        }

        // PUT api/user/{id}
        [HttpPut]
        [Route("{id:guid}")]
        public async Task<ActionResult<UserDto>> UpdateUser(Guid id, UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.Include(u => u.Licenses).FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
                return NotFound();
            

            user.Name = updateUserDto.Name;
            user.Email = updateUserDto.Email;            
            user.Phone = updateUserDto.Phone;
            user.Role = updateUserDto.Role ?? user.Role;            
            

            await _context.SaveChangesAsync();

            var userResponse = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                Licenses = user.Licenses.Select(l => new LicenseDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    ExpirationDate = l.ExpirationDate
                }).ToList()
            };

            return Ok(userResponse);
        }

        // DELETE api/user/{id}
        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user is null) 
                return NotFound();
            

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();

        }

        // POST api/user/login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Email) || string.IsNullOrWhiteSpace(dto?.Password))
                return BadRequest("Email and password are required");

            var user = await _context.Users
                .Include(u => u.Licenses)
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized("Invalid credentials");

            //Token generation would go here
            var tokenhandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.Role ?? "User")
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                //Issuer = _config["Jwt:Issuer"],
                //Audience = _config["Jwt:Audience"]
            };


            try
            {
                var token = tokenhandler.CreateToken(tokenDescriptor);
                var jwt = tokenhandler.WriteToken(token);


                return Ok(new
                {
                    Token = jwt,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Phone = user.Phone,
                        Role = user.Role,
                        Licenses = user.Licenses.Select(l => new LicenseDto
                        {
                            Id = l.Id,
                            Name = l.Name,
                            ExpirationDate = l.ExpirationDate
                        }).ToList()
                    }

                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
