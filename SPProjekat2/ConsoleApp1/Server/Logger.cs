using System;


namespace SPProjekat2.Server
{
    internal static class Logger
    {
        //private static Logger instance;

        /*private Logger() { }

        public static Logger Instance
        {
            get { if (instance == null) instance = new Logger(); return instance; }
        }*/

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
