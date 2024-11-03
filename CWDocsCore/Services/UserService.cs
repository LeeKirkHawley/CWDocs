using System;
using System.Linq;
using CWDocsCore.Models;
using Microsoft.Extensions.Logging;
using NLog;

namespace CWDocsCore.Services {

    public class UserService : IUserService {
        private readonly CWDocsDbContext _context;

        public static NLog.Logger _logger { get; set; } = LogManager.GetCurrentClassLogger();

        public UserService(CWDocsDbContext context) {
            _context = context;
        }

        public UserModel GetAllowedUser(string userName) {
            UserModel user = _context.Users.Where(u => u.userName == userName).FirstOrDefault();
            return user;
        }

        public UserModel CreateUser(string userName, string password, string role) {

            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<UserModel> newuser = _context.Users.Add(new UserModel {
                userName = userName,
                pwd = password,
                role = role
            });

            int changes = _context.SaveChanges();

            return newuser.Entity;
        }

        public bool DeleteUser(UserModel user) {
            try {
                var entity = _context.Users.Remove(user);
                int changes = _context.SaveChanges();
            }
            catch(Exception ex) {
                throw;
            }

            return true;
        }
    }
}
