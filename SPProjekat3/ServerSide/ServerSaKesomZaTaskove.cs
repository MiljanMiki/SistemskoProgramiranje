using SPProjekat3.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPProjekat3.ServerSide
{
    internal class ServerSaKesomZaTaskove
    {
        private HttpListener listener = new HttpListener();
        private readonly static NovostiAPI api = new NovostiAPI();
        private Cache cache;
        private KesZaTaskove kesZaTaskove;
        private readonly Predictor predictor;

        private const string TAG = "[SERVER]";


        public ServerSaKesomZaTaskove(int velicinaKesa, string imeModela)
        {
            cache = new Cache(velicinaKesa);
            kesZaTaskove = new KesZaTaskove(cache, api);

            listener.Prefixes.Add("http://localhost:5050/");

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
                Logger.Info(TAG,"Server pokrenut. Posaljite zahtev oblika http://localhost:5050/keyword ");

                var requestStream = new Subject<HttpListenerContext>();



                //v1
                //requestStream
                //    //.ObserveOn(NewThreadScheduler.Default)//mislim da ovo nema efekta
                //    .SelectMany(c =>
                //            Observable.FromAsync(async () => await preradiTaskRequestAsync(c))
                //    ).Subscribe(
                //     _ => { },
                //     error => Logger.Error(TAG, "[RequestStream]" + error),
                //     () => Logger.Info(TAG, "Zavrseno")
                //);

                requestStream
                            //.ObserveOn(NewThreadScheduler.Default)
                            .SelectMany(c =>
                                Observable.FromAsync(() => preradiTaskRequestAsync(c))
                                .SubscribeOn(TaskPoolScheduler.Default)
                            ).Subscribe(
                                _ => { },
                                error => Logger.Error(TAG, "[RequestStream] Unhandled error: " + error),
                                () => Logger.Info(TAG, "Request stream finished")
                            );



                while (listener.IsListening)
                {
                    Logger.Info(TAG, "Cekam za HTTPcontext...");
                    var context = await listener.GetContextAsync();
                    Logger.Info(TAG, "Dobio sam HTTPContext");
                    requestStream.OnNext(context);
                }
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
             //await Task.Run(async () =>
            //{
                string url = context.Request.RawUrl;

                string keyword = ParseHTTP(url);

                if (keyword == null)
                    throw new Exception("[preradiTaskRequestAsync]Greska prilikom parsiranja url-a!");

                Logger.Info(TAG, $"Thread sa ID: {Environment.CurrentManagedThreadId} preradjuje request");


                List<string> results;
                try
                {
                    results = await kesZaTaskove.vratiRezultatAsync(keyword);
                }
                catch (Exception e)
                {
                    Logger.Error(TAG, e.Message);
                    return;
                }

                if (results == null)//vraca null samo ako dodje do API greske
                    throw new Exception("Greska: Results je null!!!");

                foreach (var result in results)
                {
                    var predikcija = predictor.predict(result);
                    //Logger.Info(TAG, "Za " + keyword + "\t" + predikcija);da ne bude spam
                }

            //});

        }

        public async void testKes(string request)
        {
            string url = request;

            string keyword = ParseHTTP(url);

            if (keyword == null)
                throw new Exception("[preradiTaskRequestAsync]Greska prilikom parsiranja url-a!");

            Logger.Info(TAG, $"Thread sa ID: {Environment.CurrentManagedThreadId} preradjuje request");


            List<string> results;
            try
            {
                results = await kesZaTaskove.vratiRezultatAsync(keyword);
            }
            catch (Exception e)
            {
                Logger.Error(TAG, e.Message);
                return;
            }

            if (results == null)//vraca null samo ako dodje do API greske
                throw new Exception("Greska: Results je null!!!");

            foreach (var result in results)
            {
                var predikcija = predictor.predict(result);
                // Logger.Info(TAG, "Za " + keyword + "\t" + predikcija);
            }
        }
    }
}
