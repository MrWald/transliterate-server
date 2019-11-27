using System;
using System.Collections;
using System.Data.SqlClient;
using System.IO;

namespace Server
{
    public class DbConnection
    {
        public static string DIR;
        private DbConnection()
        { }

        public string DatabaseName { get; set; } = string.Empty;

        public string Password { get; set; }

        public SqlConnection Connection { get; private set; }

        private static DbConnection _instance;
        public static DbConnection Instance()
        {
            return _instance ?? (_instance = new DbConnection());
        }

        public void SaveRequest(string original, string transliterated, string date, string user)
        {
            var query = $"SELECT users_id FROM users WHERE username = '{user}'";
            var cmd = new SqlCommand(query, Connection);
            var reader = cmd.ExecuteReader();
            reader.Read();
            query = $"INSERT INTO requests(txt, trans, dateOfRequest, creator_id) VALUES (N'{original}', '{transliterated}', '{date}', {reader.GetInt32(0)})";
            reader.Close();
            cmd = new SqlCommand(query, Connection);
            cmd.ExecuteNonQuery();
        }

        public byte[] GetRequests(string login, string password)
        {
            var query = $"SELECT * FROM users WHERE login = '{login}' AND password = '{password}'";
            var cmd = new SqlCommand(query, Connection);
            var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                reader.Close();
                return null;
            }
            reader.Close();
            query = $"SELECT * FROM requests WHERE creator_id = '{login}'";
            cmd = new SqlCommand(query, Connection);
            reader = cmd.ExecuteReader();
            var requests = new ArrayList();
            while(reader.Read())
                requests.Add(Manager.Utils.ToByteArray(new Request(reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetDateTime(0))));
            reader.Close();
            var bytes = requests.Count==0?new byte[0]:Manager.Utils.ToByteArray(requests);
            return bytes;
        }

        public bool SignUp (string login, string password)
        {
            var query = $"SELECT * FROM users WHERE login = '{login}'";
            var cmd = new SqlCommand(query, Connection);
            var reader = cmd.ExecuteReader();
            var res = reader.Read();
            reader.Close();
            if (res)
                return false;
            query = $"INSERT INTO users VALUES ('{login}', '{password}')";
            cmd = new SqlCommand(query, Connection);
            cmd.ExecuteNonQuery();
            return true;
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
                var conString = $"Server=tcp:kmalfa.database.windows.net,1433;Initial Catalog={DatabaseName};Persist Security Info=False;User ID=boublik;Password={Password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
                Connection = new SqlConnection(conString);
                var evolve = new Evolve.Evolve(Connection, msg => ConsoleMessenger.Log(ConsoleMessenger.Prefix.System, msg))
                {
                    Locations = new[] { $"{DIR}{Path.DirectorySeparatorChar}db{Path.DirectorySeparatorChar}migrations{Path.DirectorySeparatorChar}" },
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