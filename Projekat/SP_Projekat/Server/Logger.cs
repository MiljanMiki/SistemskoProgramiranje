using System;


namespace SP_Projekat.Server
{
    //singleton klasa logger
    internal class Logger
    {
        private static Logger instance;

        private Logger() { }

        public static Logger Instance
        {
            get { if (instance == null) instance = new Logger(); return instance; }
        }

        public void Info(string tag,string message)
        {
            Console.WriteLine(tag+" [INFO] " + message);
        }
        public void Warning(string tag, string message)
        {
            Console.WriteLine(tag + " [WARNING] " +message);
        }
        public void Error(string tag, string message)
        {
            Console.Error.WriteLine(tag + " [ERROR] " + message);
        }
    }
}
