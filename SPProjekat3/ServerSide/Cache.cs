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

        public int VelicinaKesa { get; set; }

        public Cache(int velicinaKesa = 0)
        {
            this.velicinaKesa = velicinaKesa;
            dictionary = new Dictionary<CacheableRequest, List<string>>(velicinaKesa);
        }

        public void ubaciUKes(string request, List<string> response)
        {

            locker.EnterWriteLock();
            try
            {
                if (dictionary.Count() >= velicinaKesa)
                    cistiKes();

                //baca ArgumentException ako vec postoji u dictionary
                //hvataj u serveru!!!!
                dictionary.Add(new CacheableRequest(request, 0), response);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public List<string> vratiResponse(string request) 
        {
            try
            {
                locker.EnterReadLock();
                if (dictionary.ContainsKey(new CacheableRequest(request, 0)) == true)
                {
                    List<string> response;
                    if (dictionary.TryGetValue(new CacheableRequest(request), out response))
                    {
                        dictionary.
                                    FirstOrDefault(x => x.Key.HttpsRequest == request)
                                    .Key
                                    .incrementHit();//ovo inkrementiranje je atomicno
                                                    //ali lose sto je u read lock...

                    }
                    return response;
                }
                else
                    throw new ArgumentException(TAG + $"/[vratiResponse] \"{request}\"  se ne nalazi u kesu!");
            }
            finally
            {
                locker.ExitReadLock();
            }

        }

        private void cistiKes()
        {

            try
            {

                Logger.Info(TAG, "Cistim kes...");

                List<CacheableRequest> listaHitova = new List<CacheableRequest>(dictionary.Count);
                foreach (CacheableRequest request in dictionary.Keys)
                {
                    listaHitova.Add(request);
                }


                listaHitova = listaHitova.OrderBy(r => r.NumOfHits).ToList();

                int kolikiDeoKesaBrisemo = 2;
                for (int i = 0; i < dictionary.Count / kolikiDeoKesaBrisemo; ++i)
                    dictionary.Remove(listaHitova[i]);
            }
            finally
            {
                Logger.Info(TAG, "Ociscen kes");
            }
        }

    }

}
