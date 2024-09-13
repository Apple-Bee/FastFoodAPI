using FastFoodAPI.Data; // Add this for accessing DbContext
using FastFoodAPI.Models; // Add this for Product model
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FastFoodAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FastFoodAPIDbContext _context; // Add DbContext
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        // Updated constructor to inject SignInManager and UserManager
        public HomeController(ILogger<HomeController> logger, FastFoodAPIDbContext context, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context; // Assign the injected DbContext
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // Login GET action to display the login form
        [HttpGet]
        public IActionResult Login()
        {
            return View(); // Return the login view
        }

        // Login POST action to handle user login
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home"); // Redirect to home after successful login
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // Logout action to log out the user
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Default Index action
        public IActionResult Index()
        {
            return View();
        }

        // Privacy action
        public IActionResult Privacy()
        {
            return View();
        }

        // Route to the "ManageProducts" view (Admin-only access)
        [Authorize(Roles = "Admin")]
        [HttpGet("/Home/ManageProducts")]
        public IActionResult ManageProducts()
        {
            // Ensure that products are being retrieved from the database
            var products = _context.Products.ToList();

            // Check if the products list is null or empty
            if (products == null || !products.Any())
            {
                return NotFound("No products found.");
            }

            return View(products); // Pass the products list to the view
        }

        // Route to the "CreateProduct" view (Admin-only access)
        [Authorize(Roles = "Admin")]
        [HttpGet("/Home/CreateProduct")]  // Explicit route for CreateProduct
        public IActionResult CreateProduct()
        {
            return View();  // This should map to Views/Home/CreateProduct.cshtml
        }

        // Handle POST request for product creation (Admin-only access)
        [Authorize(Roles = "Admin")]
        [HttpPost("/Home/CreateProduct")]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product); // Add product to the database
                await _context.SaveChangesAsync(); // Save changes asynchronously
                return RedirectToAction(nameof(ManageProducts)); // Redirect to the ManageProducts view
            }

            return View(product); // If validation fails, return the form with errors
        }

        // Error handling action
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
