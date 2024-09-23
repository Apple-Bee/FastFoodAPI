using FastFoodAPI.Models;
using FastFoodAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FastFoodAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                var result = await _userService.RegisterAsync(model);

                if (result)
                {
                    return Ok(new { message = "User registered successfully" });
                }

                return BadRequest("User registration failed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var token = await _userService.LoginAsync(model);
            if (token == null)
            {
                return Unauthorized("Invalid email or password");
            }

            return Ok(new { token });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;  // Updated to ClaimTypes.Name for retrieving email
            if (email == null)
            {
                return Unauthorized();
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                email = user.Email,
                fullName = user.FullName,
                darkMode = user.DarkMode
            });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data");  // Model validation failed
            }

            var result = await _userService.UpdateUserProfileAsync(User.Identity.Name, model);

            if (result)
            {
                return Ok("Profile updated successfully");
            }
            else
            {
                return BadRequest("Failed to update profile");
            }
        }


        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("set-dark-mode")]
        public async Task<IActionResult> SetDarkMode([FromBody] DarkModeRequest request)
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
            {
                return Unauthorized();
            }

            var result = await _userService.SetDarkModePreferenceAsync(email, request.DarkMode);
            if (result)
            {
                return Ok(new { message = "Dark mode preference updated successfully" });
            }

            return BadRequest("Failed to update dark mode preference");
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-dark-mode")]
        public async Task<IActionResult> GetDarkModePreference()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
            {
                return Unauthorized();
            }

            var darkMode = await _userService.GetDarkModePreferenceAsync(email);
            return Ok(new { darkMode });
        }


        public class DarkModeRequest
        {
            public bool DarkMode { get; set; }
        }
    }
}

