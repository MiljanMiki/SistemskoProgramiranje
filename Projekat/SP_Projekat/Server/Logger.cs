using System;


namespace SP_Projekat.Server
{
    internal static class Logger
    {
        

        public static void Info(string tag,string message)
        {
            Console.WriteLine(DateTime.Now + "\t"+tag +" [INFO] " + message);
        }
        public static void Warning(string tag, string message)
        {
            Console.WriteLine(DateTime.Now + "\t" + tag + " [WARNING] " +message);
        }
        public static void Error(string tag, string message)
        {
            Console.WriteLine(DateTime.Now + "\t" + tag + " [ERROR] " + message);
        }
    }
}
