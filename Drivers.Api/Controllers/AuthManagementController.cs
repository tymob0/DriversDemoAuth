using Drivers.Api.Configurations;
using Drivers.Api.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Drivers.Api.Controllers
{
    [ApiController]
    [Route("api/AuthManager")]
    public class AuthManagementController : ControllerBase
    {

        private readonly ILogger<AuthManagementController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JWTConfig _jwtConfig;

        public AuthManagementController(ILogger<AuthManagementController> logger, UserManager<IdentityUser> userManager, IOptionsMonitor<JWTConfig> _optionsMonitor)
        {
            _logger = logger;
            _userManager = userManager;
            _jwtConfig = _optionsMonitor.CurrentValue;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDTO userRegistrationRequestDTO)
        {
            if (ModelState.IsValid)
            {
                var emailExist = await _userManager.FindByEmailAsync(userRegistrationRequestDTO.Email);

                if (emailExist != null)
                {
                    return BadRequest("Email already exists!");
                }

                var newUser = new IdentityUser()
                {
                    Email = userRegistrationRequestDTO.Email,
                    UserName = userRegistrationRequestDTO.Email
                };

                var isCreated = await _userManager.CreateAsync(newUser, userRegistrationRequestDTO.Password);

                if (isCreated.Succeeded)
                {
                    // Genearte token

                    var token = generateJWTToken(newUser);

                    return Ok(new RegistrationRequestResponseDTO()
                    {
                        Result = true,
                        Token = token
                    });
                }

                return BadRequest(isCreated.Errors.Select(x=>x.Description).ToList());
            }

            return BadRequest("Invalid request payload");
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDTO requestDTO)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(requestDTO.Email);
                if (existingUser == null)
                {
                    return BadRequest("Invalid authentication");
                }
                var isPasswordValid = await _userManager.CheckPasswordAsync(existingUser, requestDTO.Password);

                if (isPasswordValid)
                {
                    var token = generateJWTToken(existingUser);

                    return Ok(new LoginRequestResponseDTO()
                    {
                        Result = true,
                        Token = token
                    });
                }

                return BadRequest("Invalid authentication");

            }

            return BadRequest("Invalid request payload");
        }

        private string generateJWTToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(4),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256)
            };

            var token= jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);
            return jwtToken;
        }

    }
}
