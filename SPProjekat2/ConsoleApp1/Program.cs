using SPProjekat2.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(20);
            server.StartServer();

            Console.WriteLine("Server je pokrenut. Pritisni ENTER za zaustavljanje.");
            Console.ReadLine(); // čeka da korisnik pritisne ENTER

            server.StopServer(); // opcionalno, ako imaš metodu za stop

        }
    }
}
