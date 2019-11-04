using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Sozif.Controllers
{
    public class PermissionsController : Controller
    {
        private readonly sozifContext _context;

        public PermissionsController(sozifContext context)
        {
            _context = context;
        }

        // GET: Permissions
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("EditUsers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            var permLevels = await _context.UserPermissions.Include(up => up.Users).ToListAsync();
            return View(permLevels);
        }

        // GET: Permissions/Create
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("EditUsers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            var permissions = await _context.UserPermissions.ToListAsync();
            var lastLevel = permissions.Last().PermLevel;
            ViewBag.Permissions = permissions;
            ViewBag.PermLevel = lastLevel + 1;

            return View();
        }

        // POST: Permissions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PermLevel,PermName,EditUsers,EditProducts,EditCustomers,EditOrders,EditInvoices")] UserPermissions userPermissions)
        {
            if (HttpContext.Session.GetString("EditUsers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (ModelState.IsValid)
            {
                _context.Add(userPermissions);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            var permissions = await _context.UserPermissions.ToListAsync();
            var lastLevel = permissions.Last().PermLevel;
            ViewBag.Permissions = permissions;
            ViewBag.PermLevel = lastLevel + 1;

            return View(userPermissions);
        }

        // GET: Permissions/Edit/5
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

            var userPermissions = await _context.UserPermissions.FindAsync(id);
            if (userPermissions == null)
            {
                return NotFound();
            }
            var permissions = await _context.UserPermissions.ToListAsync();
            ViewBag.Permissions = permissions;

            return View(userPermissions);
        }

        // POST: Permissions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PermLevel,PermName,EditUsers,EditProducts,EditCustomers,EditOrders,EditInvoices")] UserPermissions userPermissions)
        {
            if (HttpContext.Session.GetString("EditUsers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            if (id != userPermissions.PermLevel)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userPermissions);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserPermissionsExists(userPermissions.PermLevel))
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
            var permissions = await _context.UserPermissions.ToListAsync();
            ViewBag.Permissions = permissions;

            return View(userPermissions);
        }

        // GET: Permissions/Delete/5
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
            var userPermissions = await _context.UserPermissions
                .FirstOrDefaultAsync(m => m.PermLevel == id);
            if (userPermissions == null)
            {
                return NotFound();
            }
            var permissions = await _context.UserPermissions.ToListAsync();
            ViewBag.Permissions = permissions;

            return View(userPermissions);
        }

        // POST: Permissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("EditUsers") == "false")
            {
                return RedirectToAction("Index", "Home");
            }
            var userPermissions = await _context.UserPermissions.FindAsync(id);
            _context.UserPermissions.Remove(userPermissions);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserPermissionsExists(int id)
        {
            return _context.UserPermissions.Any(e => e.PermLevel == id);
        }
    }
}
