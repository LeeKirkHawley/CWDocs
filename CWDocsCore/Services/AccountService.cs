using CWDocsCore.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace CWDocsCore.Services {
    public class AccountService : IAccountService {

        private readonly CWDocsDbContext _context;
        private readonly IUserService _userService;
        private readonly ILogger<AccountService> _logger;

        public AccountService(CWDocsDbContext context, IUserService userService, ILogger<AccountService> logger) {
            _context = context;
            _userService = userService;
            _logger = logger;
        }

        public ClaimsPrincipal Login(string userName, string password) 
        {
            _logger.LogInformation($"Logging in user {userName}");

            UserModel user = _userService.GetAllowedUser(userName);
            if (user == null) {
                return null;
            }

            if (user.pwd != password) {
                return null;
            }

            ClaimsIdentity identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.userName));

            //foreach (var role in user.Roles) {
            //    identity.AddClaim(new Claim(ClaimTypes.Role, role.Role));
            //}
            identity.AddClaim(new Claim(ClaimTypes.Role, user.role));

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            return principal;
        }

        public ClaimsPrincipal CreateUser(string userName, string password, string role) {

            //password = HashPassword(password)
            
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

        private string HashPassword(string password) 
        {
            return password;
        }

    }
}
