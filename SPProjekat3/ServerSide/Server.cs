using SPProjekat3.API;
using SPProjekat3.TextProcessing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SPProjekat3.ServerSide
{
    internal class Server
    {
        private HttpListener listener = new HttpListener();
        private readonly static NovostiAPI api = new NovostiAPI();
        private Cache cache;
        private readonly TopicModeler topicModeler;

        private const string TAG = "[SERVER]";
        //private RequestQueue;
        //private ResponseQueue;
        
        public Server(int velicinaKesa)
        {
            cache = new Cache(velicinaKesa);
            listener.Prefixes.Add("http://localhost:5050/");
            //listener.Prefixes.Add("https://localhost:5050/");
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
            try
            {
                while (true)
                {
                    var context = await listener.GetContextAsync();

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await preradiTaskRequestAsync(context);
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
                Logger.Error(TAG, e.Message);
            }
            catch (Exception e)
            {
                Logger.Error(TAG, e.Message);
            }
        }

        private string ParseHTTP(string RawUrl)
        {
            var result = new Dictionary<string, string>();

            int IndexOfStart = RawUrl.IndexOf(listener.Prefixes.First());//prvi i jedini prefiks

            if (IndexOfStart == -1)
            {
                return null;
            }

            //vraca samo uenti keyword
            return RawUrl.Substring(IndexOfStart + 1);
        }

        private async Task preradiTaskRequestAsync(HttpListenerContext context)
        {
            //otprilike ovako da izgleda

            string url = context.Request.RawUrl;

            await Task.Run(async () =>
            {

                string keyword= ParseHTTP(url);

                List<string> results;
                try
                {
                    results = cache.vratiResponse(keyword);
                    Logger.Info(TAG, "[Cache Hit] " + keyword);
                }
                catch (ArgumentException e)
                {
                    Logger.Error(TAG, "[Cache Miss] " +e.Message+" "+ keyword);
                    results = await api.vratiNajpopularnijeClankoveAsync(keyword);

                    cache.ubaciUKes(url, results); // bolje bi bilo da je normalno nego async

                }

                List<string> preradjeniTopics=preradiTopics(results);

                foreach (string topic in preradjeniTopics)
                    Logger.Info(TAG, topic);
            });
        }

        //nzm da li da bude async...
        private List<string> preradiTopics(List<string> descriptions)
        {
            return null;
        }
    }
}
