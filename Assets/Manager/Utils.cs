using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Manager
{
    public static class Utils
    {
        private class ReadAsyncState
        {
            public readonly NetworkStream Stream;
            public readonly int TotalLength;
            public readonly byte[] Target;
            public readonly Action<byte[]> Callback;
            public int Read;

            public ReadAsyncState(NetworkStream stream, int totalLength, byte[] target, Action<byte[]> callback)
            {
                Stream = stream;
                TotalLength = totalLength;
                Target = target;
                Callback = callback;
            }
        }
        public static void ReadBytes(NetworkStream stream, Action<byte[]> callback)
        {

            var messageSize = new byte[sizeof(int)];
            stream.Read(messageSize, 0, messageSize.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(messageSize);
            }
            var length = BitConverter.ToInt32(messageSize, 0);
            var message = new byte[length];
            stream.BeginRead(message, 0 , Math.Min(1024 * 16, length), AsyncReadCallback, new ReadAsyncState(stream, length, message, callback));
        }
        private static void AsyncReadCallback(IAsyncResult ar)
        {
            var state = (ReadAsyncState)ar.AsyncState;
            state.Read += state.Stream.EndRead(ar);
            if (state.Read < state.TotalLength)
            {
                state.Stream.BeginRead(state.Target, state.Read, Math.Min(1024 * 16, state.TotalLength - state.Read), AsyncReadCallback, state);
            }
            else state.Callback(state.Target);
        }
        public static byte[] GenerateBytes(byte[] origin)
        {
            var result = new byte[sizeof(int) + origin.Length];
            var lengthInBytes = BitConverter.GetBytes(origin.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lengthInBytes);
            }
            Array.Copy(lengthInBytes, result, lengthInBytes.Length);
            Array.Copy(origin, 0, result, lengthInBytes.Length, origin.Length);
            return result;
        }
        
        public static T FromByteArray<T>(byte[] data)
        {
            if(data == null)
                return default;
            var bf = new BinaryFormatter();
            using(var ms = new MemoryStream(data))
            {
                var obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }
        
        public static byte[] ToByteArray<T>(T obj)
        {
            if(obj == null)
                return null;
            var bf = new BinaryFormatter();
            using(var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
