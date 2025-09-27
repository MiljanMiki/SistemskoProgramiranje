using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SPProjekat2.Server;
using SPProjekat2.IQAirApi;
using SPProjekat2.Models;

namespace SPProjekat2.Server
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
            listener.Prefixes.Add("http://localhost:2000/");
        }

     

        public void StartServer()
        {
            listener.Start();


            _ = listenerZaZahteveAsync();

        }

        public void StopServer()
        {
            listener.Stop();
            

        }
    

        public async Task listenerZaZahteveAsync()
        {
            try
            {
                while (listener.IsListening)
                {

                    Logger.Info(TAG, "Server pokrenut. Posaljite zahtev oblika: http://localhost:5500/city?city={city}&state={state}&country={country}");

                    var context = await listener.GetContextAsync();

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await preradiTaskRequest(context);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(TAG, "Zao nam je doslo je do greske " + e.Message);
                        }
                    });

                }
            }


            catch (HttpListenerException e)
            {
                Logger.Error(TAG,e.Message);
            }
            catch (Exception e)
            {
                Logger.Error(TAG, e.Message);
            }
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
      

        public async Task preradiTaskRequest(HttpListenerContext context)
        {
            //otprilike ovako da izgleda

            string url = context.Request.RawUrl;

            await Task.Run(async () =>
            {

                string city = ParseHTTP(url, "city");
                if (string.IsNullOrEmpty(city))
                    throw new ArgumentException("City je null!");

                string state = ParseHTTP(url, "state"); ;
                if (string.IsNullOrEmpty(state))
                    throw new ArgumentException("State je null!");

                string country = ParseHTTP(url, "country");
                if (string.IsNullOrEmpty(country))
                    throw new ArgumentException("Country je null!");

                string result;

                try
                {
                    //ne bi trebalo po url da trazimo nego po keywords...(city, state,country)
                    result = cache.vratiResponse(url);
                    Logger.Info(TAG, "[Cache Hit]" + url);
                }
                catch (ArgumentException e)
                {
                    result = await api.vratiZagadjenostGrada(city, state, country);

                    cache.ubaciUKes(url, result); // bolje bi bilo da je normalno nego async

                }

                // radi sa resultString posle sta oces.Mozda moze i unutar api da se formatira izlaz
                Logger.Info(TAG, result);
            });
        }

        private async Task PosaljiOdgovorAsync(HttpListenerContext context, int statusKod, string poruka)
        {
            context.Response.StatusCode = statusKod;
            byte[] buffer = Encoding.UTF8.GetBytes(poruka);

            context.Response.ContentType = "text/plain";
            context.Response.ContentLength64 = buffer.Length;

            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            await context.Response.OutputStream.FlushAsync();

            context.Response.OutputStream.Close();
        }
    }

}
