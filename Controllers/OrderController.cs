using FastFoodAPI.Data;
using FastFoodAPI.Models;
using Microsoft.AspNetCore.Mvc;

public class OrderController : Controller
{
    private readonly FastFoodAPIDbContext _context;

    public OrderController(FastFoodAPIDbContext context)
    {
        _context = context;
    }

    // GET: /Order
    public IActionResult Index()
    {
        var orders = _context.Orders.ToList();
        return View(orders);
    }

    // POST: /Order/Create
    [HttpPost]
    public IActionResult Create(Order order)
    {
        if (ModelState.IsValid)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        return View(order);
    }

    // GET: /Order/Details/{id}
    public IActionResult Details(int id)
    {
        var order = _context.Orders.Find(id);
        if (order == null)
        {
            return NotFound();
        }
        return View(order);
    }

    // PUT: /Order/Edit/{id}
    [HttpPost]
    public IActionResult Edit(int id, Order order)
    {
        var existingOrder = _context.Orders.Find(id);
        if (existingOrder == null)
        {
            return NotFound();
        }

        existingOrder.Quantity = order.Quantity;
        existingOrder.OrderDate = order.OrderDate;
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    // DELETE: /Order/Delete/{id}
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var order = _context.Orders.Find(id);
        if (order == null)
        {
            return NotFound();
        }

        _context.Orders.Remove(order);
        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
}
