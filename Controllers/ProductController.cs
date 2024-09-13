using FastFoodAPI.Data;
using FastFoodAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FastFoodAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : Controller
    {
        private readonly FastFoodAPIDbContext _context;

        public ProductController(FastFoodAPIDbContext context)
        {
            _context = context;
        }

        // 1. Public API Access: Fetch all products (No authentication required)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetProducts()
        {
            var products = _context.Products.ToList();
            return Ok(products); // This returns products in JSON for public API access
        }

        // ------------------- Razor View Admin Operations (CRUD) ----------------------

        // 2. Admin-Only: View all products in Razor view (Admin CRUD Dashboard)
        [Authorize(Roles = "Admin")]
        [HttpGet("/products/manage")]
        public IActionResult ManageProducts()
        {
            var products = _context.Products.ToList();
            return View(products); // This renders Razor view for managing products
        }

        // 3. Admin-Only: Display the Create Product form
        [Authorize(Roles = "Admin")]
        [HttpGet("/products/create")]
        public IActionResult CreateProduct()
        {
            return View(); // Return the Razor view to create a new product
        }

        // 4. Admin-Only: Handle form submission for creating a new product
        [Authorize(Roles = "Admin")]
        [HttpPost("/products/create")]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageProducts)); // Redirect back to the admin product list
            }
            return View(product); // Return the form with errors if ModelState is invalid
        }

        // 5. Admin-Only: Display the Edit Product form
        [Authorize(Roles = "Admin")]
        [HttpGet("/products/edit/{id}")]
        public IActionResult EditProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product); // Return the Razor view to edit product
        }

        // 6. Admin-Only: Handle form submission for editing an existing product
        [Authorize(Roles = "Admin")]
        [HttpPost("/products/edit/{id}")]
        public async Task<IActionResult> EditProduct(int id, Product product)
        {
            if (id != product.Id || !ModelState.IsValid)
            {
                return BadRequest();
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ManageProducts));
        }

        // 7. Admin-Only: Display the Delete confirmation view
        [Authorize(Roles = "Admin")]
        [HttpGet("/products/delete/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product); // Confirm deletion in a Razor view
        }

        // 8. Admin-Only: Handle form submission for deleting a product
        [Authorize(Roles = "Admin")]
        [HttpPost("/products/delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageProducts));
        }
    }
}
