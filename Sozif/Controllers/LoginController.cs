﻿using Microsoft.AspNetCore.Mvc;
using Sozif.Attributes;
using Sozif.Models;
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

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        // POST: Login/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Username,Password")] LoginDTO loginDTO)
        {
            String message = "";
            Users user = _context.Users.SingleOrDefault(user => user.Username == loginDTO.Username);

            if (user == null)
            {
                message = "Błędna nazwa użytkownika";
            }
            else
            {
                if (loginDTO.Password != user.Password)
                {
                    message = "Błędne hasło";
                }
                else
                {
                    Response.Cookies.Append("AUTH", "OK");
                    Response.Cookies.Append("AUTH_USER", user.Username);
                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.LoginMessage = message;

            return View();
        }
    }

}
