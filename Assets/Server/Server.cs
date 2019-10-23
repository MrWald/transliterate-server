using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    public class Server
    {
        private TcpListener listener;
        private readonly Thread serverThread;
        private Thread readingThread;
        private Thread writingThread;
        private readonly IPAddress multicastIp = IPAddress.Parse("224.0.0.224");
        private const int ServerMulticastPort = 5120;
        private const int ServerNormalPort = 8052;
        private IPEndPoint serverMulticastEndpoint;
        private const string ServerCode = "Server";
        private const string ServerVersion = "1.0.0";
        private readonly List<ClientManager> connectedClients = new List<ClientManager>();
        private readonly Thread broadcastThread;
        private const int MaxConnection = 10;
        public readonly DbConnection DbCon;
        public event OnClientMessage OnMessage;
        public static Server Instance { get; private set; }
        public static Server Init()
        {
            Instance?.Destroy();
            Instance = new Server();
            return Instance;
        }

        private Server()
        {
            if (Instance != null)
            {
                ConsoleMessenger.Log(ConsoleMessenger.Prefix.Error, "Instance Already Created");
                return;
            }
            ConsoleMessenger.Log(ConsoleMessenger.Prefix.Message, "Server Started");
            DbCon = DbConnection.Instance();
            DbCon.DatabaseName = "users";
            serverThread = new Thread(FactoryTcpListener) {Priority = ThreadPriority.AboveNormal };

            broadcastThread = new Thread(delegate ()
            {
                foreach (var seconds in BroadCastAboutServer())
                {
                    Thread.Sleep(seconds * 1000);
                }
            });

            serverThread.Start();
            broadcastThread.Start();
        }
        private void FactoryTcpListener()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, ServerNormalPort);
                listener.Start(MaxConnection);
                for(var i = 0; i < MaxConnection; i++)
                {
                    listener.BeginAcceptTcpClient(OnClientAccepted, null);
                }
                InitClientsThreads();

            }

            catch (Exception e)
            {
                ConsoleMessenger.Log(ConsoleMessenger.Prefix.Error, e.Message);
            }
        }

        private void OnClientAccepted(IAsyncResult ar)
        {
            try
            {
                var connected = listener.EndAcceptTcpClient(ar);
                var client = new ClientManager(connected, this);
                client.OnMessage += OnMessage;
                lock (connectedClients)
                {
                    connectedClients.Add(client);
                    client.OnReading();
                    ConsoleMessenger.Log(ConsoleMessenger.Prefix.Message, "Added new client");
                }
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(SocketException) && ((SocketException)e).ErrorCode == 10004)
                {
                    ConsoleMessenger.Log(ConsoleMessenger.Prefix.Error, e.Message);
                }
                else throw;
            }
        }

        private void InitClientsThreads()
        {
            writingThread = new Thread(delegate ()
            {
                while (true)
                {
                    try
                    {
                        lock (connectedClients)
                        {
                            foreach (var client in connectedClients)
                            {
                                client.OnSending();
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        ConsoleMessenger.Log(ConsoleMessenger.Prefix.Error, e.Message);
                        Thread.Sleep(100);
                    }
                    Thread.Sleep(50);
                }
            });
            writingThread.Start();
        }
        public void CloseConnection(ClientManager clientManager)
        {
            ConsoleMessenger.Log(ConsoleMessenger.Prefix.Message, "Client disconnected");
            lock (connectedClients)
            {
                connectedClients.Remove(clientManager);
            }
            listener.BeginAcceptTcpClient(OnClientAccepted, null);
        }
        private IEnumerable<int> BroadCastAboutServer()
        {
            var client = new UdpClient();
            client.JoinMulticastGroup(multicastIp);
            serverMulticastEndpoint = new IPEndPoint(multicastIp, ServerMulticastPort);
            while (true)
            {
                SendBroadcastMessage(client);
                yield return 1;
            }
        }

        private void SendBroadcastMessage(UdpClient client)
        {
            var buffer = Encoding.ASCII.GetBytes(ServerCode + ":" + ServerVersion);
            client.Send(buffer, buffer.Length, serverMulticastEndpoint);
        }

        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    
        public void Destroy()
        {
            lock (connectedClients)
            {
                connectedClients.ForEach(delegate (ClientManager client) { client.Close(); });
                connectedClients.Clear();
            }
            listener.Stop();
            serverThread.Abort();
            writingThread?.Abort();
            broadcastThread.Abort();
            DbCon.Close();
            Instance = null;
        }
    
    }
}