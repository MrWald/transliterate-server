using System;
using System.Runtime.Serialization;

namespace Server
{
    [Serializable]
    public class RequestObject : ISerializable
    {
        public string User { get; }
        public string Text { get; set; }
        public string TransText { get; set; }
        public DateTime Date { get; set; }

        public RequestObject(string user, string text, string transText, DateTime date)
        {
            User = user;
            Text = text;
            TransText = transText;
            Date = date;
        }
        
        //Deserialization constructor.
        public RequestObject(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties
            User = (string)info.GetValue("User", typeof(string));
            Text = (string)info.GetValue("Text", typeof(string));
            TransText = (string)info.GetValue("TransText", typeof(string));
            Date = (DateTime)info.GetValue("Date", typeof(DateTime));
        }
        
        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            //You can use any custom name for your name-value pair. But make sure you
            // read the values with the same name. For ex:- If you write EmpId as "EmployeeId"
            // then you should read the same with "EmployeeId"
            info.AddValue("User", User);
            info.AddValue("Text", Text);
            info.AddValue("TransText", TransText);
            info.AddValue("Date", Date);
        }

        public override string ToString()
        {
            return $"Base: {Text}\nTransliterated: {TransText}\nDate: {Date.ToShortDateString()}\nBy User: {User}";
        }
    }
}