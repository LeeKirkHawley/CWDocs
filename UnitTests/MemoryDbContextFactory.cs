using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CWDocs;
using Microsoft.Data.Sqlite;
using CWDocsCore;

namespace UnitTests {

    public class MemoryDbContextFactory : IDisposable {
        private DbConnection _connection;

        private DbContextOptions<CWDocsDbContext> CreateOptions() {
            return new DbContextOptionsBuilder<CWDocsDbContext>()
                .UseSqlite(_connection).Options;
        }

        public CWDocsDbContext CreateContext() {
            if (_connection == null) {
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                var options = CreateOptions();
                using (var context = new CWDocsDbContext()) {
                    context.Database.EnsureCreated();
                }
            }

            return new CWDocsDbContext();
        }

        public void Dispose() {
            if (_connection != null) {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
