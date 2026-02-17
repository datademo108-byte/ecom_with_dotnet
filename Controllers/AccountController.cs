using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Authentication.Models;
using Authentication.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authentication.Controllers
{

    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult contact()
        {
            return View();
        }



      


        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            var dbUser = _context.Users.SingleOrDefault(u =>
                u.Username == user.Username && u.Password == user.Password);

            if (dbUser != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, dbUser.Username),
            new Claim(ClaimTypes.Role, dbUser.Role) // Add role claim
        };
                var claimsIdentity = new ClaimsIdentity(claims,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = "Login failed. Please try again.";
            return View();
        }




        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Set default role if not provided
                if (string.IsNullOrEmpty(user.Role))
                {
                    user.Role = "User";
                }

                // Set empty email if null (optional)
                if (string.IsNullOrEmpty(user.Email))
                {
                    user.Email = ""; // Or generate a placeholder
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                ViewBag.Message = "Registration successful. You can now log in.";
                return View();
            }

            ViewBag.Message = "Please fix the validation errors.";
            return View(user);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
