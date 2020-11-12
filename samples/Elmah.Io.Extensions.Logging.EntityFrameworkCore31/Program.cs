using Elmah.Io.Extensions.Logging.EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Elmah.Io.Extensions.Logging.EntityFrameworkCore31
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new MyDatabase())
            {
                db.Users.Add(new User { Name = "Mike Wheeler" });
                db.Users.Add(new User { Name = "Dustin Henderson" });
                db.Users.Add(new User { Name = "Lucas Sinclair" });
                db.Users.Add(new User { Name = "Will Byers" });
                db.Users.Add(new User { Name = "Eleven" });
                db.SaveChanges();

                try
                {
                    db.Users.FromSqlRaw("SELECT * FROM USERS2").ToList(); // Causes an error since there is no table named 'USERS2'
                }
                catch
                {

                }
            }

            Console.ReadLine();
        }
    }
}
