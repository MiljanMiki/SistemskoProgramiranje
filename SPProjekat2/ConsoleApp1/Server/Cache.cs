using SPProjekat2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Specialized;
using System.Threading.Tasks;
namespace SPProjekat2.Server
{

    internal class Cache
    {
        //<http-request, resposne>
        private Dictionary<CacheableRequest, string> dictionary;


        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private readonly int velicinaKesa;
        private const string TAG = "[Cache]";

        public int VelicinaKesa { get; set; }
        public Cache(int velicinaKesa=0)
        {
            this.velicinaKesa = velicinaKesa;
            dictionary = new Dictionary<CacheableRequest, string>(velicinaKesa);
        }
    
        public void ubaciUKes(string request, string response)
        {
            locker.EnterWriteLock();
            try
            {
                if (dictionary.Count() >= velicinaKesa)
                    cistiKes();

                //baca argument exception!!!
                dictionary.Add(new CacheableRequest(request, 0), response);
            }
            catch(ArgumentException e)
            {
                Logger.Error(TAG, e.Message);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public string vratiResponse(string request)
        {
            try
            {
                
                locker.EnterReadLock();
                if (dictionary.ContainsKey(new CacheableRequest(request, 0)) == true)
                {
                    string response;
                    if (dictionary.TryGetValue(new CacheableRequest(request), out response))
                    {
                        dictionary.
                                    FirstOrDefault(x => x.Key.HttpsRequest == request)
                                    .Key
                                    .incrementHit();

                    }

                    return response;
                }
                else
                    throw new ArgumentException($"{request} se ne nalazi u kesu!");
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

                int kolikiDeoKesaBrisemo = 5;
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