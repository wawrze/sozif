using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sozif.Attributes;
using Sozif.Models;
using System.Collections.Generic;
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
        public async Task<IActionResult> Index(string? name, decimal? netFrom, decimal? netTo, int? tax, decimal? grossFrom, decimal? grossTo)
        {
            var products = await _context.Products.OrderBy(p => p.ProductName).Include(p => p.TaxRate).ToListAsync();
            var productsToShow = new List<Products>();
            products.ForEach(p =>
            {
                bool isMatching = true;
                if (name != null && name != "" && !p.ProductName.ToLower().Contains(name.ToLower()))
                {
                    isMatching = false;
                }
                if (netFrom != null && p.BaseNetPrice < netFrom * 100)
                {
                    isMatching = false;
                }
                if (netTo != null && p.BaseNetPrice > netTo * 100)
                {
                    isMatching = false;
                }
                if (tax != null && p.TaxRateId != tax)
                {
                    isMatching = false;
                }
                if (grossFrom != null && p.BaseGrossPrice < grossFrom * 100)
                {
                    isMatching = false;
                }
                if (grossTo != null && p.BaseGrossPrice > grossTo * 100)
                {
                    isMatching = false;
                }
                if (isMatching)
                {
                    productsToShow.Add(p);
                }
            });
            ViewBag.TaxRates = await _context.TaxRates.ToListAsync();
            ViewBag.Name = name;
            ViewBag.NetFrom = netFrom.ToString().Replace(',', '.');
            ViewBag.NetTo = netTo.ToString().Replace(',', '.');
            ViewBag.Tax = tax;
            ViewBag.GrossFrom = grossFrom.ToString().Replace(',', '.');
            ViewBag.GrossTo = grossTo.ToString().Replace(',', '.');

            return View(productsToShow);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id, string? from, int? fromId)
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
            ViewBag.From = from;
            ViewBag.FromId = fromId;

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
            if (product.ProductName == null)
            {
                ViewBag.ErrorMessage = "Nazwa produktu nie może być pusta!";
                ViewData["TaxRateId"] = new SelectList(_context.TaxRates.OrderBy(tr => tr.Rate), "TaxRateId", "Rate", product.TaxRateId);
                return View(product);
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

            int orderPositionsWithThisProduct = await _context.OrderPositions.Where(op => op.ProductId == id).CountAsync();
            if (orderPositionsWithThisProduct > 0)
            {
                ViewBag.ErrorMessage = "Ten produkt występuje w " + orderPositionsWithThisProduct + " pozycjach zamówień. Usunięcie go spowoduje usunięcie tych pozycji.";
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
            var orderPositions = await _context.OrderPositions.Where(op => op.ProductId == id).ToListAsync();
            _context.OrderPositions.RemoveRange(orderPositions);
            await _context.SaveChangesAsync();
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
