using Xunit;
using CWDocsCore.Services;
using Moq;
using CWDocsCore.Models;
using CWDocsCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace UnitTests
{
    public class AccountServiceUnitTests
    {

        private readonly Mock<IUserService> _userService = new Mock<IUserService>();
        private readonly IConfiguration _configuration;
        private readonly Mock<ILogger> _logger = new Mock<ILogger>();
        CWDocsDbContext _dBcontext;

        public AccountServiceUnitTests()
        {
            _configuration = CreateConfiguration();
            _dBcontext = CreateDbContext();
        }

        [Fact]
        public void Should_Login()
        {
            _userService.Setup(s => s.GetAllowedUser(It.IsAny<string>()))
                .Returns(new UserModel { Id = 1, userName = "Kirk", pwd = "pwd" , role = "somerole"});

            ClaimsPrincipal claimsPrincipal = GetSut().Login("Kirk", "pwd");

            Assert.NotNull(claimsPrincipal);
            Assert.Equal("Kirk", claimsPrincipal.Identity.Name);
            Assert.True(claimsPrincipal.IsInRole("somerole"));
        }

        //[Fact]
        //public void Should_Delete_User()
        //{
        //    UserService sut = GetSut();
        //    UserModel addedUser = GetSut().CreateUser("Fred Boggs", "pwd", "somerole");

        //    bool success = sut.DeleteUser(addedUser);

        //    Assert.True(success);
        //}

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

        AccountService GetSut()
        {
            return new AccountService(_dBcontext, _userService.Object, _logger.Object);
        }
    }
}
