using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sozif.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sozif.Controllers
{
    [Auth]
    public class CustomersController : Controller
    {
        private readonly sozifContext _context;

        public CustomersController(sozifContext context)
        {
            _context = context;
        }

        // GET: Customers
        public async Task<IActionResult> Index(string? name, string? nip, string? contact, string? phone, string? address)
        {
            var customers = await _context.Customers.OrderBy(c => c.CustomerName).Include(c => c.Addresses).ToListAsync();
            var customersToShow = new List<Customers>();
            customers.ForEach(c =>
            {
                bool isMatching = true;
                if (name != null && name != "" && !c.CustomerName.ToLower().Contains(name.ToLower()))
                {
                    isMatching = false;
                }
                if (nip != null && nip != "")
                {
                    string justNumber = "";
                    foreach (char ch in nip)
                    {
                        if (ch != '-') justNumber += ch;
                    }
                    if (!c.Nip.ToString().Contains(justNumber))
                    {
                        isMatching = false;
                    }
                }
                if (contact != null && contact != "" && (c.ContactPerson == null || !c.ContactPerson.ToLower().Contains(contact.ToLower())))
                {
                    isMatching = false;
                }
                if (phone != null && phone != "")
                {
                    string justNumber = "";
                    foreach (char ch in phone)
                    {
                        if (ch != '-') justNumber += ch;
                    }
                    if (c.PhoneNumber == null || !c.PhoneNumber.ToString().Contains(justNumber))
                    {
                        isMatching = false;
                    }
                }
                if (address != null && address != "" && !c.Addresses.First(a => a.IsMainAddress).FullAddress.ToLower().Contains(address.ToLower()))
                {
                    isMatching = false;
                }
                if (isMatching)
                {
                    customersToShow.Add(c);
                }
            });
            @ViewBag.Name = name;
            @ViewBag.Nip = nip;
            @ViewBag.Contact = contact;
            @ViewBag.Phone = phone;
            @ViewBag.Address = address;

            return View(customersToShow);
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
            if (HttpContext.Session.GetString("EditCustomers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,CustomerName,NipString,ContactPerson,PhoneNumberString,Street,PostalCode,City")] Customers customers)
        {
            if (HttpContext.Session.GetString("EditCustomers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (customers.CustomerName == null || customers.NipString == "" || customers.Street == null || customers.PostalCode == null || customers.City == null)
            {
                ViewBag.ErrorMessage = "Musisz uzupełnić wszystkie dane!";
                return View(customers);
            }

            string[] postalCodeSplit = customers.PostalCode.Split("-");
            if (postalCodeSplit.Length != 2 || postalCodeSplit[0].Length != 2 || postalCodeSplit[1].Length != 3)
            {
                ViewBag.ErrorMessage = "Niepoprawny format kodu pocztowego!";
                return View(customers);
            }
            if (customers.Nip < 1000000000)
            {
                ViewBag.ErrorMessage = "Niepoprawny numer NIP!";
                return View(customers);
            }
            int customersWithSameNip = await _context.Customers.Where(c => c.Nip == customers.Nip).CountAsync();
            if (customersWithSameNip > 0)
            {
                ViewBag.ErrorMessage = "Istnieje już klient o takim numerze NIP!";
                return View(customers);
            }
            if (customers.PhoneNumber < 100000000)
            {
                ViewBag.ErrorMessage = "Niepoprawny numer telefonu!";
                return View(customers);
            }

            if (ModelState.IsValid)
            {
                _context.Add(customers);
                await _context.SaveChangesAsync();

                Addresses mainAddress = new Addresses();
                mainAddress.CustomerId = customers.CustomerId;
                mainAddress.Street = customers.Street;
                mainAddress.PostalCode = customers.PostalCode;
                mainAddress.City = customers.City;
                mainAddress.IsMainAddress = true;
                _context.Add(mainAddress);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(customers);
        }

        // GET: Customers/CreateAddress
        public async Task<IActionResult> CreateAddress(int? id)
        {
            if (HttpContext.Session.GetString("EditCustomers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
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
            if (HttpContext.Session.GetString("EditCustomers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
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
            if (HttpContext.Session.GetString("EditCustomers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
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
            if (HttpContext.Session.GetString("EditCustomers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id != customers.CustomerId)
            {
                return NotFound();
            }
            if (customers.Nip < 1000000000)
            {
                ViewBag.ErrorMessage = "Niepoprawny numer NIP!";
            }
            if (customers.PhoneNumber < 100000000)
            {
                ViewBag.ErrorMessage = "Niepoprawny numer telefonu!";
            }
            if (ModelState.IsValid && ViewBag.ErrorMessage == null)
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
                return RedirectToAction(nameof(Details), new { id = id });
            }

            return View(await _context.Customers.Include(c => c.Addresses).FirstOrDefaultAsync(m => m.CustomerId == id));
        }

        // GET: Customers/EditAddress/5
        public async Task<IActionResult> EditAddress(int? id)
        {
            if (HttpContext.Session.GetString("EditCustomers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
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
            if (HttpContext.Session.GetString("EditCustomers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
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
                    if (!CustomersExists(addresses.CustomerId))
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
            if (HttpContext.Session.GetString("EditCustomers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }

            var customers = await _context.Customers.FirstOrDefaultAsync(m => m.CustomerId == id);
            if (customers == null)
            {
                return NotFound();
            }

            int customerOrdersCount = await _context.Orders.Where(o => o.CustomerId == customers.CustomerId).CountAsync();
            if (customerOrdersCount > 0)
            {
                ViewBag.ErrorMessage = "Do klienta jest przypisanych " + customerOrdersCount + " zamówień. Zostaną one usunięte razem z klientem.";
            }

            return View(customers);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("EditCustomers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            var invoicesToUpdate = await _context.Invoices.Where(i => i.CustomerId == id).ToListAsync();
            invoicesToUpdate.ForEach(i => i.CustomerId = null);
            _context.UpdateRange(invoicesToUpdate);
            await _context.SaveChangesAsync();
            var ordersToDelete = await _context.Orders.Include(o => o.OrderPositions).Where(o => o.CustomerId == id).ToListAsync();
            var orderPositionsToDelete = new List<OrderPositions>();
            ordersToDelete.ForEach(o => orderPositionsToDelete.AddRange(o.OrderPositions));
            _context.OrderPositions.RemoveRange(orderPositionsToDelete);
            await _context.SaveChangesAsync();
            _context.Orders.RemoveRange(ordersToDelete);
            await _context.SaveChangesAsync();
            var customers = await _context.Customers.FindAsync(id);
            var addresses = await _context.Addresses.Where(a => a.CustomerId == customers.CustomerId).ToListAsync();
            _context.Addresses.RemoveRange(addresses);
            await _context.SaveChangesAsync();
            _context.Customers.Remove(customers);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Customers/DeleteAddress/5
        public async Task<IActionResult> DeleteAddress(int? id)
        {
            if (HttpContext.Session.GetString("EditCustomers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
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
            if (HttpContext.Session.GetString("EditCustomers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
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
