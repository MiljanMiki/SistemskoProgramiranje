using SPProjekat2.IQAirApi;
using SPProjekat2.Server;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace SPProjekat2.Server
{
    internal class ServerSaKesZaTaskove
    {
        private HttpListener listener;
        private Cache cache;
        private KesZaTaskove kesZaTaskove;
        private static IQAirService api;
        private readonly string TAG = "[Server]";

        //Ako nesto treba da se loguje, samo zovi Logger.Info
        //                                              .Error
        //                                              .Warning(TAG,message)
        //LOGUJ SVE! Kada uvatis gresku, da li se pocinje prerada, da li se zove API itd.


        public ServerSaKesZaTaskove(int velicinaKesa)
        {
            cache = new Cache(velicinaKesa);
            api = new IQAirService();
            kesZaTaskove = new KesZaTaskove(cache, api);
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5500/");
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
            Logger.Info(TAG, "Server pokrenut. Posaljite zahtev oblika: http://localhost:5500/city?city={city}&state={state}&country={country}");
            try
            {
                while (listener.IsListening)
                {
                    var context = await listener.GetContextAsync();

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await preradiTaskRequestAsyncV2(context);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(TAG, "Zao nam je doslo je do greske " + e.Message);
                            vratiOdgovorKorisnikuAsync(context, HttpStatusCode.BadRequest, e.Message);
                        }
                    });
                }
            }
            catch (HttpListenerException e)
            {
                Logger.Error(TAG, e.Message);
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

        //koristi obican cache
        //private async Task preradiTaskRequestAsync(HttpListenerContext context)
        //{

        //    string url = context.Request.RawUrl.ToLower();

        //    if (url == "/favicon.ico")//ovo smara, samo ignorisi
        //    {
        //        //Logger.Error(TAG, "Favicon.ico zahtev primljen!");
        //        vratiOdgovorKorisnikuAsync(context, HttpStatusCode.NoContent, "Nemamo ikonicu :( !");
        //        return;
        //    }

        //    string city = ParseHTTP(url, "city");
        //    if (string.IsNullOrEmpty(city))
        //        throw new ArgumentException("City je null!");

        //    string state = ParseHTTP(url, "state"); ;
        //    if (string.IsNullOrEmpty(state))
        //        throw new ArgumentException("State je null!");

        //    string country = ParseHTTP(url, "country");
        //    if (string.IsNullOrEmpty(country))
        //        throw new ArgumentException("Country je null!");

        //    string result;
        //    HttpStatusCode code;

        //    if ((result = cache.vratiResponse(url)) != null)
        //    {
        //        Logger.Info(TAG, "[Cache Hit]" + url);
        //        code = HttpStatusCode.OK;
        //    }
        //    else
        //    {
        //        Logger.Error(TAG, $"[vratiResponse] {url} se ne nalazi u kesu!");

        //        try
        //        {
        //            result = await api.vratiZagadjenostGradaAsync(city, state, country);
        //            Logger.Info(TAG, "Vracen rezultat od API");

        //            cache.ubaciUKes(url, result);

        //            code = HttpStatusCode.OK;
        //        }
        //        catch (Exception e)//hvata error od api
        //        {
        //            Logger.Error(TAG, e.Message);
        //            code = HttpStatusCode.NotFound;
        //            result = "Error:" + e.Message;
        //        }

               
        //    }
        //    Logger.Info(TAG, "Rezultat koji je vracen korisniku:\t" + result);
        //    vratiOdgovorKorisnikuAsync(context, code, result);
        //}

        private async Task preradiTaskRequestAsyncV2(HttpListenerContext context)
        {
            string url = context.Request.RawUrl.ToLower();

            if (url == "/favicon.ico")//ovo smara, samo ignorisi
            {
                //Logger.Error(TAG, "Favicon.ico zahtev primljen!");
                vratiOdgovorKorisnikuAsync(context, HttpStatusCode.NoContent, "Nemamo ikonicu :( !");
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


            try
            {
                result = await kesZaTaskove.vratiRezultatAsync(url,city,state,country);
                code = HttpStatusCode.OK;
            }
            catch(Exception e)
            {
                Logger.Error(TAG, e.Message);
                result = e.Message;
                code = HttpStatusCode.BadRequest;
            }

            if (result == null)//samo ako je puko api
                throw new Exception("Result je null!");

            Logger.Info(TAG, "Rezultat koji je vracen korisniku:\t" + result);

            vratiOdgovorKorisnikuAsync(context, code, result);
        }

        public async void testKes(string request)
        {
            string url = request.ToLower() ;

            

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


            try
            {
                result = await kesZaTaskove.vratiRezultatAsync(url, city, state, country);
                code = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                Logger.Error(TAG, e.Message);
                result = e.Message;
                code = HttpStatusCode.BadRequest;
            }

            if (result == null)
                throw new Exception("Result je null!");

            Logger.Info(TAG, "Rezultat koji je vracen korisniku:\t" + result+"\n");
        }
        private async void vratiOdgovorKorisnikuAsync(HttpListenerContext context, HttpStatusCode code, string result)
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
