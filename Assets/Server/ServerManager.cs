﻿using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Server
{
    public class ServerManager
    {
        private Server server;
        
        public async void Start()
        {
            await Task.Run(() => {
                server = Server.Init();
                server.OnMessage += ServerOnMessage;
            });
        }
        public void Stop()
        {
            server?.Destroy();
        }
        
        private void ServerOnMessage(ClientManager clientManager, byte[] data)
        {
            ConsoleMessenger.Log(ConsoleMessenger.Prefix.Message, "Received a request");
            var message = Message.ToMessage(data);
            switch (message.Type)
            {
                case MessageType.LoginRequest:
                    OnLoginRequest(clientManager, message);
                    break;
                case MessageType.SaveRequest:
                    OnSaveRequest(clientManager, message);
                    break;
                default:
                    ConsoleMessenger.Log(ConsoleMessenger.Prefix.Error, "No implementation for case " + message.Type);
                    break;
            }
        }
        private void OnLoginRequest(ClientManager clientManager, Message message)
        {
            var data = Encoding.ASCII.GetString(message.Value).Split(';');
            var login = data[0];
            var password = data[1];
            var requests = GetRequests(clientManager, login, password);
            clientManager.SendMessage(requests == null
                ? MessageFactory(MessageType.LoginUnsuccessful, null)
                : MessageFactory(MessageType.LoginSuccessful, requests));
        }
        
        private byte[] GetRequests(ClientManager clientManager, string login, string password)
        {
            if (!server.DbCon.IsConnect())
            {
                ConsoleMessenger.Log(ConsoleMessenger.Prefix.Error, "Cannot connect to DB");
                return null;
            }
            var query = $"SELECT * FROM users WHERE login = '{login}'";
            var cmd = new SqlCommand(query, server.DbCon.Connection);
            var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                reader.Close();
                query = $"INSERT INTO users VALUES ('{login}', '{password}')";
                cmd = new SqlCommand(query, server.DbCon.Connection);
                cmd.ExecuteNonQuery();
            }
            else
            {
                reader.Close();
                query = $"SELECT * FROM users WHERE login = '{login}' AND password = '{password}'";
                cmd = new SqlCommand(query, server.DbCon.Connection);
                reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    reader.Close();
                    return null;
                }
            }
            reader.Close();
            ConsoleMessenger.Log(ConsoleMessenger.Prefix.Message, "Logged in " + login);
            clientManager.User = login;
            query = $"SELECT * FROM requests WHERE username = '{login}'";
            cmd = new SqlCommand(query, server.DbCon.Connection);
            reader = cmd.ExecuteReader();
            var requests = new ArrayList();
            while(reader.Read())
                requests.Add(Manager.Utils.ToByteArray(new Request(reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetDateTime(0))));
            reader.Close();
            var bytes = requests.Count==0?new byte[0]:Manager.Utils.ToByteArray(requests);
            return bytes;
        }

        private void OnSaveRequest(ClientManager clientManager, Message message)
        {
            var texts = Encoding.Unicode.GetString(message.Value).Split(';');
            var query = $"INSERT INTO requests(txt, trans, dateOfRequest, username) VALUES ('{texts[0]}', '{texts[1]}', '{DateTime.Now.ToString("yyyyMMdd HH:mm:ss", CultureInfo.InvariantCulture)}', '{clientManager.User}')";
            var cmd = new SqlCommand(query, server.DbCon.Connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                ConsoleMessenger.Log(ConsoleMessenger.Prefix.Error, "Couldn't save changes\n" + e.Message); 
                clientManager.SendMessage(MessageFactory(MessageType.SaveUnsuccessful, null));
                return;
            }
            ConsoleMessenger.Log(ConsoleMessenger.Prefix.Message, "Saved new request"); 
            clientManager.SendMessage(MessageFactory(MessageType.SaveSuccessful, null));
        }
        
        private static byte[] MessageFactory(MessageType type, byte[] value)
        {
            return (new Message() { Type = type, Value = value }).ToBytes();
        }
    }
}
