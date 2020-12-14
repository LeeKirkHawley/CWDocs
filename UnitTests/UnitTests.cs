using System;
using Xunit;
using CWDocs.Services;
using Moq;
using System.Threading.Tasks;
using CWDocs.Models;

namespace UnitTests {
    public class UnitTests {


        [Fact]
        public async Task Test1() {
            using (var factory = new MemoryDbContextFactory()) {
                using (var context = factory.CreateContext()) {
                    var userService = new UserService(context);
                    User newUser = userService.CreateUser("Aloysius Aardvark", "pwd", "Admin");
                    await context.SaveChangesAsync();

                    User addedUser = userService.GetAllowedUser("Aloysius Aardvark");

                    Assert.Equal("Aloysius Aardvark", addedUser.userName);
                }
            }
        }
    }
}
