using System;

namespace Server
{
    public class Message
    {
        public MessageType Type { get; set; }
        public byte[] Value { get; set; }
        public byte[] ToBytes()
        {
            var data = new byte[(Value?.Length ?? 0) + 1];
            data[0] = (byte)Type;
            if(Value != null)
                Array.Copy(Value, 0, data, 1, Value.Length);
            return data;
        }
        public static Message ToMessage(byte[] data)
        {

            var copy = new byte[data.Length - 1 < 0? 0: data.Length - 1];
            if (data.Length > 1)
                Array.Copy(data, 1, copy, 0, copy.Length);
            else copy = new byte[] { };
            return new Message() { Type = (MessageType)data[0], Value = copy };
        }
    }
    public enum MessageType
    {
        LoginRequest,
        LoginSuccessful,
        LoginUnsuccessful,
        SignUpRequest,
        SignUpSuccessful,
        SignUpUnsuccessful,
        SaveRequest,
        SaveSuccessful,
        SaveUnsuccessful
    }
}