using SPProjekat3.API;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace SPProjekat3.ServerSide
{
    internal class Server
    {
        private HttpListener listener = new HttpListener();
        private readonly static NovostiAPI api = new NovostiAPI();
        private Cache cache;
        private readonly Predictor predictor;

        private const string TAG = "[SERVER]";


        public Server(int velicinaKesa, string imeModela)
        {
            cache = new Cache(velicinaKesa);
            listener.Prefixes.Add("http://localhost:5050/");
            //listener.Prefixes.Add("https://localhost:5050/");

            predictor = new Predictor(imeModela);
        }

        public void StartServer()
        {
            listener.Start();
            _ = listenerZaZahteveReactiveAsync();

        }

        public void StopServer()
        {
            listener.Stop();
        }

        private async Task listenerZaZahteveReactiveAsync()
        {
            try
            {
                Logger.Info(TAG, "Server pokrenut. Posaljite zahtev oblika: http://localhost:5050/keyword");

                var requestStream = new Subject<HttpListenerContext>();

                //SelectMany po default-u koristi DefaultScheduler
                //tj svaki subscriber se izvrsava na posebnom thread-u

                /*
                 requestStream
                .SelectMany(c =>
                    Observable.StartAsync(
                        async ct => await preradiTaskRequestAsync(c),
                        NewThreadScheduler.Default
                    )
                )
                .Subscribe(
                    _ => { },
                    error => Logger.Error(TAG, "[RequestStream] " + error),
                    () => Logger.Info(TAG, "Zavrseno")
                );*/

                requestStream
                    .ObserveOn(NewThreadScheduler.Default)//mislim da ovo nema efekta
                    .SelectMany(c =>
                            Observable.FromAsync(() => preradiTaskRequestAsync(c))
                    ).Subscribe(
                     _ => { },
                     error => Logger.Error(TAG, "[RequestStream]" + error),
                     () => Logger.Info(TAG, "Zavrseno")
                );

                while (listener.IsListening)
                {
                    var context = await listener.GetContextAsync();
                    requestStream.OnNext(context);

                }

                requestStream.Dispose();
            }
            catch (Exception e)
            {
                Logger.Error(TAG, "[listenerZaZahteveReactiveAsync] Greska: " + e.Message);
            }
        }

        private string ParseHTTP(string RawUrl)
        {
            RawUrl = RawUrl.Replace("%20", " ");
            RawUrl = RawUrl.ToLower();
            return RawUrl.Replace("/", "");
        }

        private async Task preradiTaskRequestAsync(HttpListenerContext context)
        {

            string url = context.Request.RawUrl;

            await Task.Run(async () =>
            {

                string keyword = ParseHTTP(url);

                if (keyword == null)
                    throw new Exception("[preradiTaskRequestAsync]Greska prilikom parsiranja url-a!");

                List<string> results;
                try
                {
                    results = cache.vratiResponse(keyword);
                    Logger.Info(TAG, "[Cache Hit] " + keyword);
                }
                catch (ArgumentException e)
                {
                    Logger.Error(TAG, "[Cache Miss] " + e.Message + " " + keyword);
                    results = await api.vratiNajpopularnijeClankoveAsync(keyword);

                    cache.ubaciUKes(keyword, results);//i ovo baca exception!!! 
                }

                foreach (var result in results)
                {
                    var predikcija = predictor.predict(result);
                    Logger.Info(TAG, "Za " + keyword + "\t" + predikcija);
                }

            });
        }

        public async void testKes(string url)
        {

            string keyword = ParseHTTP(url);

            if (keyword == null)
                throw new Exception("Greska prilikom parsiranja url-a!");
            try
            {

                List<string> results;
                try
                {
                    results = cache.vratiResponse(keyword);
                    Logger.Info(TAG, "[Cache Hit] " + keyword);
                }
                catch (ArgumentException e)
                {
                    Logger.Error(TAG, "[Cache Miss] " + e.Message + " " + keyword);
                    results = await api.vratiNajpopularnijeClankoveAsync(keyword);

                    cache.ubaciUKes(keyword, results);
                    Logger.Info(TAG, "U kes ubaceni results za: " + keyword);

                }
                foreach (var result in results)
                {
                    var predikcija = predictor.predict(result);
                    Logger.Info(TAG, "Za " + keyword + "\t" + predikcija);
                }
            }
            catch (Exception e)
            {
                Logger.Error(TAG, "[TestKes]" + e.Message);
            }
        }
    }
}
