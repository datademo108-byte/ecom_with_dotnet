using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Authentication.Models;
using Authentication.Data;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;

namespace Authentication.Controllers
{
    [Authorize] // Only logged-in users can access product management
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Product/Index - Products page (all products)
        [AllowAnonymous] // Allow guests to view products
        public async Task<IActionResult> Index(string search, string category, string sortBy = "newest")
        {
            var query = _context.Products.AsQueryable();

            // Search functionality
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search));
            }

            // Category filter
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            // Sorting
            switch (sortBy.ToLower())
            {
                case "price_low":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_high":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "name":
                    query = query.OrderBy(p => p.Name);
                    break;
                case "oldest":
                    query = query.OrderBy(p => p.CreatedAt);
                    break;
                default: // newest
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            // Get distinct categories for filter dropdown
            var categories = await _context.Products
                .Where(p => !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.Categories = categories ?? new List<string>();
            ViewBag.SearchTerm = search;
            ViewBag.SelectedCategory = category;
            ViewBag.SortBy = sortBy;

            var products = await query.ToListAsync();
            return View(products); // Goes to Views/Product/Index.cshtml
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product); // Views/Product/Details.cshtml
        }

        // GET: Product/Create - Create product form
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(); // Goes to Views/Product/Create.cshtml
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,Category,StockQuantity,ImageUrl")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;

                _context.Add(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(product);
        }

        // GET: Product/Edit/5 - Edit product form
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product); // Goes to Views/Product/Edit.cshtml
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Category,StockQuantity,ImageUrl,CreatedAt")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Product updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Product/Delete/5 - Delete confirmation page
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product); // Goes to Views/Product/Delete.cshtml
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Product/Manage - Admin product management dashboard
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var products = await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // Get statistics
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.LowStockProducts = await _context.Products
                .Where(p => p.StockQuantity < 10)
                .CountAsync();
            ViewBag.OutOfStockProducts = await _context.Products
                .Where(p => p.StockQuantity == 0)
                .CountAsync();

            return View(products); // Goes to Views/Product/Manage.cshtml
        }

        // GET: Product/ByCategory - Products by category
        [AllowAnonymous]
        public async Task<IActionResult> ByCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
            {
                return RedirectToAction(nameof(Index));
            }

            var products = await _context.Products
                .Where(p => p.Category == category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            if (!products.Any())
            {
                ViewBag.Message = $"No products found in category: {category}";
            }

            ViewBag.Category = category;
            return View(products); // Goes to Views/Product/ByCategory.cshtml
        }

        // AJAX: Check product stock
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckStock(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }

            return Json(new
            {
                success = true,
                stock = product.StockQuantity,
                message = product.StockQuantity > 0
                    ? $"Available: {product.StockQuantity} in stock"
                    : "Out of stock"
            });
        }

        // Helper method to check if product exists
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}