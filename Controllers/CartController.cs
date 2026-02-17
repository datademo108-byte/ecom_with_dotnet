using Authentication.Data;
using Authentication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // ============================
        // ADD TO CART
        // ============================
        public IActionResult AddToCart(int productId)
        {
            // 🔐 Check login
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            string userId = User.Identity.Name;

            var cartItem = _context.CartItems
                .FirstOrDefault(c => c.ProductId == productId && c.UserId == userId);

            if (cartItem == null)
            {
                var newItem = new CartItem
                {
                    ProductId = productId,
                    UserId = userId,
                    Quantity = 1
                };

                _context.CartItems.Add(newItem);
            }
            else
            {
                cartItem.Quantity += 1;
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ============================
        // CART PAGE
        // ============================
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

        // ============================
        // REMOVE ITEM FROM CART
        // ============================
        public IActionResult Remove(int id)
        {
            var item = _context.CartItems.FirstOrDefault(c => c.Id == id);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }


        // =============================
        // INCREASE QUANTITY
        // =============================
        public IActionResult Increase(int id)
        {
            var item = _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefault(c => c.Id == id);

            if (item != null)
            {
                item.Quantity++;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // =============================
        // DECREASE QUANTITY
        // =============================
        public IActionResult Decrease(int id)
        {
            var item = _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefault(c => c.Id == id);

            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    _context.CartItems.Remove(item);
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }



    }
}
