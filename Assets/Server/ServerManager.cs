using System;
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
                case MessageType.SignUpRequest:
                    OnSignUpRequest(clientManager, message);
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
            byte[] requests;
            requests = DbConnection.Instance().GetRequests(login, password);
            if (requests != null)
            {
                clientManager.User = login;
                ConsoleMessenger.Log(ConsoleMessenger.Prefix.Message, "Logged in " + login);
                clientManager.SendMessage(MessageFactory(MessageType.LoginSuccessful, requests));
            }
            else 
            {
                clientManager.SendMessage(MessageFactory(MessageType.LoginUnsuccessful, null));
            }
        }

        private void OnSignUpRequest(ClientManager clientManager, Message message)
        {
            var data = Encoding.ASCII.GetString(message.Value).Split(';');
            var login = data[0];
            var password = data[1];
            var success = DbConnection.Instance().SignUp(login, password);
            if (!success)
            {
                clientManager.SendMessage(MessageFactory(MessageType.SignUpUnsuccessful, null));
            }
            else
            {
                clientManager.SendMessage(MessageFactory(MessageType.SignUpSuccessful, null));
            }
        }

        private void OnSaveRequest(ClientManager clientManager, Message message)
        {
            var texts = Encoding.Unicode.GetString(message.Value).Split(';');
            try
            {
                DbConnection.Instance().SaveRequest(texts[0], texts[1], DateTime.Now, clientManager.User);
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
