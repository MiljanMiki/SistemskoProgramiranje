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


            ThreadPool.QueueUserWorkItem(_ =>
            {
                string url = context.Request.RawUrl;

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
                    result = cache.vratiResponse(url);
                    Logger.Info(TAG,"[preradiRequest] [Cache Hit]" + url);
                }
                catch (ArgumentException e)
                {
                    Logger.Error(TAG,e.Message);
                    result = api.vratiZagadjenostGrada(city, state, country);
                    cache.ubaciUKes(url, result);//i ovo moze da baci exception!
                }

                Logger.Info(TAG,result);
            });
        }

        public void vratiOdgovorKorisniku(HttpListenerContext context,string result,bool uspesan)
        {
            //TODO
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
