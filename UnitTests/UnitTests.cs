using System;
using Xunit;
using CWDocs.Services;
using Moq;
using System.Threading.Tasks;
using CWDocs.Models;
using CWDocs;

namespace UnitTests {
    public class UnitTests : IDisposable{

        CWDocsDbContext _context;
        private readonly MemoryDbContextFactory _contextFactory;

        public UnitTests() {
            using (var factory = new MemoryDbContextFactory()) {
                _context = factory.CreateContext();
            }
        }

        public void Dispose() {
            _context.Dispose();
        }

        [Fact]
        public async Task Test1() {
            var userService = new UserService(_context);
            User newUser = userService.CreateUser("Aloysius Aardvark", "pwd", "Admin");
            await _context.SaveChangesAsync();

            User addedUser = userService.GetAllowedUser("Aloysius Aardvark");

            Assert.Equal("Aloysius Aardvark", addedUser.userName);
        }
    }
}
