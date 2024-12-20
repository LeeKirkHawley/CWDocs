﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CWDocsCore.Services;
using CWDocs.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using NLog;

namespace CWDocs.Controllers {
    public class AccountController : Controller {
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        public static NLog.Logger _logger { get; set; } = LogManager.GetCurrentClassLogger();

        public AccountController(IUserService userService, IAccountService accountService) {
            _userService = userService;
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Login() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password) {


            ClaimsPrincipal principal = _accountService.Login(userName, password);
            if (principal == null) {
                ModelState.AddModelError("", "User not found");
                return View();
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout() {
            _logger.Info($"Logging out user.");

            await HttpContext.SignOutAsync();

            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccessDenied() {
            return View();
        }

        [HttpGet]
        public IActionResult CreateUser() {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string userName, string password) {
            ClaimsPrincipal principal = _accountService.CreateUser(userName, password, "user");
            if (principal == null) {
                ModelState.AddModelError("", "User not found");
                return View();
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("CWDocs", "Home");

        }
    }
}
