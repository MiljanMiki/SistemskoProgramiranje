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
        private Queue<string> request_queue;

        private readonly ReaderWriterLockSlim locker=new ReaderWriterLockSlim();
        private readonly int velicinaKesa;

        public Cache(int velicinaKesa=0)
        {
            dictionary = new Dictionary<CacheableRequest, string>(velicinaKesa);
        }
        public Cache(Dictionary<CacheableRequest, string> dictionary)
        {
            this.dictionary= dictionary;
        }

        public void dodajHttpsRequestURed(string request) { request_queue.Enqueue(request); }
        public string vratiPrviHttpsRequest() { return request_queue.Dequeue(); }
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
            locker.EnterWriteLock();
            try
            {
                //dal je ovde moguc deadlock, posto cistiKes uzima write lock,a ova f-ja jos drzi
                //read lock
                //prosto resenje da se samo prebaci gore
                if (dictionary.Count() > velicinaKesa)
                    cistiKes();
                if(daLiJeRequestUKesu(request)==false)
                    dictionary.Add(new CacheableRequest(request, 0), response);
                //else
                //  exception??
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

        private void cistiKes()
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
