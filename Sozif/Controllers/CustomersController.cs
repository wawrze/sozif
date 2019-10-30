using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Sozif.Controllers
{
    public class CustomersController : Controller
    {
        private readonly sozifContext _context;

        public CustomersController(sozifContext context)
        {
            _context = context;
        }

        // GET: Customers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Customers.Include(c => c.Addresses).ToListAsync());
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customers = await _context.Customers
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customers == null)
            {
                return NotFound();
            }

            return View(customers);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,CustomerName,Nip,ContactPerson,PhoneNumber")] Customers customers)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customers);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customers);
        }

        // GET: Customers/CreateAddress
        public async Task<IActionResult> CreateAddress(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var customers = await _context.Customers.FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customers == null)
            {
                return NotFound();
            }
            ViewBag.Customer = customers;
            return View();
        }

        // POST: Customers/CreateAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAddress(int id, [Bind("AddressId,Street,PostalCode,City,IsMainAddress,CustomerId")] Addresses addresses)
        {
            if (ModelState.IsValid)
            {
                if (addresses.IsMainAddress)
                {
                    var allCustomerAddresses = _context.Addresses.Where(a => a.CustomerId == id);
                    await allCustomerAddresses.ForEachAsync(a => a.IsMainAddress = false);
                }
                _context.Add(addresses);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Edit), new { id = addresses.CustomerId });
            }
            return View(addresses);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customers = await _context.Customers.Include(c => c.Addresses).FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customers == null)
            {
                return NotFound();
            }
            return View(customers);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,CustomerName,NipString,ContactPerson,PhoneNumberString")] Customers customers)
        {
            if (id != customers.CustomerId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customers);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomersExists(customers.CustomerId))
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

            return View(await _context.Customers.Include(c => c.Addresses).FirstOrDefaultAsync(m => m.CustomerId == id));
        }

        // GET: Customers/EditAddress/5
        public async Task<IActionResult> EditAddress(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var addresses = await _context.Addresses.FirstOrDefaultAsync(m => m.AddressId == id);

            if (addresses == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(m => m.CustomerId == addresses.CustomerId);
            ViewBag.Customer = customer;

            return View(addresses);
        }

        // POST: Customers/EditAddress/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(int id, [Bind("AddressId,Street,PostalCode,City,IsMainAddress,CustomerId")] Addresses addresses)
        {
            if (id != addresses.AddressId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if (addresses.IsMainAddress)
                    {
                        var allCustomerAddresses = _context.Addresses.Where(a => a.CustomerId == id && a.AddressId != id);
                        await allCustomerAddresses.ForEachAsync(a => a.IsMainAddress = false);
                    }
                    _context.Update(addresses);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomersExists(addresses.AddressId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Edit), new { id = addresses.CustomerId });
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(m => m.CustomerId == addresses.CustomerId);
            ViewBag.Customer = customer;

            return View(addresses);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customers = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customers == null)
            {
                return NotFound();
            }

            return View(customers);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customers = await _context.Customers.FindAsync(id);
            _context.Customers.Remove(customers);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Customers/DeleteAddress/5
        public async Task<IActionResult> DeleteAddress(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var addresses = await _context.Addresses.Include(a => a.Customer).FirstOrDefaultAsync(m => m.AddressId == id);
            if (addresses == null)
            {
                return NotFound();
            }

            return View(addresses);
        }

        // POST: Customers/DeleteAddress/5
        [HttpPost, ActionName("DeleteAddress")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedAddress(int id)
        {
            var addresses = await _context.Addresses.FindAsync(id);
            _context.Addresses.Remove(addresses);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Edit), new { id = addresses.CustomerId });
        }

        private bool CustomersExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
