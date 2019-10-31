using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sozif;
using Sozif.Attributes;

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
            var sozifContext = _context.Invoices.Include(i => i.InvoicePositions);
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

        // GET: Invoices/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("EditInvoices") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Firstname");
            return View();
        }

        // POST: Invoices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InvoiceId,InvoiceNumber,CustomerName,CustomerNip,CustomerAddress,CustomerPostalCode,CustomerCity,InvoiceDate,DaysToPay,UserName,CustomerId,UserId")] Invoices invoices)
        {
            if (HttpContext.Session.GetString("EditInvoices") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                _context.Add(invoices);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "CustomerName", invoices.CustomerId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Firstname", invoices.UserId);
            return View(invoices);
        }

        private bool InvoicesExists(int id)
        {
            return _context.Invoices.Any(e => e.InvoiceId == id);
        }
    }
}
