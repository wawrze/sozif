using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sozif.Attributes;
using Sozif.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Sozif.Controllers
{
    [Auth]
    public class InvoicesController : Controller
    {
        private readonly sozifContext _context;

        public InvoicesController(sozifContext context)
        {
            _context = context;
        }

        // GET: Invoices
        public async Task<IActionResult> Index()
        {
            var sozifContext = _context.Invoices
                .Include(i => i.InvoicePositions)
                .OrderByDescending(i => i.InvoiceId);
            return View(await sozifContext.ToListAsync());
        }

        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoices = await _context.Invoices
                .Include(i => i.InvoicePositions)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);
            if (invoices == null)
            {
                return NotFound();
            }

            return View(invoices);
        }

        // GET: Invoices/ChooseCustomer
        public async Task<IActionResult> ChooseCustomer()
        {
            if (HttpContext.Session.GetString("EditInvoices") == "false")
            {
                return RedirectToAction("Index", "Home");
            }

            var customers = await _context.Customers.Include(c => c.Addresses).OrderBy(c => c.CustomerName).ToListAsync();
            return View(customers);
        }

        // GET: Invoices/ChooseOrders/5
        public async Task<IActionResult> ChooseOrders(int? id)
        {
            if (HttpContext.Session.GetString("EditInvoices") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }
            var customer = await _context.Customers.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }

            var ordersToChooseFrom = await _context.Orders
                .Where(o => !o.OrderNumber.Contains("R"))
                .Include(o => o.Address)
                .Include(o => o.User)
                .Include(o => o.OrderPositions)
                    .ThenInclude(op => op.Product)
                    .ThenInclude(p => p.TaxRate)
                .Where(o => o.CustomerId == id && o.InvoiceId == null)
                .ToListAsync();
            InvoiceDTO invoice = new InvoiceDTO();
            invoice.CustomerName = customer.CustomerName;
            invoice.CustomerNip = customer.NipString;
            invoice.CustomerAddress = customer.Addresses.First(a => a.IsMainAddress).FullAddress;
            ordersToChooseFrom.ForEach(o => invoice.Orders.Add(new KeyValuePair<int, bool>(o.OrderId, false)));
            ViewBag.Orders = ordersToChooseFrom;

            return View(invoice);
        }

        // GET: Invoices/ChooseOrders/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChooseOrders(int? id, [Bind("Orders")] InvoiceDTO invoice)
        {
            if (HttpContext.Session.GetString("EditInvoices") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id == null)
            {
                return NotFound();
            }
            var customer = await _context.Customers.Include(c => c.Addresses).FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null)
            {
                return NotFound();
            }
            string userIdString = HttpContext.Session.GetString("UserId");
            if (userIdString == null)
            {
                return NotFound();
            }
            int userId = int.Parse(userIdString);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var ordersToInvoiceIds = new List<int>();
            invoice.Orders.ForEach(o => { if (o.Value) ordersToInvoiceIds.Add(o.Key); });
            if (ordersToInvoiceIds.Count == 0)
            {
                ViewBag.ErrorMessage = "Musisz wybrać przynajmniej jedno zamówienie, do którego chcesz wystawić fakturę!";
            }
            if (ViewBag.ErrorMessage == null)
            {
                var lastInvoiceInDB = await _context.Invoices
                    .OrderByDescending(o => o.InvoiceId)
                    .FirstOrDefaultAsync();
                string month = DateTime.Now.ToString("MM", DateTimeFormatInfo.InvariantInfo);
                string year = DateTime.Now.ToString("yy", DateTimeFormatInfo.InvariantInfo);
                string newInvoiceNumber;
                if (lastInvoiceInDB == null)
                {
                    newInvoiceNumber = "F/001/" + month + "/" + year;
                }
                else
                {
                    string[] lastInvoiceNumberSplit = lastInvoiceInDB.InvoiceNumber.Split('/');
                    int numberInt;
                    DateTime lastInvoiceDate = lastInvoiceInDB.InvoiceDate;
                    DateTime todayDate = DateTime.Now;
                    if (lastInvoiceDate.Month == todayDate.Month && lastInvoiceDate.Year == todayDate.Year)
                    {
                        numberInt = int.Parse(lastInvoiceNumberSplit[1]);
                    }
                    else
                    {
                        numberInt = 1;
                    }
                    string numberString = "" + (numberInt + 1);
                    while (numberString.Length < 3)
                    {
                        numberString = "0" + numberString;
                    }
                    newInvoiceNumber = "F/" + numberString + "/" + month + "/" + year;
                }
                var newInvoice = new Invoices();
                newInvoice.InvoiceNumber = newInvoiceNumber;
                newInvoice.CustomerName = customer.CustomerName;
                newInvoice.CustomerNip = customer.Nip;
                var customerMainAddress = customer.Addresses.First(a => a.IsMainAddress);
                newInvoice.CustomerAddress = customerMainAddress.Street;
                newInvoice.CustomerPostalCode = customerMainAddress.PostalCode;
                newInvoice.CustomerCity = customerMainAddress.City;
                newInvoice.InvoiceDate = DateTime.Now;
                newInvoice.DaysToPay = invoice.DaysToPay;
                newInvoice.UserName = user.Firstname + " " + user.Lastname;
                newInvoice.CustomerId = customer.CustomerId;
                newInvoice.UserId = userId;
                _context.Add(newInvoice);
                await _context.SaveChangesAsync();

                var ordersToInvoice = await _context.Orders
                    .Where(o => ordersToInvoiceIds.Contains(o.OrderId))
                    .Include(o => o.OrderPositions)
                    .ThenInclude(op => op.Product)
                    .ThenInclude(p => p.TaxRate)
                    .ToListAsync();
                var newInvoicePositions = new List<InvoicePositions>();
                ordersToInvoice.ForEach(o =>
                {
                    foreach (OrderPositions op in o.OrderPositions)
                    {
                        var invoicePosition = new InvoicePositions();
                        invoicePosition.ProductName = op.Product.ProductName;
                        invoicePosition.ProductCount = op.Count;
                        invoicePosition.ProductNetPrice = op.Product.BaseNetPrice;
                        invoicePosition.ProductTaxRate = op.Product.TaxRate.Rate;
                        invoicePosition.Discount = op.Discount;
                        invoicePosition.InvoiceId = newInvoice.InvoiceId;
                        newInvoicePositions.Add(invoicePosition);
                    }
                });
                _context.AddRange(newInvoicePositions);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var ordersToChooseFrom = await _context.Orders
                .Where(o => !o.OrderNumber.Contains("R"))
                .Include(o => o.Address)
                .Include(o => o.User)
                .Include(o => o.OrderPositions)
                .ThenInclude(op => op.Product)
                .ThenInclude(p => p.TaxRate)
                .Where(o => o.CustomerId == id && o.InvoiceId == null)
                .ToListAsync();
            ViewBag.Orders = ordersToChooseFrom;
            invoice.Orders.Clear();
            ordersToChooseFrom.ForEach(o => invoice.Orders.Add(new KeyValuePair<int, bool>(o.OrderId, false)));
            invoice.CustomerName = customer.CustomerName;
            invoice.CustomerNip = customer.NipString;
            invoice.CustomerAddress = customer.Addresses.First(a => a.IsMainAddress).FullAddress;

            return View(invoice);
        }

        private bool InvoicesExists(int id)
        {
            return _context.Invoices.Any(e => e.InvoiceId == id);
        }
    }
}
