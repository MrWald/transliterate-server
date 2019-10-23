using System.Collections.Generic;
using UnityEngine;

namespace Server
{
    public static class ConsoleMessenger
    {
        public static bool[] ShowPrefixes = { true, true, true, true, true };

        private const int Limit = 50;

        private static readonly Color MessageColor = Color.green;
        private static readonly Color WarningColor = Color.yellow;
        private static readonly Color ErrorColor = Color.red;
        private static readonly Color SystemColor = Color.blue;
        private static readonly Color DefaultColor = Color.black;
        public static readonly List<string> LogStack = new List<string>();

        private static string RgbToHex(Color color)
        {
            int r = (int)(color.r * 255), g = (int)(color.g*255), b = (int)(color.b*255);
            return $"{r:X2}{g:X2}{b:X2}";
        }
        public enum Prefix
        {
            Message = 0,
            Warning = 1,
            Error = 2,
            System = 3,
            Default = 4
        }

        public static void Log(Prefix type, string message)
        {
            Color color;
            string prefix;
            switch(type)
            {
                case Prefix.Message:
                    color = MessageColor;
                    prefix = "Message";
                    break;
                case Prefix.Warning:
                    color = WarningColor;
                    prefix = "Warning";
                    break;
                case Prefix.Error:
                    color = ErrorColor;
                    prefix = "Error";
                    break;
                case Prefix.System:
                    color = SystemColor;
                    prefix = "System";
                    break;
                default:
                    color = DefaultColor;
                    prefix = "";
                    break;
            }

            if (!ShowPrefixes[(int) type]) return;
            var line = "<color=#" + RgbToHex(color) + ">" + prefix + "</color>" + message;
            if (LogStack.Count >= Limit)
            {
                LogStack.Clear();
            }
            LogStack.Add(line);
            Debug.Log(line);
        }
    }
}
