using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sozif.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace Sozif.Controllers
{
    [Auth]
    public class TaxRatesController : Controller
    {
        private readonly sozifContext _context;

        public TaxRatesController(sozifContext context)
        {
            _context = context;
        }

        // GET: TaxRates
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("EditProducts") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            return View(await _context.TaxRates.OrderBy(tr => tr.Rate).ToListAsync());
        }

        // GET: TaxRates/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("EditProducts") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: TaxRates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaxRateId,Rate")] TaxRates taxRates)
        {
            if (HttpContext.Session.GetString("EditProducts") == "false")
            {
                return RedirectToAction("Index", "Home");
            }

            int taxRatesWithSameRate = await _context.TaxRates.Where(t => t.Rate == taxRates.Rate).CountAsync();
            if (taxRatesWithSameRate > 0)
            {
                ViewBag.ErrorMessage = "Istnieje już taka stawka VAT!";
                return View(taxRates);
            }

            if (ModelState.IsValid)
            {
                _context.Add(taxRates);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(taxRates);
        }

        // GET: TaxRates/Edit/5
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

            var taxRates = await _context.TaxRates.FindAsync(id);
            if (taxRates == null)
            {
                return NotFound();
            }
            return View(taxRates);
        }

        // POST: TaxRates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TaxRateId,Rate")] TaxRates taxRates)
        {
            if (HttpContext.Session.GetString("EditProducts") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id != taxRates.TaxRateId)
            {
                return NotFound();
            }

            int taxRatesWithSameRate = await _context.TaxRates.Where(t => t.Rate == taxRates.Rate).CountAsync();
            if (taxRatesWithSameRate > 0)
            {
                ViewBag.ErrorMessage = "Istnieje już taka stawka VAT!";
                return View(taxRates);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taxRates);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaxRatesExists(taxRates.TaxRateId))
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
            return View(taxRates);
        }

        // GET: TaxRates/Delete/5
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

            var taxRates = await _context.TaxRates
                .FirstOrDefaultAsync(m => m.TaxRateId == id);
            if (taxRates == null)
            {
                return NotFound();
            }

            int productsWithThisTaxRate = await _context.Products.Where(p => p.TaxRateId == id).CountAsync();
            if (productsWithThisTaxRate > 0)
            {
                ViewBag.ErrorMessage = "Ta stawka VAT jest przypisana do " + productsWithThisTaxRate + " produktów - nie możesz jej usunąć!";
            }

            return View(taxRates);
        }

        // POST: TaxRates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("EditProducts") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            var taxRates = await _context.TaxRates.FindAsync(id);
            _context.TaxRates.Remove(taxRates);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaxRatesExists(int id)
        {
            return _context.TaxRates.Any(e => e.TaxRateId == id);
        }
    }
}
