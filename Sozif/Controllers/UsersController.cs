using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sozif.Attributes;
using Sozif.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sozif.Controllers
{

    [Auth]
    public class UsersController : Controller
    {
        private readonly sozifContext _context;

        public UsersController(sozifContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index(string? userName, string? firstName, string? lastName, int? permLevel)
        {
            if (HttpContext.Session.GetString("EditUsers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            var users = await _context.Users
                .OrderBy(u => u.PermLevel)
                .Include(u => u.PermLevelNavigation)
                .ToListAsync();
            var usersToShow = new List<Users>();
            users.ForEach(u =>
            {
                bool isMatching = true;
                if (userName != null && userName != "" && !u.Username.ToLower().Contains(userName.ToLower()))
                {
                    isMatching = false;
                }
                if (firstName != null && firstName != "" && !u.Firstname.ToLower().Contains(firstName.ToLower()))
                {
                    isMatching = false;
                }
                if (lastName != null && lastName != "" && !u.Lastname.ToLower().Contains(lastName.ToLower()))
                {
                    isMatching = false;
                }
                if (permLevel != null && u.PermLevel != permLevel)
                {
                    isMatching = false;
                }
                if (isMatching)
                {
                    usersToShow.Add(u);
                }
            });
            var permLevels = await _context.UserPermissions.ToListAsync();

            ViewBag.UserName = userName;
            ViewBag.FirstName = firstName;
            ViewBag.LastName = lastName;
            ViewBag.PermLevel = permLevel;
            ViewBag.PermLevels = permLevels;

            return View(usersToShow);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("EditUsers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["PermLevel"] = new SelectList(_context.UserPermissions, "PermLevel", "PermName");
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Username,Password,Firstname,Lastname,PermLevel")] Users users)
        {
            if (HttpContext.Session.GetString("EditUsers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }

            if (users.Username == null || users.Firstname == null || users.Lastname == null || users.Password == null)
            {
                ViewBag.ErrorMessage = "Musisz wypełnić wszystkie pola!";
                ViewData["PermLevel"] = new SelectList(_context.UserPermissions, "PermLevel", "PermName", users.PermLevel);
                return View(users);
            }

            int usersWithSameUsername = await _context.Users.Where(u => u.Username == users.Username).CountAsync();
            if (usersWithSameUsername > 0)
            {
                ViewBag.ErrorMessage = "Istnieje już użytkownik o takiej nazwie!";
                ViewData["PermLevel"] = new SelectList(_context.UserPermissions, "PermLevel", "PermName", users.PermLevel);
                return View(users);
            }

            if (ModelState.IsValid)
            {
                users.Password = PasswordHelper.HashPassword(users.Password);
                _context.Add(users);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PermLevel"] = new SelectList(_context.UserPermissions, "PermLevel", "PermName", users.PermLevel);
            return View(users);
        }

        // GET: Users/Edit/5
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

            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }
            ViewData["PermLevel"] = new SelectList(_context.UserPermissions, "PermLevel", "PermName", users.PermLevel);
            return View(users);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,Password,Firstname,Lastname,PermLevel")] Users users)
        {
            if (HttpContext.Session.GetString("EditUsers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id != users.UserId)
            {
                return NotFound();
            }

            var oldUser = await _context.Users.Where(u => u.UserId == id).AsNoTracking().FirstOrDefaultAsync();

            if (oldUser.Password != users.Password)
            {
                users.Password = PasswordHelper.HashPassword(users.Password);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(users);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsersExists(users.UserId))
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
            ViewData["PermLevel"] = new SelectList(_context.UserPermissions, "PermLevel", "PermName", users.PermLevel);
            return View(users);
        }

        // GET: Users/Delete/5
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

            var users = await _context.Users
                .Include(u => u.PermLevelNavigation)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (users == null)
            {
                return NotFound();
            }

            int usersOrders = await _context.Orders.Where(o => o.UserId == id).CountAsync();
            if (usersOrders > 0)
            {
                ViewBag.ErrorMessage = "Do użytkownika jest przypisanych " + usersOrders + " zamówień. Aby go usunąć musisz wybrać użytkownika, który przejmie te zamówienia!";
            }

            var usersToPassOrders = await _context.Users
                .Include(u => u.PermLevelNavigation)
                .Where(u => u.PermLevelNavigation.EditOrders && u.UserId != id)
                .ToListAsync();

            ViewData["UserId"] = new SelectList(usersToPassOrders, "UserId", "UserData");

            return View(users);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int UserId)
        {
            if (HttpContext.Session.GetString("EditUsers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }

            var invoicesToUpdate = await _context.Invoices.Where(i => i.UserId == id).ToListAsync();
            invoicesToUpdate.ForEach(i => i.UserId = null);
            _context.UpdateRange(invoicesToUpdate);
            await _context.SaveChangesAsync();
            var ordersToUpdate = await _context.Orders.Where(o => o.UserId == id).ToListAsync();
            ordersToUpdate.ForEach(o => o.UserId = UserId);
            _context.UpdateRange(ordersToUpdate);
            await _context.SaveChangesAsync();
            var users = await _context.Users.FindAsync(id);
            _context.Users.Remove(users);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsersExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
