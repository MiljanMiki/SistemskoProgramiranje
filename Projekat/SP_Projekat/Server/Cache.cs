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
        //ranjivo na cache stampede problem
        private Dictionary<CacheableRequest, string> dictionary;


        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private readonly int velicinaKesa;
        private readonly string TAG = "[Cache]";

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

                //baca arguemnt exception!!!
                dictionary.Add(new CacheableRequest(request, 0), response);
            }
            catch(ArgumentException e)
            {
                Logger.Error(TAG, $"[ubaciUKes] {e.Message}");
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
                    return null;
                    
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