﻿using System;
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
            if (HttpContext.Session.GetString("EditUsers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            return View(await _context.TaxRates.ToListAsync());
        }

        // GET: TaxRates/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("EditUsers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: TaxRates/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaxRateId,Rate")] TaxRates taxRates)
        {
            if (HttpContext.Session.GetString("EditUsers") == "false")
            {
                return RedirectToAction("Index", "Home");
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
            if (HttpContext.Session.GetString("EditUsers") == "false")
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
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TaxRateId,Rate")] TaxRates taxRates)
        {
            if (HttpContext.Session.GetString("EditUsers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id != taxRates.TaxRateId)
            {
                return NotFound();
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
            if (HttpContext.Session.GetString("EditUsers") == "false")
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

            return View(taxRates);
        }

        // POST: TaxRates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("EditUsers") == "false")
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
