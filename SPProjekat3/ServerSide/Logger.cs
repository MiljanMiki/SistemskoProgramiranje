using System;


namespace SPProjekat3.ServerSide
{
    internal static class Logger
    {
        public static void Info(string tag, string message)
        {
            Console.WriteLine(tag + " [INFO] " + message);
        }
        public static void Warning(string tag, string message)
        {
            Console.WriteLine(tag + " [WARNING] " + message);
        }
        public static void Error(string tag, string message)
        {
            Console.Error.WriteLine(tag + " [ERROR] " + message);
        }
    }
}
