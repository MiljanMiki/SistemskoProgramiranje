using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SP_Projekat.IQAirApi;
using SP_Projekat.Models;

namespace SP_Projekat.Server
{
    internal class Server
    {
        private HttpListener listener;
        private Cache cache;
        private static IQAirApi.IQAirService api;
        private readonly string TAG = "[Server]";

        //Ako nesto treba da se loguje, samo zovi Logger.Info
        //                                              .Error
        //                                              .Warning(TAG,message)
        //LOGUJ SVE! Kada uvatis gresku, da li se pocinje prerada, da li se zove API itd.


        public Server(int velicinaKesa)
        {
            cache = new Cache(velicinaKesa);
            api = new IQAirService();
            listener = new HttpListener();
        }


        public string ParseHTTP(string RawUrl, string key)
        {
            var result = new Dictionary<string, string>();

            int IndexOfStart = RawUrl.IndexOf("?");


            if (IndexOfStart == -1)
            {
                return null;
            }


            string QueryString = RawUrl.Substring(IndexOfStart + 1);
            QueryString = QueryString.Replace("%20", " "); // Menjanje svih %20 u space


            string[] deloviurl = QueryString.Split('&');

            foreach (var deoUrl in deloviurl)
            {
                string[] ParametarUrl = deoUrl.Split('=');

                if (ParametarUrl.Length == 2 && ParametarUrl[0] == key)
                {
                    return ParametarUrl[1]; // ako ima i jednak je ovom vrati
                }

            }

            return null; // ako nema ne vraca nist


        }
        public void preradiRequest(HttpListenerContext context)
        {
            //otprilike ovako da izgleda

            string url = context.Request.RawUrl;

            ThreadPool.QueueUserWorkItem(_ =>
            {

                string city = ParseHTTP(url, "city");
                string state = ParseHTTP(url, "state"); ;
                string country = ParseHTTP(url, "country");
                string result;

                try
                {
                    result = cache.vratiResponse(url);
                    Console.WriteLine("[Cache Hit]" + url);
                }
                catch (ArgumentException e)
                {
                    result = api.vratiZagadjenostGrada(city, state, country);
                    cache.ubaciUKes(url, result);
                }

                // radi sa resultString posle sta oces.Mozda moze i unutar api da se formatira izlaz
                Console.Write(result);
            });
        }
    }
}
