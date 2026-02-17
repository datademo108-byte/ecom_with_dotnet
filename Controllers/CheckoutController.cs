using Authentication.Data;
using Authentication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly AppDbContext _context;

        public CheckoutController(AppDbContext context)
        {
            _context = context;
        }

        // =============================
        // CHECKOUT PAGE
        // =============================
        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            string userId = User.Identity.Name;

            var cartItems = _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToList();

            return View(cartItems);
        }

        // =============================
        // PAYMENT SUCCESS
        // =============================
        [HttpPost]
        public IActionResult PaymentSuccess(string paymentId)
        {
            string userId = User.Identity.Name;

            var cartItems = _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToList();

            if (!cartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            decimal totalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity);

            // SAVE ORDER
            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                PaymentId = paymentId,
                OrderDate = DateTime.Now
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // SAVE ORDER ITEMS
            foreach (var item in cartItems)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                });

                // OPTIONAL: Reduce stock or hide product
                item.Product.StockQuantity = 0;
            }

            // CLEAR CART
            _context.CartItems.RemoveRange(cartItems);

            _context.SaveChanges();

            return RedirectToAction("Success");
        }

        // =============================
        // SUCCESS PAGE
        // =============================
        public IActionResult Success()
        {
            return View();
        }
    }
}
