using FastFoodAPI.Models;
using FastFoodAPI.Services;
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
                Console.WriteLine($"Email: {model.Email}, Password: {model.Password}");  // Log the data for debugging
                var result = await _userService.RegisterAsync(model);

                if (result)
                {
                    return Ok(new { message = "User registered successfully" });
                }

                return BadRequest("User registration failed");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);  // Log the exception to the console
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

            return Ok(new { Token = token });
        }

        [HttpGet("order-history")]
        public async Task<IActionResult> GetOrderHistory()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null)
            {
                return Unauthorized();
            }

            var orders = await _userService.GetOrderHistoryAsync(email);
            return Ok(orders);
        }

        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null)
            {
                return Unauthorized();
            }

            var result = await _userService.UpdateUserProfileAsync(email, model);
            if (result)
            {
                return Ok(new { message = "Profile updated successfully" });
            }

            return BadRequest("Failed to update profile");
        }


        [HttpPost("set-dark-mode")]
        public async Task<IActionResult> SetDarkMode([FromBody] bool darkMode)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null)
            {
                return Unauthorized();
            }

            var result = await _userService.SetDarkModePreferenceAsync(email, darkMode);
            if (result)
            {
                return Ok(new { message = "Dark mode preference updated successfully" });
            }

            return BadRequest("Failed to update preference");
        }

        [HttpGet("get-dark-mode")]
        public async Task<IActionResult> GetDarkModePreference()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email == null)
            {
                return Unauthorized();
            }

            var darkMode = await _userService.GetDarkModePreferenceAsync(email);
            return Ok(new { darkMode });
        }

    }
}
