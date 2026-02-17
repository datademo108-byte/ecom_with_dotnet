using Authentication.Data;
using Authentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // =========================
        // USER - MY ORDERS
        // =========================
        public IActionResult MyOrders()
        {
            string userId = User.Identity.Name;

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // =========================
        // ORDER DETAILS
        // =========================
        public IActionResult Details(int id)
        {
            var orderItems = _context.OrderItems
                .Include(o => o.Product)
                .Where(o => o.OrderId == id)
                .ToList();

            return View(orderItems);
        }

        // =========================
        // ADMIN - ALL ORDERS
        // =========================
        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            var orders = _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }
    }
}
