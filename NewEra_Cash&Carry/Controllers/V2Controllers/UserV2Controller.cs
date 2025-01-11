using AutoMapper;
using global::NewEra_Cash_Carry.Data;
using global::NewEra_Cash_Carry.DTOs.UserDTOs;
using global::NewEra_Cash_Carry.Helpers;
using global::NewEra_Cash_Carry.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NewEra_Cash_Carry.Data;
using NewEra_Cash_Carry.DTOs.UserDTOs;
using NewEra_Cash_Carry.Helpers;
using NewEra_Cash_Carry.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NewEra_Cash_Carry.Controllers.V2Controllers;

[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class UserV2Controller : ControllerBase
{
    private readonly ECommerceDbContext _context;
    private readonly AuthSettings _authSettings;
    private readonly IMapper _mapper;

    public UserV2Controller(ECommerceDbContext context, IOptions<AuthSettings> authSettings, IMapper mapper)
    {
        _context = context;
        _authSettings = authSettings.Value;
        _mapper = mapper;
    }

    // POST: api/User/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto userDto)
    {


        if (await _context.Users.AnyAsync(u => u.UserName == userDto.UserName))
        {
            return BadRequest("Email is already taken");
        }

        var user = _mapper.Map<User>(userDto);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.PasswordHash);
        user.Role = string.IsNullOrEmpty(user.Role) ? "Customer" : userDto.Role; // Default role is Customer

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok("User registered successfully!");
    }

    // POST: api/User/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto userDto)
    {
        var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userDto.UserName);

        if (dbUser == null || !BCrypt.Net.BCrypt.Verify(userDto.PasswordHash, dbUser.PasswordHash))
        {
            return Unauthorized("Invalid username or password");
        }

        var token = GenerateJwtToken(dbUser);
        return Ok(new { Token = token });
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHAndler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_authSettings.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role) // Role based authorization
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHAndler.CreateToken(tokenDescriptor);
        return tokenHAndler.WriteToken(token);

    }
}

