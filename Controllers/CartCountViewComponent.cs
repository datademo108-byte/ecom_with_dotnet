using Authentication.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Authentication.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public CartCountViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View(0);
            }

            string userId = User.Identity.Name;

            int count = _context.CartItems
                .Where(c => c.UserId == userId)
                .Sum(c => c.Quantity);

            return View(count);
        }
    }
}
