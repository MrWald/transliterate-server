using System;
using MySql.Data.MySqlClient;
using UnityEngine;

namespace Server
{
    public class DbConnection
    {
        private static readonly string dir = Application.dataPath;
        private DbConnection()
        { }

        public string DatabaseName { get; set; } = string.Empty;

        //public string Password { get; set; }

        public MySqlConnection Connection { get; private set; }

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
                var conString = $"server=127.0.0.1;uid=root;pwd=root;database={DatabaseName}";
                Connection = new MySqlConnection(conString);
                var evolve = new Evolve.Evolve(Connection, msg => ConsoleMessenger.Log(ConsoleMessenger.Prefix.System, msg))
                {
                    Locations = new[] { $"{dir}/db/migrations" },
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