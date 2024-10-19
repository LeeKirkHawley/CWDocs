using Microsoft.EntityFrameworkCore;
using CWDocsCore.Models;
using Microsoft.Extensions.Configuration;

namespace CWDocsCore {
    public class CWDocsDbContext : DbContext {
        private readonly IConfiguration _settings;


        public DbSet<UserModel> Users { get; set; }
        public DbSet<DocumentModel> Documents { get; set; }

        public CWDocsDbContext(IConfiguration settings)
        {
            _settings = settings;
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //    => options.UseSqlite("Data Source=C:\\Work\\CWDocs\\CWDocsCore\\CWDocs.db");
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string sqLiteDbPath = _settings["SQLiteDbPath"];
            options.UseSqlite($"Data Source={sqLiteDbPath}");
        }
    }
}
