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
        public async Task<IActionResult> Index(
            string? invoice,
            DateTime? invoiceFrom,
            DateTime? invoiceTo,
            string? customer,
            string? nip,
            string? address,
            int? positionsFrom,
            int? positionsTo,
            double? netFrom,
            double? netTo,
            double? taxFrom,
            double? taxTo,
            double? grossFrom,
            double? grossTo,
            DateTime? paymentFrom,
            DateTime? paymentTo,
            string? user
        )
        {
            var invoices = await _context.Invoices
                .Include(i => i.InvoicePositions)
                .OrderByDescending(i => i.InvoiceId)
                .ToListAsync();
            var invoicesToShow = new List<Invoices>();
            invoices.ForEach(i =>
            {
                bool isMatching = true;
                if (invoice != null && invoice != "" && !i.InvoiceNumber.ToLower().Contains(invoice.ToLower()))
                {
                    isMatching = false;
                }
                if (invoiceFrom != null && i.InvoiceDate < invoiceFrom)
                {
                    isMatching = false;
                }
                if (invoiceTo != null && i.InvoiceDate > invoiceTo)
                {
                    isMatching = false;
                }
                if (customer != null && customer != "" && !i.CustomerName.ToLower().Contains(customer.ToLower()))
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
                    if (!i.CustomerNip.ToString().Contains(justNumber))
                    {
                        isMatching = false;
                    }
                }
                if (address != null && address != "" && !i.CustomerAddress.ToLower().Contains(address.ToLower()))
                {
                    isMatching = false;
                }
                if (positionsFrom != null && i.PositionsCount < positionsFrom)
                {
                    isMatching = false;
                }
                if (positionsTo != null && i.PositionsCount > positionsTo)
                {
                    isMatching = false;
                }
                if (netFrom != null && i.NetValue < netFrom * 100)
                {
                    isMatching = false;
                }
                if (netTo != null && i.NetValue > netTo * 100)
                {
                    isMatching = false;
                }
                if (taxFrom != null && i.TaxValue < taxFrom * 100)
                {
                    isMatching = false;
                }
                if (taxTo != null && i.TaxValue > taxTo * 100)
                {
                    isMatching = false;
                }
                if (grossFrom != null && i.GrossValue < grossFrom * 100)
                {
                    isMatching = false;
                }
                if (grossTo != null && i.GrossValue > grossTo * 100)
                {
                    isMatching = false;
                }
                if (paymentFrom != null && i.InvoiceDate.AddDays(i.DaysToPay) < paymentFrom)
                {
                    isMatching = false;
                }
                if (paymentTo != null && i.InvoiceDate.AddDays(i.DaysToPay) > paymentTo)
                {
                    isMatching = false;
                }
                if (user != null && user != "" && !i.UserName.ToLower().Contains(user.ToLower()))
                {
                    isMatching = false;
                }
                if (isMatching)
                {
                    invoicesToShow.Add(i);
                }
            });
            ViewBag.Invoice = invoice;
            ViewBag.InvoiceFrom = invoiceFrom?.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            ViewBag.InvoiceTo = invoiceTo?.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            ViewBag.Customer = customer;
            ViewBag.Nip = nip;
            ViewBag.Address = address;
            ViewBag.PositionsFrom = positionsFrom;
            ViewBag.PositionsTo = positionsTo;
            ViewBag.NetFrom = netFrom.ToString().Replace(',', '.');
            ViewBag.NetTo = netTo.ToString().Replace(',', '.');
            ViewBag.TaxFrom = taxFrom.ToString().Replace(',', '.');
            ViewBag.TaxTo = taxTo.ToString().Replace(',', '.');
            ViewBag.GrossFrom = grossFrom.ToString().Replace(',', '.');
            ViewBag.GrossTo = grossTo.ToString().Replace(',', '.');
            ViewBag.PaymentFrom = paymentFrom?.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            ViewBag.PaymentTo = paymentTo?.ToString("yyyy-MM-dd", DateTimeFormatInfo.InvariantInfo);
            ViewBag.User = user;

            return View(invoicesToShow);
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
                .Where(o => !o.OrderNumber.Contains("R") && o.RealisationDate != null)
                .Include(o => o.Address)
                .Include(o => o.User)
                .Include(o => o.OrderPositions)
                    .ThenInclude(op => op.Product)
                    .ThenInclude(p => p.TaxRate)
                .Where(o => o.CustomerId == id && o.InvoiceId == null && o.RealisationDate != null)
                .ToListAsync();
            InvoiceDTO invoice = new InvoiceDTO();
            invoice.CustomerName = customer.CustomerName;
            invoice.CustomerNip = customer.NipString;
            invoice.CustomerAddress = customer.Addresses.First(a => a.IsMainAddress).FullAddress;
            ordersToChooseFrom.ForEach(o => invoice.Orders.Add(new KeyValuePair<int, bool>(o.OrderId, false)));
            ViewBag.Orders = ordersToChooseFrom;

            return View(invoice);
        }

        // GET: Invoices/FromOrder?customerId=5&orderId=5
        public async Task<IActionResult> FromOrder(int? customerId, int? orderId)
        {
            if (HttpContext.Session.GetString("EditInvoices") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (orderId == null || customerId == null)
            {
                return NotFound();
            }
            var customer = await _context.Customers.Include(c => c.Addresses).Where(c => c.CustomerId == customerId).FirstOrDefaultAsync();
            if (customer == null)
            {
                return NotFound();
            }
            var newInvoice = new InvoiceDTO();
            newInvoice.CustomerName = customer.CustomerName;
            newInvoice.CustomerNip = customer.NipString;
            newInvoice.CustomerAddress = customer.Addresses.First(a => a.IsMainAddress).FullAddress;
            var customerAllOrders = await _context.Orders
                .Where(o => o.CustomerId == customerId && o.RealisationDate != null)
                .ToListAsync();
            customerAllOrders.ForEach(o => newInvoice.Orders.Add(new KeyValuePair<int, bool>(o.OrderId, o.OrderId == orderId)));

            return await ChooseOrders(customerId, "order", newInvoice);
        }


        // GET: Invoices/ChooseOrders/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChooseOrders(int? id, string? from, [Bind("Orders")] InvoiceDTO invoice)
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
            if (from != null)
            {
                ViewBag.From = from;
            }
            var ordersToInvoiceIds = new List<int>();
            invoice.Orders.ForEach(o => { if (o.Value) ordersToInvoiceIds.Add(o.Key); });
            if (ordersToInvoiceIds.Count == 0)
            {
                ViewBag.ErrorMessage = "Musisz wybrać przynajmniej jedno zamówienie, do którego chcesz wystawić fakturę!";
            }
            if (ViewBag.ErrorMessage == null && ViewBag.From == null)
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
                ordersToInvoice.ForEach(o => o.InvoiceId = newInvoice.InvoiceId);
                _context.UpdateRange(ordersToInvoice);
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
                .Where(o => o.CustomerId == id && o.InvoiceId == null && o.RealisationDate != null)
                .ToListAsync();
            ViewBag.Orders = ordersToChooseFrom;
            if (invoice.Orders.Count == 0)
            {
                ordersToChooseFrom.ForEach(o =>
                {
                    invoice.Orders.Add(new KeyValuePair<int, bool>(o.OrderId, false));
                });
            }
            invoice.CustomerName = customer.CustomerName;
            invoice.CustomerNip = customer.NipString;
            invoice.CustomerAddress = customer.Addresses.First(a => a.IsMainAddress).FullAddress;

            return View("ChooseOrders", invoice);
        }

        private bool InvoicesExists(int id)
        {
            return _context.Invoices.Any(e => e.InvoiceId == id);
        }
    }
}
