using System;
using System.Data.SqlClient;
using UnityEngine;
using System.IO;

namespace Server
{
    public class DbConnection
    {
        private static readonly string dir = Application.dataPath;
        private DbConnection()
        { }

        public string DatabaseName { get; set; } = string.Empty;

        //public string Password { get; set; }

        public SqlConnection Connection { get; private set; }

        private static DbConnection _instance;
        public static DbConnection Instance()
        {
            return _instance ?? (_instance = new DbConnection());
        }

        public bool IsConnect()
        {
            if (Connection != null) 
            {
                return true;
            }
            if (string.IsNullOrEmpty(DatabaseName))
                return false;
            try
            {
                var conString = $"Server=tcp:kmalfa.database.windows.net,1433;Initial Catalog={DatabaseName};Persist Security Info=False;User ID=boublik;Password=moop11!!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
                Connection = new SqlConnection(conString);
                var evolve = new Evolve.Evolve(Connection, msg => ConsoleMessenger.Log(ConsoleMessenger.Prefix.System, msg))
                {
                    Locations = new[] { $"{dir}" + Path.DirectorySeparatorChar + "db" + Path.DirectorySeparatorChar + "migrations" },
                    IsEraseDisabled = true,
                };

                evolve.Migrate();
            }
            catch (Exception ex)
            {
                ConsoleMessenger.Log(ConsoleMessenger.Prefix.Error, $"Database migration failed.{ex}");
                throw;
            }
            
            Connection.Open();
            ConsoleMessenger.Log(ConsoleMessenger.Prefix.Message, "Connected to Database");
            return true;
        }

        public void Close()
        {
            Connection?.Close();
            Connection = null;
        }   
    }
}