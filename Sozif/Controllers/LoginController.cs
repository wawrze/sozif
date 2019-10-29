using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sozif.Attributes;
using Sozif.Models;
using Sozif.Utils;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Sozif.Controllers
{
    public class LoginController : Controller
    {
        private readonly sozifContext _context;

        public LoginController(sozifContext context)
        {
            _context = context;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            ViewBag.HideMenu = true;
            return View();
        }

        // POST: Login/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login([Bind("Username,Password")] LoginDTO loginDTO)
        {
            String message = "";
            Users user = _context.Users.SingleOrDefault(user => user.Username == loginDTO.Username);

            if (user == null)
            {
                message = "Błędna nazwa użytkownika";
            }
            else
            {
                if (!PasswordHelper.VerifyHashedPassword(user.Password, loginDTO.Password))
                {
                    message = "Błędne hasło";
                }
                else
                {
                    UserPermissions perms = _context.UserPermissions.Find(user.PermLevel);
                    string token = Guid.NewGuid().ToString();
                    Response.Cookies.Append("AUTH", token);
                    HttpContext.Session.SetString("AUTH", token);
                    HttpContext.Session.SetString("EditUsers", perms.EditUsers ? "true" : "false");
                    HttpContext.Session.SetString("EditProducts", perms.EditProducts ? "true" : "false");
                    HttpContext.Session.SetString("EditCustomers", perms.EditCustomers ? "true" : "false");
                    HttpContext.Session.SetString("EditOrders", perms.EditOrders ? "true" : "false");
                    HttpContext.Session.SetString("EditInvoices", perms.EditInvoices ? "true" : "false");

                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.LoginMessage = message;

            return View();
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("AUTH");
            Response.Cookies.Delete("AUTH_USER");
            return RedirectToAction("Index");
        }
    }

}
