using SP_Projekat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Specialized;
namespace SP_Projekat.Server
{
   
    internal class Cache
    {
        //<http-request, resposne>
        private Dictionary<CacheableRequest, string> dictionary;

        //red sa https zahtevima
        //private Queue<string> request_queue;

        private readonly ReaderWriterLockSlim locker=new ReaderWriterLockSlim();
        private readonly int velicinaKesa;
        private readonly string TAG = "[Cache]";

        public Cache(int velicinaKesa=0)
        {
            dictionary = new Dictionary<CacheableRequest, string>(velicinaKesa);
        }
        public Cache(Dictionary<CacheableRequest, string> dictionary)
        {
            this.dictionary= dictionary;
        }

       /* public void dodajHttpsRequestURed(string request) { request_queue.Enqueue(request); }
        public string vratiPrviHttpsRequest() { return request_queue.Dequeue(); }*/
        private bool daLiJeRequestUKesu(string request)
        {
            locker.EnterReadLock();
            try
            { 
                return dictionary.ContainsKey(new CacheableRequest(request, 0));
            }
            finally
            {
                locker.ExitReadLock();
            }
           
        }
        public void ubaciUKes(string request,string response)
        {
            if (dictionary.Count() > velicinaKesa)
                cistiKes();

            //if (daLiJeRequestUKesu(request) == false)
            {
                locker.EnterWriteLock();
                try
                { 
                        //baca ArgumentException ako vec postoji u dictionary
                        dictionary.Add(new CacheableRequest(request, 0), response);
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            //else
            //  throw new ArgumentException(TAG+"/[ubaciUKes] Request se vec nalazi u kesu!")
        }

        public string vratiResponse(string request)
        {
            //pitanje kako hocemo da ga koristimo, ako prvo pitamo server dal uopste postoji
            //ne mora imamo ovaj if
            if (daLiJeRequestUKesu(request) == true)
            {
                locker.EnterReadLock();
                try
                {
                    string response;
                    if (dictionary.TryGetValue(new CacheableRequest(request), out response))
                    {
                        dictionary.
                                    FirstOrDefault(x => x.Key.HttpsRequest==request)
                                    .Key
                                    .incrementHit();

                    }
                    return response;
                    
                }
                finally
                {
                    locker.ExitReadLock();
                }

            }
            else
                throw new ArgumentException(TAG+"/[vratiResponse] Request se ne nalazi u kesu!");//mozda bolje da ovde baca exception, pa da server hvata i da 
                                                                    //zove API
        }

        private void cistiKes()
        {
            //Logger.Info(TAG,"Cistim kes...");
            locker.EnterWriteLock();
            try
            {
                List<CacheableRequest> listaHitova = new List<CacheableRequest>(dictionary.Count);
                foreach (CacheableRequest request in dictionary.Keys)
                {
                    listaHitova.Add(request);
                }


                listaHitova = listaHitova.OrderBy(r => r.NumOfHits).ToList();

                int kolikiDeoKesaBrisemo = 5;
                for (int i = 0; i < dictionary.Count / kolikiDeoKesaBrisemo; ++i)
                    dictionary.Remove(listaHitova[i]);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

    }
}
