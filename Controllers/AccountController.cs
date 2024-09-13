using FastFoodAPI.Models;
using FastFoodAPI.Services;
using Microsoft.AspNetCore.Mvc;
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

    }
}

