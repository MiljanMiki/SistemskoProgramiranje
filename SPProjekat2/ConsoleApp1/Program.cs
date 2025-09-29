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
        static void testirajKes(ServerSaKesZaTaskove server)
        {
            List<string> urls = new List<string>(20);

            urls.Add("http://localhost:5500/city?city=Nis&state=Central%20Serbia&country=Serbia");
            urls.Add("http://localhost:5500/city?city=Nis&state=Central%20Serbia&country=Serbia");
            urls.Add("http://localhost:5500/city?city=Nis&state=Central%20Serbia&country=Serbia");
            urls.Add("http://localhost:5500/city?city=Los%20Angeles&state=California&country=USA");
            urls.Add("http://localhost:5500/city?city=Los%20Angeles&state=California&country=USA");
            urls.Add("http://localhost:5500/city?city=Addison&state=New%20York&country=USA");
            urls.Add("http://localhost:5500/city?city=Addison&state=New%20York&country=USA");
            urls.Add("http://localhost:5500/city?city=Nis&state=Central%20Serbia&country=Serbia");
            urls.Add("http://localhost:5500/city?city=Los%20Angeles&state=California&country=USA");
            urls.Add("http://localhost:5500/city?city=Addison&state=New%20York&country=USA");
            urls.Add("http://localhost:5500/city?city=Albany&state=New%20York&country=USA");

            foreach (var url in urls)
            {
                Task.Run(async () =>
                {
                    server.testKes(url);
                });
            }
        }
        static void Main(string[] args)
        {
            //Server server = new Server(20);
            ServerSaKesZaTaskove server = new ServerSaKesZaTaskove(20);

            server.StartServer();
            Console.ReadLine(); // čeka da korisnik pritisne ENTER
            server.StopServer(); // opcionalno, ako imaš metodu za stop

        }
    }
}
