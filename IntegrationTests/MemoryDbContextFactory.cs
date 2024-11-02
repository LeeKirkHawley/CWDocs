using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using CWDocsCore;
using Microsoft.Extensions.Configuration;

namespace UnitTests {

    public class MemoryDbContextFactory : IDisposable {
        private DbConnection _connection;
        private readonly IConfiguration _settings;

        public MemoryDbContextFactory(IConfiguration settings)
        {
            _settings = settings;
        }

        private DbContextOptions<CWDocsDbContext> CreateOptions() {
            return new DbContextOptionsBuilder<CWDocsDbContext>()
                .UseSqlite(_connection).Options;
        }

        public CWDocsDbContext CreateContext() {
            if (_connection == null) {
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                var options = CreateOptions();
                using (var context = new CWDocsDbContext(_settings)) {
                    context.Database.EnsureCreated();
                }
            }

            return new CWDocsDbContext(_settings);
        }

        public void Dispose() {
            if (_connection != null) {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
