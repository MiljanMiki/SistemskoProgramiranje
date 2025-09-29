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
    

        private async Task listenerZaZahteveAsync()
        {
            Logger.Info(TAG, "Server pokrenut. Posaljite zahtev oblika: http://localhost:2000/city?city={city}&state={state}&country={country}");
            try
            {
                while (listener.IsListening)
                {
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
                            vratiOdgovorKorisniku(context, HttpStatusCode.BadRequest, e.Message);
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

        private string ParseHTTP(string RawUrl, string key)
        {

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
      

        private async Task preradiTaskRequest(HttpListenerContext context)
        {

            string url = context.Request.RawUrl.ToLower();

            if (url == "/favicon.ico")//ovo smara, samo ignorisi
            {
                //Logger.Error(TAG, "Favicon.ico zahtev primljen!");
                vratiOdgovorKorisniku(context, HttpStatusCode.NoContent, "Nemamo ikonicu :( !");
                return;
            }

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
            HttpStatusCode code;

            if ((result = cache.vratiResponse(url)) != null)
            {
                Logger.Info(TAG, "[Cache Hit]" + url);
                code = HttpStatusCode.OK;
            }
            else
            {
                Logger.Error(TAG, $"[vratiResponse] {url} se ne nalazi u kesu!");

                try
                {
                    result = await api.vratiZagadjenostGrada(city, state, country);
                    Logger.Info(TAG, "Vracen rezultat od API");

                    cache.ubaciUKes(url, result);

                    code = HttpStatusCode.OK;
                }
                catch (Exception e)//hvata error od api
                {
                    Logger.Error(TAG, e.Message);
                    code = HttpStatusCode.NotFound;
                    result = "Error:" + e.Message;
                }

                //try
                //{
                //    //ne bi trebalo po url da trazimo nego po keywords...(city, state,country)
                //    result = cache.vratiResponse(url);
                //    Logger.Info(TAG, "[Cache Hit]" + url);
                //}
                //catch (ArgumentException e)
                //{
                //    result = await api.vratiZagadjenostGrada(city, state, country);

                //    cache.ubaciUKes(url, result); // bolje bi bilo da je normalno nego async

                //}
            }
            // radi sa resultString posle sta oces.Mozda moze i unutar api da se formatira izlaz
            Logger.Info(TAG, "Rezultat koji je vracen korisniku:\t" + result);
            vratiOdgovorKorisniku(context, code, result);
        }

        private async void vratiOdgovorKorisniku(HttpListenerContext context, HttpStatusCode code, string result)
        {
            context.Response.StatusCode = (int)code;
            byte[] buffer = Encoding.UTF8.GetBytes(result);
            context.Response.ContentType = "text/plain";
            context.Response.ContentLength64 = buffer.Length;

            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            await context.Response.OutputStream.FlushAsync();
            context.Response.OutputStream.Close();
        }
    }

}
