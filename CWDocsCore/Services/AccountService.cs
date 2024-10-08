﻿using CWDocsCore;
using CWDocsCore.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CWDocsCore.Services {
    public class AccountService : IAccountService {

        private readonly CWDocsDbContext _context;
        private readonly IUserService _userService;

        public AccountService(CWDocsDbContext context, IUserService userService) {
            _context = context;
            _userService = userService;
        }

        public ClaimsPrincipal Login(string userName, string password) {

            UserModel user = _userService.GetAllowedUser(userName);
            if (user == null) {
                return null;
            }

            if (user.pwd != password) {
                return null;
            }

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.userName));

            //foreach (var role in user.Roles) {
            //    identity.AddClaim(new Claim(ClaimTypes.Role, role.Role));
            //}
            identity.AddClaim(new Claim(ClaimTypes.Role, user.role));

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            return principal;
        }

        public ClaimsPrincipal CreateUser(string userName, string password, string role) {

            UserModel user = _userService.CreateUser(userName, password, role);
            if (user == null) {
                return null;
            }

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.userName));
            //identity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
            //identity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));

            //foreach (var role in user.Roles) {
            //    identity.AddClaim(new Claim(ClaimTypes.Role, role.Role));
            //}
            identity.AddClaim(new Claim(ClaimTypes.Role, user.role));

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            return principal;
        }

    }
}
