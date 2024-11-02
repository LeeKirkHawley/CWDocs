using System;
using Xunit;
using CWDocsCore.Services;
using Moq;
using System.Threading.Tasks;
using CWDocsCore.Models;
using CWDocs;
using CWDocsCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests {
    public class UserServiceUnitTests{

        private readonly Mock<IAccountService> _accountService = new Mock<IAccountService>();
        CWDocsDbContext _dBcontext;

        public UserServiceUnitTests() {
            _dBcontext = CreateDbContext();
        }

        CWDocsDbContext CreateDbContext()
        {
            // https://medium.com/@TheLe0/mocking-your-appsettings-in-unit-tests-on-net-cb057de7db64
            var inMemorySettings = new Dictionary<string, string> {
                {"SQLiteDataContext", "CWDocs"},
                {"SQLiteDbPath", "C:\\Work\\A_My_Websites\\CWDocs\\UnitTests\\CWDocs.db"},
                //{"SectionName:SomeKey", "SectionValue"},
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            return new CWDocsDbContext(configuration);
        }

        [Fact]
        public void Should_Create_New_User() {

            UserModel addedUser = GetSut().CreateUser("Fred Boggs", "pwd", "somerole");

            Assert.Equal("Fred Boggs", addedUser.userName);


        }

        //[Fact]
        //public async Task Should_Delete_User() {
        //    var userService = new UserService(_context);
        //    UserModel newUser = userService.CreateUser("Aloysius Aardvark", "pwd", "Admin");
        //    await _context.SaveChangesAsync();

        //    userService.DeleteUser(newUser);
        //    await _context.SaveChangesAsync();

        //    UserModel addedUser = userService.GetAllowedUser("Aloysius Aardvark");
        //    Assert.Null(addedUser);
        //}

        //[Fact]
        //public void Should_Hash_Pwd()
        //{

        //}

        UserService GetSut()
        {
            return new UserService(_dBcontext);
        }
    }
}
