using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Manager
{
    public class TcpClient : MonoBehaviour {
        private System.Net.Sockets.TcpClient client;
        private Thread clientThread;
        private IPEndPoint lookUpperEndpoint;
        private UdpClient lookUpper;
        private const int LookupPort = 5120;
        private const int ServerPort = 8052;
        private readonly IPAddress multicastIp = IPAddress.Parse("224.0.0.224");
        private const string ServerCode = "Server";
        private const string ServerVersion = "1.0.0";
        public event ConnectedToServer OnConnection;
        public event OnServerMessage OnMessage;

        private void Start ()
        {
            InitLookUpper();
        }

        private void InitLookUpper()
        {
            OnDestroy();
            clientThread = new Thread(FindServerAndConnect);
            clientThread.Start();
        }
        private void OnDestroy()
        {
            client?.Close();
            lookUpper?.Close();
            clientThread?.Abort();
        }
        private void FindServerAndConnect()
        {
            lookUpperEndpoint = new IPEndPoint(IPAddress.Any, LookupPort);
            lookUpper = new UdpClient(lookUpperEndpoint);
            lookUpper.JoinMulticastGroup(multicastIp);
            lookUpper.BeginReceive(ParseServerIp, null);

        }
        private void ParseServerIp(IAsyncResult ar)
        {
            var bytes = lookUpper.EndReceive(ar, ref lookUpperEndpoint);
            var serverType = Encoding.ASCII.GetString(bytes);
            if (serverType == (ServerCode + ':' + ServerVersion))
            {
                var communicationEndpoint = new IPEndPoint(lookUpperEndpoint.Address, ServerPort);
                lookUpper.Close();
                ConnectToServer(communicationEndpoint);
            }
            else
            {
                lookUpper.BeginReceive(ParseServerIp, null);
            }
        }

        private void ConnectToServer(IPEndPoint endpoint)
        {
            try
            {
                client = new System.Net.Sockets.TcpClient();
                client.Connect(endpoint);
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
            }

            OnConnection?.Invoke();
            ReadServerMessage();
        }

        private void ReadServerMessage()
        {
            NetworkStream stream;
            if (!(stream = client.GetStream()).CanRead) return;
            try
            {
                Utils.ReadBytes(stream, delegate (byte[] bytes)
                {
                    OnMessage?.Invoke(bytes);
                    Thread.Sleep(100);

                    ReadServerMessage();
                });
            }
            catch (Exception e)
            {
                Debug.Log("Error: " + e);
                InitLookUpper();
            }
        }

        public void SendingMessage(byte[] message)
        {
            NetworkStream stream = null;
            if (client == null || !client.Connected) return;
            try
            {
                stream = client.GetStream();
                if (!stream.CanWrite) return;
                var bytes = Utils.GenerateBytes(message);
                stream.Write(bytes, 0, bytes.Length);
            }
            catch(SocketException e)
            {
                stream?.Close();
                Debug.Log("Error: " + e);
                InitLookUpper();
            }
        }

    }
    public delegate void ConnectedToServer();
    public delegate void OnServerMessage(byte[] bytes);
}