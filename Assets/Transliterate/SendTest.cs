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
    
        // Start is called before the first frame update
        void Start()
        {
            log = GetComponent<Text>();
            client = GetComponent<TcpClient>();
            client.OnServerMessage += OnMessage;
        }

        void OnMessage(byte[] data)
        {
            var msg = Message.ToMessage(data);
            switch (msg.Type)
            {
                case MessageType.LoginUnsuccessful:
                    log.text = "Incorrect Password";
                    break;
                case MessageType.LoginSuccessful:
                    var requests = Utils.FromByteArray<ArrayList>(msg.Value);
                    foreach (var request in requests)
                    {
                        log.text += $"{Utils.FromByteArray<Request>((byte[])request)}\n";
                    }
                    break;
                case MessageType.SaveSuccessful:
                    log.text = "Saved Successfully";
                    break;
            }
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
            return (new Message() { Type = type, Value = value }).ToBytes();
        }
    }
}
