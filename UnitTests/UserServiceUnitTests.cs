using Xunit;
using CWDocsCore.Services;
using Moq;
using CWDocsCore.Models;
using CWDocsCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace UnitTests {
    public class UserServiceUnitTests{

        private readonly IConfiguration _configuration;
        private readonly Mock<ILogger<UserService>> _logger = new Mock<ILogger<UserService>>();
        CWDocsDbContext _dBcontext;

        public UserServiceUnitTests() {
            _configuration = CreateConfiguration();
            _dBcontext = CreateDbContext();
        }

        [Fact]
        public void Should_Create_New_User() {
            UserService sut = GetSut();

            UserModel addedUser = sut.CreateUser("Fred Boggs", "pwd", "somerole");

            Assert.Equal("Fred Boggs", addedUser.userName);
            bool success = sut.DeleteUser(addedUser);
        }

        [Fact]
        public void Should_Delete_User()
        {
            UserService sut = GetSut();
            UserModel addedUser = sut.CreateUser("Fred Boggs", "pwd", "somerole");

            bool success = sut.DeleteUser(addedUser);

            UserModel deletedUser = sut.GetAllowedUser("Fred Boggs");
            Assert.True(success);
            Assert.Null(deletedUser);
        }

        private IConfiguration CreateConfiguration()
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

            return configuration;
        }

        private CWDocsDbContext CreateDbContext()
        {
            return new CWDocsDbContext(_configuration);
        }

        UserService GetSut()
        {
            return new UserService(_dBcontext, _logger.Object);
        }
    }
}
