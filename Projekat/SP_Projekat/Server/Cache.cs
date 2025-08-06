using SP_Projekat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Collections.Specialized;
namespace SP_Projekat.Server
{
   
    internal class Cache
    {
        //<http-request, resposne>
        private Dictionary<CacheableRequest, string> dictionary;

        //red sa https zahtevima
        private Queue<string> request_queue;

        private readonly ReaderWriterLockSlim locker=new ReaderWriterLockSlim();
        private System.Timers.Timer timer;
        private readonly int velicinaKesa;

        public Cache(int velicinaKesa=0)
        {
            dictionary = new Dictionary<CacheableRequest, string>(velicinaKesa);
            timer = new System.Timers.Timer(1000*60*5);//na svaka 5 minuta
            timer.Elapsed += cistiKes;
            timer.AutoReset = true;
        }
        public Cache(Dictionary<CacheableRequest, string> dictionary)
        {
            this.dictionary= dictionary;
        }

        public void dodajHttpsRequestURed(string request) { request_queue.Enqueue(request); }
        public string vratiPrviHttpsRequest() { return request_queue.Dequeue(); }
        public bool daLiJeRequestUKesu(string request)
        {
            if (dictionary.Count == 0)
                return false;
            try
            {
                locker.EnterReadLock();
                return dictionary.ContainsKey(new CacheableRequest(request, 0));
            }
            finally
            {

                locker.ExitReadLock();
            }
           
        }
        public void ubaciUKes(string request,string response)
        {
            locker.EnterWriteLock();
            try
            {
                if (dictionary.Count() == 0)
                    timer.Start();
                else if (dictionary.Count() > velicinaKesa)
                    cistiKes(null,null);
                dictionary.Add(new CacheableRequest(request, 0), response);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public string vratiResponse(string request)
        {
            locker.EnterReadLock();
            try
            {
                //pitanje kako hocemo da ga koristimo, ako prvo pitamo server dal uopste postoji
                //ne mora imamo ovaj if
                if (daLiJeRequestUKesu(request) == true)
                {
                    string response;
                    dictionary.TryGetValue(new CacheableRequest(request),out response);
                    return response;
                }
                return null;//mozda bolje da ovde baca exception, pa da server hvata i da 
                            //zove API
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public void cistiKes(object sender, ElapsedEventArgs e)
        {
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
