using System;


namespace SPProjekat3.ServerSide
{
    internal static class Logger
    {
        public static void Info(string tag, string message)
        {
            Console.WriteLine(DateTime.Now + "\t"+tag + "[INFO] " + message);
        }
        public static void Warning(string tag, string message)
        {
            Console.WriteLine(DateTime.Now + "\t" + tag + " [WARNING] " + message);
        }
        public static void Error(string tag, string message)
        {
            Console.Error.WriteLine(DateTime.Now + "\t" + tag + " [ERROR] " + message);
        }
    }
}
