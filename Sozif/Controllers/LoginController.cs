using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sozif.Models;
using Sozif.Utils;
using System;
using System.Linq;
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

        // GET: Login
        public IActionResult Index()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([Bind("Username,Password")] LoginDTO loginDTO)
        {
            String message = "";
            Users user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDTO.Username);

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
                    HttpContext.Session.SetString("UserId", user.UserId.ToString());
                    HttpContext.Session.SetString("EditUsers", perms.EditUsers ? "true" : "false");
                    HttpContext.Session.SetString("EditProducts", perms.EditProducts ? "true" : "false");
                    HttpContext.Session.SetString("EditCustomers", perms.EditCustomers ? "true" : "false");
                    HttpContext.Session.SetString("EditOrders", perms.EditOrders ? "true" : "false");
                    HttpContext.Session.SetString("EditInvoices", perms.EditInvoices ? "true" : "false");

                    return RedirectToAction("Index", "Orders");
                }
            }
            ViewBag.ErrorMessage = message;

            return View();
        }

        // GET: Login/PasswordChange
        public IActionResult PasswordChange(int? id)
        {
            string userIdString = HttpContext.Session.GetString("UserId");
            if(userIdString == null)
            {
                return NotFound();
            }
            if (id == 0)
            {
                ViewBag.SuccessMessage = "Hasło zostało zmienione.";
            }
            ViewBag.OldPassword = "";
            ViewBag.NewPassword = "";
            ViewBag.NewPasswordConfirmation = "";
            return View();
        }

        // POST: Login/PasswordChange
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasswordChange([Bind("OldPassword,NewPassword,NewPasswordConfirmation")] PasswordChangeDTO passwordChange)
        {
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
            if (passwordChange.OldPassword == null || passwordChange.NewPassword == null || passwordChange.NewPasswordConfirmation == null)
            {
                ViewBag.ErrorMessage = "Musisz wypełnić wszystkie pola!";
                ViewBag.OldPassword = "";
                ViewBag.NewPassword = "";
                ViewBag.NewPasswordConfirmation = "";
                return View();
            }
            ViewBag.OldPassword = passwordChange.OldPassword;
            ViewBag.NewPassword = passwordChange.NewPassword;
            ViewBag.NewPasswordConfirmation = passwordChange.NewPasswordConfirmation;
            string passwordInDB = user.Password;
            if (!PasswordHelper.VerifyHashedPassword(passwordInDB, passwordChange.OldPassword))
            {
                ViewBag.ErrorMessage = "Podałeś błędne hasło!";
                ViewBag.OldPassword = "";
            }
            if (!passwordChange.NewPassword.Any(c => char.IsDigit(c)))
            {
                ViewBag.ErrorMessage = "Hasło musi zawierać przynajmniej jedną cyfrę!";
            }
            if (passwordChange.NewPassword.IndexOfAny("!@#$%^&*?_~-£().,".ToCharArray()) == -1)
            {
                ViewBag.ErrorMessage = "Nowe hasło musi zawierać przynajmniej jeden znak specjalny!";
            }
            if (!passwordChange.NewPassword.Any(c => char.IsUpper(c)))
            {
                ViewBag.ErrorMessage = "Nowe hasło musi zawierać małe i duże litery!";
            }
            if (!passwordChange.NewPassword.Any(c => char.IsLower(c)))
            {
                ViewBag.ErrorMessage = "Nowe hasło musi zawierać małe i duże litery!";
            }
            if (passwordChange.NewPassword.Length < 8)
            {
                ViewBag.ErrorMessage = "Hasło musi zawierać przynajmniej 8 znaków!";
            }
            if (passwordChange.NewPassword != passwordChange.NewPasswordConfirmation)
            {
                ViewBag.ErrorMessage = "Podane hasła różnią się!";
                ViewBag.NewPasswordConfirmation = "";
            }
            if (ViewBag.ErrorMessage == null)
            {
                user.Password = PasswordHelper.HashPassword(passwordChange.NewPassword);
                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("PasswordChange", new { id = 0 });
            }

            return View();
        }

        public IActionResult LogOut()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("AUTH");
            return RedirectToAction("Index");
        }
    }

}
