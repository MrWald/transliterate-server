using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Manager;
using TcpClient = System.Net.Sockets.TcpClient;

namespace Server
{
    public class ClientManager
    {
        public Queue<byte[]> Queue { get; }
        private readonly TcpClient receiver;
        private readonly Server server;
        public string User;
        public event OnClientMessage OnMessage;
        
        public ClientManager(TcpClient client, Server server)
        {
            receiver = client;
            this.server = server;
            Queue = new Queue<byte[]>();
        }
        public void SendMessage(byte[] message)
        {
            Queue.Enqueue(message);
        }
        public void OnSending()
        {
            if (!receiver.Connected) return;
            if (Queue.Count <= 0) return;
            try
            {
                var stream = receiver.GetStream();
                var bytes = Utils.GenerateBytes(Queue.Dequeue());
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
            catch(SocketException e)
            {
                ConsoleMessenger.Log(ConsoleMessenger.Prefix.Error, "Client was disconnected - " + e.Message);
                Close();
            }

        }

        public void OnReading()
        {
            if (receiver.Connected)
            {
                var stream = receiver.GetStream();
                try
                {

                    Utils.ReadBytes(stream, delegate (byte[] message)
                    {
                        OnMessage?.Invoke(this, message);
                        OnReading();
                        stream.Flush();
                    });
                }
                catch (Exception e)
                {
                    ConsoleMessenger.Log(ConsoleMessenger.Prefix.Error, "Close connection, because - " + e.Message);
                    Close();
                }
            

            }
            else
            {
                Close();
            }
        }
        public void Close(bool update = true)
        {
            receiver.Close();
            if(update) server.CloseConnection(this);
        }
    }
    public delegate void OnClientMessage(ClientManager clientManager, byte[] message);
}