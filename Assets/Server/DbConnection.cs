using System;
using System.Collections;
using System.Data.SqlClient;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace Server
{
    public class DbConnection : DbContext
    {
        public static string DIR;

        private static readonly string CONNECTION_STRING = "Server=tcp:kmalfa.database.windows.net,1433;Initial Catalog=users;Persist Security Info=False;User ID=boublik;Password=moop11!!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        public DbSet<User> Users { get; set; }
        public DbSet<Request> Requests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(CONNECTION_STRING);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>().HasKey(user => new { user.Users_id });
            builder.Entity<Request>().HasKey(request => new { request.Request_id });
        }
        private DbConnection()
        { 
            Database.EnsureCreated();
            var evolve = new Evolve.Evolve(new SqlConnection(CONNECTION_STRING), msg => ConsoleMessenger.Log(ConsoleMessenger.Prefix.System, msg))
            {
                Locations = new[] { $"{DIR}{Path.DirectorySeparatorChar}db{Path.DirectorySeparatorChar}migrations{Path.DirectorySeparatorChar}" },
                IsEraseDisabled = true,
            };
            evolve.Migrate();
        }

        private static DbConnection _instance;
        public static DbConnection Instance()
        {
            return _instance ?? (_instance = new DbConnection());
        }

        public void SaveRequest(string original, string transliterated, DateTime date, string user)
        {
            var userEntity = Users.SingleOrDefaultAsync(us => us.Login == user).Result;
            Requests.Add(new Request () {Creator_id = userEntity.Users_id, Txt = original, Trans = transliterated, DateOfRequest = date});
            SaveChanges();
        }

        public byte[] GetRequests(string login, string password)
        {
            var userEntity = Users.SingleOrDefaultAsync(user => user.Login == login && user.Password == password).Result;
            if (userEntity == null)
                return null;
            var requests = Requests.ToListAsync().Result.FindAll(request => request.Creator_id == userEntity.Users_id);
            var result = new ArrayList();
            foreach (var request in requests)
            {
                result.Add(Manager.Utils.ToByteArray(new RequestObject(userEntity.Login, request.Txt, request.Trans, request.DateOfRequest)));
            }
            return result.Count==0?new byte[0]:Manager.Utils.ToByteArray(result);
        }

        public bool SignUp (string login, string password)
        {
            var newUser = new User () { Login = login, Password = password };
            var exists = Users.SingleOrDefaultAsync(user => user.Login == login).Result != null;
            if (!exists)
            {
                Users.Add(newUser);
                SaveChanges();
                return true;
            }
            else
                return false;
        }
    }
}