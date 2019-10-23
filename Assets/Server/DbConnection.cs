using MySql.Data.MySqlClient;

namespace Server
{
    public class DbConnection
    {
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
            if (Connection != null) return true;
            if (string.IsNullOrEmpty(DatabaseName))
                return false;
            var conString = $"server=127.0.0.1;uid=root;pwd=root;database={DatabaseName}";
            Connection = new MySqlConnection(conString);
            Connection.Open();
            ConsoleMessenger.Log(ConsoleMessenger.Prefix.Message, "Connected to Database");
            return true;
        }

        public void Close()
        {
            Connection?.Close();
        }   
    }
}