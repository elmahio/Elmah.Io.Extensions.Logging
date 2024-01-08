using Elmah.Io.Extensions.Logging.EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;

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
        // USERS2 exception will be automatically logged
    }
}

Console.ReadLine();