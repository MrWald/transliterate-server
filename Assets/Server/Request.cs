using System;

namespace Server
{
    public class Request
    {
        public string User { get; }
        public string Text { get; }
        public string TransText { get; }
        public DateTime Date { get; }

        public Request(string user, string text, string transText, DateTime date)
        {
            User = user;
            Text = text;
            TransText = transText;
            Date = date;
        }

        public override string ToString()
        {
            return $"Base: {Text}\nTransliterated: {TransText}\nDate: {Date.ToShortDateString()}\nBy User: {User}";
        }
    }
}