using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sozif.Attributes;
using Sozif.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Sozif.Controllers
{
    [Auth]
    public class ProductsController : Controller
    {
        private readonly sozifContext _context;

        public ProductsController(sozifContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var sozifContext = _context.Products.OrderBy(p => p.ProductName).Include(p => p.TaxRate);
            return View(await sozifContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var products = await _context.Products
                .Include(c => c.TaxRate)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (products == null)
            {
                return NotFound();
            }

            return View(products);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("EditProducts") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["TaxRateId"] = new SelectList(_context.TaxRates.OrderBy(tr => tr.Rate), "TaxRateId", "Rate");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,BaseNetPrice,TaxRateId")] ProductDTO product)
        {
            if (HttpContext.Session.GetString("EditProducts") == "false")
            {
                return RedirectToAction("Index", "Home");
            }

            int productsWithSameName = await _context.Products.Where(p => p.ProductName == product.ProductName).CountAsync();
            if (productsWithSameName > 0)
            {
                ViewBag.ErrorMessage = "Istnieje już produkt o takiej nazwie!";
                ViewData["TaxRateId"] = new SelectList(_context.TaxRates.OrderBy(tr => tr.Rate), "TaxRateId", "Rate", product.TaxRateId);
                return View(product);
            }

            if (ModelState.IsValid)
            {
                var productToInsert = new Products();
                productToInsert.ProductName = product.ProductName;
                productToInsert.TaxRateId = product.TaxRateId;
                productToInsert.BaseNetPrice = (int)(product.BaseNetPrice * 100);

                _context.Add(productToInsert);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TaxRateId"] = new SelectList(_context.TaxRates.OrderBy(tr => tr.Rate), "TaxRateId", "Rate", product.TaxRateId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("EditProducts") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }

            var products = await _context.Products.FindAsync(id);
            if (products == null)
            {
                return NotFound();
            }
            var product = new ProductDTO();
            product.ProductId = products.ProductId;
            product.ProductName = products.ProductName;
            product.BaseNetPrice = ((decimal)products.BaseNetPrice) / 100;
            product.TaxRateId = products.TaxRateId;

            ViewData["TaxRateId"] = new SelectList(_context.TaxRates.OrderBy(tr => tr.Rate), "TaxRateId", "Rate", product.TaxRateId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,BaseNetPrice,TaxRateId")] ProductDTO product)
        {
            if (HttpContext.Session.GetString("EditProducts") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id != product.ProductId)
            {
                return NotFound();
            }

            int productsWithSameName = await _context.Products.Where(p => p.ProductName == product.ProductName).CountAsync();
            if (productsWithSameName > 0)
            {
                ViewBag.ErrorMessage = "Istnieje już produkt o takiej nazwie!";
                ViewData["TaxRateId"] = new SelectList(_context.TaxRates.OrderBy(tr => tr.Rate), "TaxRateId", "Rate", product.TaxRateId);
                return View(product);
            }

            var productToInsert = new Products();
            productToInsert.ProductId = product.ProductId;
            productToInsert.ProductName = product.ProductName;
            productToInsert.BaseNetPrice = (int)(product.BaseNetPrice * 100);
            productToInsert.TaxRateId = product.TaxRateId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productToInsert);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductsExists(productToInsert.ProductId))
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
            ViewData["TaxRateId"] = new SelectList(_context.TaxRates.OrderBy(tr => tr.Rate), "TaxRateId", "Rate", product.TaxRateId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("EditProducts") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }

            var products = await _context.Products
                .Include(p => p.TaxRate)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (products == null)
            {
                return NotFound();
            }

            return View(products);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("EditProducts") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            var products = await _context.Products.FindAsync(id);
            _context.Products.Remove(products);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductsExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
