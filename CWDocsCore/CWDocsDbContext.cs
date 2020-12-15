using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CWDocsCore.Models;

namespace CWDocsCore {
    public class CWDocsDbContext : DbContext {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<DocumentModel> Documents { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=C:\\Work\\CWDocs\\CWDocsCore\\CWDocs.db");
    }
}
