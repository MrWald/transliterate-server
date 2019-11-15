using System.Collections;
using System.Text;
using Manager;
using Server;
using UnityEngine;
using UnityEngine.UI;

namespace Transliterate
{
    public class SendTest : MonoBehaviour
    {
        private TcpClient client;
        private Text log;
        private string text;
    
        // Start is called before the first frame update
        void Start()
        {
            text = "NULL";
            log = GetComponent<Text>();
            client = GetComponent<TcpClient>();
            client.OnConnection += OnConnection;
            client.OnMessage += OnMessage;
        }

        void OnConnection()
        {
            text = "Connected to server";
        }

        void OnMessage(byte[] data)
        {
            var msg = Message.ToMessage(data);
            switch (msg.Type)
            {
                case MessageType.LoginUnsuccessful:
                    text = "Incorrect Password";
                    break;
                case MessageType.LoginSuccessful:
                    var requests = Utils.FromByteArray<ArrayList>(msg.Value);
                    foreach (var request in requests)
                    {
                        text += $"{Utils.FromByteArray<Request>((byte[])request)}\n";
                    }
                    break;
                case MessageType.SaveSuccessful:
                    text = "Saved Successfully";
                    break;
            }
        }

        void Update()
        {
            log.text = text;
        }

        public void Login()
        {
            client.SendingMessage(MessageFactory(MessageType.LoginRequest, Encoding.ASCII.GetBytes("test;test")));
        }

        public void Save()
        {
            client.SendingMessage(MessageFactory(MessageType.SaveRequest, Encoding.ASCII.GetBytes("test;test")));
        }
    
        private static byte[] MessageFactory(MessageType type, byte[] value)
        {
            return new Message { Type = type, Value = value }.ToBytes();
        }
    }
}
