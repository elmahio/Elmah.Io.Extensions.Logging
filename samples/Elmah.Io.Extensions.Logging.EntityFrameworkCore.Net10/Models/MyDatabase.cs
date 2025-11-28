using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.Logging.EntityFrameworkCore.Models
{
    public class MyDatabase : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var loggerFactory = new LoggerFactory()
                .AddElmahIo("API_KEY", new Guid("LOG_ID"));

            optionsBuilder
                .UseLoggerFactory(loggerFactory)
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Elmah.Io.Extensions.Logging.EntityFrameworkCore;Trusted_Connection=True;");
        }

        public DbSet<User> Users { get; set; }
    }
}