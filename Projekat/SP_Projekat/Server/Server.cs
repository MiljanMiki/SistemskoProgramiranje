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

            listener.Prefixes.Add("http://localhost:5500/");
        }

        public void StartServer()
        {
            listener.Start();

            Logger.Info(TAG, "Server pokrenut. Posaljite zahtev oblika: http://localhost:5500/city?city={city}&state={state}&country={country}");

            while(listener.IsListening)
            {
                var context = listener.GetContext();

                preradiRequest(context);
            }

        }

        public void StopServer()
        {
            listener.Stop();

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
        private void preradiRequest(HttpListenerContext context)
        
        {


            ThreadPool.QueueUserWorkItem(_ =>
            {
                string url = context.Request.RawUrl.ToLower();
                
                if(url == "/favicon.ico")
                { 
                    //Logger.Error(TAG, "Favicon.ico zahtev primljen!");
                    vratiOdgovorKorisniku(context, HttpStatusCode.NoContent, "Nemamo ikonicu :( !");
                    return;
                }


                string city = ParseHTTP(url, "city");
                if (string.IsNullOrEmpty(city))
                {
                    Logger.Error(TAG, "City parametar je null!");
                    vratiOdgovorKorisniku(context, HttpStatusCode.BadRequest, "City parametar je null!");
                    return;
                }

                string state = ParseHTTP(url, "state"); ;
                if (string.IsNullOrEmpty(state))
                {
                    Logger.Error(TAG, "State parametar je null!");
                    vratiOdgovorKorisniku(context, HttpStatusCode.BadRequest, "State parametar je null!");
                    return;
                }

                string country = ParseHTTP(url, "country");
                if (string.IsNullOrEmpty(country))
                {
                    Logger.Error(TAG, "Country parametar je null!");
                    vratiOdgovorKorisniku(context, HttpStatusCode.BadRequest, "Country parametar je null!");
                    return;
                }

                string result;
                HttpStatusCode code;


                if ((result = cache.vratiResponse(url)) != null)
                {
                    Logger.Info(TAG, "[preradiRequest] [Cache Hit]" + url);
                    code = HttpStatusCode.OK;

                    Logger.Info(TAG, result);

                }
                else
                {
                    Logger.Error(TAG, $"[vratiResponse] {url} se ne nalazi u kesu!");

                    try
                    {

                        result = api.vratiZagadjenostGrada(city, state, country);
                        cache.ubaciUKes(url, result);//i ovo moze da baci exception!
                                                     //ali se hvata unutar cache-a
                        Logger.Info(TAG, result);
                        code = HttpStatusCode.OK;
                    }
                    catch (Exception e)//ako API vrati gresku
                    {
                        Logger.Error(TAG, "[IQAirApi]"+e.Message);
                        result = "ERROR:"+e.Message;
                        code = HttpStatusCode.NotFound;
                    }
                }

                vratiOdgovorKorisniku(context, code, result);

                
            });
        }

        private void vratiOdgovorKorisniku(HttpListenerContext context,HttpStatusCode code,string result)
        {
            context.Response.StatusCode = (int)code;
            byte[] buffer = Encoding.UTF8.GetBytes(result);
            context.Response.ContentType = "text/plain";
            context.Response.ContentLength64 = buffer.Length;

            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Flush();
            context.Response.OutputStream.Close();
        }

        

        public void preradiRequestString(string s)
        {
            //otprilike ovako da izgleda

            string url = s;
            var done = new ManualResetEvent(false);
            string result;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    result = cache.vratiResponse(url);
                    Logger.Info(TAG, "[preradiRequest] [Cache Hit]" + url);
                }
                catch (ArgumentException e)
                {
                    Logger.Error(TAG, e.Message);

                    string city = ParseHTTP(url, "city");
                    string state = ParseHTTP(url, "state"); ;
                    string country = ParseHTTP(url, "country");

                    result = api.vratiZagadjenostGrada(city, state, country);//baca exception ako
                    //je request los, mora da se preradi

                    cache.ubaciUKes(url, result);
                }

                Logger.Info(TAG, result);
                done.Set();
            });
            done.WaitOne();
        }
    }
}
