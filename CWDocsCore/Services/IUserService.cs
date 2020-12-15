using CWDocsCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CWDocsCore.Services {
    public interface IUserService {
        public abstract UserModel GetAllowedUser(string userName);
        public abstract UserModel CreateUser(string userName, string password, string role);
    }
}
