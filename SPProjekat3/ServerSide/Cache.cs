using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


using SPProjekat3.Models;


namespace SPProjekat3.ServerSide
{
    internal class Cache
    {
        private Dictionary<CacheableRequest, List<string>> dictionary;

        

        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private readonly int velicinaKesa;
        private readonly string TAG = "[Cache]";

        public Cache(int velicinaKesa = 0)
        {
            this.velicinaKesa=velicinaKesa;
            dictionary = new Dictionary<CacheableRequest, List<string>>(velicinaKesa);
        }
        public Cache(Dictionary<CacheableRequest, List<string>> dictionary)
        {
            this.dictionary = dictionary;
        }

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
        public void ubaciUKes(string request, List<string> response)
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

        public List<string> vratiResponse(string request) 
        {
            if (daLiJeRequestUKesu(request) == true)
            {
                locker.EnterReadLock();
                try
                {
                    List<string> response;
                    if (dictionary.TryGetValue(new CacheableRequest(request), out response))
                    {
                        dictionary.
                                    FirstOrDefault(x => x.Key.HttpsRequest == request)
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
                throw new ArgumentException(TAG + "/[vratiResponse] Request se ne nalazi u kesu!");//mozda bolje da ovde baca exception, pa da server hvata i da 
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
