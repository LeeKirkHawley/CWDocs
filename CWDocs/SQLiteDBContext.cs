using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CWDocs.Models;

namespace CWDocs {
    public class SQLiteDBContext : DbContext {
        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=CWDocs.db");
    }
}
