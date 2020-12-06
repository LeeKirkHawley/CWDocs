using CWDocs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CWDocs.Services {
    public interface IUserService {
        public abstract User GetAllowedUser(string userName);
        public abstract User CreateUser(string userName, string password, string role);
    }
}
